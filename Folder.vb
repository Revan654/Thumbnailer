Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Security.Permissions
Imports System.Text
Imports Microsoft.Win32

Public Class Folder
    Shared ReadOnly Property Desktop() As String
        Get
            Return Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        End Get
    End Property

    Shared ReadOnly Property Startup() As String
        Get
            Return Forms.Application.StartupPath
        End Get
    End Property

    Shared ReadOnly Property FontSystem As String
        Get
            Return Environment.GetFolderPath(Environment.SpecialFolder.Fonts)
        End Get
    End Property

    Shared ReadOnly Property Current() As String
        Get
            Return Environment.CurrentDirectory
        End Get
    End Property

    Shared ReadOnly Property Programs() As String
        Get
            Return GetFolderPath(Environment.SpecialFolder.ProgramFiles).FixDir
        End Get
    End Property

    Shared ReadOnly Property AppDataCommon() As String
        Get
            Return GetFolderPath(Environment.SpecialFolder.CommonApplicationData).FixDir
        End Get
    End Property

    Shared ReadOnly Property AppDataLocal() As String
        Get
            Return GetFolderPath(Environment.SpecialFolder.LocalApplicationData).FixDir
        End Get
    End Property

    Shared ReadOnly Property AppDataRoaming() As String
        Get
            Return GetFolderPath(Environment.SpecialFolder.ApplicationData).FixDir
        End Get
    End Property

    Shared ReadOnly Property Windows() As String
        Get
            Return GetFolderPath(Environment.SpecialFolder.Windows).FixDir
        End Get
    End Property

    Shared ReadOnly Property Temp() As String
        Get
            Return Path.GetTempPath
        End Get
    End Property

    Shared ReadOnly Property Apps As String
        Get
            Return Startup + "\Apps"
        End Get
    End Property


    Shared ReadOnly Property Plugins As String
        Get
            Return Apps + "\Plugins"
        End Get
    End Property

    Shared ReadOnly Property Fonts As String
        Get
            Return Apps + "\Fonts"
        End Get
    End Property

    <DllImport("shfolder.dll", CharSet:=CharSet.Unicode)>
    Private Shared Function SHGetFolderPath(hwndOwner As IntPtr, nFolder As Integer, hToken As IntPtr, dwFlags As Integer, lpszPath As StringBuilder) As Integer
    End Function

    Private Shared Function GetFolderPath(folder As Environment.SpecialFolder) As String
        Dim sb As New StringBuilder(260)
        SHGetFolderPath(IntPtr.Zero, CInt(folder), IntPtr.Zero, 0, sb)
        Dim ret = sb.ToString.FixDir '.NET fails on 'D:'
        Call New FileIOPermission(FileIOPermissionAccess.PathDiscovery, ret).Demand()
        Return ret
    End Function
End Class

Public Class PathBase

    Shared ReadOnly Property Separator() As Char
        Get
            Return Path.DirectorySeparatorChar
        End Get
    End Property

    Shared Function IsSameBase(a As String, b As String) As Boolean
        Return FilePath.GetBase(a).EqualIgnoreCase(FilePath.GetBase(b))
    End Function

    Shared Function IsSameDir(a As String, b As String) As Boolean
        Return FilePath.GetDir(a).EqualIgnoreCase(FilePath.GetDir(b))
    End Function

    Shared Function IsValidFileSystemName(name As String) As Boolean
        If name = "" Then Return False
        Dim chars = """*/:<>?\|^".ToCharArray

        For Each i In name.ToCharArray
            If chars.Contains(i) Then Return False
            If Convert.ToInt32(i) < 32 Then Return False
        Next

        Return True
    End Function

    Shared Function RemoveIllegalCharsFromName(name As String) As String
        If name = "" Then Return ""

        Dim chars = """*/:<>?\|^".ToCharArray

        For Each i In name.ToCharArray
            If chars.Contains(i) Then
                name = name.Replace(i, "_")
            End If
        Next

        For x = 1 To 31
            If name.Contains(Convert.ToChar(x)) Then
                name = name.Replace(Convert.ToChar(x), "_"c)
            End If
        Next

        Return name
    End Function

