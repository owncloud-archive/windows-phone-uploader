Imports System.Diagnostics
Imports System.Resources
Imports System.Windows.Markup

Partial Public Class App
    Inherits Application

    ''' <summary>
    ''' Permet d'accéder facilement au frame racine de l'application téléphonique.
    ''' </summary>
    ''' <returns>Frame racine de l'application téléphonique.</returns>
    Public Shared Property RootFrame As PhoneApplicationFrame

    ''' <summary>
    ''' Constructeur pour l'objet Application.
    ''' </summary>
    Public Sub New()
        ' Initialisation du XAML standard
        InitializeComponent()

        ' Initialisation spécifique au téléphone
        InitializePhoneApplication()

        ' Initialisation de l'affichage de la langue
        InitializeLanguage()

        ' Affichez des informations de profilage graphique lors du débogage.
        If Debugger.IsAttached Then
            ' Affichez les compteurs de fréquence des trames actuels.
            Application.Current.Host.Settings.EnableFrameRateCounter = True

            ' Affichez les zones de l'application qui sont redessinées dans chaque frame.
            'Application.Current.Host.Settings.EnableRedrawRegions = True

            ' Activez le mode de visualisation d'analyse hors production,
            ' qui montre les zones d'une page sur lesquelles une accélération GPU est produite avec une superposition colorée.
            'Application.Current.Host.Settings.EnableCacheVisualization = True


            ' Empêche l'écran de s'éteindre lorsque le débogueur est utilisé en désactivant
            ' la détection de l'état inactif de l'application.
            ' Attention :- À utiliser uniquement en mode de débogage. Les applications qui désactivent la détection d'inactivité de l'utilisateur continueront de s'exécuter
            ' et seront alimentées par la batterie lorsque l'utilisateur ne se sert pas du téléphone.
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled
        End If
    End Sub

    ' Code à exécuter quand une activation de contrat telle qu’une ouverture de fichier ou un sélecteur d’enregistrement de fichier retourne 
    ' avec le fichier sélectionné ou autres valeurs de retour
    Private Sub Application_ContractActivated(ByVal sender As Object, ByVal e As Windows.ApplicationModel.Activation.IActivatedEventArgs)
    End Sub

    ' Code à exécuter lorsque l'application démarre (par exemple, à partir de Démarrer)
    ' Ce code ne s'exécute pas lorsque l'application est réactivée
    Private Sub Application_Launching(ByVal sender As Object, ByVal e As LaunchingEventArgs)
    End Sub

    ' Code à exécuter lorsque l'application est activée (affichée au premier plan)
    ' Ce code ne s'exécute pas lorsque l'application est démarrée pour la première fois
    Private Sub Application_Activated(ByVal sender As Object, ByVal e As ActivatedEventArgs)
    End Sub

    ' Code à exécuter lorsque l'application est désactivée (envoyée à l'arrière-plan)
    ' Ce code ne s'exécute pas lors de la fermeture de l'application
    Private Sub Application_Deactivated(ByVal sender As Object, ByVal e As DeactivatedEventArgs)
    End Sub

    ' Code à exécuter lors de la fermeture de l'application (par exemple, lorsque l'utilisateur clique sur Précédent)
    ' Ce code ne s'exécute pas lorsque l'application est désactivée
    Private Sub Application_Closing(ByVal sender As Object, ByVal e As ClosingEventArgs)
    End Sub

    ' Code à exécuter en cas d'échec d'une navigation
    Private Sub RootFrame_NavigationFailed(ByVal sender As Object, ByVal e As NavigationFailedEventArgs)
        If Diagnostics.Debugger.IsAttached Then
            ' Échec d'une navigation ; arrêt dans le débogueur
            Diagnostics.Debugger.Break()
        End If
    End Sub

    Public Sub Application_UnhandledException(ByVal sender As Object, ByVal e As ApplicationUnhandledExceptionEventArgs) Handles Me.UnhandledException

        ' Affichez des informations de profilage graphique lors du débogage.
        If Diagnostics.Debugger.IsAttached Then
            Diagnostics.Debugger.Break()
        Else
            e.Handled = True
            MessageBox.Show(e.ExceptionObject.Message & Environment.NewLine & e.ExceptionObject.StackTrace,
                            "Erreur", MessageBoxButton.OK)
        End If
    End Sub

#Region "Initialisation de l'application téléphonique"
    ' Éviter l'initialisation double
    Private phoneApplicationInitialized As Boolean = False

    ' Ne pas ajouter de code supplémentaire à cette méthode
    Private Sub InitializePhoneApplication()
        If phoneApplicationInitialized Then
            Return
        End If

        ' Créez le frame, mais ne le définissez pas encore comme RootVisual ; cela permet à l'écran de
        ' démarrage de rester actif jusqu'à ce que l'application soit prête pour le rendu.
        RootFrame = New PhoneApplicationFrame()
        AddHandler RootFrame.Navigated, AddressOf CompleteInitializePhoneApplication

        ' Gérer les erreurs de navigation
        AddHandler RootFrame.NavigationFailed, AddressOf RootFrame_NavigationFailed

        ' Gérer une activation de contrat telle qu’une ouverture de fichier ou un sélecteur d’enregistrement de fichier
        AddHandler PhoneApplicationService.Current.ContractActivated, AddressOf Application_ContractActivated

        'Gérer les requêtes de réinitialisation pour effacer la pile arrière
        AddHandler RootFrame.Navigated, AddressOf CheckForResetNavigation

        ' Garantir de ne pas retenter l'initialisation
        phoneApplicationInitialized = True
    End Sub

    ' Ne pas ajouter de code supplémentaire à cette méthode
    Private Sub CompleteInitializePhoneApplication(ByVal sender As Object, ByVal e As NavigationEventArgs)
        ' Définir le Visual racine pour permettre à l'application d'effectuer le rendu
        If RootVisual IsNot RootFrame Then
            RootVisual = RootFrame
        End If

        ' Supprimer ce gestionnaire, puisqu'il est devenu inutile
        RemoveHandler RootFrame.Navigated, AddressOf CompleteInitializePhoneApplication
    End Sub

    Private Sub CheckForResetNavigation(ByVal sender As Object, ByVal e As NavigationEventArgs)
        ' Si l'application a reçu une navigation de « réinitialisation », nous devons vérifier
        ' sur la navigation suivante pour voir si la pile de la page doit être réinitialisée
        If e.NavigationMode = NavigationMode.Reset Then
            AddHandler RootFrame.Navigated, AddressOf ClearBackStackAfterReset
        End If
    End Sub

    Private Sub ClearBackStackAfterReset(ByVal sender As Object, ByVal e As NavigationEventArgs)
        ' Désinscrire l'événement pour qu'il ne soit plus appelé
        RemoveHandler RootFrame.Navigated, AddressOf ClearBackStackAfterReset

        ' Effacer uniquement la pile des « nouvelles » navigations (avant) et des actualisations
        If e.NavigationMode <> NavigationMode.New And e.NavigationMode <> NavigationMode.Refresh Then
            Return
        End If

        ' Pour une interface utilisateur cohérente, effacez toute la pile de la page
        While Not RootFrame.RemoveBackEntry() Is Nothing
            ' ne rien faire
        End While
    End Sub
#End Region

    ' Initialise la police de l'application et le sens du flux tels qu'ils sont définis dans ses chaînes de ressource localisées.
    '
    ' Pour vous assurer que la police de votre application est alignée avec les langues prises en charge et que le
    ' FlowDirection pour chacune de ces langues respecte le sens habituel, ResourceLanguage
    ' et ResourceFlowDirection doivent être initialisés dans chaque fichier resx pour faire correspondre ces valeurs avec la
    ' culture du fichier. Par exemple :
    '
    ' AppResources.es-ES.resx
    '    La valeur de ResourceLanguage doit être « es-ES »
    '    La valeur de ResourceFlowDirection doit être « LeftToRight »
    '
    ' AppResources.ar-SA.resx
    '     La valeur de ResourceLanguage doit être « ar-SA »
    '     La valeur de ResourceFlowDirection doit être « RightToLeft »
    '
    ' Pour plus d'informations sur la localisation des applications Windows Phone, consultez le site http://go.microsoft.com/fwlink/?LinkId=262072.
    '
    Private Sub InitializeLanguage()
        Try
            ' Définissez la police pour qu'elle corresponde à la langue d'affichage définie par la
            ' chaîne de ressource ResourceLanguage pour chaque langue prise en charge.
            '
            ' Rétablit la police de la langue neutre si la langue d'affichage
            ' du téléphone n'est pas prise en charge.
            '
            ' Si une erreur de compilateur est détectée, ResourceLanguage est manquant dans
            ' le fichier de ressources.
            RootFrame.Language = XmlLanguage.GetLanguage(AppResources.ResourceLanguage)

            ' Définit FlowDirection pour tous les éléments sous le frame racine en fonction de la
            ' de la chaîne de ressource ResourceFlowDirection pour chaque
            ' langue prise en charge.
            '
            ' Si une erreur de compilateur est détectée, ResourceFlowDirection est manquant dans
            ' le fichier de ressources.
            Dim flow As FlowDirection = DirectCast([Enum].Parse(GetType(FlowDirection), AppResources.ResourceFlowDirection), FlowDirection)
            RootFrame.FlowDirection = flow
        Catch
            ' Si une exception est détectée ici, elle est probablement due au fait que
            ' ResourceLanguage n'est pas correctement défini sur un code de langue pris en charge
            ' ou que ResourceFlowDirection est défini sur une valeur différente de LeftToRight
            ' ou RightToLeft.

            If Debugger.IsAttached Then
                Debugger.Break()
            End If

            Throw
        End Try
    End Sub

End Class