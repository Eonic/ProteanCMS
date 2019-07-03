using System;
using System.Data;
using System.Configuration;
using System.Web;
/// <summary>
/// Exception thrown by PAINT when your code attempts to access an action
/// for which it has insufficient privileges
/// </summary>
public class PaintSecurityException : ApplicationException
{
    public PaintSecurityException(String message)
        : base(message)
	{
		
	}
}
