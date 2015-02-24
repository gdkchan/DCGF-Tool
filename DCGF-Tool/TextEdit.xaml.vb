Public Class TextEdit
    Public SelectedIndex As Integer
    Private Sub MenuItem_Click(sender As Object, e As RoutedEventArgs)
        Aplica_Texto(TxtEdit.Text, SelectedIndex)
        Dim MainW As Object = Me.Owner
        MainW.Atualiza_Lista()
    End Sub
    Private Sub MnuBusca_Click(sender As Object, e As RoutedEventArgs) Handles MnuBusca.Click
        Dim Busca As String = InputBox("Digite o termo que você deseja pesquisar:", "Buscar")
        If Busca <> Nothing Then
            Dim Index As Integer = TxtEdit.Text.IndexOf(Busca)
            If Index > -1 Then
                TxtEdit.SelectionStart = Index
                TxtEdit.SelectionLength = Len(Busca)
            Else
                MsgBox("Não encontrado!", vbExclamation)
            End If
        End If
    End Sub
    Private Sub Chk_Click()
        MudaAcento = Not MudaAcento
    End Sub
End Class
