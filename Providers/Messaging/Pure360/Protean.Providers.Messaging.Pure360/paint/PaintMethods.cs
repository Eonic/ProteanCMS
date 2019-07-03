using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using Eonic.Pure360.paint;


/// <summary>
/// Class holding short-cuts to the different operations this 
/// application will make using PAINT.  This inherits from PaintSession
/// which handles the login and context id plus provides handler methods.
/// </summary>
public class PaintMethods : PaintSession
{
    /**
     * Empty constructor calling parent constructor only
     */
    public PaintMethods()
        : base()
    {
    }

    /**
     * Create a new message on the account.  This function isolates some of the basic features
     * of a message.  More complicated features must be accessed using the sendRequest
     * function and using the data dictionary to discover the required data fields
     */
    public Hashtable createEmail(String messageName,
                                    String subject,
                                    String bodyPlain,
                                    String bodyHtml)
    {
        bool messageFound = false;
        Hashtable searchInput = new Hashtable();
        Hashtable deleteInput = new Hashtable();
        Hashtable emailInput = new Hashtable();
        Hashtable resultOutput = null;

        // Put the data for the email into a hashtable keyed on the field names taken from the 
        // data dictionary
        emailInput.Add("messageName", messageName);
        emailInput.Add("subject", subject);
        emailInput.Add("bodyPlain", bodyPlain);
        emailInput.Add("bodyHtml", bodyHtml);

        // Search to see if an email already exists with this name (assumes no SMS on the account)
        if (messageName != null && messageName != "")
        {
            searchInput.Add("messageName", messageName);
            resultOutput = this.search("bus_facade_campaign_email", searchInput);
        }

        if (resultOutput.Count > 0)
        {
            // Loop through the results in case there are other messages that contain the 
            // same string within their message name
            for (int index = 0; index < resultOutput.Count && !messageFound; index++)
            {
                Hashtable loadOutput = null;
                Hashtable loadOutputFields = null;
                Hashtable loadInput = (Hashtable)resultOutput[index + ""];

                // Use the id data returned from the search to load the specific email
                loadOutput = this.sendRequest("bus_facade_campaign_email", "load", loadInput);
                loadOutputFields = (Hashtable)loadOutput["bus_entity_campaign_email"];
                if (((String)loadOutputFields["messageName"]) == messageName)
                {
                    resultOutput = loadOutput;
                    messageFound = true;
                }
            }
        }
        
        if(!messageFound)
        {
            // No existing message found so we'll create a new one
            resultOutput = this.sendRequest("bus_facade_campaign_email", "create", null);
        }

        // Whether we loaded the bean or created a new one, we'll have a bean id now. 
        // Put the bean id along with the rest of the data and request to store. After
        // this the bean will have been cleared away.
        emailInput.Add("beanId", (String)((Hashtable)resultOutput["bus_entity_campaign_email"])["beanId"]);

        resultOutput = this.sendRequest("bus_facade_campaign_email", "store", emailInput);

        return resultOutput;
    }

