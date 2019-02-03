Imports System.IO
Imports System.Text.RegularExpressions


Public Class Package
    Implements IComparable(Of Package)
    Property Description As String
    Property DirPath As String
    Property FileNotFoundMessage As String
    Property HelpURLFunc As Func(Of ScriptEngine, String)
    Property HintDirFunc As Func(Of String)
    Property IgnoreVersion As Boolean
    Property IsRequiredFunc As Func(Of Boolean)
    Property Name As String
    Property StatusFunc As Func(Of String)
    Property TreePath As String
    Property Version As String
    Property VersionDate As Date
    Property Filename As String
    Overridable Property FixedDir As String

    Shared Property Items As New SortedDictionary(Of String, Package)
    Shared Property AviSynth As Package = Add(New AviSynthPlusPackage)

    Shared Property MediaInfo As Package = Add(New Package With {
        .Name = "MediaInfo",
        .DirPath = Folders.Startup + "Apps\Plugins\MediaInfo\MediaInfo.dll",
        .Filename = "MediaInfo.dll",
        .Description = "MediaInfo is used by StaxRip to read infos from media files."})

    Shared Property HTMLAgility As Package = Add(New Package With {
        .Name = "HtmlAgilityPack",
        .DirPath = Folders.Startup + "Apps\Plugins\HtmlAgilityPack\HtmlAgilityPack.dll",
        .Filename = "HtmlAgilityPack.dll",
        .Description = "Dll File Used to Search for Updates."})

    Shared Property ffms2 As Package = Add(New PluginPackage With {
        .Name = "ffms2",
        .Filename = "ffms2.dll",
        .DirPath = Folders.Startup + "Apps\Plugins\FFMS2\ffms2.dll",
        .Description = "AviSynth+ and VapourSynth source filter supporting various input formats.",
        .AvsFilterNames = {"FFVideoSource", "FFAudioSource"},
        .AvsFiltersFunc = Function() {New VideoFilter("Source", "FFVideoSource", $"FFVideoSource(""%source_file%"", colorspace = ""YV12"", \{BR}              cachefile = ""%source_temp_file%.ffindex"")")}})

    Shared Property LSmash As Package = Add(New PluginPackage With {
        .Name = "L-SMASH-Works",
        .Filename = "LSMASHSource.dll",
        .DirPath = Folders.Startup + "Apps\Plugins\L-Smash\LSMASHSource.dll",
        .Description = "AviSynth and VapourSynth source filter based on Libav supporting a wide range of input formats.",
        .AvsFilterNames = {"LSMASHVideoSource", "LSMASHAudioSource", "LWLibavVideoSource", "LWLibavAudioSource"},
        .AvsFiltersFunc = Function() {
            New VideoFilter("Source", "LSMASHVideoSource", "LSMASHVideoSource(""%source_file%"", format = ""YUV420P8"")"),
            New VideoFilter("Source", "LWLibavVideoSource", "LWLibavVideoSource(""%source_file%"", format = ""YUV420P8"")")}})


    Shared Function Add(pack As Package) As Package
        Items(pack.ID) = pack
        Return pack
    End Function

    Function GetStatusLocation() As String
        Dim pathVar = Path

        If pathVar = "" Then
            If FileNotFoundMessage <> "" Then
                Return "File Not Found"
            End If

            Return "File Not Found"
        End If
    End Function

    Function IsStatusCritical() As Boolean
        Return GetStatusLocation() <> "" OrElse GetStatus() <> ""
    End Function

    Function VerifyOK(Optional showEvenIfNotRequired As Boolean = False) As Boolean
        If IsStatusCritical() Then Return False

        Return True
    End Function
    ReadOnly Property ID As String
        Get
            If TypeOf Me Is PluginPackage Then
                Dim plugin = DirectCast(Me, PluginPackage)

                If Not plugin.AvsFilterNames.NothingOrEmpty AndAlso
                    Not plugin.VSFilterNames.NothingOrEmpty Then

                    Return Name + " avs+vs"
                ElseIf Not plugin.AvsFilterNames.NothingOrEmpty Then
                    Return Name + " avs"
                ElseIf Not plugin.VSFilterNames.NothingOrEmpty Then
                    Return Name + " vs"
                End If
            End If

            Return Name
        End Get
    End Property

    Overridable Function GetStatus() As String
        If IsOutdated() Then Return "Unsupported Version"
        If Not StatusFunc Is Nothing Then Return StatusFunc.Invoke
    End Function

    Function IsOutdated() As Boolean
        Dim fp = Path

        If fp <> "" AndAlso Not IgnoreVersion Then
            If (VersionDate - File.GetLastWriteTimeUtc(fp)).TotalDays > 3 Then Return True
        End If
    End Function

    Overridable Function IsCorrectVersion() As Boolean
        Dim fp = Path

        If fp <> "" Then
            Dim dt = File.GetLastWriteTimeUtc(fp)
            Return dt.AddDays(-2) < VersionDate AndAlso dt.AddDays(2) > VersionDate
        End If
    End Function

    Function GetDir() As String
        Return Path.Dir
    End Function

    Function GetStoredPath() As String
        Dim ret As String

        ret = Folders.Apps
        If ret <> "" Then
            If File.Exists(ret) Then
                Return ret
            Else
                MsgBox("File Not Found")
            End If
        End If

        Return ret
    End Function

    Overridable ReadOnly Property Path As String
        Get
            Dim ret = GetStoredPath()
            If File.Exists(ret) Then Return ret

            If FixedDir <> "" Then
                If File.Exists(FixedDir + Filename) Then Return FixedDir + Filename
                Return Nothing
            End If

            If DirPath <> "" AndAlso File.Exists(Folders.Apps + DirPath + "\" + Filename) Then
                Return Folders.Apps + DirPath + "\" + Filename
            End If

            If Not HintDirFunc Is Nothing Then
                If File.Exists(HintDirFunc.Invoke + Filename) Then Return HintDirFunc.Invoke + Filename
            End If

            Dim plugin = TryCast(Me, PluginPackage)

            If Not plugin Is Nothing Then
                If Not plugin.VSFilterNames Is Nothing AndAlso Not plugin.AvsFilterNames Is Nothing Then
                    ret = Folders.Apps + "Plugins\both\" + Name + "\" + Filename
                    If File.Exists(ret) Then Return ret
                Else
                    If plugin.VSFilterNames Is Nothing Then
                        ret = Folders.Apps + "Plugins\avs\" + Name + "\" + Filename
                        If File.Exists(ret) Then Return ret
                    Else
                        ret = Folders.Apps + "Plugins\vs\" + Name + "\" + Filename
                        If File.Exists(ret) Then Return ret
                    End If
                End If
            End If

            ret = Folders.Apps + Name + "\" + Filename
            If File.Exists(ret) Then Return ret
        End Get
    End Property

    Overrides Function ToString() As String
        Return Name
    End Function

    Function CompareTo(other As Package) As Integer Implements IComparable(Of Package).CompareTo
        Return Name.CompareTo(other.Name)
    End Function

End Class

Public Class AviSynthPlusPackage
    Inherits Package

    Sub New()
        Name = "AviSynth+"
        Filename = "avisynth.dll"
        Description = "StaxRip support both AviSynth+ x64 and VapourSynth x64 as scripting based video processing tool."
        FixedDir = Folders.Apps + "\Plugins\Avisynth\AviSynth.dll"
    End Sub

End Class

Public Class PluginPackage
    Inherits Package

    Property AvsFilterNames As String()
    Property VSFilterNames As String()
    Property VSFiltersFunc As Func(Of VideoFilter())
    Property AvsFiltersFunc As Func(Of VideoFilter())


    Shared Function IsPluginPackageRequired(package As PluginPackage) As Boolean
        If p Is Nothing Then Return False

        If p.Script.Engine = ScriptEngine.AviSynth AndAlso
            Not package.AvsFilterNames.NothingOrEmpty Then

            Dim fullScriptLower = p.Script.GetFullScript().ToLowerInvariant

            For Each filterName In package.AvsFilterNames
                If fullScriptLower.Contains(filterName.ToLowerInvariant) Then Return True

                If fullScriptLower.Contains("import") Then
                    Dim match = Regex.Match(fullScriptLower, "\bimport\s*\(\s*""\s*(.+\.avsi*)\s*""\s*\)",
                                            RegexOptions.IgnoreCase)

                    If match.Success AndAlso File.Exists(match.Groups(1).Value) Then
                        If File.ReadAllText(match.Groups(1).Value).ToLowerInvariant.Contains(
                                filterName.ToLowerInvariant) Then

                            Return True
                        End If
                    End If
                End If
            Next
        End If
    End Function
End Class
