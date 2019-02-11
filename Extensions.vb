Imports System.Globalization
Imports System.Runtime.CompilerServices
Imports System.IO
Imports VB6 = Microsoft.VisualBasic
Imports Microsoft.Win32
Imports System.Text

Module Extensions
    Public Const BR As String = vbCrLf
    Public Const BR2 As String = vbCrLf + vbCrLf

    <Extension()>
    Function Ext(instance As String) As String
        Return FilePath.GetExt(instance)
    End Function

    <Extension()>
    Function ExtFull(instance As String) As String
        Return FilePath.GetExtFull(instance)
    End Function
    <Extension>
    Function IsValidFileName(instance As String) As Boolean
        If instance = "" Then Return False
        Dim chars = """*/:<>?\|"

        For Each i In instance
            If chars.Contains(i) Then Return False
            If Convert.ToInt32(i) < 32 Then Return False
        Next

        Return True
    End Function

    <Extension()>
    Function FixDir(instance As String) As String
        If instance = "" Then Return ""

        While instance.EndsWith(DirPath.Separator + DirPath.Separator)
            instance = instance.Substring(0, instance.Length - 1)
        End While

        If instance.EndsWith(DirPath.Separator) Then Return instance
        Return instance + DirPath.Separator
    End Function
    <Extension()>
    Function Dir(instance As String) As String
        Return FilePath.GetDir(instance)
    End Function

    <Extension()>
    Function DirName(instance As String) As String
        Return DirPath.GetName(instance)
    End Function

    <Extension()>
    Function ChangeExt(instance As String, value As String) As String
        If instance = "" Then Return ""
        If value = "" Then Return instance
        If Not value.StartsWith(".") Then value = "." + value
        Return instance.DirAndBase + value.ToLower
    End Function

    <Extension()>
    Function DirAndBase(instance As String) As String
        Return FilePath.GetDirAndBase(instance)
    End Function

    <Extension()>
    Function Base(instance As String) As String
        Return FilePath.GetBase(instance)
    End Function

    <Extension()>
    Function FileName(instance As String) As String
        If instance = "" Then Return ""
        Dim index = instance.LastIndexOf(Path.DirectorySeparatorChar)
        If index > -1 Then Return instance.Substring(index + 1)
        Return instance
    End Function

    <Extension()>
    Function Upper(instance As String) As String
        If instance = "" Then Return ""
        Return instance.ToUpperInvariant
    End Function

    <Extension()>
    Function Lower(instance As String) As String
        If instance = "" Then Return ""
        Return instance.ToLowerInvariant
    End Function

    <Extension()>
    Function ContainsAll(instance As String, all As IEnumerable(Of String)) As Boolean
        If instance <> "" Then Return all.All(Function(arg) instance.Contains(arg))
    End Function

    <Extension()>
    Function ContainsAny(instance As String, any As IEnumerable(Of String)) As Boolean
        If instance <> "" Then Return any.Any(Function(arg) instance.Contains(arg))
    End Function

    <Extension()>
    Function EqualsAny(instance As String, ParamArray values As String()) As Boolean
        If instance = "" OrElse values.NothingOrEmpty Then Return False
        Return values.Contains(instance)
    End Function

    <Extension()>
    Function FixBreak(value As String) As String
        value = value.Replace(VB6.ChrW(13) + VB6.ChrW(10), VB6.ChrW(10))
        value = value.Replace(VB6.ChrW(13), VB6.ChrW(10))
        Return value.Replace(VB6.ChrW(10), VB6.ChrW(13) + VB6.ChrW(10))
    End Function

    <Extension()>
    Function ToTitleCase(value As String) As String
        'TextInfo.ToTitleCase won't work on all upper strings
        Return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower)
    End Function

    <Extension()>
    Function IsInt(value As String) As Boolean
        Return Integer.TryParse(value, Nothing)
    End Function

    <Extension()>
    Function ToInt(value As String, Optional defaultValue As Integer = 0) As Integer
        If Not Integer.TryParse(value, Nothing) Then Return defaultValue
        Return CInt(value)
    End Function

    <Extension()>
    Function IsSingle(value As String) As Boolean
        If value <> "" Then
            If value.Contains(",") Then value = value.Replace(",", ".")

            Return Single.TryParse(value,
                                   NumberStyles.Float Or NumberStyles.AllowThousands,
                                   CultureInfo.InvariantCulture,
                                   Nothing)
        End If
    End Function

    <Extension()>
    Function ToSingle(value As String, Optional defaultValue As Single = 0) As Single
        If value <> "" Then
            If value.Contains(",") Then value = value.Replace(",", ".")

            Dim ret As Single

            If Single.TryParse(value,
                               NumberStyles.Float Or NumberStyles.AllowThousands,
                               CultureInfo.InvariantCulture,
                               ret) Then
                Return ret
            End If
        End If

        Return defaultValue
    End Function

    <Extension()>
    Function IsDouble(value As String) As Boolean
        If value <> "" Then
            If value.Contains(",") Then value = value.Replace(",", ".")

            Return Double.TryParse(value,
                                   NumberStyles.Float Or NumberStyles.AllowThousands,
                                   CultureInfo.InvariantCulture,
                                   Nothing)
        End If
    End Function

    <Extension()>
    Function ToDouble(value As String, Optional defaultValue As Single = 0) As Double
        If value <> "" Then
            If value.Contains(",") Then value = value.Replace(",", ".")

            Dim ret As Double

            If Double.TryParse(value,
                               NumberStyles.Float Or NumberStyles.AllowThousands,
                               CultureInfo.InvariantCulture,
                               ret) Then
                Return ret
            End If
        End If

        Return defaultValue
    End Function

    <Extension()>
    Function Shorten(value As String, maxLength As Integer) As String
        If value = "" OrElse value.Length <= maxLength Then
            Return value
        End If

        Return value.Substring(0, maxLength)
    End Function

    <Extension()>
    Function SplitNoEmpty(value As String, ParamArray delimiters As String()) As String()
        Return value.Split(delimiters, StringSplitOptions.RemoveEmptyEntries)
    End Function

    <Extension()>
    Function SplitKeepEmpty(value As String, ParamArray delimiters As String()) As String()
        Return value.Split(delimiters, StringSplitOptions.None)
    End Function

    <Extension()>
    Function SplitNoEmptyAndWhiteSpace(value As String, ParamArray delimiters As String()) As String()
        If value = "" Then Return {}

        Dim a = SplitNoEmpty(value, delimiters)

        For i = 0 To a.Length - 1
            a(i) = a(i).Trim
        Next

        Dim l = a.ToList

        While l.Contains("")
            l.Remove("")
        End While

        Return l.ToArray
    End Function

    <Extension()>
    Function SplitLinesNoEmpty(value As String) As String()
        Return SplitNoEmpty(value, Environment.NewLine)
    End Function

    <Extension()>
    Function DeleteRight(value As String, count As Integer) As String
        Return Left(value, value.Length - count)
    End Function

    <Extension()>
    Function Sort(Of T)(instance As IEnumerable(Of T)) As IEnumerable(Of T)
        Dim ret = instance.ToArray
        Array.Sort(Of T)(ret)
        Return ret
    End Function

    <Extension()>
    Function NothingOrEmpty(strings As IEnumerable(Of String)) As Boolean
        If strings Is Nothing OrElse strings.Count = 0 Then Return True

        For Each i In strings
            If i = "" Then Return True
        Next
    End Function

    '<Extension()>
    'Sub SetFontStyle(instance As Control, style As FontStyle)
    '    instance.Font = New Font(instance.Font.FontFamily, instance.Font.Size, style)
    'End Sub
    <Extension()>
    Function Join(instance As IEnumerable(Of String),
                  delimiter As String,
                  Optional removeEmpty As Boolean = False) As String

        If instance Is Nothing Then Return Nothing
        Dim containsEmpty As Boolean

        For Each item In instance
            If item = "" Then
                containsEmpty = True
                Exit For
            End If
        Next

        If containsEmpty AndAlso removeEmpty Then instance = instance.Where(Function(arg) arg <> "")
        Return String.Join(delimiter, instance)
    End Function

    <Extension()>
    Function FormatColumn(value As String, delimiter As String) As String
        If value = "" Then Return ""
        Dim lines = value.SplitKeepEmpty(BR)
        Dim leftSides As New List(Of String)

        For Each i In lines
            Dim pos = i.IndexOf(delimiter)

            If pos > 0 Then
                leftSides.Add(i.Substring(0, pos).Trim)
            Else
                leftSides.Add(i)
            End If
        Next

        Dim highest = Aggregate i In leftSides Into Max(i.Length)
        Dim ret As New List(Of String)

        For i = 0 To lines.Length - 1
            Dim line = lines(i)

            If line.Contains(delimiter) Then
                ret.Add(leftSides(i).PadRight(highest) + " " + delimiter + " " + line.Substring(line.IndexOf(delimiter) + 1).Trim)
            Else
                ret.Add(leftSides(i))
            End If
        Next

        Return ret.Join(BR)
    End Function

    <Extension()>
    Sub WriteFile(value As String, path As String, encoding As Encoding)
        Try
            File.WriteAllText(path, value, encoding)
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    <Extension()>
    Function ToInvariantString(instance As Double, format As String) As String
        Dim ret = instance.ToString(format, CultureInfo.InvariantCulture)

        If (ret.Contains(".") OrElse ret.Contains(",")) AndAlso ret.EndsWith("0") Then
            ret = ret.TrimEnd("0"c)
        End If

        Return ret
    End Function

    <Extension()>
    Function ToInvariantString(instance As IConvertible) As String
        If Not instance Is Nothing Then Return instance.ToString(CultureInfo.InvariantCulture)
    End Function

    <Extension>
    Function IsANSICompatible(instance As String) As Boolean
        If instance = "" Then Return True
        Dim bytes = Encoding.Convert(Encoding.Unicode, Encoding.Default, Encoding.Unicode.GetBytes(instance))
        Return instance = Encoding.Unicode.GetString(Encoding.Convert(Encoding.Default, Encoding.Unicode, bytes))
    End Function

    '<Extension()>
    'Sub SetSelectedPath(d As FolderBrowserDialog, path As String)
    '    If Not Directory.Exists(path) Then path = path.ExistingParent
    '    If Directory.Exists(path) Then d.SelectedPath = path
    'End Sub

    <Extension()>
    Sub WriteANSIFile(instance As String, path As String)
        WriteFile(instance, path, Encoding.Default)
    End Sub

    <Extension()>
    Function EqualIgnoreCase(a As String, b As String) As Boolean
        If a = "" OrElse b = "" Then Return False
        Return String.Compare(a, b, StringComparison.OrdinalIgnoreCase) = 0
    End Function

    <Extension()>
    Function LeftLast(value As String, start As String) As String
        If Not value.Contains(start) Then Return ""
        Return value.Substring(0, value.LastIndexOf(start))
    End Function

    <Extension()>
    Function RightLast(value As String, start As String) As String
        If value = "" OrElse start = "" Then Return ""
        If Not value.Contains(start) Then Return ""
        Return value.Substring(value.LastIndexOf(start) + start.Length)
    End Function

End Module

Module RegistryKeyExtensions
    Private Function GetValue(Of T)(rootKey As RegistryKey, key As String, name As String) As T
        Using k = rootKey.OpenSubKey(key)
            If Not k Is Nothing Then
                Dim r = k.GetValue(name)

                If Not r Is Nothing Then
                    Try
                        Return CType(r, T)
                    Catch ex As Exception
                    End Try
                End If
            End If
        End Using
    End Function

    <Extension()>
    Function GetString(rootKey As RegistryKey, subKey As String, name As String) As String
        Return GetValue(Of String)(rootKey, subKey, name)
    End Function

    <Extension()>
    Function GetInt(rootKey As RegistryKey, subKey As String, name As String) As Integer
        Return GetValue(Of Integer)(rootKey, subKey, name)
    End Function

    <Extension()>
    Function GetBoolean(rootKey As RegistryKey, subKey As String, name As String) As Boolean
        Return GetValue(Of Boolean)(rootKey, subKey, name)
    End Function

    <Extension()>
    Function GetValueNames(rootKey As RegistryKey, subKeyName As String) As IEnumerable(Of String)
        Using k = rootKey.OpenSubKey(subKeyName)
            If Not k Is Nothing Then
                Return k.GetValueNames
            End If
        End Using

        Return {}
    End Function

    <Extension()>
    Sub GetSubKeys(rootKey As RegistryKey, keys As List(Of RegistryKey))
        If Not rootKey Is Nothing Then
            keys.Add(rootKey)

            For Each i In rootKey.GetSubKeyNames
                GetSubKeys(rootKey.OpenSubKey(i), keys)
            Next
        End If
    End Sub

    <Extension()>
    Sub Write(rootKey As RegistryKey, subKey As String, valueName As String, valueValue As Object)
        Dim k = rootKey.OpenSubKey(subKey, True)

        If k Is Nothing Then
            k = rootKey.CreateSubKey(subKey, RegistryKeyPermissionCheck.ReadWriteSubTree)
        End If

        k.SetValue(valueName, valueValue)
        k.Close()
    End Sub

    <Extension()>
    Sub DeleteValue(rootKey As RegistryKey, key As String, valueName As String)
        Using k = rootKey.OpenSubKey(key, True)
            If Not k Is Nothing Then
                k.DeleteValue(valueName, False)
            End If
        End Using
    End Sub
End Module

Public Class FileTypes
    Shared Property AudioRaw As String() = {"thd", "aac", "eac3"}
    Shared Property Audio As String() = {"flac", "dtshd", "dtsma", "dtshr", "thd", "thd+ac3", "true-hd", "truehd", "aac", "ac3", "dts", "eac3", "m4a", "mka", "mp2", "mp3", "mpa", "opus", "wav", "w64"}
    Shared Property VideoAudio As String() = {"avi", "mp4", "mkv", "divx", "flv", "mov", "mpeg", "mpg", "ts", "m2ts", "vob", "webm", "wmv", "pva", "ogg", "ogm", "m4v", "3gp"}
    Shared Property DGDecNVInput As String() = {"264", "h264", "265", "h265", "avc", "hevc", "hvc", "mkv", "mp4", "m4v", "mpg", "vob", "ts", "m2ts", "mts", "m2t", "mpv", "m2v"}
    Shared Property eac3toInput As String() = {"dts", "dtshd", "dtshr", "dtsma", "evo", "vob", "ts", "m2ts", "wav", "w64", "pcm", "raw", "flac", "ac3", "eac3", "thd", "thd+ac3", "mlp", "mp2", "mp3", "mpa"}
    Shared Property NicAudioInput As String() = {"wav", "mp2", "mpa", "mp3", "ac3", "dts"}
    Shared Property qaacInput As String() = {"wav", "flac", "w64"}
    Shared Property SubtitleExludingContainers As String() = {"srt", "ass", "idx", "sup", "ttxt", "ssa", "smi"}
    Shared Property SubtitleSingle As String() = {"srt", "ass", "sup", "ttxt", "ssa", "smi"}
    Shared Property SubtitleIncludingContainers As String() = {"m2ts", "mkv", "mp4", "m4v", "ass", "idx", "smi", "srt", "ssa", "sup", "ttxt"}
    Shared Property TextSub As String() = {"ass", "idx", "smi", "srt", "ssa", "ttxt", "usf", "ssf", "psb", "sub"}
    Shared Property Video As String() = {"264", "265", "avc", "avi", "avs", "d2v", "dgi", "dgim", "divx", "flv", "h264", "h265", "hevc", "hvc", "ivf", "m2t", "m2ts", "m2v", "mkv", "mov", "mp4", "m4v", "mpeg", "mpg", "mpv", "mts", "ogg", "ogm", "pva", "rmvb", "ts", "vdr", "vob", "vpy", "webm", "wmv", "y4m", "3gp"}
    Shared Property VideoIndex As String() = {"d2v", "dgi", "dga", "dgim"}
    Shared Property VideoOnly As String() = {"264", "265", "avc", "gif", "h264", "h265", "hevc", "hvc", "ivf", "m2v", "mpv", "apng", "png", "y4m"}
    Shared Property VideoRaw As String() = {"264", "265", "h264", "h265", "avc", "hevc", "hvc", "ivf"}
    Shared Property VideoText As String() = {"d2v", "dgi", "dga", "dgim", "avs", "vpy"}
    Shared Property VideoDemuxOutput As String() = {"mpg", "h264", "avi", "h265"}
End Class