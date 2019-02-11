Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Globalization
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text.RegularExpressions

Public Class Misc

End Class

<Serializable>
Public Class AudioStream
    Property BitDepth As Integer
    Property Bitrate As Integer
    Property Bitrate2 As Integer
    Property Channels As Integer
    Property Channels2 As Integer
    Property Codec As String
    Property CodecString As String
    Property Delay As Integer
    Property Format As String
    Property FormatProfile As String 'was only field to show DTS MA
    Property ID As Integer
    Property StreamOrder As Integer
    Property Index As Integer
    Property Language As Language
    Property SamplingRate As Integer
    Property Title As String
    Property Enabled As Boolean = True
    Property [Default] As Boolean
    Property Forced As Boolean
    Property Lossy As Boolean

    ReadOnly Property Name As String
        Get
            Dim ret = "#" & Index + 1

            If Codec = "Atmos / TrueHD" OrElse
                FormatProfile.EqualsAny("TrueHD+Atmos / TrueHD",
                                        "E-AC-3+Atmos / E-AC-3",
                                        "TrueHD+Atmos / TrueHD / AC-3") Then
                ret += " Atmos"
            ElseIf CodecString = "TrueHD / AC3" OrElse Codec = "TrueHD / AC3" Then
                ret += " TrueHD"
            ElseIf CodecString = "MPEG-1 Audio layer 2" Then
                ret += " MP2"
            ElseIf CodecString = "MPEG-1 Audio layer 3" Then
                ret += " MP3"
            ElseIf CodecString = "AC3+" Then
                ret += " E-AC3"
            ElseIf FormatProfile.StartsWith("MA /") Then
                ret += " DTS-MA"
            ElseIf FormatProfile.StartsWith("HRA /") Then
                ret += " DTS-HRA"
            Else
                ret += " " + CodecString
            End If

            If Not ret.Contains("Atmos") Then
                If Channels <> Channels2 AndAlso Channels > 0 AndAlso Channels2 > 0 Then
                    ret += " " & Channels & "/" & Channels2 & "ch"
                ElseIf Channels > 0 Then
                    ret += " " & Channels & "ch"
                ElseIf Channels2 > 0 Then
                    ret += " " & Channels2 & "ch"
                End If
            End If

            If BitDepth > 0 AndAlso Not Lossy Then ret += " " & BitDepth & "Bit"

            If SamplingRate > 0 Then
                If SamplingRate Mod 1000 = 0 Then
                    ret += " " & SamplingRate / 1000 & "kHz"
                Else
                    ret += " " & SamplingRate & "Hz"
                End If
            End If

            If Bitrate2 > 0 Then
                ret += " " & If(Bitrate = 0, "?", Bitrate.ToString) & "/" & Bitrate2 & "Kbps"
            ElseIf Bitrate > 0 Then
                ret += " " & Bitrate & "Kbps"
            End If

            If Delay <> 0 Then ret += " " & Delay & "ms"
            If Language.TwoLetterCode <> "iv" Then ret += " " + Language.Name
            If Title <> "" AndAlso Title <> " " Then ret += " " + Title

            Return ret
        End Get
    End Property

    ReadOnly Property Extension() As String
        Get
            Select Case CodecString
                Case "AAC LC", "AAC LC-SBR", "AAC LC-SBR-PS"
                    Return ".m4a"
                Case "AC3"
                    Return ".ac3"
                Case "DTS"
                    Return ".dts"
                Case "DTS-HD"
                    If FormatProfile.StartsWith("MA /") Then
                        Return ".dtsma"
                    ElseIf FormatProfile.StartsWith("HRA /") Then
                        Return ".dtshr"
                    Else
                        Return ".dtshd"
                    End If
                Case "PCM", "ADPCM"
                    Return ".wav"
                Case "MPEG-1 Audio layer 2"
                    Return ".mp2"
                Case "MPEG-1 Audio layer 3"
                    Return ".mp3"
                Case "TrueHD / AC3"
                    Return ".thd"
                Case "FLAC"
                    Return ".flac"
                Case "Vorbis"
                    Return ".ogg"
                Case "Opus"
                    Return ".opus"
                Case "TrueHD", "Atmos / TrueHD"
                    Return ".thd"
                Case "AC3+"
                    Return ".eac3"
                Case Else
                    Return ".mka"
            End Select
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return Name
    End Function
