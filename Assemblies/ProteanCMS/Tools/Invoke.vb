Imports System.Reflection
Imports System.Web.Configuration
Imports System.Configuration
Imports System

Public Class Invoke

    ''' <summary>
    ''' Instantiates an object and invokes a method, with optional error event handler mapping.
    ''' </summary>
    ''' <param name="calledObjectType">The type to call against</param>
    ''' <param name="calledObjectMethod">The name of method to invoke</param>
    ''' <param name="calledObjectConstructorArgs">An array of args to be passed to object when instantiated.  Must match up to a valid argument set on a New constructor of that object</param>
    ''' <param name="calledObjectMethodArgs">An array of args to be passed to the method when invoked. Must match up to a valid argument set on the method</param>
    ''' <param name="callerObject">If passing an error handler, then the object calling the method must be passed </param>
    ''' <param name="callerErrorMethodName">The name of the calling object's error handling method</param>
    ''' <param name="calledObjectErrorEventName">The name of the called object's error event</param>
    ''' <remarks>Use Protean.Cms.TypeExtensions methods to generate a type</remarks>
    Public Shared Sub InvokeObjectMethod( _
            ByVal calledObjectType As System.Type, _
            ByVal calledObjectMethod As String, _
            ByVal calledObjectConstructorArgs() As Object, _
            ByVal calledObjectMethodArgs() As Object, _
            ByVal callerObject As Object, _
            ByVal callerErrorMethodName As String, _
            ByVal calledObjectErrorEventName As String _
        )

        Try
            ' Build the constructor
            Dim objectInstance As Object = Activator.CreateInstance(calledObjectType, calledObjectConstructorArgs)

            ' Add the error handler - if specified
            If Not String.IsNullOrEmpty(callerErrorMethodName) Then
                Dim errorEvent As EventInfo = calledObjectType.GetEvent(calledObjectErrorEventName)
                Dim errorDelegate As Type = errorEvent.EventHandlerType

                Dim errorHandler As MethodInfo = _
                    callerObject.GetType().GetMethod(callerErrorMethodName, _
                        BindingFlags.NonPublic Or BindingFlags.Instance)

                If errorHandler IsNot Nothing Then
                    ' Create an instance of the delegate. Using the overloads
                    ' of CreateDelegate that take MethodInfo is recommended.
                    '
                    Dim d As [Delegate] = _
                        [Delegate].CreateDelegate(errorDelegate, callerObject, errorHandler)

                    ' Get the "add" accessor of the event and invoke it late-
                    ' bound, passing in the delegate instance. This is equivalent
                    ' to using the += operator in C#, or AddHandler in Visual
                    ' Basic. The instance on which the "add" accessor is invoked
                    ' is the form; the arguments must be passed as an array.
                    '
                    Dim miAddHandler As MethodInfo = errorEvent.GetAddMethod()
                    Dim addHandlerArgs() As Object = {d}
                    miAddHandler.Invoke(objectInstance, addHandlerArgs)
                End If


            End If

            ' Call the method
            calledObjectType.InvokeMember(calledObjectMethod, BindingFlags.InvokeMethod, Nothing, objectInstance, calledObjectMethodArgs)

        Catch ex As System.TypeLoadException
            ' Don't do anything for duff classes
        Catch ex As Exception
            Throw ex
        End Try

    End Sub


    ''' <summary>
    '''   Instantiates an object and invokes a method based on a full method name.
    ''' </summary>
    ''' <param name="calledObjectAndMethod">The full name of the method to call e.g. MyClass.MyMethod</param>
    ''' <param name="calledObjectConstructorArgs">An array of args to be passed to object when instantiated.  Must match up to a valid argument set on a New constructor of that object</param>
    ''' <param name="calledObjectMethodArgs">An array of args to be passed to the method when invoked. Must match up to a valid argument set on the method</param>
    ''' <remarks></remarks>
    Public Shared Sub InvokeObjectMethod( _
    ByVal calledObjectAndMethod As String, _
    Optional ByVal calledObjectConstructorArgs() As Object = Nothing, _
    Optional ByVal calledObjectMethodArgs() As Object = Nothing _
)
        Dim typeAndMethod As New Protean.Tools.TypeExtensions.TypeMethodParser(calledObjectAndMethod)
        InvokeObjectMethod(System.Type.GetType(typeAndMethod.TypeName), typeAndMethod.MethodName, calledObjectConstructorArgs, calledObjectMethodArgs, Nothing, "", "")
    End Sub

    ''' <summary>
    ''' Instantiates an object from the current assembly and invokes a method based on a full method name, with optional error event handler mapping.
    ''' </summary>
    ''' <param name="calledObjectAndMethod">The full name of the method to call e.g. MyClass.MyMethod</param>
    ''' <param name="calledObjectConstructorArgs">An array of args to be passed to object when instantiated.  Must match up to a valid argument set on a New constructor of that object</param>
    ''' <param name="calledObjectMethodArgs">An array of args to be passed to the method when invoked. Must match up to a valid argument set on the method</param>
    ''' <param name="callerObject">If passing an error handler, then the object calling the method must be passed </param>
    ''' <param name="callerErrorMethodName">The name of the calling object's error handling method</param>
    ''' <param name="calledObjectErrorEventName">The name of the called object's error event</param>
    ''' <remarks></remarks>
    Public Shared Sub InvokeObjectMethod( _
        ByVal calledObjectAndMethod As String, _
        ByVal calledObjectConstructorArgs() As Object, _
        ByVal calledObjectMethodArgs() As Object, _
        ByVal callerObject As Object, _
        ByVal callerErrorMethodName As String, _
        ByVal calledObjectErrorEventName As String _
    )

        Dim typeAndMethod As New Protean.Tools.TypeExtensions.TypeMethodParser(calledObjectAndMethod)
        InvokeObjectMethod(System.Type.GetType(typeAndMethod.TypeName), typeAndMethod.MethodName, calledObjectConstructorArgs, calledObjectMethodArgs, callerObject, callerErrorMethodName, calledObjectErrorEventName)
    End Sub

    ''' <summary>
    ''' Instantiates an object from the current  and invokes a method.
    ''' </summary>
    ''' <param name="calledObjectType">The full name of the object to call</param>
    ''' <param name="calledObjectMethod">The name of method to invoke</param>
    ''' <param name="calledObjectConstructorArgs">An array of args to be passed to object when instantiated.  Must match up to a valid argument set on a New constructor of that object</param>
    ''' <param name="calledObjectMethodArgs">An array of args to be passed to the method when invoked. Must match up to a valid argument set on the method</param>
    ''' <remarks></remarks>
    Public Shared Sub InvokeObjectMethod( _
            ByVal calledObjectType As Type, _
            ByVal calledObjectMethod As String, _
            Optional ByVal calledObjectConstructorArgs() As Object = Nothing, _
            Optional ByVal calledObjectMethodArgs() As Object = Nothing _
        )
        InvokeObjectMethod(calledObjectType, calledObjectMethod, calledObjectConstructorArgs, calledObjectMethodArgs, Nothing, "", "")
    End Sub




End Class
