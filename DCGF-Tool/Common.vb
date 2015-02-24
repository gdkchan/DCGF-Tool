Imports System.Text.RegularExpressions
Module Common
    Public Structure Pack_File
        Dim Name As String
        Dim Unknow As Integer
        Dim Offset As Integer
        Dim Comp_Size As Integer
        Dim Decomp_Size As Integer
        Dim Data() As Byte
    End Structure
    Public Files() As Pack_File
    Public Header(&HB) As Byte

    Public Structure TxtReplace
        Dim TargetData() As Byte
        Dim ReplaceStr As String
        Dim Args As Integer
    End Structure
    Public Replaces() As TxtReplace

    Public OpenedFile As String
    Public MudaAcento As Boolean = True
    Public Sub Aplica_Texto(Texto As String, FileIndex As Integer)
        If MudaAcento Then
            Texto = Texto.Replace("á", "#")
            Texto = Texto.Replace("à", "%")
            Texto = Texto.Replace("Ó", "<")
            Texto = Texto.Replace("í", ">")
            Texto = Texto.Replace("É", "[")
            Texto = Texto.Replace("â", "$")
            Texto = Texto.Replace("ã", "]")
            Texto = Texto.Replace("ç", "^")
            Texto = Texto.Replace("é", "_")
            Texto = Texto.Replace("ê", "`")
            Texto = Texto.Replace("ó", "{")
            Texto = Texto.Replace("ô", "|")
            Texto = Texto.Replace("õ", "}")
            Texto = Texto.Replace("ú", "~")
        End If
        Dim Temp As String = Texto

        'Calcula novo tamanho do arquivo, considerando também dados substituídos por mnemônicos
        Dim NewSize As Integer
        For i As Integer = 0 To Replaces.Length - 1
            Dim Num_Matches As Integer = CountWords(Temp, Replaces(i).ReplaceStr)
            NewSize += Replaces(i).TargetData.Length * Num_Matches
            Temp = Temp.Replace(Replaces(i).ReplaceStr, Nothing)
        Next
        Dim HTPatt As String = "<0x[0-9ABCDEFabcdef]+>"
        Dim HexCodes As MatchCollection = Regex.Matches(Texto, HTPatt)
        NewSize += HexCodes.Count * 2
        Temp = Regex.Replace(Temp, HTPatt, "")
        NewSize += Len(Temp) * 2

        Dim OutFile(NewSize) As Byte
        Dim Pos As Integer
        For i As Integer = 0 To Len(Texto) - 1
            Dim NoText As Boolean = False
            For j As Integer = 0 To Replaces.Length - 1
                If Mid(Texto, i + 1, Len(Replaces(j).ReplaceStr)) = Replaces(j).ReplaceStr Then
                    Buffer.BlockCopy(Replaces(j).TargetData, 0, OutFile, Pos, Replaces(j).TargetData.Length)
                    Pos += Replaces(j).TargetData.Length
                    i += (Len(Replaces(j).ReplaceStr) - 1)
                    NoText = True
                    Exit For
                End If
            Next
            If Not NoText Then
                Dim Num As Integer
                If Mid(Texto, i + 1, 3) = "<0x" And Mid(Texto, i + 1, 8).IndexOf(">") <= 8 Then
                    Dim CloseChar As Integer = Mid(Texto, i + 1, 8).IndexOf(">")
                    Num = Val("&h" & Texto.Substring(i + 3, CloseChar - 3)) And &HFFFF
                    i += (CloseChar + 1) - 1
                Else
                    'Converte caractere de volta no valor!!!
                    Num = Char.ConvertToUtf32(Texto, i) And &HFFFF
                End If

                OutFile(Pos) = Num And &HFF
                OutFile(Pos + 1) = ((Num And &HFF00) / &H100) And &HFF
                Pos += 2
            End If
        Next
        Files(FileIndex).Decomp_Size = OutFile.Length
        Files(FileIndex).Data = LZSSComp(OutFile)
        Files(FileIndex).Comp_Size = Files(FileIndex).Data.Length

        RecalcAll()
    End Sub
    Private Function CountWords(InStr As String, Word As String) As Integer
        Dim Collection As MatchCollection = Regex.Matches(InStr, Word)
        Return IIf(Collection.Count >= 0, Collection.Count, 0)
    End Function
    Public Sub RecalcAll()
        'Recalcula os offsets de todos os arquivos, visto que o tamanho pode ter mudado
        Dim CurrOffset As Integer = Files(0).Offset + IIf(Files(0).Comp_Size > 0, Files(0).Comp_Size, Files(0).Decomp_Size)
        For j As Integer = 1 To Files.Length - 1
            Files(j).Offset = CurrOffset
            CurrOffset += IIf(Files(j).Comp_Size > 0, Files(j).Comp_Size, Files(j).Decomp_Size)
        Next
    End Sub
End Module
