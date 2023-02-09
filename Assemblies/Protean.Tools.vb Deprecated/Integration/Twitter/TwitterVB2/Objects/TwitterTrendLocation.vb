Namespace Integration.Twitter.TwitterVB2

    Public Class TwitterTrendLocation
        Private _ID As String = String.Empty
        Private _LocationName As String = String.Empty
        Private _PlaceTypeCode As String = String.Empty
        Private _PlaceTypeName As String = String.Empty
        Private _CountryName As String = String.Empty
        Private _CountryCode As String = String.Empty
        Private _URL As String = String.Empty


        Public Property ID() As String
            Get
                Return _ID
            End Get
            Set(ByVal value As String)
                _ID = value
            End Set
        End Property


        Public Property LocationName() As String
            Get
                Return _LocationName
            End Get
            Set(ByVal value As String)
                _LocationName = value
            End Set
        End Property


        Public Property PlaceTypeCode() As String
            Get
                Return _PlaceTypeCode
            End Get
            Set(ByVal value As String)
                _PlaceTypeCode = value
            End Set
        End Property


        Public Property PlaceTypeName() As String
            Get
                Return _PlaceTypeName
            End Get
            Set(ByVal value As String)
                _PlaceTypeName = value
            End Set
        End Property


        Public Property CountryName() As String
            Get
                Return _CountryName
            End Get
            Set(ByVal value As String)
                _CountryName = value
            End Set
        End Property


        Public Property CountryCode() As String
            Get
                Return _CountryCode
            End Get
            Set(ByVal value As String)
                _CountryCode = value
            End Set
        End Property


        Public Property URL() As String
            Get
                Return _URL
            End Get
            Set(ByVal value As String)
                _URL = value
            End Set
        End Property


        Public Sub New(ByVal SearchResultNode As System.Xml.XmlNode)
            Me._ID = SearchResultNode("woeid").InnerText

            If SearchResultNode("name") IsNot Nothing Then
                Me._LocationName = SearchResultNode("name").InnerText
            End If

            If SearchResultNode("placeTypeName") IsNot Nothing Then
                Me._PlaceTypeName = SearchResultNode("placeTypeName").InnerText
            End If

            Dim PTNList As System.Xml.XmlNodeList = SearchResultNode.SelectNodes("placeTypeName")

            If PTNList(0).Attributes("code") IsNot Nothing Then
                Me._PlaceTypeCode = PTNList(0).Attributes("code").Value
            End If

            If SearchResultNode("country") IsNot Nothing Then
                Me._CountryName = SearchResultNode("country").InnerText
            End If

            Dim CountryList As System.Xml.XmlNodeList = SearchResultNode.SelectNodes("country")

            If CountryList(0).Attributes("code") IsNot Nothing Then
                Me._CountryCode = CountryList(0).Attributes("code").Value
            End If

            If SearchResultNode("url") IsNot Nothing Then
                Me._URL = SearchResultNode("url").InnerText
            End If
        End Sub
    End Class
End Namespace