    /**
     * Send a request to upload a new list.  If the list already exists then we'll
     * remove it first and create a new one.  Note that the list data should be a CSV
     * string starting with a comma separated list of field names. At least one header 
     * must be either "email" or "mobile".
     */
    public Hashtable addList(String listName,
                                String listDataSource,
                                String notifyUri
                                )
    {
        try
        {

            bool listFound = false;
            Hashtable searchInput = new Hashtable();
            Hashtable listInput = new Hashtable();
            Hashtable resultOutput = null;
            string addType = "store";


        // Search to see if an email already exists with this name (assumes no SMS on the account)
        if (listName != null && listName != "")
        {
            searchInput.Add("listName", listName);
            resultOutput = this.search("bus_facade_campaign_list", searchInput);
        }

        // If we found the correct list then remove it first
        if (resultOutput.Count > 0)
        {
            // Loop through the results in case there are other lists that contain the 
            // same string within their list name
            for (int index = 0; index < resultOutput.Count && !listFound; index++)
            {
                Hashtable loadOutput = null;
                Hashtable loadOutputFields = null;
                Hashtable loadInput = (Hashtable)resultOutput[index + ""];

                // Use the id data returned from the search to load the specific list
                loadOutput = this.sendRequest("bus_facade_campaign_list", "load", loadInput);
                loadOutputFields = (Hashtable)loadOutput["bus_entity_campaign_list"];
                if (((String)loadOutputFields["listName"]) == listName)
                {
                        Hashtable removeInput = new Hashtable();

                        // Remove the existing list
                        removeInput.Add("beanId", ((String)loadOutputFields["beanId"]));
                        this.sendRequest("bus_facade_campaign_list", "remove", removeInput);
                        listFound = true;

                       // addType = "append";
                }
            }
        }

        // Put the data for the list into the hashtable.  Note that the header row needs to
        // be split out and is used to create the custom field names.  
        listInput.Add("listName", listName);

        if (notifyUri != null)
        {
            listInput.Add("uploadFileNotifyEmail", notifyUri);
        }

        if (listDataSource != null)
        {
            int endFirstRowPos = 0;
            int customFieldCount = 0;
            String firstRow = null;
            String[] fieldNames = null;
            
            // Extract the first row from the list data
            endFirstRowPos = (listDataSource.IndexOf("\n") + 1);
            if (endFirstRowPos > 0)
            {
                firstRow = listDataSource.Substring(0, endFirstRowPos);
                listDataSource = listDataSource.Substring(endFirstRowPos);
            }

            // Split this into the different column names
            fieldNames = firstRow.Split(',');

            // Loop through each column name and add them to the custom field
            // names list until all have been added or we have reached the maximum 
            // allowed
            for (int index = 0; (index < fieldNames.Length & customFieldCount <= 10); index++)
            {
                String fieldName = fieldNames[index];

                switch (fieldName)
                {
                    case "email":
                        listInput.Add("emailCol", index);
                        break;

                    case "mobile":
                        listInput.Add("mobileCol", index);
                        break;

                    default:
                        String fieldColStr = "field" + index + "Col";
                        String fieldNameStr = "field" + index + "Name";

                            // Replace illegal spaces
                            fieldName = fieldName.Replace(@" ", @"_");
                          
                       // Add data to the list so PAINT knows about the fields
                        listInput.Add(fieldColStr, index);
                        listInput.Add(fieldNameStr, fieldName);

                        // Keep count so we don't go over ten (PAINT would ignore them)
                        customFieldCount++;
                        break;
                }
            }
        }

        // Use the "paste" field to pass in the string of data.  File uploads are not currently
        // supported via PAINT.
        listInput.Add("pasteFile", listDataSource);

        // Now create the new list bean for us to reference and load with data
        resultOutput = this.sendRequest("bus_facade_campaign_list", "create", null);

        // Set the data onto the list and save to the system.  Note that the bean will
        // bean cleared away from the session after this
        listInput.Add("beanId", (String)((Hashtable)resultOutput["bus_entity_campaign_list"])["beanId"]);
        resultOutput = this.sendRequest("bus_facade_campaign_list", addType, listInput);

        return resultOutput;

        }
        catch (Exception ex)
        {
           // if (OnError != null)
          //  {
              //  OnError(this, new Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "GetApplicant", ex, ""));
          //  }
            return null;
        }
    }

