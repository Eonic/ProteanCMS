Imports System.ComponentModel
Imports System.Configuration.Install

Public Class Installer

    Public Sub New()
        MyBase.New()

        'This call is required by the Component Designer.
        InitializeComponent()

        'Add initialization code after the call to InitializeComponent



    End Sub

    Public Overrides Sub Commit(ByVal savedState As System.Collections.IDictionary)

        MyBase.Commit(savedState)
        System.Diagnostics.Process.Start("http://www.eonicweb.com")

    End Sub

End Class