End Class


Public Class VideoStream
    Property Format As String
    Property StreamOrder As Integer
    Property ID As Integer
    Property Index As Integer

    ReadOnly Property Ext() As String
        Get
            Select Case Format
                Case "MPEG Video"
                    Return "mpg"
                Case "AVC"
                    Return "h264"
                Case "MPEG-4 Visual", "JPEG"
                    Return "avi"
                Case "HEVC"
                    Return "h265"
                Case "AV1"
                    Return "ivf"
                Case Else
                    Throw New NotImplementedException("Video format " + Format + " is not supported.")
            End Select
        End Get
    End Property

    ReadOnly Property ExtFull() As String
        Get
            Return "." + Ext
        End Get
    End Property
End Class

Public Class BitmapUtil
    Property Data As Byte()
    Property BitmapData As BitmapData

    Shared Function Create(bmp As Bitmap) As BitmapUtil
        Dim util As New BitmapUtil
        Dim rect As New Rectangle(0, 0, bmp.Width, bmp.Height)
        util.BitmapData = bmp.LockBits(rect, Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat)
        Dim ptr = util.BitmapData.Scan0
        Dim bytesCount = Math.Abs(util.BitmapData.Stride) * bmp.Height
        util.Data = New Byte(bytesCount - 1) {}
        Marshal.Copy(ptr, util.Data, 0, bytesCount)
        bmp.UnlockBits(util.BitmapData)
        Return util
    End Function

    Function GetPixel(x As Integer, y As Integer) As Color
        Dim pos = y * BitmapData.Stride + x * 4
        Return Color.FromArgb(Data(pos), Data(pos + 1), Data(pos + 2))
    End Function

    Function GetMax(x As Integer, y As Integer) As Integer
        Dim col = GetPixel(x, y)
        Dim max = Math.Max(col.R, col.G)
        Return Math.Max(max, col.B)
    End Function
End Class

Public Enum ContainerStreamType
    Unknown
    Audio
    Video
    Subtitle
    Attachment
    Chapters
End Enum

Public Class Comparer(Of T)
    Implements IComparer(Of T)

    Property PropName As String
    Property Ascending As Boolean = True

    Sub New(propName As String, Optional ascending As Boolean = True)
        Me.PropName = propName
        Me.Ascending = ascending
    End Sub

    Public Function Compare(x As T, y As T) As Integer Implements IComparer(Of T).Compare
        If Not Ascending Then
            Dim x1 = x
            x = y
            y = x1
        End If

        Dim type = x.GetType
        Dim propInfo = type.GetProperty(PropName)

        Return DirectCast(propInfo.GetValue(x), IComparable).CompareTo(propInfo.GetValue(y))
    End Function
End Class

