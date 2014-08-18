Imports System.Windows.Media.Imaging
Imports System.IO

Imports Microsoft.Devices
Imports Microsoft.Xna.Framework.Media


Imports Windows.Media.MediaProperties
Imports Windows.Devices.Enumeration


Partial Public Class pic
    Inherits PhoneApplicationPage

    Dim c As Windows.Media.Capture.MediaCapture
    Dim WithEvents cam As PhotoCamera
    Dim library As MediaLibrary = New MediaLibrary

    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub pic_BackKeyPress(sender As Object, e As ComponentModel.CancelEventArgs) Handles Me.BackKeyPress
        'TODO : que se passe-t-il quand on clique sur précédent ?
    End Sub

    Private Sub pic_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If PhotoCamera.IsCameraTypeSupported(CameraType.Primary) = True Then
            cam = New PhotoCamera(CameraType.Primary)
            viewfinderBrush.SetSource(cam)
        Else
            clem2k.notifyMe(AppResources.CamDetectionKO)
        End If
        setOrientation()
        displayNetwork()
        setFlash("auto")
    End Sub
    Protected Overrides Sub OnNavigatingFrom(e As System.Windows.Navigation.NavigatingCancelEventArgs)
        If cam IsNot Nothing Then
            cam.Dispose()
        End If

    End Sub
    Private Sub viewfinder_Tapped(sender As Object, e As GestureEventArgs)
        'TODO set focus ?

    End Sub
    Private Async Sub cam_CaptureImageAvailable(sender As Object, e As Microsoft.Devices.ContentReadyEventArgs) Handles cam.CaptureImageAvailable
        Dim fichier As Windows.Storage.StorageFile = Await clem2k.getNewPhotoFile()
        Dim br As System.IO.BinaryReader = New System.IO.BinaryReader(e.ImageStream)
        Dim result As Byte() = br.ReadBytes(CInt(e.ImageStream.Length))

        Await Windows.Storage.FileIO.WriteBytesAsync(fichier, result)
        Await clem2k.transferFile(fichier)
    End Sub
    Protected Overrides Sub OnOrientationChanged(e As OrientationChangedEventArgs)
        If cam IsNot Nothing Then
            Dispatcher.BeginInvoke(AddressOf setOrientation)
        End If
        MyBase.OnOrientationChanged(e)
        displayNetwork()
    End Sub
    Private Sub setOrientation()
        Dim rotation As Double = cam.Orientation
        Select Case Me.Orientation
            Case PageOrientation.LandscapeLeft
                rotation = cam.Orientation - 90
                imgTakePic3.Visibility = System.Windows.Visibility.Visible
                imgTakePic.Visibility = System.Windows.Visibility.Collapsed
                imgTakePic2.Visibility = System.Windows.Visibility.Collapsed
                Exit Select
            Case PageOrientation.LandscapeRight
                rotation = cam.Orientation + 90
                imgTakePic3.Visibility = System.Windows.Visibility.Collapsed
                imgTakePic.Visibility = System.Windows.Visibility.Collapsed
                imgTakePic2.Visibility = System.Windows.Visibility.Visible
                Exit Select
            Case Else
                imgTakePic3.Visibility = System.Windows.Visibility.Collapsed
                imgTakePic.Visibility = System.Windows.Visibility.Visible
                imgTakePic2.Visibility = System.Windows.Visibility.Collapsed
        End Select
        viewfinderTransform.Rotation = rotation

    End Sub
    Private Sub displayNetwork()
        Dim state As Int16 = 1
        Dim icp As Windows.Networking.Connectivity.ConnectionProfile = Windows.Networking.Connectivity.NetworkInformation.GetInternetConnectionProfile()

        Try
            If icp Is Nothing Then
                state = 2
            Else
                state = 1
            End If
        Catch ex As Exception
            state = 3
        End Try


        Select Case state
            Case 1
                imgNetOK.Visibility = System.Windows.Visibility.Visible
                imgNetKO.Visibility = System.Windows.Visibility.Collapsed
                imgNetPending.Visibility = System.Windows.Visibility.Collapsed
            Case 2
                imgNetOK.Visibility = System.Windows.Visibility.Collapsed
                imgNetKO.Visibility = System.Windows.Visibility.Visible
                imgNetPending.Visibility = System.Windows.Visibility.Collapsed
            Case 3
                imgNetOK.Visibility = System.Windows.Visibility.Collapsed
                imgNetKO.Visibility = System.Windows.Visibility.Collapsed
                imgNetPending.Visibility = System.Windows.Visibility.Visible
            Case Else
                imgNetOK.Visibility = System.Windows.Visibility.Collapsed
                imgNetKO.Visibility = System.Windows.Visibility.Collapsed
                imgNetPending.Visibility = System.Windows.Visibility.Collapsed
        End Select


    End Sub
    Private Sub takePicture()
        If cam IsNot Nothing Then
            Try
                cam.CaptureImage()
            Catch ex As Exception
                Throw
            End Try
        End If
        displayNetwork()
    End Sub
    Private Sub imgTakePic_Tap(sender As Object, e As GestureEventArgs) Handles imgTakePic.Tap
        takePicture()
    End Sub
    Private Sub imgTakePic2_Tap(sender As Object, e As GestureEventArgs) Handles imgTakePic2.Tap
        takePicture()
    End Sub
    Private Sub imgTakePic3_Tap(sender As Object, e As GestureEventArgs) Handles imgTakePic3.Tap
        takePicture()
    End Sub
    Private Sub imgSettings_Tap(sender As Object, e As GestureEventArgs) Handles imgSettings.Tap
        Dim localSettings As Windows.Storage.ApplicationDataContainer = Windows.Storage.ApplicationData.Current.LocalSettings
        localSettings.Values("user") = ""
        Dim u As System.Uri = New Uri("/MainPage.xaml", UriKind.Relative)
        NavigationService.Navigate(u)
    End Sub
    Private Sub setFlash(mode As String)
        Select Case mode
            Case "auto"
                imgFlashAuto.Visibility = System.Windows.Visibility.Visible
                imgFlashON.Visibility = System.Windows.Visibility.Collapsed
                imgFlashOFF.Visibility = System.Windows.Visibility.Collapsed
            Case "on"
                imgFlashAuto.Visibility = System.Windows.Visibility.Collapsed
                imgFlashON.Visibility = System.Windows.Visibility.Visible
                imgFlashOFF.Visibility = System.Windows.Visibility.Collapsed
            Case "off"
                imgFlashAuto.Visibility = System.Windows.Visibility.Collapsed
                imgFlashON.Visibility = System.Windows.Visibility.Collapsed
                imgFlashOFF.Visibility = System.Windows.Visibility.Visible
        End Select
    End Sub
    Private Sub imgFlashAuto_Tap(sender As Object, e As GestureEventArgs) Handles imgFlashAuto.Tap
        setFlash("on")
        cam.FlashMode = FlashMode.Auto
    End Sub
    Private Sub imgFlashOFF_Tap(sender As Object, e As GestureEventArgs) Handles imgFlashOFF.Tap
        setFlash("auto")
        cam.FlashMode = FlashMode.Off
    End Sub
    Private Sub imgFlashON_Tap(sender As Object, e As GestureEventArgs) Handles imgFlashON.Tap
        setFlash("off")
        cam.FlashMode = FlashMode.On
    End Sub
    Private Sub imgList_Tap(sender As Object, e As GestureEventArgs) Handles imgList.Tap
        Dim u As System.Uri = New Uri("/picList.xaml", UriKind.Relative)
        NavigationService.Navigate(u)
    End Sub
End Class
