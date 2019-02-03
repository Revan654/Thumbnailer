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