End Class

Public Class DirPath
    Inherits PathBase

    Shared Function TrimTrailingSeparator(path As String) As String
        If path = "" Then Return ""

        If path.EndsWith(Separator) AndAlso Not path.Length <= 3 Then
            Return path.TrimEnd(Separator)
        End If

        Return path
    End Function

    Shared Function FixSeperator(path As String) As String
        If path.Contains("\") AndAlso Separator <> "\" Then
            path = path.Replace("\", Separator)
        End If

        If path.Contains("/") AndAlso Separator <> "/" Then
            path = path.Replace("/", Separator)
        End If

        Return path
    End Function

    Shared Function GetParent(path As String) As String
        If path = "" Then Return ""
        Dim temp = TrimTrailingSeparator(path)
        If temp.Contains(Separator) Then path = temp.LeftLast(Separator) + Separator
        Return path
    End Function

    Shared Function GetName(path As String) As String
        If path = "" Then Return ""
        path = TrimTrailingSeparator(path)
        Return path.RightLast(Separator)
    End Function

    Shared Function IsInSysDir(path As String) As Boolean
        If path = "" Then Return False
        If Not path.EndsWith("\") Then path += "\"
        Return path.ToUpper.Contains(Folder.Programs.ToUpper)
    End Function

    Shared Function IsFixedDrive(path As String) As Boolean
        Try
            If path <> "" Then Return New DriveInfo(path).DriveType = DriveType.Fixed
        Catch ex As Exception
        End Try
    End Function
End Class

Public Class FilePath
    Inherits PathBase

    Private Value As String

    Sub New(path As String)
        Value = path
    End Sub

    Shared Function GetDir(path As String) As String
        If path = "" Then Return ""
        If path.Contains("\") Then path = path.LeftLast("\") + "\"
        Return path
    End Function

    Shared Function GetDirAndBase(path As String) As String
        Return GetDir(path) + GetBase(path)
    End Function

    Shared Function GetName(path As String) As String
        If Not path Is Nothing Then
            Dim index = path.LastIndexOf(IO.Path.DirectorySeparatorChar)

            If index > -1 Then
                Return path.Substring(index + 1)
            End If
        End If

        Return path
    End Function

    Shared Function GetExtFull(filepath As String) As String
        Return GetExt(filepath, True)
    End Function

    Shared Function GetExt(filepath As String) As String
        Return GetExt(filepath, False)
    End Function

    Shared Function GetExt(filepath As String, dot As Boolean) As String
        If filepath = "" Then Return ""
        Dim chars = filepath.ToCharArray

        For x = filepath.Length - 1 To 0 Step -1
            If chars(x) = Separator Then Return ""
            If chars(x) = "."c Then Return filepath.Substring(x + If(dot, 0, 1)).ToLower
        Next

        Return ""
    End Function

    Shared Function GetDirNoSep(path As String) As String
        path = GetDir(path)
        If path.EndsWith(Separator) Then path = TrimSep(path)
        Return path
    End Function

    Shared Function GetBase(path As String) As String
        If path = "" Then Return ""
        Dim ret = path
        If ret.Contains(Separator) Then ret = ret.RightLast(Separator)
        If ret.Contains(".") Then ret = ret.LeftLast(".")
        Return ret
    End Function

    Shared Function TrimSep(path As String) As String
        If path = "" Then Return ""

        If path.EndsWith(Separator) AndAlso Not path.EndsWith(":" + Separator) Then
            Return path.TrimEnd(Separator)
        End If

        Return path
    End Function

    Shared Function GetDirNameOnly(path As String) As String
        Return FilePath.GetDirNoSep(path).RightLast("\")
    End Function

End Class
