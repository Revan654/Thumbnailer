Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.ComponentModel
Imports System.Globalization
Imports System.Reflection
Imports System.Runtime.Serialization.Formatters.Binary


<Serializable()>
Public Class VideoScript
    Inherits Profile

    <NonSerialized()> Private Framerate As Double
    <NonSerialized()> Private Frames As Integer
    <NonSerialized()> Private Size = Size
    <NonSerialized()> Private ErrorMessage As String

    Property Filters As New List(Of VideoFilter)

    Overridable Property Engine As ScriptEngine = ScriptEngine.AviSynth
    Overridable Property Path As String = ""

    Shared Event Changed(script As VideoScript)

    Sub RaiseChanged()
        RaiseEvent Changed(Me)
    End Sub

    Overridable ReadOnly Property FileType As String
        Get
            If Engine = ScriptEngine.VapourSynth Then Return "vpy"
            Return "avs"
        End Get
    End Property

    Overridable Function GetScript() As String
        Return GetScript(Nothing)
    End Function

    Overridable Function GetScript(skipCategory As String) As String
        Dim sb As New StringBuilder()
        If p.CodeAtTop <> "" Then sb.AppendLine(p.CodeAtTop)

        For Each filter As VideoFilter In Filters
            If filter.Active Then
                If skipCategory Is Nothing OrElse filter.Category <> skipCategory Then
                    sb.Append(filter.Script + BR)
                End If
            End If
        Next

        Return sb.ToString
    End Function

    Function GetFullScript() As String
        Return ModifyScript(GetScript, Engine).Trim
    End Function

    Sub RemoveFilter(category As String, Optional name As String = Nothing)
        For Each i In Filters.ToArray
            If i.Category = category AndAlso (name = "" OrElse i.Path = name) Then
                Filters.Remove(i)
                RaiseChanged()
            End If
        Next
    End Sub

    Sub RemoveFilterAt(index As Integer)
        If Filters.Count > 0 AndAlso index < Filters.Count Then
            Filters.RemoveAt(index)
            RaiseChanged()
        End If
    End Sub

    Sub RemoveFilter(filter As VideoFilter)
        If Filters.Contains(filter) Then
            Filters.Remove(filter)
            RaiseChanged()
        End If
    End Sub

    Sub InsertFilter(index As Integer, filter As VideoFilter)
        Filters.Insert(index, filter)
        RaiseChanged()
    End Sub

    Sub AddFilter(filter As VideoFilter)
        Filters.Add(filter)
        RaiseChanged()
    End Sub

    Sub SetFilter(index As Integer, filter As VideoFilter)
        Filters(index) = filter
        RaiseChanged()
    End Sub

    Sub SetFilter(category As String, name As String, script As String)
        For Each i In Filters
            If i.Category = category Then
                i.Path = name
                i.Script = script
                i.Active = True
                RaiseChanged()
                Exit Sub
            End If
        Next

        If Filters.Count > 0 Then
            Filters.Insert(1, New VideoFilter(category, name, script))
            RaiseChanged()
        End If
    End Sub

    Sub InsertAfter(category As String, af As VideoFilter)
        Dim f = GetFilter(category)
        Filters.Insert(Filters.IndexOf(f) + 1, af)
        RaiseChanged()
    End Sub

    Function Contains(category As String, search As String) As Boolean
        If category = "" OrElse search = "" Then Return False
        Dim filter = GetFilter(category)
        If filter?.Script?.ToLower.Contains(search.ToLower) AndAlso filter?.Active Then Return True
    End Function

    Sub ActivateFilter(category As String)
        Dim filter = GetFilter(category)
        If Not filter Is Nothing Then filter.Active = True
    End Sub

    Function IsFilterActive(category As String) As Boolean
        Dim filter = GetFilter(category)
        Return Not filter Is Nothing AndAlso filter.Active
    End Function

    Function IsFilterActive(category As String, name As String) As Boolean
        Dim filter = GetFilter(category)
        Return Not filter Is Nothing AndAlso filter.Active AndAlso filter.Name = name
    End Function

    Function GetFiltersCopy() As List(Of VideoFilter)
        Dim ret = New List(Of VideoFilter)

        For Each i In Filters
            ret.Add(i.GetCopy)
        Next

        Return ret
    End Function

    Function GetFilter(category As String) As VideoFilter
        For Each i In Filters
            If i.Category = category Then Return i
        Next
    End Function

    <NonSerialized()> Public LastCode As String
    <NonSerialized()> Public LastPath As String

    Sub Synchronize(Optional convertToRGB As Boolean = False,
                    Optional comparePath As Boolean = True)

        If Path <> "" Then
            Dim code = GetScript()

            If convertToRGB Then
                If Engine = ScriptEngine.AviSynth Then
                    If p.SourceHeight > 576 Then
                        code += BR + "ConvertBits(8)" + BR + "ConvertToRGB(matrix=""Rec709"")"
                    Else
                        code += BR + "ConvertBits(8)" + BR + "ConvertToRGB(matrix=""Rec601"")"
                    End If
                Else
                    Dim vsCode = "