<Serializable()>
Public Class Language
    Implements IComparable(Of Language)

    <NonSerialized>
    Public IsCommon As Boolean

    Sub New()
        Me.New("")
    End Sub

    Sub New(ci As CultureInfo, Optional isCommon As Boolean = False)
        Me.IsCommon = isCommon
        CultureInfoValue = ci
    End Sub

    Sub New(twoLetterCode As String, Optional isCommon As Boolean = False)
        Try
            Me.IsCommon = isCommon

            Select Case twoLetterCode
                Case "iw"
                    twoLetterCode = "he"
                Case "jp"
                    twoLetterCode = "ja"
            End Select

            CultureInfoValue = New CultureInfo(twoLetterCode)
        Catch ex As Exception
            CultureInfoValue = CultureInfo.InvariantCulture
        End Try
    End Sub

    Private CultureInfoValue As CultureInfo

    ReadOnly Property CultureInfo() As CultureInfo
        Get
            Return CultureInfoValue
        End Get
    End Property

    ReadOnly Property TwoLetterCode() As String
        Get
            Return CultureInfo.TwoLetterISOLanguageName
        End Get
    End Property

    <NonSerialized()> Private ThreeLetterCodeValue As String

    ReadOnly Property ThreeLetterCode() As String
        Get
            If ThreeLetterCodeValue Is Nothing Then
                If CultureInfo.TwoLetterISOLanguageName = "iv" Then
                    ThreeLetterCodeValue = "und"
                Else
                    Select Case CultureInfo.ThreeLetterISOLanguageName
                        Case "deu"
                            ThreeLetterCodeValue = "ger"
                        Case "ces"
                            ThreeLetterCodeValue = "cze"
                        Case "zho"
                            ThreeLetterCodeValue = "chi"
                        Case "nld"
                            ThreeLetterCodeValue = "dut"
                        Case "ell"
                            ThreeLetterCodeValue = "gre"
                        Case "fra"
                            ThreeLetterCodeValue = "fre"
                        Case "sqi"
                            ThreeLetterCodeValue = "alb"
                        Case "hye"
                            ThreeLetterCodeValue = "arm"
                        Case "eus"
                            ThreeLetterCodeValue = "baq"
                        Case "mya"
                            ThreeLetterCodeValue = "bur"
                        Case "kat"
                            ThreeLetterCodeValue = "geo"
                        Case "isl"
                            ThreeLetterCodeValue = "ice"
                        Case "bng"
                            ThreeLetterCodeValue = "ben"
                        Case Else
                            ThreeLetterCodeValue = CultureInfo.ThreeLetterISOLanguageName
                    End Select
                End If
            End If

            Return ThreeLetterCodeValue
        End Get
    End Property

    ReadOnly Property Name() As String
        Get
            If CultureInfo.TwoLetterISOLanguageName = "iv" Then
                Return "Undetermined"
            Else
                Return CultureInfo.EnglishName
            End If
        End Get
    End Property

    Private Shared LanguagesValue As List(Of Language)

    Shared ReadOnly Property Languages() As List(Of Language)
        Get
            If LanguagesValue Is Nothing Then
                Dim l As New List(Of Language)

                l.Add(New Language("en", True))
                l.Add(New Language("es", True))
                l.Add(New Language("de", True))
                l.Add(New Language("fr", True))
                l.Add(New Language("it", True))
                l.Add(New Language("ru", True))
                l.Add(New Language("zh", True))
                l.Add(New Language("hi", True))
                l.Add(New Language("ja", True))
                l.Add(New Language("pt", True))
                l.Add(New Language("ar", True))
                l.Add(New Language("bn", True))
                l.Add(New Language("pa", True))
                l.Add(New Language("ms", True))
                l.Add(New Language("ko", True))

                l.Add(New Language(CultureInfo.InvariantCulture, True))

                Dim current = l.Where(Function(a) a.TwoLetterCode = CultureInfo.CurrentCulture.TwoLetterISOLanguageName).FirstOrDefault
                If current Is Nothing Then l.Add(CurrentCulture)

                l.Sort()

                Dim l2 As New List(Of Language)

                For Each i In CultureInfo.GetCultures(CultureTypes.NeutralCultures)
                    l2.Add(New Language(i))
                Next

                l2.Sort()
                l.AddRange(l2)
                LanguagesValue = l
            End If

            Return LanguagesValue
        End Get
    End Property

    Shared ReadOnly Property CurrentCulture As Language
        Get
            Return New Language(CultureInfo.CurrentCulture.IsNeutralCulture, True)
        End Get
    End Property


    Overrides Function ToString() As String
        Return Name
    End Function

    Function CompareTo(other As Language) As Integer Implements System.IComparable(Of Language).CompareTo
        Return Name.CompareTo(other.Name)
    End Function

    Overrides Function Equals(o As Object) As Boolean
        If TypeOf o Is Language Then
            Return CultureInfo.Equals(DirectCast(o, Language).CultureInfo)
        End If
    End Function
End Class

Public Class StringLogicalComparer
    Implements IComparer, IComparer(Of String)

    <DllImport("shlwapi.dll", CharSet:=CharSet.Unicode)>
    Public Shared Function StrCmpLogical(x As String, y As String) As Integer
    End Function

    Private Function IComparer_Compare(x As Object, y As Object) As Integer Implements IComparer.Compare
        Return StrCmpLogical(x.ToString(), y.ToString())
    End Function

    Private Function IComparerOfString_Compare(x As String, y As String) As Integer Implements IComparer(Of String).Compare
        Return StrCmpLogical(x, y)
    End Function
End Class

