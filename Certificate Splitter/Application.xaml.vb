Class Application

    ' Application-level events, such as Startup, Exit, and DispatcherUnhandledException
    ' can be handled in this file.

#Region "WindowChrome"

    'Close'
    Private Sub Button_Close(sender As Object, e As RoutedEventArgs)
        Application.Current.Shutdown()
    End Sub
    'Minimize'
    Private Sub Button_Minimize(sender As Object, e As RoutedEventArgs)
        MainWindow.WindowState = WindowState.Minimized
    End Sub
    'Maximize'
    Private Sub Button_Maximize(sender As Object, e As RoutedEventArgs)
        'MainWindow.WindowState = WindowState.Maximized
    End Sub
    'Restore'
    Private Sub Button_Restore(sender As Object, e As RoutedEventArgs)
        MainWindow.WindowState = WindowState.Normal
    End Sub

#End Region

End Class
