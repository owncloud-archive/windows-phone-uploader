Imports Windows.Storage.Search
Imports Windows.Storage
Imports System.Windows.Media.Imaging
Imports Windows.Storage.Pickers
Imports Windows.Storage.FileProperties

Partial Public Class picList
    Inherits PhoneApplicationPage

    Private localSettings As Windows.Storage.ApplicationDataContainer = Windows.Storage.ApplicationData.Current.LocalSettings
    Dim listFiles() As StorageFile

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Async Sub picList_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded

        txtTitre.Text = AppResources.ApplicationTitle
        btnEnvoyer.Content = AppResources.btnSend
        Await checkTodo()


    End Sub

    Private Async Function checkTodo() As System.Threading.Tasks.Task
        Dim cpt As Int16 = 0
        Dim files = Await ApplicationData.Current.LocalFolder.GetFilesAsync()
        txt.Text = ""
        For i As Integer = 0 To files.Count - 1
            Dim dataFichier As New Windows.Storage.ApplicationDataCompositeValue
            Dim isSynced As Boolean

            Try
                dataFichier = localSettings.Values(files(i).Name)
                isSynced = dataFichier("synced")
            Catch ex As Exception
                isSynced = False
            End Try

            If Not isSynced And files(i).Name.Split(".")(1).ToLower = ".jpg" Then
                cpt += 1
                txt.Text &= files(i).Name & vbCrLf
                ReDim Preserve listFiles(cpt)
                listFiles(cpt) = files(i)
            End If
        Next

        If Not listFiles Is Nothing Then
            txt.Text &= cpt & " " & AppResources.todo & vbCrLf
            btnEnvoyer.Visibility = System.Windows.Visibility.Visible
        Else
            txt.Text = AppResources.picListOK
            btnEnvoyer.Visibility = System.Windows.Visibility.Collapsed
        End If
    End Function

    Private Async Sub btnEnvoyer_Click(sender As Object, e As RoutedEventArgs) Handles btnEnvoyer.Click
        txt.Text = AppResources.picListBeginTrans & vbCrLf
        For i As Int16 = 1 To listFiles.Count - 1
            txt.Text &= AppResources.picListFileTrans & " " & listFiles(i).Name & vbCrLf
            Await clem2k.transferFile(listFiles(i))
        Next
        txt.Text &= AppResources.picListEndTrans & vbCrLf
        Await checkTodo()
    End Sub

End Class
