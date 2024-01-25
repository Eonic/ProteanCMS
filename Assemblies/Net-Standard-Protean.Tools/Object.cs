using System;
using System.Reflection;

namespace Protean.Tools
{
    public partial class TypeExtensions
    {

        public static Type Type(string typeFromCurrentAssembly)
        {

            return System.Type.GetType(typeFromCurrentAssembly, true, true);

        }

        public static Type Type(string typeFromSpecifiedAssembly, string assemblyName, string assemblyType = "")
        {

            if (string.IsNullOrEmpty(assemblyName))
            {
                // Use the current assembly
                return Type(typeFromSpecifiedAssembly);
            }
            else
            {
                if (!string.IsNullOrEmpty(assemblyType))
                {
                    typeFromSpecifiedAssembly += ", " + assemblyType;
                }
                var assemblyInstance = Assembly.Load(assemblyName);
                return assemblyInstance.GetType(typeFromSpecifiedAssembly, true);
                //assemblyInstance = null;
            }

        }


        public partial class TypeMethodParser
        {

            private string _type = "";
            private string _method = "";

            public TypeMethodParser(string typeAndMethod)
            {
                Parse(typeAndMethod);
            }

            public string TypeName
            {
                get
                {
                    return _type;
                }
            }

            public string MethodName
            {
                get
                {
                    return _method;
                }
            }

            private void Parse(string typeAndMethod)
            {


                // Searches for a fully qualified assembly name
                // as specified in http://msdn.microsoft.com/en-us/library/w3f99sx1.aspx

                string typeMethodPattern = @"(\w[\w\.\+\\]+)\.(\w+)";

                string typeName =Text.SimpleRegexFind(typeAndMethod, typeMethodPattern, 1);
                string methodName = Text.SimpleRegexFind(typeAndMethod, typeMethodPattern, 2);

                if (string.IsNullOrEmpty(typeName) | string.IsNullOrEmpty(methodName))
                {

                    throw new FormatException("The input string did not match the expected format for type and methods.");
                }

                else
                {
                    _type = typeName;
                    _method = methodName;
                }

            }

        }

    }
}
