using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using Eonic.Pure360.paint;
using System.Runtime.Serialization.Formatters.Soap;
using System.IO;

/// <summary>
/// Utility class for creating and re-using a session within PAINT.  Utility
/// methods are included to implement the standard actions e.g. creating and
/// email or uploading a list.
/// </summary>
public class PaintSession
{
    /** Id to link requests together between logging in and out **/
    protected String contextId;

    protected String loginName;
    protected String password;

    /** Hashtable containing the data for this context **/
    protected Hashtable contextData;

    // Construct the class with the relevant credentials
    public PaintSession()
	{
        // ** ENTER YOUR CREDENTIALS HERE **
        //this.loginName = "yourUsername";
       // this.password = "yourPassword";

    }

    /**
     * Log into the Pure system and obtain a context id.
     * This is automatically called from the class constructor 
     * and so is probably only required if manually logging
     * out and then back in again. 
     */    
    public void login(string username, string password)
    {
        Hashtable entityInput = null;
        Hashtable resultOutput = null;

        this.loginName = username;
        this.password = password;

        // Sanity check that the user name and password have been set correctly
        //if (this.loginName == username && this.password == password)
        //{
        //    throw new PaintSystemException("You have not set the user name and password " + 
        //                                    "for your account.  Please see the PaintSession " + 
        //                                    "class in the com.pure360.paint namespace and " + 
        //                                    "update the values set in the constructor.");
        //}

        // Create argument data into a hashtable
        entityInput = new Hashtable();
        entityInput.Add("userName", this.loginName);
        entityInput.Add("password", this.password);

        // Login 
        resultOutput = this.sendRequest("bus_facade_context", "login", entityInput);

        // Store the context id on the class
        this.contextData = resultOutput;
        this.contextId = (String)((Hashtable)resultOutput["bus_entity_context"])["beanId"];        
    }

    /**
     * Log out of the current context.  This will remove the context id and you won't be
     * able to issue any other requests after this other than login.
     */
    public void logout()
    {
        // No data needs to be sent to this request
        this.sendRequest("bus_facade_context", "logout", null);

        this.contextId = null;
    }

    /**
     * Search for an entity by a set of search parameters and return the key fields for
     * the entity or entities found.
     */
    public Hashtable search(String facadeBean, Hashtable searchParameters)
    {
        Hashtable resultOutput = null;
        String searchBean = facadeBean.Replace("bus_facade", "bus_search");

        // First search to see if an email already exists with this name (assumes no SMS on the account)
        resultOutput = this.sendRequest(facadeBean, "search", searchParameters);

        // Access the data using the search bean name NOT the facade bean name
        resultOutput = (Hashtable)resultOutput[searchBean];
        resultOutput = (Hashtable)resultOutput["idData"];

        return resultOutput;
    }

    /**
    * Search for an entity using search parameters but ensure that only an exact match
    * for all parameters is returned.
    */
    public Hashtable searchExactMatch(String facadeBean, Hashtable searchParameters)
    {
    	Hashtable searchResults = null;
    	Hashtable exactMatchData = null;
        String entityBean = facadeBean.Replace("bus_facade", "bus_entity");
        
    	// Perform the general search to obtain a list of ids
    	searchResults = this.search(facadeBean, searchParameters);
    	
    	// Loop through the ids and call the load method until we find an exact match
    	foreach(String tmpKey in searchResults.Keys)
    	{
            Hashtable loadInput = (Hashtable)searchResults[tmpKey];
    		Hashtable beanInst = this.sendRequest(facadeBean, "load", loadInput, null);
    		
    		if(beanInst != null)
    		{
    			bool exactMatch = true;
                Hashtable beanData = null;
    			
    			foreach(String paramName in searchParameters.Keys)
    			{
                    String paramValue = (String)searchParameters[paramName];
    				beanData = (Hashtable)beanInst[entityBean];
    				
    				if(beanData[paramName] != null && ((String)beanData[paramName]) != paramValue)
    				{
    					exactMatch = false;
   					}
   				}
   				
   				if(exactMatch)
   				{
	   				exactMatchData = beanData;
   				}
    		}
   		}
   		
   		if(exactMatchData == null)
   		{
            Hashtable errors = new Hashtable();
            errors.Add("searchExactMatch", "No exact match found for " + facadeBean);
   			throw new PaintValidationException(errors);
   		}
   		
    	return exactMatchData;
   	}

    /**
     * sendRequest override version to pass null for the process data since this is rarely
     * required.
     */
    public Hashtable sendRequest(String className,
                                    String processName,
                                    Hashtable entityInput)
    {
        return this.sendRequest(className, processName, entityInput, null);
    }