if clip.format.id == vs.RGB24:
    _matrix_in_s = 'rgb'
else:
    if clip.height > 576:
        _matrix_in_s = '709'
    else:
        _matrix_in_s = '470bg'
clip = clip.resize.Bicubic(matrix_in_s = _matrix_in_s, format = vs.COMPATBGR32)
clip.set_output()
"
                    code += BR + vsCode
                End If
            End If

            If Frames = 240 OrElse code <> LastCode OrElse (comparePath AndAlso Path <> LastPath) Then
                If Directory.Exists(FilePath.GetDir(Path)) Then
                    If Engine = ScriptEngine.VapourSynth Then
                        ModifyScript(code, Engine).WriteFile(Path, Encoding.UTF8)
                    Else
                        ModifyScript(code, Engine).WriteANSIFile(Path)
                    End If



                    Using avi As New AVIFile(Path)
                        Framerate = avi.FrameRate
                        Frames = avi.FrameCount
                        Size = avi.FrameSize
                        ErrorMessage = avi.ErrorMessage

                        If Double.IsNaN(Framerate) Then
                        End If
                    End Using

                    LastCode = code
                    LastPath = Path
                End If
            End If
        End If
    End Sub

    Shared Function ModifyScript(script As String, engine As ScriptEngine) As String
        Return ModifyAVSScript(script)
    End Function


    Shared Function GetAVSLoadCode(script As String, scriptAlready As String) As String
        Dim loadCode = ""
        Dim plugins = Package.Items.Values.OfType(Of PluginPackage)()

        For Each plugin In plugins
            Dim fp = plugin.DirPath

            If fp <> "" Then
                If Not plugin.AvsFilterNames Is Nothing Then
                    For Each filterName In plugin.AvsFilterNames
                        If script.Contains(filterName.ToLower) Then
                            If plugin.Name = "ffms2" And plugin.Name <> "ffms2k" Then
                                Dim loadC = "LoadCPlugin(""" + fp + """)" + BR

                                If Not script.Contains(loadC.ToLower) AndAlso
                                    Not loadCode.Contains(loadC) AndAlso
                                    Not scriptAlready.Contains(loadC.ToLower) Then

                                    loadCode += loadC
                                End If

                            ElseIf plugin.Filename.Ext = "dll" Then
                                Dim load = "LoadPlugin(""" + fp + """)" + BR

                                If File.Exists(Folder.Plugins + fp.FileName) AndAlso
                                        File.GetLastWriteTimeUtc(Folder.Plugins + fp.FileName) <
                                        File.GetLastWriteTimeUtc(fp) Then

                                End If

                                If Not script.Contains(load.ToLower) AndAlso
                                    Not loadCode.Contains(load) AndAlso
                                    Not scriptAlready.Contains(load.ToLower) Then

                                    loadCode += load
                                End If

                            ElseIf plugin.Filename.Ext = "avsi" Then
                                Dim avsiImport = "Import(""" + fp + """)" + BR

                                If Not script.Contains(avsiImport.ToLower) AndAlso
                                    Not loadCode.Contains(avsiImport) AndAlso
                                    Not scriptAlready.Contains(avsiImport.ToLower) Then

                                    loadCode += avsiImport
                                End If
                            End If
                        End If
                    Next
                End If
            End If
        Next
        Return loadCode
    End Function

    Shared Function GetAVSLoadCodeFromImports(code As String) As String
        Dim ret = ""

        For Each line In code.SplitLinesNoEmpty
            If line.Contains("import") Then
                Dim match = Regex.Match(line, "\bimport\s*\(\s*""\s*(.+\.avsi*)\s*""\s*\)", RegexOptions.IgnoreCase)

                If match.Success AndAlso File.Exists(match.Groups(1).Value) Then
                    ret += GetAVSLoadCode(File.ReadAllText(match.Groups(1).Value).ToLowerInvariant, code)
                End If
            End If
        Next

        Return ret
    End Function

    Shared Function ModifyAVSScript(script As String) As String
        Dim clip As String
        Dim lowerScript = script.ToLower
        Dim loadCode = GetAVSLoadCode(lowerScript, "")
        clip = loadCode + script
        clip = GetAVSLoadCodeFromImports(clip.ToLowerInvariant) + clip
        Return clip
    End Function

    Function GetFramerate() As Double
        Synchronize(False, False)
        Return Framerate
    End Function

    Function GetErrorMessage() As String
        Synchronize(False, False)
        Return ErrorMessage
    End Function

    Function GetSeconds() As Integer
        Dim fr = GetFramerate()
        If fr = 0 Then fr = p.SourceFrameRate
        If fr = 0 Then fr = 25
        Return CInt(GetFrames() / fr)
    End Function

    Function GetFrames() As Integer
        Synchronize()
        Return Frames
    End Function

    Function GetSize() As Size
        Synchronize()
        Return Size
    End Function

    Shared Function GetDefaults() As List(Of TargetVideoScript)
        Dim ret As New List(Of TargetVideoScript)

        Dim script As New TargetVideoScript("AviSynth")
        script.Engine = ScriptEngine.AviSynth
        script.Filters.Add(New VideoFilter("Source", "Automatic", "# can be configured at: Tools > Settings > Source Filters"))
        ret.Add(script)

        script = New TargetVideoScript("VapourSynth")
        script.Engine = ScriptEngine.VapourSynth
        script.Filters.Add(New VideoFilter("Source", "Automatic", "# can be configured at: Tools > Settings > Source Filters"))
        ret.Add(script)

        Return ret
    End Function

    Private Function GetDocument() As VideoScript
        Return Me
    End Function
End Class

<Serializable()>
Public Class TargetVideoScript
    Inherits VideoScript

    Sub New(name As String)
        Me.Name = name
        CanEditValue = True
    End Sub

    Overrides Property Path() As String
        Get
            If p.SourceFile = "" Then Return ""
            Return p.TempDir + "." + FileType
        End Get
        Set(value As String)
        End Set
    End Property
End Class

<Serializable()>
Public Class SourceVideoScript
    Inherits VideoScript

    Overrides Property Path() As String
        Get
            If p.SourceFile = "" Then Return ""
            Return p.TempDir + "_source." + p.Script.FileType
        End Get
        Set(value As String)
        End Set
    End Property

    Public Overrides Property Engine As ScriptEngine
        Get
            Return p.Script.Engine
        End Get
        Set(value As ScriptEngine)
        End Set
    End Property

    Overrides Function GetScript() As String
        Return p.Script.Filters(0).Script
    End Function
End Class

<Serializable()>
Public Class VideoFilter
    Implements IComparable(Of VideoFilter)

    Property Active As Boolean
    Property Category As String
    Property Path As String
    Property Script As String

    Sub New()
        Me.New("???", "???", "???", True)
    End Sub

    Sub New(code As String)
        Me.New("???", "???", code, True)
    End Sub

    Sub New(category As String,
            name As String,
            script As String,
            Optional active As Boolean = True)

        Me.Path = name
        Me.Script = script
        Me.Category = category
        Me.Active = active
    End Sub

    ReadOnly Property Name As String
        Get
            If Path.Contains("|") Then Return Path.RightLast("|").Trim
            Return Path
        End Get
    End Property

    Function GetCopy() As VideoFilter
        Return New VideoFilter(Category, Path, Script, Active)
    End Function

    Overrides Function ToString() As String
        Return Path
    End Function

    Function CompareTo(other As VideoFilter) As Integer Implements System.IComparable(Of VideoFilter).CompareTo
        Return Path.CompareTo(other.Path)
    End Function

    Shared Function GetDefault(category As String, name As String) As VideoFilter
        Return FilterCategory.GetAviSynthDefaults.First(Function(val) val.Name = category).Filters.First(Function(val) val.Name = name)
    End Function
End Class

<Serializable()>
Public Class FilterCategory
    Sub New(name As String)
        Me.Name = name
    End Sub

    Property Name As String

    Private FitersValue As New List(Of VideoFilter)

    ReadOnly Property Filters() As List(Of VideoFilter)
        Get
            If FitersValue Is Nothing Then FitersValue = New List(Of VideoFilter)
            Return FitersValue
        End Get
    End Property

    Overrides Function ToString() As String
        Return Name
    End Function

    Shared Sub AddFilter(filter As VideoFilter, list As List(Of FilterCategory))
        Dim matchingCategory = list.Where(Function(category) category.Name = filter.Category).FirstOrDefault

        If matchingCategory Is Nothing Then
            Dim newCategory As New FilterCategory(filter.Category)
            newCategory.Filters.Add(filter)
            list.Add(newCategory)
        Else
            matchingCategory.Filters.Add(filter)
        End If
    End Sub

    Shared Sub AddDefaults(engine As ScriptEngine, list As List(Of FilterCategory))
        For Each i In Package.Items.Values.OfType(Of PluginPackage)
            Dim filters As VideoFilter() = Nothing

            If engine = ScriptEngine.AviSynth Then
                If Not i.AvsFiltersFunc Is Nothing Then filters = i.AvsFiltersFunc.Invoke
            Else
                If Not i.VSFiltersFunc Is Nothing Then filters = i.VSFiltersFunc.Invoke
            End If

            If Not filters Is Nothing Then
                For Each iFilter In filters
                    Dim matchingCategory = list.Where(Function(category) category.Name = iFilter.Category).FirstOrDefault

                    If matchingCategory Is Nothing Then
                        Dim newCategory As New FilterCategory(iFilter.Category)
                        newCategory.Filters.Add(iFilter)
                        list.Add(newCategory)
                    Else
                        matchingCategory.Filters.Add(iFilter)
                    End If
                Next
            End If
        Next
    End Sub

    Shared Function GetAviSynthDefaults() As List(Of FilterCategory)
        Dim ret As New List(Of FilterCategory)

        Dim src As New FilterCategory("Source")
        src.Filters.AddRange(
            {New VideoFilter("Source", "Manual", "# shows the filter selection dialog"),
             New VideoFilter("Source", "Automatic", "# can be configured at: Tools > Settings > Source Filters"),
             New VideoFilter("Source", "AviSource", "AviSource(""%source_file%"", Audio = False)"),
             New VideoFilter("Source", "DirectShowSource", "DirectShowSource(""%source_file%"", audio = False)")})
        ret.Add(src)

        FilterCategory.AddDefaults(ScriptEngine.AviSynth, ret)

        For Each i In ret
            i.Filters.Sort()
        Next

        Return ret
    End Function

    Shared Function GetVapourSynthDefaults() As List(Of FilterCategory)
        Dim ret As New List(Of FilterCategory)

        Dim src As New FilterCategory("Source")
        src.Filters.AddRange(
            {New VideoFilter("Source", "Manual", "# shows filter selection dialog"),
             New VideoFilter("Source", "Automatic", "# can be configured at: Tools > Settings > Source Filters"),
             New VideoFilter("Source", "AVISource", "clip = core.avisource.AVISource(r""%source_file%"")")})
        ret.Add(src)

        FilterCategory.AddDefaults(ScriptEngine.VapourSynth, ret)

        For Each i In ret
            i.Filters.Sort()
        Next

        Return ret
    End Function
End Class

Public Class FilterParameter
    Property Name As String
    Property Value As String

    Sub New(name As String, value As String)
        Me.Name = name
        Me.Value = value
    End Sub
End Class

Public Class FilterParameters
    Property FunctionName As String
    Property Text As String

    Property Parameters As New List(Of FilterParameter)

    Shared DefinitionsValue As List(Of FilterParameters)

    Shared ReadOnly Property Definitions As List(Of FilterParameters)
        Get
            If DefinitionsValue Is Nothing Then
                DefinitionsValue = New List(Of FilterParameters)

                Dim add = Sub(func As String(),
                              path As String,
                              params As FilterParameter())

                              For Each i In func
                                  Dim ret As New FilterParameters
                                  ret.FunctionName = i
                                  ret.Text = path
                                  DefinitionsValue.Add(ret)
                                  ret.Parameters.AddRange(params)
                              Next
                          End Sub

                Dim add2 = Sub(func As String(),
                               param As String,
                               value As String,
                               path As String)

                               For Each i In func
                                   Dim ret As New FilterParameters
                                   ret.FunctionName = i
                                   ret.Text = path
                                   DefinitionsValue.Add(ret)
                                   ret.Parameters.Add(New FilterParameter(param, value))
                               Next
                           End Sub

                add({"DGSource"}, "Hardware Resizing", {
                    New FilterParameter("resize_w", "%target_width%"),
                    New FilterParameter("resize_h", "%target_height%")})

                add({"DGSource"}, "Hardware Cropping", {
                    New FilterParameter("crop_l", "%crop_left%"),
                    New FilterParameter("crop_t", "%crop_top%"),
                    New FilterParameter("crop_r", "%crop_right%"),
                    New FilterParameter("crop_b", "%crop_bottom%")})

                add2({"DGSource"}, "deinterlace", "0", "deinterlace | 0 (no deinterlacing)")
                add2({"DGSource"}, "deinterlace", "1", "deinterlace | 1 (single rate deinterlacing)")
                add2({"DGSource"}, "deinterlace", "2", "deinterlace | 2 (double rate deinterlacing)")
                add2({"DGSource"}, "fulldepth", "true", "fulldepth = true")

                add({"FFVideoSource",
                     "LWLibavVideoSource",
                     "LSMASHVideoSource",
                     "ffms2.Source",
                     "LibavSMASHSource",
                     "LWLibavSource"}, "fpsnum, fpsden | 30000, 1001", {
                    New FilterParameter("fpsnum", "30000"),
                    New FilterParameter("fpsden", "1001")})

                add({"FFVideoSource",
                     "LWLibavVideoSource",
                     "LSMASHVideoSource",
                     "ffms2.Source",
                     "LibavSMASHSource",
                     "LWLibavSource"}, "fpsnum, fpsden | 60000, 1001", {
                    New FilterParameter("fpsnum", "60000"),
                    New FilterParameter("fpsden", "1001")})

                add({"FFVideoSource",
                     "LWLibavVideoSource",
                     "LSMASHVideoSource",
                     "ffms2.Source",
                     "LibavSMASHSource",
                     "LWLibavSource"}, "fpsnum, fpsden | 24, 1", {
                    New FilterParameter("fpsnum", "24"),
                    New FilterParameter("fpsden", "1")})

                add({"FFVideoSource",
                     "LWLibavVideoSource",
                     "LSMASHVideoSource",
                     "ffms2.Source",
                     "LibavSMASHSource",
                     "LWLibavSource"}, "fpsnum, fpsden | 25, 1", {
                    New FilterParameter("fpsnum", "25"),
                    New FilterParameter("fpsden", "1")})

                add({"FFVideoSource",
                     "LWLibavVideoSource",
                     "LSMASHVideoSource",
                     "ffms2.Source",
                     "LibavSMASHSource",
                     "LWLibavSource"}, "fpsnum, fpsden | 30, 1", {
                    New FilterParameter("fpsnum", "30"),
                    New FilterParameter("fpsden", "1")})

                add({"FFVideoSource",
                     "LWLibavVideoSource",
                     "LSMASHVideoSource",
                     "ffms2.Source",
                     "LibavSMASHSource",
                     "LWLibavSource"}, "fpsnum, fpsden | 50, 1", {
                    New FilterParameter("fpsnum", "50"),
                    New FilterParameter("fpsden", "1")})

                add({"FFVideoSource",
                     "LWLibavVideoSource",
                     "LSMASHVideoSource",
                     "ffms2.Source",
                     "LibavSMASHSource",
                     "LWLibavSource"}, "fpsnum, fpsden | 60, 1", {
                    New FilterParameter("fpsnum", "60"),
                    New FilterParameter("fpsden", "1")})

                add2({"ffms2.Source", "FFVideoSource"}, "rffmode", "0", "rffmode | 0 (ignore all flags (default))")
                add2({"ffms2.Source", "FFVideoSource"}, "rffmode", "1", "rffmode | 1 (honor all pulldown flags)")
                add2({"ffms2.Source", "FFVideoSource"}, "rffmode", "2", "rffmode | 2 (force film)")

                add2({"havsfunc.QTGMC"}, "TFF", "True", "TFF | True (top field first)")
                add2({"havsfunc.QTGMC"}, "TFF", "False", "TFF | False (bottom field first)")

                add2({"QTGMC"}, "Preset", """Draft""", "Preset | Draft")
                add2({"QTGMC"}, "Preset", """Ultra Fast""", "Preset | Ultra Fast")
                add2({"QTGMC"}, "Preset", """Super Fast""", "Preset | Super Fast")
                add2({"QTGMC"}, "Preset", """Very Fast""", "Preset | Very Fast")
                add2({"QTGMC"}, "Preset", """Faster""", "Preset | Faster")
                add2({"QTGMC"}, "Preset", """Fast""", "Preset | Fast")
                add2({"QTGMC"}, "Preset", """Medium""", "Preset | Medium")
                add2({"QTGMC"}, "Preset", """Slow""", "Preset | Slow")
                add2({"QTGMC"}, "Preset", """Slower""", "Preset | Slower")
                add2({"QTGMC"}, "Preset", """Very Slow""", "Preset | Very Slow")
                add2({"QTGMC"}, "Preset", """Placebo""", "Preset | Placebo")

                add2({"LSMASHVideoSource",
                      "LWLibavVideoSource",
                      "LibavSMASHSource",
                      "LWLibavSource"}, "decoder", """h264_qsv""", "decoder | h264_qsv")

                add2({"LSMASHVideoSource",
                      "LWLibavVideoSource",
                      "LibavSMASHSource",
                      "LWLibavSource"}, "decoder", """hevc_qsv""", "decoder | hevc_qsv")

                add2({"LSMASHVideoSource",
                      "LWLibavVideoSource",
                      "LibavSMASHSource",
                      "LWLibavSource"}, "decoder", """h264_nvenc""", "decoder | h264_nvenc")

                add2({"LSMASHVideoSource",
                      "LWLibavVideoSource",
                      "LibavSMASHSource",
                      "LWLibavSource"}, "decoder", """hevc_nvenc""", "decoder | hevc_nvenc")
            End If

            Return DefinitionsValue
        End Get
    End Property

    Shared Function SplitCSV(input As String) As String()
        Dim chars = input.ToCharArray()
        Dim values As New List(Of String)()
        Dim tempString As String
        Dim isString As Boolean
        Dim characterCount As Integer
        Dim level As Integer

        For Each i In chars
            characterCount += 1

            If i = """"c Then
                If isString Then
                    isString = False
                Else
                    isString = True
                End If
            End If

            If Not isString Then
                If i = "("c Then level += 1
                If i = ")"c Then level -= 1
            End If

            If i <> ","c Then
                tempString = tempString & i
            ElseIf i = ","c AndAlso (isString OrElse level > 0) Then
                tempString = tempString & i
            Else
                values.Add(tempString.Trim)
                tempString = ""
            End If

            If characterCount = chars.Length Then
                values.Add(tempString.Trim)
                tempString = ""
            End If
        Next

        Return values.ToArray()
    End Function
