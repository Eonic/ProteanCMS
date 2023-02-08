Imports System.Reflection
Imports Protean.Tools.Text

Public Class TypeExtensions

    Shared Function Type(ByVal typeFromCurrentAssembly As String) As System.Type

        Return System.Type.GetType(typeFromCurrentAssembly, True, True)

    End Function

    Shared Function Type(ByVal typeFromSpecifiedAssembly As String, ByVal assemblyName As String, Optional ByVal assemblyType As String = "") As System.Type

        If String.IsNullOrEmpty(assemblyName) Then
            ' Use the current assembly
            Return Type(typeFromSpecifiedAssembly)
        Else
            If Not String.IsNullOrEmpty(assemblyType) Then
                typeFromSpecifiedAssembly &= ", " & assemblyType
            End If
            Dim assemblyInstance As [Assembly] = [Assembly].Load(assemblyName)
            Return assemblyInstance.GetType(typeFromSpecifiedAssembly, True)
            assemblyInstance = Nothing
        End If

    End Function


    Public Class TypeMethodParser

        Private _type As String = ""
        Private _method As String = ""

        Public Sub New(ByVal typeAndMethod As String)
            Parse(typeAndMethod)
        End Sub

        Public ReadOnly Property TypeName() As String
            Get
                Return _type
            End Get
        End Property

        Public ReadOnly Property MethodName() As String
            Get
                Return _method
            End Get
        End Property

        Private Sub Parse(ByVal typeAndMethod As String)


            ' Searches for a fully qualified assembly name
            ' as specified in http://msdn.microsoft.com/en-us/library/w3f99sx1.aspx

            Dim typeMethodPattern As String = "(\w[\w\.\+\\]+)\.(\w+)"

            Dim typeName As String = SimpleRegexFind(typeAndMethod, typeMethodPattern, 1)
            Dim methodName As String = SimpleRegexFind(typeAndMethod, typeMethodPattern, 2)

            If String.IsNullOrEmpty(typeName) Or String.IsNullOrEmpty(methodName) Then

                Throw New FormatException("The input string did not match the expected format for type and methods.")

            Else
                _type = typeName
                _method = methodName
            End If

        End Sub

    End Class

End Class