    /**
     * Send a request to PAINT passing the required parameters
     * and returning a hashtable of hashtables as the result
     * if successful, or throw an exception if unsuccessful.
     */
    public Hashtable sendRequest(   String className,
                                    String processName,
                                    Hashtable entityInput,
                                    Hashtable processInput)
    {
        paintKeyValuePair[] entityPairs = null;
        paintKeyValuePair[] processPairs = null;
        paintKeyValuePair[] resultPairs = null;
        paintService ps = null;
        Hashtable resultOutput = null;

        // Check that the context is valid
        if (processName != "login" && this.contextId == null)
        {
            throw new PaintSystemException("No context available for this request");
        }

        // Convert the hashtable parameters into PAINT parameters
        entityPairs = this.convertDataToPaintPairs(entityInput);
        processPairs = this.convertDataToPaintPairs(processInput);

        // Call the PAINT service
        ps = new paintService();
        resultPairs = ps.handleRequest(this.contextId,
                                        className,
                                        processName,
                                        entityPairs,
                                        processPairs);

        // Convert the data into a hashtable of hashtables
        resultOutput = convertPaintPairsToHashtable(resultPairs);

        switch (resultOutput["result"].ToString())
        {
            case "success":
                if (resultOutput["resultData"] != null)
                {
                    resultOutput = (Hashtable)resultOutput["resultData"];
                }
                else
                {
                    // Update requests return no data back
                    resultOutput = new Hashtable();
                }
                break;

            case "bean_exception_validation":
                SaveToFile(resultOutput, "C://Web/logs/xmlresponse.xml");
                throw new PaintValidationException((Hashtable)resultOutput["resultData"]);
               
            case "bean_exception_security":
                SaveToFile(resultOutput, "C://Web/logs/xmlresponse.xml");
                throw new PaintSecurityException((String)resultOutput["resultData"]);

            case "bean_exception_system":
                SaveToFile(resultOutput, "C://Web/logs/xmlresponse.xml");
                throw new PaintSystemException((String)resultOutput["resultData"]);

            default:
                SaveToFile(resultOutput, "C://Web/logs/xmlresponse.xml");
                throw new Exception("Unhandled exception thrown from PAINT");
        }

        return resultOutput;
    }

    private static void SaveToFile(Hashtable hashtable, string fileName)
    {
        using (Stream stream = File.Create(fileName))
        {

            SoapFormatter serializer = new SoapFormatter();
            serializer.Serialize(stream, hashtable);
        }
    }

    /**
     * Return the data from the context.  This data is loaded whe
     * logging in and contains details about the login, profile and group
     */
    public Hashtable getContextData()
    {
        return this.contextData;
    }

    /**
     * Convert a result hashtable into a string for debug purposes
     */
    public String convertResultToDebugString(Hashtable result)
    {
        string resultStr = "";

        foreach (string tmpKey in result.Keys)
        {
            if (result[tmpKey] != null && result[tmpKey] is Hashtable)
            {
                resultStr = resultStr + "<BR/>---><BR/>[Nested Hashtable Key] [" + tmpKey + "]" + this.convertResultToDebugString((Hashtable)result[tmpKey]);
            }
            else
            {
                resultStr = resultStr + "<BR/>Key [" + tmpKey + "] Value [" + result[tmpKey].ToString() + "]";
            }
        }

        resultStr = resultStr + "<BR/><---<BR/>";

        return resultStr;
    }

    /**
     * Convert a hashtable into an array of PAINT pairs
     */
    protected paintKeyValuePair[] convertDataToPaintPairs(Hashtable source)
    {
        int elementCounter = 0;
        paintKeyValuePair[] convertedPairs = null;

        // Protection against nulls
        if (source == null)
        {
            source = new Hashtable();
        }

        // Convert each item in the hashtable into a pair.  Note that the hashtable must only
        // contain key-value pairs of strings.
        convertedPairs = new paintKeyValuePair[source.Count];
        elementCounter = 0;
        foreach (String tmpKey in source.Keys)
        {
            paintKeyValuePairValue tmpValue = new paintKeyValuePairValue();
            paintKeyValuePair tmpPair = new paintKeyValuePair();

            if (source[tmpKey] is Hashtable)
            {
                tmpValue.arr = this.convertDataToPaintPairs((Hashtable)source[tmpKey]);
            }
            else
            {
                tmpValue.str = source[tmpKey].ToString();
            }

            tmpPair.key = tmpKey;
            tmpPair.value = tmpValue;

            convertedPairs[elementCounter] = tmpPair;
            elementCounter++;
        }

        return convertedPairs;
    }

    /**
     * Convert an array of PAINT pairs into a hashtable of hashtables
     */
    protected Hashtable convertPaintPairsToHashtable(paintKeyValuePair[] source)
    {
        Hashtable convertedHashtable = new Hashtable();

        if (source != null)
        {
            for (int index = 0; index < source.Length; index++)
            {
                paintKeyValuePair item = source[index];
                String key = item.key;
                String valueStr = item.value.str;
                paintKeyValuePair[] valueArr = item.value.arr;

                if (valueArr != null)
                {
                    // We have a nested array of more pairs
                    convertedHashtable.Add(item.key, convertPaintPairsToHashtable(valueArr));
                }
                else
                {
                    // We have an end value in the form of a string
                    convertedHashtable.Add(item.key, valueStr);
                }
            }
        }

        return convertedHashtable;
    }
}