End Class

Public Enum ScriptEngine
    AviSynth
    VapourSynth
End Enum

<Serializable()>
Public MustInherit Class Profile
    Implements IComparable(Of Profile)

    Sub New()
    End Sub

    Sub New(name As String)
        Me.Name = name
    End Sub

    Private NameValue As String

    Overridable Property Name() As String
        Get
            If NameValue = "" Then
                Return DefaultName
            Else
                If NameValue = DefaultName Then
                    NameValue = Nothing
                    Return DefaultName
                End If
            End If

            Return NameValue
        End Get
        Set(Value As String)
            If Value = DefaultName Then
                NameValue = Nothing
            Else
                NameValue = Value
            End If
        End Set
    End Property

    Overridable ReadOnly Property DefaultName As String
        Get
            Return "untitled"
        End Get
    End Property

    Protected CanEditValue As Boolean

    Overridable ReadOnly Property CanEdit() As Boolean
        Get
            Return CanEditValue
        End Get
    End Property

    Overridable Function Edit() As Forms.DialogResult
    End Function

    Overridable Function CreateEditControl() As Controls.Control
        Return Nothing
    End Function

    Overridable Sub Clean()
    End Sub

    Overridable Function GetCopy() As Profile
        Return DirectCast(ObjectHelp.GetCopy(Me), Profile)
    End Function

    Overrides Function ToString() As String
        Return Name
    End Function

    Function CompareTo(other As Profile) As Integer Implements System.IComparable(Of Profile).CompareTo
        Return Name.CompareTo(other.Name)
    End Function
