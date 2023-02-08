using System;

namespace Protean.Tools
{
    public class EnumUtility
    {


        // .NET 4 Upgrade would provide this function natively.
        // Try not to use too much as it's a bit ineffective.
        public static bool TryParse(Type enumType, string value, bool ignoreCase, ref object output)
        {
            try
            {
                output = Enum.Parse(enumType, value, ignoreCase);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}