Imports Windows.UI.Notifications
Imports Windows.Data.Xml.Dom
Imports Windows.Storage
Imports System.Threading
Imports Windows.Storage.Search




Public Class clem2k

    Public Class data
        Private Shared localSettings As Windows.Storage.ApplicationDataContainer = Windows.Storage.ApplicationData.Current.LocalSettings
        Public Shared Sub insert(fichier As StorageFile, isSynced As Boolean)
            Dim dataFichier As New Windows.Storage.ApplicationDataCompositeValue
            Try
                localSettings.DeleteContainer(fichier.Name)
            Catch ex As Exception

            End Try

            dataFichier("fichier") = fichier.Name
            dataFichier("synced") = isSynced
            localSettings.Values(fichier.Name) = (dataFichier)
        End Sub
        Private Shared Async Function getAllFiles() As Tasks.Task(Of StorageFile())
            Dim fileTypeFilter As New List(Of String)()
            fileTypeFilter.Add(".jpg")
            Dim queryOptions As New QueryOptions(Windows.Storage.Search.CommonFileQuery.OrderByName, fileTypeFilter)
            queryOptions.IndexerOption = IndexerOption.OnlyUseIndexer
            Dim queryResult As StorageFileQueryResult = ApplicationData.Current.LocalFolder.CreateFileQueryWithOptions(queryOptions)
            Dim files = Await queryResult.GetFilesAsync()

            Return files
        End Function
        Public Shared Async Sub retryUnSyncedFiles()
            Dim f As StorageFile() = Await getAllFiles()
            For Each file As StorageFile In f
                Dim dataFichier As New Windows.Storage.ApplicationDataCompositeValue
                Dim isSynced As Boolean = False
                dataFichier = localSettings.Values(file.Name)
                isSynced = dataFichier("synced")
                If Not isSynced Then
                    Await clem2k.transferFile(file)
                End If
            Next
        End Sub
    End Class

    Private Shared localSettings As Windows.Storage.ApplicationDataContainer = Windows.Storage.ApplicationData.Current.LocalSettings

    Public Shared Sub notifyMe(texte As String)
        Dim xmlToast As XmlDocument = New XmlDocument
        Dim tnToast As ToastNotification = Nothing
        xmlToast = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01)
        Dim ttTextNode = xmlToast.GetElementsByTagName("text")
        ttTextNode(0).AppendChild(xmlToast.CreateTextNode(texte))
        tnToast = New ToastNotification(xmlToast)
        tnToast.ExpirationTime = DateTime.Now.AddSeconds(10)
        ToastNotificationManager.CreateToastNotifier().Show(tnToast)
    End Sub
    Public Shared Async Function getNewPhotoFile() As Tasks.Task(Of StorageFile)
        Dim titre As String = Date.Now.Year & Date.Now.Month.ToString.PadLeft(2, "0") & Date.Now.Day.ToString.PadLeft(2, "0") & "_" & Date.Now.Hour.ToString.PadLeft(2, "0") & Date.Now.Minute.ToString.PadLeft(2, "0") & Date.Now.Second.ToString.PadLeft(2, "0")
        Dim file As StorageFile = Await ApplicationData.Current.LocalFolder.CreateFileAsync(titre & ".jpg", CreationCollisionOption.GenerateUniqueName)
        Return file
    End Function
    Public Shared Async Function transferFile(fichier As StorageFile) As Tasks.Task
        Dim url As String = localSettings.Values("server").ToString & "/remote.php/webdav/" & AppResources.uploadFolder & "/"
        If localSettings.Values("folderOK") Is Nothing Then
            Await createFolder()
        Else
            If CBool(localSettings.Values("folderOK")) Then
                Await createFolder()
            End If
        End If

        Dim userName As String = localSettings.Values("user").ToString
        Dim password As String = localSettings.Values("password").ToString
        Dim cred As NetworkCredential = New NetworkCredential(userName, password)
        Dim httpReq As HttpWebRequest = WebRequest.Create(url & fichier.Name)
        Dim data As Byte() = getFileBytes(fichier.Path)
        Dim s As System.IO.Stream = Nothing

        httpReq.Credentials = cred
        httpReq.Method = "PUT"
        httpReq.ContentLength = data.Length
        s = Await httpReq.GetRequestStreamAsync
        s.Write(data, 0, data.Length)
        s.Close()

        Try
            Dim httpRep As HttpWebResponse = Await httpReq.GetResponseAsync
            clem2k.data.insert(fichier, True)
            notifyMe(AppResources.PictureTransfered)
        Catch ex As Exception
            notifyMe(AppResources.serverUnavaible)
            clem2k.data.insert(fichier, False)
        End Try



    End Function

    Public Shared Async Function createFolder() As Tasks.Task
        Dim url As String = localSettings.Values("server").ToString & "/remote.php/webdav/" & AppResources.uploadFolder & "/"
        Dim userName As String = localSettings.Values("user").ToString
        Dim password As String = localSettings.Values("password").ToString
        Dim cred As NetworkCredential = New NetworkCredential(userName, password)
        Dim httpReq As HttpWebRequest = WebRequest.Create(url)

        Dim s As System.IO.Stream = Nothing

        httpReq.Credentials = cred
        httpReq.Method = "MKCOL"

        Try
            Dim httpRep As HttpWebResponse = Await httpReq.GetResponseAsync
            localSettings.Values("folderOK") = True
        Catch ex As Exception
            localSettings.Values("folderOK") = False
        End Try



    End Function

    Private Shared Function getFileBytes(fichier As String) As Byte()
        Dim s As System.IO.Stream = New System.IO.FileStream(fichier, IO.FileMode.Open)
        Dim br As System.IO.BinaryReader = New System.IO.BinaryReader(s)
        Dim result As Byte() = br.ReadBytes(CInt(s.Length))
        s.Close()
        Return result
    End Function

End Class