End Class

<Serializable()>
Public Class Project
    Public CodeAtTop As String = ""
    Public SaveThumbnails As Boolean
    Public Script As TargetVideoScript
    Public SourceFile As String
    Public SourceFrameRate As Double
    Public SourceHeight As Integer = 1080
    Public TempDir As String
    Public TrimCode As String = ""
End Class

Public Module ShortcutModule
    Public p As New Project
End Module

Public Class ObjectHelp
    Public Class ObjectHelp
        'parse recursive serializable fields and lists 
        Shared Function GetCompareString(obj As Object) As String
            Dim sb As New StringBuilder
            ParseCompareString(obj, Nothing, sb)
            Return sb.ToString
        End Function

        Shared Sub ParseCompareString(obj As Object, declaringObj As Object, sb As StringBuilder)
            If TypeOf obj Is ICollection Then
                For Each i In DirectCast(obj, ICollection)
                    If IsGoodType(i) Then
                        If IsToString(i) Then
                            sb.Append(i.ToString)
                        Else
                            ParseCompareString(i, obj, sb)
                        End If
                    End If
                Next
            Else
                If IsGoodType(obj) Then
                    Dim t = obj.GetType

                    While Not t Is Nothing
                        For Each i As FieldInfo In t.GetFields(BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.Instance Or BindingFlags.DeclaredOnly)
                            If Not i.IsNotSerialized Then
                                Dim o = i.GetValue(obj)

                                If IsGoodType(o) Then
                                    If IsToString(o) Then
                                        sb.Append(i.Name + "=" + o.ToString + BR)
                                    Else
                                        If Not o Is declaringObj Then
                                            ParseCompareString(o, obj, sb)
                                        End If
                                    End If
                                End If
                            End If
                        Next

                        t = t.BaseType
                    End While
                End If
            End If
        End Sub

        Private Shared Function IsGoodType(o As Object) As Boolean
            If o Is Nothing Then Return False
            If TypeOf o Is Pointer Then Return False
            If TypeOf o Is PropertyChangedEventHandler Then Return False
            If Not o.GetType.IsSerializable Then Return False
            Return True
        End Function

        Private Shared Function IsToString(o As Object) As Boolean
            If Not o Is Nothing Then
                If o.GetType.IsPrimitive Then Return True
                If TypeOf o Is String Then Return True
                'some fields change here
                If TypeOf o Is CultureInfo Then Return True
                'some fields change here
                If TypeOf o Is StringBuilder Then Return True
            End If
        End Function
    End Class

    <DebuggerHidden()>
    Shared Function GetCopy(Of T)(o As T) As T
        Using ms As New MemoryStream
            Dim bf As New BinaryFormatter
            bf.Serialize(ms, o)
            ms.Position = 0
            Return DirectCast(bf.Deserialize(ms), T)
        End Using
    End Function
End Class