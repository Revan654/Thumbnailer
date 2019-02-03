Imports System.IO

Public Class ThumbnailViewer

    Private Sub ThumbnailViewer_Loaded(sender As Object, e As RoutedEventArgs) Handles MyBase.Loaded
        Dim MainWindow As Main = New Main()

        Try
            Dim uri = New Uri(MainWindow.SetCurrentImage)
            ThumbnailPB.Source = New BitmapImage(uri)
            Me.Title = Path.GetFileName(MainWindow.CurrentImage.Text)
            ThumbnailPB.Stretch = Stretch.Fill
        Catch ex As Exception

            Using fd As New Forms.OpenFileDialog
            fd.Filter = "MediaFile|*.jpg;*.jpeg;*.png;*.tiff;*.bmp"
            fd.FilterIndex = 1
            If fd.ShowDialog = Forms.DialogResult.OK Then
                Dim uri = New Uri(fd.FileName)
                ThumbnailPB.Source = New BitmapImage(uri)
                Me.Title = Path.GetFileName(fd.FileName)
            End If
        End Using
        End Try
    End Sub

    Private Sub FitScreen_Click(sender As Object, e As RoutedEventArgs)
        Me.Width = 1480
        Me.Height = 1084
        ThumbnailPB.Stretch = Stretch.Fill
    End Sub

    Private Sub Zoom_Click(sender As Object, e As RoutedEventArgs)
        Me.Width = 1480
        Me.Height = 1084
        ThumbnailPB.Stretch = Stretch.Uniform
    End Sub

    Private Sub File_Open(sender As Object, e As RoutedEventArgs)
        Using fd As New Forms.OpenFileDialog
            fd.Filter = "MediaFile|*.jpg;*.jpeg;*.png;*.tiff;*.bmp"
            fd.FilterIndex = 1
            If fd.ShowDialog = Forms.DialogResult.OK Then
                Dim uri = New Uri(fd.FileName)
                ThumbnailPB.Source = New BitmapImage(uri)
                Me.Title = Path.GetFileName(fd.FileName)
            End If
        End Using
    End Sub

    Private Sub Close_Form(sender As Object, e As RoutedEventArgs)
        Close()
    End Sub

    Private Sub ThumbnailViewer_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs)
        Close()
    End Sub
End Class
