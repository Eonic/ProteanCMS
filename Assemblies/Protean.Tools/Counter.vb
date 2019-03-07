' ================================================================================================
'   Protean.Tools.Counter & Protean.Tools.CounterCollection
'   Desc:       A simple counter class as well as a CounterCollection
'   Author:     Ali Granger
'   Updated:    03-Feb-10
'   Docs:       
'
'   
'
' ================================================================================================
Imports System
Imports System.Collections


Public Class Counter

#Region "Declarations"

    Private _name As String
    Private _count As Double
    Private _base As Double
    Private _increment As Double

#End Region

#Region "Enums and Constants"

    Public Const DEFAULT_INCREMENT As Double = 1
    Public Const DEFAULT_BASE As Double = 0

#End Region

#Region "Events"

    Public Event OnChange(ByVal sender As Counter, ByVal e As EventArgs)

    Public Shadows Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)

    Private Sub _OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)
        RaiseEvent OnError(sender, e)
    End Sub

#End Region

#Region "Constructor"

    Public Sub New()
        Me.New(Tools.Text.RandomPassword(16, , TextOptions.LowerCase Or TextOptions.UseAlpha Or TextOptions.UseNumeric), DEFAULT_INCREMENT, DEFAULT_BASE)
    End Sub

    Public Sub New(ByVal name As String)
        Me.New(name, DEFAULT_INCREMENT, DEFAULT_BASE)
    End Sub

    Public Sub New(ByVal name As String, ByVal incrementInterval As Integer)
        Me.New(name, Convert.ToDouble(incrementInterval), DEFAULT_BASE)
    End Sub

    Public Sub New(ByVal name As String, ByVal incrementInterval As Double)
        Me.New(name, incrementInterval, DEFAULT_BASE)

    End Sub

    Public Sub New(ByVal name As String, ByVal incrementInterval As Double, ByVal baseCounter As Double)
        _name = name
        Increment = incrementInterval
        _base = baseCounter
        Reset()
    End Sub


#End Region

#Region "Private Properties"


#End Region

#Region "Public Properties"
    Public ReadOnly Property Name() As String
        Get
            Return _name
        End Get
    End Property

    Public ReadOnly Property Count() As Double
        Get
            Return _count
        End Get
    End Property


    Public ReadOnly Property Base() As Double
        Get
            Return _base
        End Get
    End Property


    Public Property Increment() As Double
        Get
            Return _increment
        End Get
        Set(ByVal value As Double)
            _increment = value
        End Set
    End Property

#End Region

#Region "Private Members"


#End Region

#Region "Public Members"

    Public Sub Reset()
        Reset(DEFAULT_BASE)
    End Sub

    Public Sub Reset(ByVal baseCounter As Integer)
        Convert.ToDouble(baseCounter)
    End Sub

    Public Sub Reset(ByVal baseCounter As Double)
        _count = baseCounter
        RaiseEvent OnChange(Me, Nothing)
    End Sub

    Public Sub Add()
        Add(Increment)
    End Sub

    Public Sub Add(ByVal incremementInterval As Integer)
        Add(Convert.ToDouble(incremementInterval))
    End Sub

    Public Sub Add(ByVal incremementInterval As Double)
        _count += incremementInterval
        RaiseEvent OnChange(Me, Nothing)
    End Sub

    Public Function ToInt() As Integer

        Return Convert.ToInt32(_count)

    End Function

#End Region

#Region "Shared Members"


#End Region

End Class ' Counter

Public Class CounterCollection
    Inherits CollectionBase

#Region "Inherited Overrides"

    Default Public Property Item(ByVal index As Integer) As Counter
        Get
            Return CType(List(index), Counter)
        End Get
        Set(ByVal value As Counter)
            List(index) = value
        End Set
    End Property

    Default Public Property Item(ByVal counterName As String) As Counter
        Get
            Return CType(List(IndexOf(counterName)), Counter)
        End Get
        Set(ByVal value As Counter)
            List(IndexOf(counterName)) = value
        End Set
    End Property

    ''' <summary>
    ''' Creates a counter by counter name and adds it to the collection
    ''' </summary>
    ''' <param name="counterName">The name of the counter</param>
    ''' <returns>Returns the actual Counter object</returns>
    ''' <remarks></remarks>
    Public Function Add(ByVal counterName As String) As Counter
        If Not Exists(counterName) Then
            Return Item(List.Add(New Counter(counterName)))
        Else
            Throw New Exception("Counter name already exists")
            Return Nothing
        End If
    End Function 'Add

    ''' <summary>
    ''' Adds a counter by object
    ''' </summary>
    ''' <param name="value">The counter object to add</param>
    ''' <returns>The index of counter in the collection</returns>
    ''' <remarks></remarks>
    Public Function Add(ByVal value As Counter) As Integer
        Return List.Add(value)
    End Function 'Add


    Public Function IndexOf(ByVal counterName As String) As Integer
        Dim index As Integer = -1
        For Each counterItem As Counter In List
            If counterItem.Name = counterName Then
                index = IndexOf(counterItem)
            End If
        Next
        Return index
    End Function 'IndexOf

    Public Function IndexOf(ByVal value As Counter) As Integer
        Return List.IndexOf(value)
    End Function 'IndexOf


    Public Sub Insert(ByVal index As Integer, ByVal value As Counter)
        List.Insert(index, value)
    End Sub 'Insert


    Public Sub Remove(ByVal value As Counter)
        List.Remove(value)
    End Sub 'Remove


    Public Function Contains(ByVal value As Counter) As Boolean
        ' If value is not of type Counter, this will return false.
        Return List.Contains(value)
    End Function 'Contains


    Protected Overrides Sub OnInsert(ByVal index As Integer, ByVal value As [Object])
        ' Insert additional code to be run only when inserting values.
    End Sub 'OnInsert


    Protected Overrides Sub OnRemove(ByVal index As Integer, ByVal value As [Object])
        ' Insert additional code to be run only when removing values.
    End Sub 'OnRemove


    Protected Overrides Sub OnSet(ByVal index As Integer, ByVal oldValue As [Object], ByVal newValue As [Object])
        ' Insert additional code to be run only when setting values.
    End Sub 'OnSet


    Protected Overrides Sub OnValidate(ByVal value As [Object])
        If Not value.GetType() Is Type.GetType("Protean.Tools.Counter") Then
            Throw New ArgumentException("value must be of type Protean.Tools.Counter.", "value")
        End If
    End Sub 'OnValidate 

#End Region

#Region "Public Non-inherited Members"

    Public Function Exists(ByVal value As String) As Boolean
        Dim isFound As Boolean = False
        For Each counterItem As Counter In List
            If counterItem.Name = value Then
                isFound = True
                Exit For
            End If
        Next
        Return isFound
    End Function 'Exists

    Public Sub ResetAll()
        For Each counterItem As Counter In List
            counterItem.Reset()
        Next
    End Sub

#End Region

End Class 'CounterCollection
