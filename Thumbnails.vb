Imports System.IO
Imports System.Globalization
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Text
Imports System.Drawing.Imaging

Public Class Thumbnails

    Public Shared PictureIndex As Integer
    Public Shared ImageList As String

    Shared Async Function RunThumbnailer(inputFile As String, ExportPath As String, FormatImage As String, mode As Integer, pos As Integer, width As Integer, columnCount As Integer, rowcount As Integer, intervalSec As Integer, LogoStatus As Boolean, Optional Quality As Integer = 95) As Task(Of String)
        ImageList = ""
        Dim Results As String = Await CreateThumbnails(inputFile, ExportPath, FormatImage, mode, pos, width, columnCount, rowcount, intervalSec, LogoStatus, Quality)
        Return ImageList

    End Function

    Shared Sub LoopImage()
        Try
            Main.UI.ImageListLBx.UpdateLayout()
            If PictureIndex > 0 Then
                Main.UI.ImageListLBx.SelectedIndex = 0
            End If
            PictureDB()
        Catch ex As Exception
        End Try
    End Sub

    Shared Sub PictureDB()
        Try
            PictureIndex += 1
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
            Main.UI.ThumbnailPreviewPB.Source = imgSrc
        Catch ex As Exception
        End Try
    End Sub

    Shared Async Function CreateThumbnails(inputFile As String, ExportPath As String, FormatImage As String, mode As Integer, pos As Integer, width As Integer, columnCount As Integer, rowcount As Integer,
        intervalSec As Integer, LogoStatus As Boolean, Optional Quality As Integer = 95) As Task(Of String)

        Dim fontname = "DejaVu Serif"
        Dim Fontoptions = "Mikadan"
        'Dim width = CInt(MainWindow.WidthNB.Value)        
        'Dim columnCount = CInt(MainWindow.ColumnsNB.Value)       
        'Dim rowCount = CInt(MainWindow.RowsNB.Value)       
        Dim dar = MediaInfo.GetVideo(inputFile, "DisplayAspectRatio")
        Dim height = CInt(width / Convert.ToSingle(dar, CultureInfo.InvariantCulture))
        Dim gap = CInt((width * columnCount) * 0.000)
        Dim font = New Font(fontname, (width * columnCount) \ 80, FontStyle.Bold, GraphicsUnit.Pixel)
        Dim foreColor = Color.Black

        width = width - width Mod 4
        height = height - height Mod 4

        Dim avsdoc As New VideoScript
        avsdoc.Path = Folder.Temp + "Thumbnails.avs"
        If inputFile.EndsWith("mp4") Then
            avsdoc.Filters.Add(New VideoFilter("LWLibavVideoSource(""" + inputFile + "" + """, format = ""YUV420P8"").Spline64Resize(" & width & "," & height & ")"))
        Else
            avsdoc.Filters.Add(New VideoFilter("FFVideoSource(""" + inputFile + "" + """, colorspace = ""YV12"").Spline64Resize(" & width & "," & height & ")"))
        End If
        avsdoc.Filters.Add(New VideoFilter("ConvertToRGB(matrix=""Rec709"")"))

        Dim errorMsg = ""

        Try
            avsdoc.Synchronize()
            'Dim mode = Modes
            'Dim intervalSec = 0
            If intervalSec <> 0 AndAlso mode = 1 Then rowcount = CInt((avsdoc.GetSeconds / intervalSec) / columnCount)
            'errorMsg = p.SourceScript.GetErrorMessage
        Catch ex As Exception
        End Try

        If errorMsg <> "" Then
            MsgBox("Failed to open file." + BR2 + inputFile, errorMsg)
            Exit Function
        End If

        Dim frames = avsdoc.GetFrames
        Dim count = columnCount * rowcount
        Dim bitmaps As New List(Of Bitmap)

        Using avi As New AVIFile(avsdoc.Path)
            For x = 1 To count
                avi.Position = CInt((frames / count) * x) - CInt((frames / count) / 2)
                Dim bitmap = New Bitmap(avi.GetBitmap())

                Using g = Graphics.FromImage(bitmap)
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic
                    g.TextRenderingHint = TextRenderingHint.AntiAlias
                    g.SmoothingMode = SmoothingMode.AntiAlias
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality

                    Dim dur = TimeSpan.FromSeconds(avi.FrameCount / avi.FrameRate)
                    Dim timestamp = Extras.GetTimeString(avi.Position / avi.FrameRate)
                    Dim ft As New Font("Segoe UI", font.Size, FontStyle.Bold, GraphicsUnit.Pixel)

                    Dim gp As New GraphicsPath()
                    Dim sz = g.MeasureString(timestamp, ft)
                    Dim pt As Point
                    'Dim pos = Position

                    If pos = 0 OrElse pos = 2 Then
                        pt.X = ft.Height \ 10
                    Else
                        pt.X = CInt(bitmap.Width - sz.Width - ft.Height / 10)
                    End If

                    If pos = 2 OrElse pos = 3 Then
                        pt.Y = CInt(bitmap.Height - sz.Height)
                    Else
                        pt.Y = 0
                    End If

                    gp.AddString(timestamp, ft.FontFamily, ft.Style, ft.Size, pt, New StringFormat())

                    Using pen As New Pen(Brushes.Black, ft.Height \ 5)
                        g.DrawPath(pen, gp)
                    End Using

                    g.FillPath(Brushes.Gainsboro, gp)
                End Using

                bitmaps.Add(bitmap)
            Next

            width = width + gap
            height = height + gap
        End Using

        Try

        Catch ex As Exception

        End Try

        Try
            File.Delete(avsdoc.Path)
        Catch ex As Exception
        End Try

        Try
            If inputFile.EndsWith("mp4") Then
                File.Delete(inputFile + ".lwi")
            Else
                File.Delete(inputFile + ".ffindex")
            End If
        Catch ex As Exception
        End Try

        Dim infoSize As String
        Dim infoWidth = MediaInfo.GetVideo(inputFile, "Width")
        Dim infoHeight = MediaInfo.GetVideo(inputFile, "Height")
        Dim infoLength = New FileInfo(inputFile).Length
        Dim infoDuration = MediaInfo.GetGeneral(inputFile, "Duration").ToInt
        Dim audioCodecs = MediaInfo.GetAudioCodecs(inputFile)
        If audioCodecs = "" Then audioCodecs = ""
        Dim Channels = MediaInfo.GetAudio(inputFile, "Channel(s)").ToInt
        Dim SubSampling = MediaInfo.GetVideo(inputFile, "ChromaSubsampling").Replace(":", "")
        If SubSampling = "" Then SubSampling = ""
        Dim ColorSpace = MediaInfo.GetVideo(inputFile, "ColorSpace").ToLower
        If ColorSpace = "" Then ColorSpace = ""
        Dim Profile = MediaInfo.GetVideo(inputFile, "Format_Profile").Shorten(4)
        If Profile = "" Then Profile = "Main"
        Dim ScanType = MediaInfo.GetVideo(inputFile, "ScanType")

        Dim AudioSound As String
        If Channels = 2 Then AudioSound = "Stereo"
        If Channels = 1 Then AudioSound = "Mono"
        If Channels = 6 Then AudioSound = "Surround Sound"
        If Channels = 8 Then AudioSound = "Surround Sound"
        If Channels = 0 Then AudioSound = ""

        If infoLength / 1024 ^ 3 > 1 Then
            infoSize = (infoLength / 1024 ^ 3).ToInvariantString("f2") + "GB"
        Else
            infoSize = CInt(infoLength / 1024 ^ 2).ToString + "MB"
        End If

        Dim caption = "File: " + FilePath.GetName(inputFile) + BR & "Size: " + MediaInfo.GetGeneral(inputFile, "FileSize") + " bytes" + " (" + infoSize + ")" & ", " + "Duration: " + Extras.GetTimeString(infoDuration / 1000) + ", avg.bitrate: " + MediaInfo.GetGeneral(inputFile, "OverallBitRate_String") + BR +
            "Audio: " + audioCodecs + ", " + MediaInfo.GetAudio(inputFile, "SamplingRate_String") + ", " + AudioSound + ", " + MediaInfo.GetAudio(inputFile, "BitRate_String") + BR +
            "Video: " + MediaInfo.GetVideo(inputFile, "Format") + " (" + Profile + ")" + ", " + ColorSpace + SubSampling + ScanType.Shorten(1).ToLower() + ", " + MediaInfo.GetVideo(inputFile, "Width") & "x" & MediaInfo.GetVideo(inputFile, "Height") & ", " + MediaInfo.GetVideo(inputFile, "BitRate_String") + ", " & MediaInfo.GetVideo(inputFile, "FrameRate").ToSingle.ToInvariantString + "fps".Replace(", ", "")

        caption = caption.Replace(" ,", "")
        Dim captionSize = Forms.TextRenderer.MeasureText(caption, font)
        Dim captionHeight = captionSize.Height + font.Height \ 3
        Dim imageWidth = width * columnCount + gap
        Dim imageHeight = height * rowcount + captionHeight

        Using bitmap As New Bitmap(imageWidth, imageHeight)
            Using g = Graphics.FromImage(bitmap)
                g.InterpolationMode = InterpolationMode.HighQualityBicubic
                g.TextRenderingHint = TextRenderingHint.AntiAlias
                g.SmoothingMode = SmoothingMode.AntiAlias
                g.PixelOffsetMode = PixelOffsetMode.HighQuality
                g.Clear(Color.AliceBlue)
                Dim rect = New RectangleF(gap, 0, imageWidth - gap * 2, captionHeight)
                Dim format As New StringFormat
                format.LineAlignment = StringAlignment.Center

                Using brush As New SolidBrush(foreColor)
                    g.DrawString(caption, font, brush, rect, format)
                    format.Alignment = StringAlignment.Far
                    format.LineAlignment = StringAlignment.Center
                    If LogoStatus = False Then
                        g.DrawString("Thumbnailer", New Font(Fontoptions, font.Height * 2, FontStyle.Bold, GraphicsUnit.Pixel), brush, rect, format)
                    End If
                End Using

                For x = 0 To bitmaps.Count - 1
                    Dim rowPos = x \ columnCount
                    Dim columnPos = x Mod columnCount
                    g.DrawImage(bitmaps(x), columnPos * width + gap, rowPos * height + captionHeight)
                Next
            End Using

            Dim Export As String = Path.GetFullPath(Path.Combine(ExportPath, inputFile.Base + "." + FormatImage.ToLower()))

            If FormatImage = "JPG" Then
                Try

                    Dim params = New EncoderParameters(1)
                    params.Param(0) = New EncoderParameter(Imaging.Encoder.Quality, Quality)
                    Dim info = ImageCodecInfo.GetImageEncoders.Where(Function(arg) arg.FormatID = ImageFormat.Jpeg.Guid).First
                    bitmap.Save(Export, info, params)
                    ImageList += Export + BR
                    Return Export
                Catch ex As Exception
                End Try

            ElseIf FormatImage = "PNG" Then
                Try
                    Dim info = ImageCodecInfo.GetImageEncoders.Where(Function(arg) arg.FormatID = ImageFormat.Png.Guid).First
                    bitmap.Save(Export, info, Nothing)
                    ImageList += Export + BR
                    Return Export
                Catch ex As Exception
                End Try

            ElseIf FormatImage = "TIFF" Then
                Try

                    Dim info = ImageCodecInfo.GetImageEncoders.Where(Function(arg) arg.FormatID = ImageFormat.Tiff.Guid).First
                    bitmap.Save(Export, info, Nothing)
                    ImageList += Export + BR
                    Return Export
                Catch ex As Exception
                End Try

            ElseIf FormatImage = "BMP" Then
                Try
                    Dim info = ImageCodecInfo.GetImageEncoders.Where(Function(arg) arg.FormatID = ImageFormat.Bmp.Guid).First
                    bitmap.Save(Export, info, Nothing)
                    ImageList += Export + BR
                    Return Export
                Catch ex As Exception
                End Try
            End If
        End Using
    End Function

    Public Class Extras
        Shared Function GetTimeString(sec As Double) As String
            Dim ts = TimeSpan.FromSeconds(sec)
            Return CInt(Math.Floor(ts.TotalMinutes)).ToString("00") + ":" + CInt(Math.Floor(ts.Seconds)).ToString("00")
        End Function
    End Class
End Class
