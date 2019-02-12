Imports System.IO
Imports System.Runtime.Serialization
Imports System.Runtime.Serialization.Formatters.Binary

Public Class Settings
    Protected Shared SettingsFile As String = Folder.Apps + "\Settings.bin"

    Shared Sub Serialize()
        Dim SettingsV2 As New Hashtable
        Try
            SettingsV2.Add("TNMode", Main.UI.TNModeCBx.Text)
            SettingsV2.Add("TNRows", Main.UI.TNRowsNB.Value)
            SettingsV2.Add("TNWidth", Main.UI.TNWidthNB.Value)
            SettingsV2.Add("TNColumns", Main.UI.TNColumnsNB.Value)
            SettingsV2.Add("TNIntervals", Main.UI.TNIntervalsNB.Value)
            SettingsV2.Add("TNQuality", Main.UI.TNQualityNB.Value)
            SettingsV2.Add("TNPosition", Main.UI.TNPositionCBx.Text)
            SettingsV2.Add("TNFormat", Main.UI.TNFormatCBx.Text)
            SettingsV2.Add("TNLogo", Main.UI.TNLogoCB.IsChecked)

        Catch ex As Exception
        End Try

        Dim fs As New FileStream(SettingsFile, FileMode.Create)
        Dim formatter As New BinaryFormatter
        Try
            formatter.Serialize(fs, SettingsV2)
        Catch f As SerializationException
            Throw
        Finally
            fs.Close()
        End Try

    End Sub


    Shared Sub DeSerialize()

        Dim SettingsV2 As Hashtable = Nothing
        Dim fs As New FileStream(SettingsFile, FileMode.Open)

        Try
            Dim formatter As New BinaryFormatter
            SettingsV2 = DirectCast(formatter.Deserialize(fs), Hashtable)

        Catch f As SerializationException
            'Throw
        Finally
            fs.Close()
        End Try

        Try
            Main.UI.TNFormatCBx.Text = SettingsV2.Item("TNFormat")
            Main.UI.TNModeCBx.Text = SettingsV2.Item("TNMode")
            Main.UI.TNColumnsNB.Value = SettingsV2.Item("TNColumns")
            Main.UI.TNRowsNB.Value = SettingsV2.Item("TNRows")
            Main.UI.TNWidthNB.Value = SettingsV2.Item("TNWidth")
            Main.UI.TNQualityNB.Value = SettingsV2.Item("TNQuality")
            Main.UI.TNPositionCBx.Text = SettingsV2.Item("TNPosition")
            Main.UI.TNLogoCB.IsChecked = SettingsV2.Item("TNLogo")
        Catch ex As Exception
        End Try

    End Sub

End Class
