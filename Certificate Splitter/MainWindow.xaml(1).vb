Imports System.IO
Imports Microsoft.Win32
Imports System.Windows.Forms
Imports System.Threading
Imports System.Diagnostics
Imports System.Windows.Threading
Imports System.Reflection

Class MainWindow

    Dim tempfolder As String = IO.Path.GetTempPath

#Region "Browse PFX Button"

    'Browse PFX - Deprecated by Drag&Drop'
    Private Sub Button_Click(sender As Object, e As RoutedEventArgs) Handles browsepfx.Click
        Dim dlg As New Microsoft.Win32.OpenFileDialog()
        dlg.Title = "Please select your PFX file"
        dlg.InitialDirectory = "C:\"
        dlg.FileName = ""
        dlg.DefaultExt = ".pfx"
        dlg.Filter = "PFX (.pfx)|*.pfx"
        Dim result As Boolean = dlg.ShowDialog()
        If result = True Then
            Dim pfxname As String = dlg.FileName
            pfxlabel.Content = pfxname
            chk1.Visibility = Windows.Visibility.Visible
        End If
    End Sub

#End Region

#Region "DropPFX"

    'Drag Enter'
    Private Sub dropimg_DragEnter(sender As Object, e As Windows.DragEventArgs) Handles dropimg.DragEnter
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            e.Effects = DragDropEffects.Copy
            Dim myblur As New Effects.BlurEffect
            myblur.Radius = 5
            dropimg.Effect = myblur
        End If
    End Sub
    'Drag Drop'
    Private Sub dropimg_Drop(sender As Object, e As Windows.DragEventArgs) Handles dropimg.Drop
        Dim files() As String = e.Data.GetData(DataFormats.FileDrop)
        If Path.GetExtension(files(0)) = ".pfx" Then
            For Each path In files
                pfxlabel.Content = path
                chk1.Visibility = Windows.Visibility.Visible
            Next
        Else
            MsgBox("File does not have a .pfx extension. Please drop a .pfx file only.")
        End If
    End Sub
    'Drag Leave'
    Private Sub dropimg_DragLeave(sender As Object, e As Windows.DragEventArgs) Handles dropimg.DragLeave
        dropimg.Effect = Nothing
    End Sub
    'Mouse Leave'
    Private Sub dropimg_MouseLeave(sender As Object, e As Input.MouseEventArgs) Handles dropimg.MouseLeave
        dropimg.Effect = Nothing
    End Sub

#End Region

#Region "Browse Export Path Button"

    'Browse Export Folder'
    Private Sub exportpathbtn_Click(sender As Object, e As RoutedEventArgs) Handles exportpathbtn.Click
        Dim dlg As New FolderBrowserDialog
        dlg.RootFolder = Environment.SpecialFolder.MyComputer
        dlg.SelectedPath = "C:\Program Files"
        dlg.ShowDialog()
        Dim folder As String = dlg.SelectedPath
        pathlabel.Content = folder
        chk2.Visibility = Windows.Visibility.Visible
    End Sub

#End Region

#Region "Split Certificate Button"

    'Split Cert'
    Private Sub Button_Click_1(sender As Object, e As RoutedEventArgs)
        If pfxlabel.Content = "PFX file path.." Or pathlabel.Content = "PFX file export path.." Then
            MsgBox("Please select the PFX file and/or Export location first!")
        Else
            Me.Dispatcher.Invoke(Sub() pb.Value = 35, DispatcherPriority.Background)
            step1()
            Thread.Sleep(1000)
            Me.Dispatcher.Invoke(Sub() pb.Value = 75, DispatcherPriority.Background)
            step2()
            Thread.Sleep(1000)
            Me.Dispatcher.Invoke(Sub() pb.Value = 100, DispatcherPriority.Background)
            step3()
            Thread.Sleep(500)
            donelabel.Content = "Finished! Please copy cert.crt and cert.key to 3MHIS\3mws\ssl\"
            Thread.Sleep(500)
            My.Computer.FileSystem.DeleteFile(pathlabel.Content & "\key.pem")
            Process.Start(pathlabel.Content)
        End If
    End Sub

