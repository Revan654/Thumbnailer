﻿Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Drawing
Imports System.Reflection
Imports System.Security.Permissions
Imports System.Windows.Forms


Public Class Native

    Public Delegate Function CallbackHandler(handle As IntPtr, parameter As Integer) As Boolean
    <DllImport("gdi32.dll")>
    Public Shared Function ExcludeClipRect(hdc As IntPtr, nLeftRect As Integer, nTopRect As Integer, nRightRect As Integer, nBottomRect As Integer) As Integer
    End Function
    Friend Const EM_SETCUEBANNER As Integer = &H1501
    Friend Const CB_SETCUEBANNER As Integer = &H1703
    Friend Const SC_CLOSE As Integer = &HF060
    Friend Const SC_MINIMIZE As Integer = &HF020
    Friend Const FORMAT_MESSAGE_ALLOCATE_BUFFER = &H100
    Friend Const FORMAT_MESSAGE_IGNORE_INSERTS = &H200
    Friend Const FORMAT_MESSAGE_FROM_STRING = &H400
    Friend Const FORMAT_MESSAGE_FROM_HMODULE = &H800
    Friend Const FORMAT_MESSAGE_FROM_SYSTEM = &H1000
    Friend Const FORMAT_MESSAGE_ARGUMENT_ARRAY = &H2000

#Region "User32"
    <DllImport("user32.dll", SetLastError:=True)>
    Shared Function SetWindowPos(hWnd As IntPtr,
                             hWndInsertAfter As IntPtr,
                             X As Integer,
                             Y As Integer,
                             cx As Integer,
                             cy As Integer,
                             uFlags As UInteger) As Boolean
    End Function
    <DllImport("user32.dll")>
    Shared Function GetWindowLong(hWnd As IntPtr, nIndex As Integer) As Integer
    End Function

    <DllImport("user32.dll", CharSet:=CharSet.Unicode)>
    Shared Function RegisterWindowMessage(id As String) As Integer
    End Function

    <DllImport("user32.dll")>
    Shared Function RegisterHotKey(hWnd As IntPtr, id As Integer, fsModifiers As Integer, vk As Integer) As Boolean
    End Function

    <DllImport("user32.dll")>
    Shared Function MapVirtualKey(wCode As Integer, wMapType As Integer) As Integer
    End Function

    <DllImport("user32.dll")>
    Shared Function UnregisterHotKey(hWnd As IntPtr, id As Integer) As Boolean
    End Function

    <DllImport("user32.dll")>
    Shared Function GetForegroundWindow() As IntPtr
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Shared Function GetWindowThreadProcessId(hwnd As IntPtr, ByRef lpdwProcessId As Integer) As Integer
    End Function

    <DllImport("user32.dll")>
    Shared Function SetForegroundWindow(handle As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll", CharSet:=CharSet.Unicode)>
    Shared Function GetWindowModuleFileName(hwnd As IntPtr,
                                        lpszFileName As StringBuilder,
                                        cchFileNameMax As UInteger) As UInteger
    End Function

    <DllImport("user32.dll")>
    Shared Function SendMessage(handle As IntPtr,
                            message As Int32,
                            wParam As IntPtr,
                            lParam As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll", CharSet:=CharSet.Unicode)>
    Shared Function SendMessage(hWnd As IntPtr,
                            Msg As Int32,
                            wParam As IntPtr,
                            lParam As String) As IntPtr
    End Function

    <DllImport("user32.dll", CharSet:=CharSet.Unicode)>
    Shared Function SendMessage(hWnd As IntPtr,
                            Msg As Int32,
                            wParam As Integer,
                            lParam As Integer) As IntPtr
    End Function

    <DllImport("user32.dll", CharSet:=CharSet.Unicode)>
    Shared Function SendMessage(hWnd As IntPtr,
                            Msg As Int32,
                            wParam As Integer,
                            lParam As String) As IntPtr
    End Function

    <DllImport("user32.dll", CharSet:=CharSet.Unicode)>
    Shared Function SendMessage(hWnd As IntPtr,
                            Msg As Int32,
                            ByRef wParam As IntPtr,
                            lParam As StringBuilder) As IntPtr
    End Function

    <DllImport("user32.dll")>
    Shared Function SendMessageTimeout(windowHandle As IntPtr,
                                   msg As Integer,
                                   wParam As IntPtr,
                                   lParam As IntPtr,
                                   flags As Integer,
                                   timeout As Integer,
                                   ByRef result As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll", CharSet:=CharSet.Unicode)>
    Shared Function PostMessage(hwnd As IntPtr,
                            wMsg As Integer,
                            wParam As IntPtr,
                            lParam As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll")>
    Shared Sub ReleaseCapture()
    End Sub

    <DllImport("user32.dll")>
    Public Shared Function GetWindowRect(hWnd As IntPtr, ByRef lpRect As RECT) As Boolean
    End Function

    <DllImport("user32.dll")>
    Public Shared Function GetWindowDC(hWnd As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll")>
    Public Shared Function ReleaseDC(hWnd As IntPtr, hDC As IntPtr) As Integer
    End Function
#End Region
#Region "Kernal"
    <DllImport("kernel32.dll", CharSet:=CharSet.Unicode)>
    Shared Function LoadLibrary(path As String) As IntPtr
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)>
    Shared Function FreeLibrary(hModule As IntPtr) As Boolean
    End Function

    <DllImport("kernel32.dll", SetLastError:=True, CharSet:=CharSet.Unicode)>
    Shared Function FormatMessage(dwFlags As Integer,
                              lpSource As IntPtr,
                              dwMessageId As Integer,
                              dwLanguageId As Integer,
                              ByRef lpBuffer As String,
                              nSize As Integer,
                              Arguments As IntPtr) As Integer
    End Function


#End Region
#Region "Misc"
    <DllImport("uxtheme.dll", CharSet:=CharSet.Unicode)>
    Shared Function SetWindowTheme(hWnd As IntPtr,
                               pszSubAppName As String,
                               pszSubIdList As String) As Integer
    End Function

    <DllImport("Shlwapi.dll", SetLastError:=True, CharSet:=CharSet.Unicode)>
    Shared Function AssocQueryString(
   flags As UInteger,
   str As UInteger,
   pszAssoc As String,
   pszExtra As String,
   pszOut As StringBuilder,
   ByRef pcchOut As UInteger) As UInteger
    End Function
#End Region
#Region "Structure"
    Public Structure RECT
        Public Left As Integer
        Public Top As Integer
        Public Right As Integer
        Public Bottom As Integer

        Sub New(r As Rectangle)
            Left = r.Left
            Top = r.Top
            Right = r.Right
            Bottom = r.Bottom
        End Sub

        Public Sub New(left As Integer, top As Integer, right As Integer, bottom As Integer)
            Me.Left = left
            Me.Top = top
            Me.Right = right
            Me.Bottom = bottom
        End Sub

        Function ToRectangle() As Rectangle
            Return Rectangle.FromLTRB(Left, Top, Right, Bottom)
        End Function
    End Structure

    Public Structure SHFILEINFO
        Public hIcon As IntPtr
        Public iIcon As Integer
        Public dwAttributes As Integer
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=260)>
        Public szDisplayName As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=80)>
        Public szTypeName As String
    End Structure

    Public Structure NMHDR
        Public hwndFrom As Integer
        Public idFrom As Integer
        Public code As Integer
    End Structure

    Public Structure NCCALCSIZE_PARAMS
        Public rgrc0, rgrc1, rgrc2 As RECT
        Public lppos As IntPtr
    End Structure
#End Region
End Class