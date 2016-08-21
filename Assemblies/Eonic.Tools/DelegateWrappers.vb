' Namespace Eonic.Tools.DelegateWrappers

' Collection of classes for delegate wrappers - predicates, actions and converters etc.
' At the moment only predicates are implemented


Namespace DelegateWrappers

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <typeparam name="A"></typeparam>
    ''' <param name="item"></param>
    ''' <param name="argument"></param>
    ''' <returns></returns>
    ''' <remarks>From http://www.paulstovell.com/vb-anonymous-methods</remarks>
    Public Delegate Function PredicateWrapperDelegate(Of T, A) _
 (ByVal item As T, ByVal argument As A) As Boolean

    Public Class PredicateWrapper(Of T, A)

        Private _argument As A
        Private _wrapperDelegate As PredicateWrapperDelegate(Of T, A)

        Public Sub New(ByVal argument As A, _
         ByVal wrapperDelegate As PredicateWrapperDelegate(Of T, A))

            _argument = argument
            _wrapperDelegate = wrapperDelegate
        End Sub

        Private Function InnerPredicate(ByVal item As T) As Boolean
            Return _wrapperDelegate(item, _argument)
        End Function

        Public Shared Widening Operator CType( _
         ByVal wrapper As PredicateWrapper(Of T, A)) _
         As Predicate(Of T)

            Return New Predicate(Of T)(AddressOf wrapper.InnerPredicate)
        End Operator

    End Class

    ' My attempt at modifying the above for COnverters - fails as I think the converter delegate needs to be explicitly typed.
    'Public Delegate Function ConverterWrapperDelegate(Of InputType, OutputType, A) _
    '        (ByVal item As InputType, ByVal argument As A) As OutputType

    'Public Class ConverterWrapper(Of InputType, OutputType, A)

    '    Private _argument As A
    '    Private _wrapperDelegate As ConverterWrapperDelegate(Of InputType, OutputType, A)

    '    Public Sub New(ByVal argument As A, _
    '     ByVal wrapperDelegate As ConverterWrapperDelegate(Of InputType, OutputType, A))
    '        _argument = argument
    '        _wrapperDelegate = wrapperDelegate
    '    End Sub

    '    Private Function InnerPredicate(ByVal item As InputType) As OutputType
    '        Return _wrapperDelegate(item, _argument)
    '    End Function

    '    Public Shared Widening Operator CType( _
    '     ByVal wrapper As ConverterWrapper(Of InputType, OutputType, A)) _
    '     As Converter(Of InputType, OutputType)

    '        Return New Converter(Of InputType, OutputType)(AddressOf wrapper.InnerPredicate)
    '    End Operator

    'End Class

End Namespace

