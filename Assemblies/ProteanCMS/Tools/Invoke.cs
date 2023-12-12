using System;
using System.Reflection;

namespace Protean
{

    public class Invoke
    {

        /// <summary>
    /// Instantiates an object and invokes a method, with optional error event handler mapping.
    /// </summary>
    /// <param name="calledObjectType">The type to call against</param>
    /// <param name="calledObjectMethod">The name of method to invoke</param>
    /// <param name="calledObjectConstructorArgs">An array of args to be passed to object when instantiated.  Must match up to a valid argument set on a New constructor of that object</param>
    /// <param name="calledObjectMethodArgs">An array of args to be passed to the method when invoked. Must match up to a valid argument set on the method</param>
    /// <param name="callerObject">If passing an error handler, then the object calling the method must be passed </param>
    /// <param name="callerErrorMethodName">The name of the calling object's error handling method</param>
    /// <param name="calledObjectErrorEventName">The name of the called object's error event</param>
    /// <remarks>Use Protean.Cms.TypeExtensions methods to generate a type</remarks>
        public static void InvokeObjectMethod(Type calledObjectType, string calledObjectMethod, object[] calledObjectConstructorArgs, object[] calledObjectMethodArgs, object callerObject, string callerErrorMethodName, string calledObjectErrorEventName)







        {

            try
            {
                // Build the constructor
                var objectInstance = Activator.CreateInstance(calledObjectType, calledObjectConstructorArgs);

                // Add the error handler - if specified
                if (!string.IsNullOrEmpty(callerErrorMethodName))
                {
                    var errorEvent = calledObjectType.GetEvent(calledObjectErrorEventName);
                    var errorDelegate = errorEvent.EventHandlerType;

                    var errorHandler = callerObject.GetType().GetMethod(callerErrorMethodName, BindingFlags.NonPublic | BindingFlags.Instance);


                    if (errorHandler is not null)
                    {
                        // Create an instance of the delegate. Using the overloads
                        // of CreateDelegate that take MethodInfo is recommended.
                        // 
                        var d = Delegate.CreateDelegate(errorDelegate, callerObject, errorHandler);

                        // Get the "add" accessor of the event and invoke it late-
                        // bound, passing in the delegate instance. This is equivalent
                        // to using the += operator in C#, or AddHandler in Visual
                        // Basic. The instance on which the "add" accessor is invoked
                        // is the form; the arguments must be passed as an array.
                        // 
                        var miAddHandler = errorEvent.GetAddMethod();
                        object[] addHandlerArgs = new object[] { d };
                        miAddHandler.Invoke(objectInstance, addHandlerArgs);
                    }


                }

                // Call the method
                calledObjectType.InvokeMember(calledObjectMethod, BindingFlags.InvokeMethod, null, objectInstance, calledObjectMethodArgs);
            }

            catch (TypeLoadException ex)
            {
            }
            // Don't do anything for duff classes
            catch (Exception ex)
            {
                throw ex;
            }

        }


        /// <summary>
    ///   Instantiates an object and invokes a method based on a full method name.
    /// </summary>
    /// <param name="calledObjectAndMethod">The full name of the method to call e.g. MyClass.MyMethod</param>
    /// <param name="calledObjectConstructorArgs">An array of args to be passed to object when instantiated.  Must match up to a valid argument set on a New constructor of that object</param>
    /// <param name="calledObjectMethodArgs">An array of args to be passed to the method when invoked. Must match up to a valid argument set on the method</param>
    /// <remarks></remarks>
        public static void InvokeObjectMethod(string calledObjectAndMethod, object[] calledObjectConstructorArgs = null, object[] calledObjectMethodArgs = null)



        {
            var typeAndMethod = new Tools.TypeExtensions.TypeMethodParser(calledObjectAndMethod);
            InvokeObjectMethod(Type.GetType(typeAndMethod.TypeName), typeAndMethod.MethodName, calledObjectConstructorArgs, calledObjectMethodArgs, null, "", "");
        }

        /// <summary>
    /// Instantiates an object from the current assembly and invokes a method based on a full method name, with optional error event handler mapping.
    /// </summary>
    /// <param name="calledObjectAndMethod">The full name of the method to call e.g. MyClass.MyMethod</param>
    /// <param name="calledObjectConstructorArgs">An array of args to be passed to object when instantiated.  Must match up to a valid argument set on a New constructor of that object</param>
    /// <param name="calledObjectMethodArgs">An array of args to be passed to the method when invoked. Must match up to a valid argument set on the method</param>
    /// <param name="callerObject">If passing an error handler, then the object calling the method must be passed </param>
    /// <param name="callerErrorMethodName">The name of the calling object's error handling method</param>
    /// <param name="calledObjectErrorEventName">The name of the called object's error event</param>
    /// <remarks></remarks>
        public static void InvokeObjectMethod(string calledObjectAndMethod, object[] calledObjectConstructorArgs, object[] calledObjectMethodArgs, object callerObject, string callerErrorMethodName, string calledObjectErrorEventName)






        {

            var typeAndMethod = new Tools.TypeExtensions.TypeMethodParser(calledObjectAndMethod);
            InvokeObjectMethod(Type.GetType(typeAndMethod.TypeName), typeAndMethod.MethodName, calledObjectConstructorArgs, calledObjectMethodArgs, callerObject, callerErrorMethodName, calledObjectErrorEventName);
        }

        /// <summary>
    /// Instantiates an object from the current  and invokes a method.
    /// </summary>
    /// <param name="calledObjectType">The full name of the object to call</param>
    /// <param name="calledObjectMethod">The name of method to invoke</param>
    /// <param name="calledObjectConstructorArgs">An array of args to be passed to object when instantiated.  Must match up to a valid argument set on a New constructor of that object</param>
    /// <param name="calledObjectMethodArgs">An array of args to be passed to the method when invoked. Must match up to a valid argument set on the method</param>
    /// <remarks></remarks>
        public static void InvokeObjectMethod(Type calledObjectType, string calledObjectMethod, object[] calledObjectConstructorArgs = null, object[] calledObjectMethodArgs = null)




        {
            InvokeObjectMethod(calledObjectType, calledObjectMethod, calledObjectConstructorArgs, calledObjectMethodArgs, null, "", "");
        }




    }
}