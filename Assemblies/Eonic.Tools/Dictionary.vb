Public Module Dictionary

    Enum Dimension
        Key = 0
        Value = 1
    End Enum

    ''' <summary>
    '''     Creates a hashtable based on a colon and comma separated list of key/value string pairs.
    ''' </summary>
    ''' <param name="cCSVString">Format is "key1:value1,key2:value2,key3:value3 etc.</param>
    ''' <returns>Hashtable</returns>
    ''' <remarks></remarks>
    Public Function getSimpleHashTable(ByVal cCSVString As String) As Hashtable

        Try
            Dim hTable As New Hashtable
            Dim cArray As String() = cCSVString.Trim(",").Split(",")
            Dim cItemSplit As String()

            For Each cItem As String In cArray
                cItemSplit = cItem.Split(":")
                If cItemSplit.Length = 2 Then
                    hTable.Add(cItemSplit(0), cItemSplit(1))
                End If
            Next

            Return hTable

        Catch ex As Exception
            Return New Hashtable()
        End Try

    End Function

    ''' <summary>
    '''    Creates a CSV from a hashtable.
    '''    If the key/value cannot be converted to a string, then this will fail.
    '''    If the key/value contains the separator then it will be removed.
    '''    
    ''' </summary>
    ''' <param name="hashTableToConvert">The hashtable to convert</param>
    ''' <param name="dimension">Determines whether to use the keys or values in the list </param>
    ''' <param name="separator">The separator character</param>
    ''' <returns>A CSV list of the hastable key or values</returns>
    ''' <remarks></remarks>
    Public Function hashtableToCSV(ByRef hashTableToConvert As Hashtable, Optional ByVal dimension As Dimension = Dimension.Value, Optional ByVal separator As Char = ",") As String

        Dim csvList As String = ""
        Try

            For Each Item As DictionaryEntry In hashTableToConvert

                If Not (String.IsNullOrEmpty(csvList)) Then csvList &= separator
                Select Case dimension
                    Case Dictionary.Dimension.Key
                        csvList &= Replace(Item.Key, separator, "")
                    Case Dictionary.Dimension.Value
                        csvList &= Replace(Item.Value, separator, "")
                End Select

            Next

            Return csvList

        Catch ex As Exception
            Return ""
        End Try
    End Function

End Module
