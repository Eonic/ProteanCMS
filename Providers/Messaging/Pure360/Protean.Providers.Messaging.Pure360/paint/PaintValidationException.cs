using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;

/// <summary>
/// Exception thrown when PAINT encounters validation errors during an update or store.
/// Errors are stored in a hashtable keyed by a unique reference for the field
/// or particular validation error.
/// </summary>
public class PaintValidationException : ApplicationException
{
    /** Hashtable of errors keyed on the error field/name **/
    protected Hashtable errors;

    /**
     * Construct the exception with a hashtable of errors
     */
    public PaintValidationException(Hashtable errors) : base("Validation error")
	{
        this.errors = errors;
	}

    /**
     * Return the hash table or errors 
     */
    public Hashtable getErrors()
    {
        return this.errors;
    }
}
