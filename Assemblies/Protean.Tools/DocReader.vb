Imports System
Imports System.Diagnostics
Imports System.Runtime.InteropServices
Imports System.Text

Namespace IFilter
    <Flags()> _
    Public Enum IFILTER_INIT As UInteger
        NONE = 0
        CANON_PARAGRAPHS = 1
        HARD_LINE_BREAKS = 2
        CANON_HYPHENS = 4
        CANON_SPACES = 8
        APPLY_INDEX_ATTRIBUTES = 16
        APPLY_CRAWL_ATTRIBUTES = 256
        APPLY_OTHER_ATTRIBUTES = 32
        INDEXING_ONLY = 64
        SEARCH_LINKS = 128
        FILTER_OWNED_VALUE_OK = 512
    End Enum

    Public Enum CHUNK_BREAKTYPE
        CHUNK_NO_BREAK = 0
        CHUNK_EOW = 1
        CHUNK_EOS = 2
        CHUNK_EOP = 3
        CHUNK_EOC = 4
    End Enum

    <Flags()> _
    Public Enum CHUNKSTATE
        CHUNK_TEXT = &H1
        CHUNK_VALUE = &H2
        CHUNK_FILTER_OWNED_VALUE = &H4
    End Enum

    <StructLayout(LayoutKind.Sequential)> _
    Public Structure PROPSPEC
        Public ulKind As UInteger
        Public propid As UInteger
        Public lpwstr As IntPtr
    End Structure

    <StructLayout(LayoutKind.Sequential)> _
    Public Structure FULLPROPSPEC
        Public guidPropSet As Guid
        Public psProperty As PROPSPEC
    End Structure

    <StructLayout(LayoutKind.Sequential)> _
    Public Structure STAT_CHUNK
        Public idChunk As UInteger
        <MarshalAs(UnmanagedType.U4)> _
        Public breakType As CHUNK_BREAKTYPE
        <MarshalAs(UnmanagedType.U4)> _
        Public flags As CHUNKSTATE
        Public locale As UInteger
        <MarshalAs(UnmanagedType.Struct)> _
        Public attribute As FULLPROPSPEC
        Public idChunkSource As UInteger
        Public cwcStartSource As UInteger
        Public cwcLenSource As UInteger
    End Structure

    <StructLayout(LayoutKind.Sequential)> _
    Public Structure FILTERREGION
        Public idChunk As UInteger
        Public cwcStart As UInteger
        Public cwcExtent As UInteger
    End Structure

    <ComImport()> _
    <Guid("89BCB740-6119-101A-BCB7-00DD010655AF")> _
    <InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IFilter
        <PreserveSig()> _
        Function Init(<MarshalAs(UnmanagedType.U4)> grfFlags As IFILTER_INIT, cAttributes As UInteger, <MarshalAs(UnmanagedType.LPArray, SizeParamIndex:=1)> aAttributes As FULLPROPSPEC(), ByRef pdwFlags As UInteger) As Integer

        <PreserveSig()> _
        Function GetChunk(pStat As STAT_CHUNK) As Integer

        <PreserveSig()> _
        Function GetText(ByRef pcwcBuffer As UInteger, <MarshalAs(UnmanagedType.LPWStr)> buffer As StringBuilder) As Integer

        Sub GetValue(ByRef ppPropValue As UIntPtr)
        Sub BindRegion(<MarshalAs(UnmanagedType.Struct)> origPos As FILTERREGION, ByRef riid As Guid, ByRef ppunk As UIntPtr)
    End Interface

    <ComImport()> _
    <Guid("f07f3920-7b8c-11cf-9be8-00aa004b9986")> _
    Public Class CFilter
    End Class

    Public Class IFilterConstants
        Public Const PID_STG_DIRECTORY As UInteger = &H2
        Public Const PID_STG_CLASSID As UInteger = &H3
        Public Const PID_STG_STORAGETYPE As UInteger = &H4
        Public Const PID_STG_VOLUME_ID As UInteger = &H5
        Public Const PID_STG_PARENT_WORKID As UInteger = &H6
        Public Const PID_STG_SECONDARYSTORE As UInteger = &H7
        Public Const PID_STG_FILEINDEX As UInteger = &H8
        Public Const PID_STG_LASTCHANGEUSN As UInteger = &H9
        Public Const PID_STG_NAME As UInteger = &HA
        Public Const PID_STG_PATH As UInteger = &HB
        Public Const PID_STG_SIZE As UInteger = &HC
        Public Const PID_STG_ATTRIBUTES As UInteger = &HD
        Public Const PID_STG_WRITETIME As UInteger = &HE
        Public Const PID_STG_CREATETIME As UInteger = &HF
        Public Const PID_STG_ACCESSTIME As UInteger = &H10
        Public Const PID_STG_CHANGETIME As UInteger = &H11
        Public Const PID_STG_CONTENTS As UInteger = &H13
        Public Const PID_STG_SHORTNAME As UInteger = &H14
        Public Const FILTER_E_END_OF_CHUNKS As Int64 = CType(&H80041700UI, Int64)
        Public Const FILTER_E_NO_MORE_TEXT As Int64 = CType(&H80041701UI, Int64)
        Public Const FILTER_E_NO_MORE_VALUES As Int64 = CType(&H80041702UI, Int64)
        Public Const FILTER_E_NO_TEXT As Int64 = CType(&H80041705UI, Int64)
        Public Const FILTER_E_NO_VALUES As Int64 = CType(&H80041706UI, Int64)
        Public Const FILTER_S_LAST_TEXT As Integer = (CInt(&H41709))
    End Class

    ''' 
    ''' IFilter return codes
    ''' 
    Public Enum IFilterReturnCodes As UInteger
        ''' 
        ''' Success
        ''' 
        S_OK = 0
        ''' 
        ''' The function was denied access to the filter file. 
        ''' 
        E_ACCESSDENIED = &H80070005UI
        ''' 
        ''' The function encountered an invalid handle, probably due to a low-memory situation. 
        ''' 
        E_HANDLE = &H80070006UI
        ''' 
        ''' The function received an invalid parameter.
        ''' 
        E_INVALIDARG = &H80070057UI
        ''' 
        ''' Out of memory
        ''' 
        E_OUTOFMEMORY = &H8007000EUI
        ''' 
        ''' Not implemented
        ''' 
        E_NOTIMPL = &H80004001UI
        ''' 
        ''' Unknown error
        ''' 
        E_FAIL = &H80000008UI
        ''' 
        ''' File not filtered due to password protection
        ''' 
        FILTER_E_PASSWORD = &H8004170BUI
        ''' 
        ''' The document format is not recognised by the filter
        ''' 
        FILTER_E_UNKNOWNFORMAT = &H8004170CUI
        ''' 
        ''' No text in current chunk
        ''' 
        FILTER_E_NO_TEXT = &H80041705UI
        ''' 
        ''' No more chunks of text available in object
        ''' 
        FILTER_E_END_OF_CHUNKS = &H80041700UI
        ''' 
        ''' No more text available in chunk
        ''' 
        FILTER_E_NO_MORE_TEXT = &H80041701UI
        ''' 
        ''' No more property values available in chunk
        ''' 
        FILTER_E_NO_MORE_VALUES = &H80041702UI
        ''' 
        ''' Unable to access object
        ''' 
        FILTER_E_ACCESS = &H80041703UI
        ''' 
        ''' Moniker doesn't cover entire region
        ''' 
        FILTER_W_MONIKER_CLIPPED = &H41704
        ''' 
        ''' Unable to bind IFilter for embedded object
        ''' 
        FILTER_E_EMBEDDING_UNAVAILABLE = &H80041707UI
        ''' 
        ''' Unable to bind IFilter for linked object
        ''' 
        FILTER_E_LINK_UNAVAILABLE = &H80041708UI
        ''' 
        ''' This is the last text in the current chunk
        ''' 
        FILTER_S_LAST_TEXT = &H41709
        ''' 
        ''' This is the last value in the current chunk
        ''' 
        FILTER_S_LAST_VALUES = &H4170A
    End Enum

    ''' 
    ''' Convenience class which provides static methods to extract text from files using installed IFilters
    ''' 
    Public Class DefaultParser
        Public Sub New()
        End Sub

        <DllImport("query.dll", CharSet:=CharSet.Unicode)> _
        Private Shared Function LoadIFilter(pwcsPath As String, <MarshalAs(UnmanagedType.IUnknown)> pUnkOuter As Object, ByRef ppIUnk As IFilter) As Integer
        End Function

        Private Shared Function loadIFilter(filename As String) As IFilter
            Dim outer As Object = Nothing
            Dim filter As IFilter = Nothing

            ' Try to load the corresponding IFilter
            Dim resultLoad As Integer = loadIFilter(filename, outer, filter)
            If resultLoad <> CInt(IFilterReturnCodes.S_OK) Then
                Return Nothing
            End If
            Return filter
        End Function

        Public Shared Function IsParseable(filename As String) As Boolean
            Return LoadIFilter(filename) IsNot Nothing
        End Function

        Public Shared Function Extract(path As String) As String
            Dim sb As New StringBuilder()
            Dim filter As IFilter = Nothing

            Try
                filter = LoadIFilter(path)

                If filter Is Nothing Then
                    Return [String].Empty
                End If

                Dim i As UInteger = 0
                Dim ps As New STAT_CHUNK()

                Dim iflags As IFILTER_INIT = IFILTER_INIT.CANON_HYPHENS Or IFILTER_INIT.CANON_PARAGRAPHS Or IFILTER_INIT.CANON_SPACES Or IFILTER_INIT.APPLY_CRAWL_ATTRIBUTES Or IFILTER_INIT.APPLY_INDEX_ATTRIBUTES Or IFILTER_INIT.APPLY_OTHER_ATTRIBUTES Or IFILTER_INIT.HARD_LINE_BREAKS Or IFILTER_INIT.SEARCH_LINKS Or IFILTER_INIT.FILTER_OWNED_VALUE_OK

                If filter.Init(iflags, 0, Nothing, i) <> CInt(IFilterReturnCodes.S_OK) Then
                    Throw New Exception("Problem initializing an IFilter for:" & vbLf + path + " " & vbLf & vbLf)
                End If

                While filter.GetChunk(ps) = CInt(IFilterReturnCodes.S_OK)
                    If ps.flags = CHUNKSTATE.CHUNK_TEXT Then
                        Dim scode As IFilterReturnCodes = 0
                        While scode = IFilterReturnCodes.S_OK OrElse scode = IFilterReturnCodes.FILTER_S_LAST_TEXT
                            Dim pcwcBuffer As UInteger = 65536
                            Dim sbBuffer As New System.Text.StringBuilder(CInt(pcwcBuffer))

                            'ts changed directCast to Ctype when adding from C#
                            scode = CType(filter.GetText(pcwcBuffer, sbBuffer), IFilterReturnCodes)

                            If pcwcBuffer > 0 AndAlso sbBuffer.Length > 0 Then
                                If sbBuffer.Length < pcwcBuffer Then
                                    ' Should never happen, but it happens !
                                    pcwcBuffer = CUInt(sbBuffer.Length)
                                End If

                                sb.Append(sbBuffer.ToString(0, CInt(pcwcBuffer)))
                                ' "\r\n"
                                sb.Append(" ")

                            End If
                        End While

                    End If
                End While
            Finally
                If filter IsNot Nothing Then
                    Marshal.ReleaseComObject(filter)
                    System.GC.Collect()
                    System.GC.WaitForPendingFinalizers()
                End If
            End Try

            Return sb.ToString()
        End Function
    End Class
End Namespace