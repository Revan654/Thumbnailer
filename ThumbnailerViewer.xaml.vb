Imports System.Drawing
Imports System.IO
Imports System.Net
Imports System
Imports System.Text
Imports Microsoft.VisualBasic

Public Class ThumbnailerViewer

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        Try
            Dim data As Byte()
            data = File.ReadAllBytes(Main.UI.CurrentImageTB.Text)
            Dim strm As MemoryStream = New MemoryStream()
            strm.Write(data, 0, data.Length)
            strm.Position = 0
            Dim img As System.Drawing.Image = System.Drawing.Image.FromStream(strm)
            Dim bi As BitmapImage = New BitmapImage()
            bi.BeginInit()
            Dim ms As MemoryStream = New MemoryStream()
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp)
            ms.Seek(0, SeekOrigin.Begin)
            bi.StreamSource = ms
            bi.EndInit()
            Dim imgSrc As ImageSource = TryCast(bi, ImageSource)
            ViewerPB.Source = imgSrc
            Me.Title = Main.UI.CurrentImageTB.Text.Base
            ViewerPB.Stretch = Stretch.Fill
        Catch ex As Exception
            Using fd As New Forms.OpenFileDialog
                fd.Filter = "MediaFile|*.jpg;*.jpeg;*.png;*.tiff;*.bmp"
                fd.FilterIndex = 1
                If fd.ShowDialog = Forms.DialogResult.OK Then
                    Try
                        Dim data As Byte()
                        data = File.ReadAllBytes(Path.GetFullPath(fd.FileName))
                        Dim strm As MemoryStream = New MemoryStream()
                        strm.Write(data, 0, data.Length)
                        strm.Position = 0
                        Dim img As System.Drawing.Image = System.Drawing.Image.FromStream(strm)
                        Dim bi As BitmapImage = New BitmapImage()
                        bi.BeginInit()
                        Dim ms As MemoryStream = New MemoryStream()
                        img.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp)
                        ms.Seek(0, SeekOrigin.Begin)
                        bi.StreamSource = ms
                        bi.EndInit()
                        Dim imgSrc As ImageSource = TryCast(bi, ImageSource)
                        ViewerPB.Source = imgSrc
                        Me.Title = fd.FileName.Base
                        ViewerPB.Stretch = Stretch.Fill
                    Catch ex2 As Exception
                    End Try
                End If
            End Using
        End Try
    End Sub

    Private Sub Fit_Screen_Click(sender As Object, e As RoutedEventArgs)
        Me.Width = 1480
        Me.Height = 1084
        ViewerPB.Stretch = Stretch.Fill
    End Sub

    Private Sub Zoom_Screen_Click(sender As Object, e As RoutedEventArgs)
        Me.Width = 1480
        Me.Height = 1084
        ViewerPB.Stretch = Stretch.Uniform
    End Sub

    Private Sub FileOpen_Click(sender As Object, e As RoutedEventArgs)
        Using fd As New Forms.OpenFileDialog
            fd.Filter = "MediaFile|*.jpg;*.jpeg;*.png;*.tiff;*.bmp"
            fd.FilterIndex = 1
            If fd.ShowDialog = Forms.DialogResult.OK Then
                Try
                    Dim data As Byte()
                    data = File.ReadAllBytes(Path.GetFullPath(fd.FileName))
                    Dim strm As MemoryStream = New MemoryStream()
                    strm.Write(data, 0, data.Length)
                    strm.Position = 0
                    Dim img As System.Drawing.Image = System.Drawing.Image.FromStream(strm)
                    Dim bi As BitmapImage = New BitmapImage()
                    bi.BeginInit()
                    Dim ms As MemoryStream = New MemoryStream()
                    img.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp)
                    ms.Seek(0, SeekOrigin.Begin)
                    bi.StreamSource = ms
                    bi.EndInit()
                    Dim imgSrc As ImageSource = TryCast(bi, ImageSource)
                    ViewerPB.Source = imgSrc
                    Me.Title = fd.FileName.Base
                    ViewerPB.Stretch = Stretch.Fill
                Catch ex2 As Exception
                End Try
            End If
        End Using
    End Sub

    Private Sub CloseForm_Click(sender As Object, e As RoutedEventArgs)
        Close()
    End Sub

    Private Sub ViewerPB_MouseLeftButtonUp(sender As Object, e As MouseButtonEventArgs)
        Close()
    End Sub
End Class
