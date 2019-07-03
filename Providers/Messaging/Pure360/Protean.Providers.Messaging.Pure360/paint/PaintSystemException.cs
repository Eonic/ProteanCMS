using System;
using System.Data;
using System.Configuration;
using System.Web;

/// <summary>
/// Exception thrown by PAINT when the PureResponse application terminates
/// unexpectedly.
/// </summary>
public class PaintSystemException : ApplicationException
{
    /**
     * Construct the exception with a basic error message
     */
	public PaintSystemException(String message) : base(message)
	{
		
	}
}