    public Hashtable addList(String listName,
                               DataTable listDataTable,
                               String notifyUri, 
                               String addType = "store"
                               )
    {

        Hashtable listInput = new Hashtable();
        Hashtable resultOutput = null;
        string listDataSource = "";
        try
        {

            bool listFound = false;
            Hashtable searchInput = new Hashtable();
            


            // Search to see if an email already exists with this name (assumes no SMS on the account)
            if (listName != null && listName != "")
            {
                searchInput.Add("listName", listName);
                resultOutput = this.search("bus_facade_campaign_list", searchInput);
            }

            

            // If we found the correct list then remove it first
            if (resultOutput.Count > 0)
            {
                // Loop through the results in case there are other lists that contain the 
                // same string within their list name
                for (int index = 0; index < resultOutput.Count && !listFound; index++)
                {
                    Hashtable loadOutput = null;
                    Hashtable loadOutputFields = null;
                    Hashtable loadInput = (Hashtable)resultOutput[index + ""];

                    // Use the id data returned from the search to load the specific list
                    loadOutput = this.sendRequest("bus_facade_campaign_list", "load", loadInput);
                    loadOutputFields = (Hashtable)loadOutput["bus_entity_campaign_list"];
                    if (((String)loadOutputFields["listName"]) == listName)
                    {
                        Hashtable actionInput = new Hashtable();

                        // Remove the existing list
                        
                        if (addType == "store")
                        {
                            actionInput.Add("beanId", ((String)loadOutputFields["beanId"]));
                            this.sendRequest("bus_facade_campaign_list", "remove", actionInput);
                        
                        }
                        else {
                           // actionInput.Add("entityId", ((String)loadOutputFields["entityId"]));
                           // this.sendRequest("bus_facade_campaign_list", "load", actionInput);
                        }
                        listFound = true;
                    }
                }
            }
            // Put the data for the list into the hashtable.  Note that the header row needs to
            // be split out and is used to create the custom field names.  
            listInput.Add("listName", listName);
            if (addType == "append")
            {
                listInput.Add("uploadTransactionType", "APPEND");
                addType = "store";
            }

            if (notifyUri != null)
            {
                listInput.Add("uploadFileNotifyEmail", notifyUri);
            }
            if (addType == "store")
            {
                int colCount = 0;
                if (listDataTable != null)
                {
                    // Add the column Definititions
                    foreach (DataColumn dc in listDataTable.Columns)
                    {

                        switch (dc.ColumnName)
                        {
                            case "Email":
                                listInput.Add("emailCol", colCount);
                                break;

                            case "Mobile":
                                listInput.Add("mobileCol", colCount);
                                break;
                            case "Registration_Datetime":
                                listInput.Add("signupDateCol", colCount);
                                break;

                            default:
                                String fieldColStr = "field" + colCount + "Col";
                                String fieldNameStr = "field" + colCount + "Name";
                                String fieldTypeStr = "field" + colCount + "DataType";

                                // Add data to the list so PAINT knows about the fields
                                listInput.Add(fieldColStr, colCount);
                                listInput.Add(fieldNameStr, dc.ColumnName);
                                if (dc.DataType == typeof(DateTime))
                                {
                                    listInput.Add(fieldTypeStr, "date");
                                    listInput.Add("field" + colCount + "DataFormat", "yyyy-mm-dd");
                                }
                                break;
                        }
                        colCount++;
                    };
                }
            }


                foreach (DataRow dr in listDataTable.Rows)
                {
                    foreach (DataColumn dc in listDataTable.Columns)
                    {

                        switch (dc.ColumnName)
                        {
                            case "Registration_Datetime":
                                listDataSource += Convert.ToDateTime(dr[dc.ColumnName]).ToString("yyyy-MM-dd hh:mm:ss") + ",";
                                break;

                            default:

                                switch (dc.DataType.ToString())
                                {
                                    case "System.DateTime":
                                        if (dr[dc.ColumnName] != DBNull.Value)
                                        {
                                            listDataSource += Convert.ToDateTime(dr[dc.ColumnName]).ToString("yyyy-MM-dd") + ",";
                                        }
                                        else {
                                            listDataSource += "0000-00-00 00:00:00,";
                                        }
                                        break;
                                    case "System.DBNull":
                                        listDataSource += @",";
                                        break;
                                    default:
                                        if (dr[dc.ColumnName] != DBNull.Value)
                                        {
                                            string myVal = (String)dr[dc.ColumnName];
                                        myVal = myVal.Replace(@"""", @"\'""\'");
                                        myVal = @"""" + myVal + @"""";
                                        myVal = myVal.Replace(@"/", @"\'/\'");

                                        listDataSource += myVal + @",";

                                        }
                                        else {
                                            listDataSource += @",";
                                        }
                                        break;
                                    }
                                break;
                        };
                    };
                    listDataSource += "\r\n";
                };
                // Use the "paste" field to pass in the string of data.  File uploads are not currently
                // supported via PAINT.
                listInput.Add("pasteFile", listDataSource);


            // Now create the new list bean for us to reference and load with data
            resultOutput = this.sendRequest("bus_facade_campaign_list", "create", null);

            // Set the data onto the list and save to the system.  Note that the bean will
            // bean cleared away from the session after this
            listInput.Add("beanId", (String)((Hashtable)resultOutput["bus_entity_campaign_list"])["beanId"]);
            resultOutput = this.sendRequest("bus_facade_campaign_list", addType, listInput);

            return resultOutput;

        }
        catch (Exception ex)
        {
            // if (OnError != null)
            //  {
            //  OnError(this, new Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "GetApplicant", ex, ""));
            //  }
            Hashtable errorHt = new Hashtable();
            errorHt[1] = ex.Message;
            errorHt[2] = ex.StackTrace;
            listInput.Add("Error", errorHt);
            listInput.Add("ListData", listDataSource);
            if (resultOutput == null)
            {
                return listInput;
            }
            else
            {
                resultOutput.Add("Error", errorHt);
                resultOutput.Add("ListData", listDataSource);
                return resultOutput;
            }
            
        }
    }

    /**
     * Schedule a delivery to the named list and message to run immediately
     */
    public Hashtable createDelivery(String listName, String messageName)
    {
        String deliveryDtTmStr = null;
        DateTime deliveryDtTm = DateTime.Now;
        Hashtable deliveryInput = new Hashtable();
        Hashtable resultOutput = null;
        Hashtable messageData = null;
        Hashtable listData = null;
        Hashtable listSearchInput = new Hashtable();
        Hashtable msgSearchInput = new Hashtable();
        Hashtable listIds = new Hashtable();


        // Request to create a new delivery record.  This wil return with a list of 
        // messages and lists so we can use those lists to get the ids of the 
        // list and message we want to send to
        resultOutput = this.sendRequest("bus_facade_campaign_delivery", "create", null);
        resultOutput = (Hashtable)resultOutput["bus_entity_campaign_delivery"];

        // Find the list id based on the name
        listSearchInput.Add("listName", listName);
        listData = this.searchExactMatch("bus_facade_campaign_list", listSearchInput);
        listIds.Add("0", (String)listData["listId"]);
        deliveryInput.Add("listIds", listIds);

        // Loop through the messages to find the ID that matches the name we've received
        msgSearchInput.Add("messageName", messageName);
        messageData = this.searchExactMatch("bus_facade_campaign_email", msgSearchInput);
        deliveryInput.Add("messageId", (String)messageData["messageId"]);

        // Finally, add the a time five minutes into the future as the scheduled time
        deliveryDtTm = deliveryDtTm.AddMinutes(5);
        deliveryDtTmStr = deliveryDtTm.Day + "/" +
                            deliveryDtTm.Month + "/" +
                            deliveryDtTm.Year + " " +
                            deliveryDtTm.Hour.ToString().PadLeft(2, '0') + ":" +
                            deliveryDtTm.Minute.ToString().PadLeft(2, '0');

        deliveryInput.Add("deliveryDtTm", deliveryDtTmStr);

        // Set the data onto the list and save to the system.  Note that the bean will
        // bean cleared away from the session after this
        deliveryInput.Add("beanId", (String)resultOutput["beanId"]);
        resultOutput = this.sendRequest("bus_facade_campaign_delivery", "store", deliveryInput);

        return resultOutput;
    }

	/**
	* Load an existing delivery using a reference number.  High level report data will be returned
	*/
	public Hashtable loadDelivery(String deliveryId)
	{
        Hashtable entityInput = new Hashtable();
        Hashtable resultOutput = null;

        entityInput.Add("deliveryId", deliveryId);
        
        // Use the unique id to retrieve the delivery and return the bean data
        resultOutput = this.sendRequest("bus_facade_campaign_delivery", "load", entityInput, null);
        resultOutput = (Hashtable)resultOutput["bus_entity_campaign_delivery"];

        return resultOutput;
	}

    /**
     * Create a new one-to-one delivery to a specified email address and passing
     * any custom data that should merge into the message.  Note that the message must 
     * already exist in the account.
     */
    public Hashtable createOne2One(String emailTo,
                                    String messageName,
                                    String customData,
                                    String messageType)
    {
        Hashtable entityInput = new Hashtable();
        Hashtable processInput = new Hashtable();
        Hashtable resultOutput = null;

        // Put the data for the email into a hashtable keyed on the field names taken from the 
        // data dictionary
        processInput.Add("message_messageName", messageName);
        if (messageType.ToLower() == "sms")
        {
            entityInput.Add("message_contentType", "SMS");
            entityInput.Add("toAddress", emailTo);
        }
        else
        {
            entityInput.Add("message_contentType", "EMAIL");
            entityInput.Add("toAddress", emailTo);
        }
        

        // Load the string of custom data as separate arguments into the 
        // customData parameter
        if (customData != null)
        {
            Hashtable customDataAll = new Hashtable();
            String[] customDataRows = null;
            char[] splitChars = { '\n', '\r' };

            // Split into rows and load into the input hashtable
            customDataRows = customData.Split(splitChars);

            for(int index=0; index<customDataRows.Length; index++)
            {
                String[] customDataField = null;

                // Split name value and load
                customDataField = customDataRows[index].Split('=');

                // Add the value to pass in as custom data for the message
                if (customDataField.Length == 2 && customDataField[0]!="")
                {
                    customDataAll.Add(customDataField[0], customDataField[1]);
                }
            }

            if (customDataAll.Count > 0)
            {
                entityInput.Add("customData", customDataAll);
            }
        }

        // Create a blank one2one
        resultOutput = this.sendRequest("bus_facade_campaign_one2one", "create", entityInput, processInput);
        resultOutput = (Hashtable)resultOutput["bus_entity_campaign_one2one"];
        entityInput = new Hashtable();
        entityInput.Add("beanId", (String)resultOutput["beanId"]);

        // Update with data and save
        resultOutput = this.sendRequest("bus_facade_campaign_one2one", "store", entityInput);

        return resultOutput;
    }

    /**
    * Retrieve a batch of event notifications from a PureResponse profile.  Note that the profile must be set-up 
	* to capture these events, and for click and open event notifications, the campaign email must be set-up
	* to capture these events too.
	*
	* Data is returned in a base 64 encoded cvs string so it requires decoding before it can be used.
    */
    public Hashtable retrieveEventNotifications(String notificationTypesCsv, String maxNotifications, String markAsReadInd, String customFieldNamesCsv)
    {
        Hashtable processInput = new Hashtable();
        Hashtable resultOutput = null;

        processInput.Add("notificationTypes", notificationTypesCsv);
        processInput.Add("maxNotifications", maxNotifications);
        processInput.Add("markAsReadInd", markAsReadInd);
        processInput.Add("customFieldNames", customFieldNamesCsv);

        // Use the unique id to retrieve the delivery and return the bean data
        resultOutput = this.sendRequest("bus_facade_eventNotification", "getBatch", null, processInput);
        resultOutput = (Hashtable)resultOutput["bus_entity_eventNotificationBatch"];

        return resultOutput;
    }


    /**
    * Switch between different profiles.  You'll need to use the key values from the roleList in the context
	* bean returned from logging in.  The key values are static and don't change over time.
    */
    public Hashtable switchProfile(String profileKey)
    {
        Hashtable entityInput = new Hashtable();
        Hashtable resultOutput = null;

        entityInput.Add("currentRoleKey", profileKey);

        // Send the profile key to switch the current context
        resultOutput = this.sendRequest("bus_facade_context", "switchRole", entityInput, null);
        
        return resultOutput;
    }

    /**
     * Decode a base 64 string
     */
    public String base64Decode(String toDecode)
    {
        byte[] decodedBytes = null;
        String decodedString = null;

        decodedBytes = System.Convert.FromBase64String(toDecode);
        decodedString = System.Text.ASCIIEncoding.ASCII.GetString(decodedBytes);
        
        return decodedString;
    }
}
