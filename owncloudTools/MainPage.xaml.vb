Imports System
Imports System.Threading
Imports System.Windows.Controls
Imports Microsoft.Phone.Controls
Imports Microsoft.Phone.Shell
Imports Windows.Media.MediaProperties
Imports Windows.Storage
Imports System.Windows.Media.Imaging

Partial Public Class MainPage
    Inherits PhoneApplicationPage
    Dim localSettings As Windows.Storage.ApplicationDataContainer = Windows.Storage.ApplicationData.Current.LocalSettings

    ' Constructeur
    Public Sub New()
        InitializeComponent()

        SupportedOrientations = SupportedPageOrientation.Portrait Or SupportedPageOrientation.Landscape

        ' Exemple de code pour la localisation d'ApplicationBar
        'BuildLocalizedApplicationBar()

    End Sub
    ' Exemple de code pour la conception d'une ApplicationBar localisée
    'Private Sub BuildLocalizedApplicationBar()
    '    ' Définit l'ApplicationBar de la page sur une nouvelle instance d'ApplicationBar.
    '    ApplicationBar = New ApplicationBar()

    '    ' Crée un bouton et définit la valeur du texte sur la chaîne localisée issue d'AppResources.
    '    Dim appBarButton As New ApplicationBarIconButton(New Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative))
    '    appBarButton.Text = AppResources.AppBarButtonText
    '    ApplicationBar.Buttons.Add(appBarButton)

    '    ' Crée un nouvel élément de menu avec la chaîne localisée d'AppResources.
    '    Dim appBarMenuItem As New ApplicationBarMenuItem(AppResources.AppBarMenuItemText)
    '    ApplicationBar.MenuItems.Add(appBarMenuItem)
    'End Sub
    Private Sub btnOK_Click(sender As Object, e As RoutedEventArgs) Handles btnOK.Click
        'save and test server data

        saveSettings(oclServer.Text, oclUser.Text, oclPassword.Password)
        gotoPic()


    End Sub
    Private Sub MainPage_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        setText()
        'if server data are present then go to picture page
        Dim ok As Boolean = readSettings()

        If ok Then
            gotoPic()
        End If

    End Sub
    Private Sub setText()
        btnOK.Content = AppResources.mpBtnConnect
        txtParam.Text = AppResources.mpTitle
        txtServer.Text = AppResources.mpServer
        txtUser.Text = AppResources.mpUser
        txtPassword.Text = AppResources.mpPassword
    End Sub
    Private Sub gotoPic()
        Dim u As System.Uri = Nothing
        u = New System.Uri("/pic.xaml", UriKind.Relative)
        NavigationService.Navigate(u)

    End Sub
    Private Function readSettings() As Boolean
        Dim result As Boolean = True

        If localSettings.Values("user") Is Nothing Then
            result = False
        Else
            If localSettings.Values("user").ToString = "" Then
                result = False
            Else
                result = True
            End If
        End If

        Return result
    End Function
    Private Sub saveSettings(server As String, user As String, password As String)
        localSettings.Values("server") = server
        localSettings.Values("user") = user
        localSettings.Values("password") = password
    End Sub


End Class