#End Region

#Region "Split Certificate Steps"

    'Step 1'
    Private Sub step1()
        Dim exportkey As New Process()
        exportkey.StartInfo.UseShellExecute = False
        exportkey.StartInfo.FileName = tempfolder & "openssl.exe"
        exportkey.StartInfo.Arguments = "pkcs12 -nocerts -passin pass:" & pfxpassbox.Password & " -passout pass:phrase -in " & """" & pfxlabel.Content & """" & " -out " & """" & pathlabel.Content & "\key.pem" & """"
        exportkey.StartInfo.WindowStyle = ProcessWindowStyle.Hidden
        exportkey.StartInfo.CreateNoWindow = True
        exportkey.Start()
    End Sub

    'Step 2'
    Private Sub step2()
        Dim exportcert As New Process()
        exportcert.StartInfo.UseShellExecute = False
        exportcert.StartInfo.FileName = tempfolder & "openssl.exe"
        exportcert.StartInfo.Arguments = "pkcs12 -clcerts -nokeys -passin pass:" & pfxpassbox.Password & " -passout pass:phrase -in " & """" & pfxlabel.Content & """" & " -out " & """" & pathlabel.Content & "\cert.crt" & """"
        exportcert.StartInfo.WindowStyle = ProcessWindowStyle.Hidden
        exportcert.StartInfo.CreateNoWindow = True
        exportcert.Start()
    End Sub

    'Step 3'
    Private Sub step3()
        Dim removepass As New Process()
        removepass.StartInfo.UseShellExecute = False
        removepass.StartInfo.FileName = tempfolder & "openssl.exe"
        removepass.StartInfo.Arguments = "rsa -passin pass:phrase -in " & """" & pathlabel.Content & "\key.pem" & """" & " -out " & """" & pathlabel.Content & "\cert.key" & """"
        removepass.StartInfo.WindowStyle = ProcessWindowStyle.Hidden
        removepass.StartInfo.CreateNoWindow = True
        removepass.Start()
    End Sub

#End Region

#Region "Misc"

    'Password Prompt Text'
    Private Sub pfxpassbox_GotFocus(sender As Object, e As RoutedEventArgs) Handles pfxpassbox.GotFocus
        passprompt.Visibility = Windows.Visibility.Hidden
    End Sub
    'Window Move'
    Private Sub Window_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
        DragMove()
    End Sub
    'Window Close'
    Private Sub Button_Click_3(sender As Object, e As RoutedEventArgs)
        Application.Current.Shutdown()
    End Sub
    'Enable Split Button'
    Private Sub pfxpassbox_PasswordChanged(sender As Object, e As RoutedEventArgs) Handles pfxpassbox.PasswordChanged
        If pfxpassbox.Password.Length = 0 Then
            splitbtn.IsEnabled = False
            chk3.Visibility = Windows.Visibility.Hidden
        Else
            splitbtn.IsEnabled = True
            chk3.Visibility = Windows.Visibility.Visible
        End If
    End Sub
    'Window Loaded - Disable Split Button - Copy Files'
    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        splitbtn.IsEnabled = False
        System.IO.File.WriteAllBytes(tempfolder & "openssl.exe", My.Resources.openssl)
        System.IO.File.WriteAllBytes(tempfolder & "libeay32.dll", My.Resources.libeay32)
        System.IO.File.WriteAllBytes(tempfolder & "ssleay32.dll", My.Resources.ssleay32)
    End Sub
    'Window Closed - Delete Files
    Private Sub Window_Closed(sender As Object, e As EventArgs)
        System.IO.File.Delete(tempfolder & "openssl.exe")
        System.IO.File.Delete(tempfolder & "libeay32.dll")
        System.IO.File.Delete(tempfolder & "ssleay32.dll")
    End Sub

#End Region

End Class



