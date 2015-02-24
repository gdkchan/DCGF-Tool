Imports System.IO
Imports System.Threading
Class MainWindow
    Dim CompText As New CompText
    Dim ProcessedFiles, TotalFiles As Integer
    Dim InsertMode As Boolean
    Dim Erro As Boolean
    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        CompStackPanel.DataContext = CompText

        ReDim Replaces(11)
        Add_Replace(0, New Integer() {0, 0, &HAB, 0, &HAA}, vbCrLf & vbCrLf & vbCrLf, 1)
        Add_Replace(1, New Integer() {0, &HAB, 0, &HAA}, vbCrLf & vbCrLf, 1)
        Add_Replace(2, New Integer() {&H7C, &H7C}, vbCrLf)
        Add_Replace(3, New Integer() {0, &HAB, 0}, "<script>") 'Isso está certo?
        Add_Replace(4, New Integer() {&H47A1, 0, &HA2, 2}, "<clear>") '???
        Add_Replace(5, New Integer() {&HA2, &HD6, &HAD, &H3E8}, "<fade>") '???
        Add_Replace(6, New Integer() {&HA8, 0}, "<title>") '???
        Add_Replace(7, New Integer() {0, &HA9, 0, &HAA}, "</title>") '???
        Add_Replace(8, New Integer() {&H4A1, 0, &HA7}, "<bg>", 1)
        Add_Replace(9, New Integer() {&H7A1, 0, &HA7}, "<change_bg>", 1)
        Add_Replace(10, New Integer() {&H7CA1, 0, &HA7}, "<alpha_bg>", 1)
        Add_Replace(11, New Integer() {&H4CA1, 0}, "<no_petals>")
    End Sub
    Private Sub Add_Replace(Index As Integer, Data() As Integer, Str As String, Optional Args As Integer = 0)
        Dim Pos As Integer
        With Replaces(Index)
            ReDim .TargetData((Data.Length * 2) - 1)
            For i As Integer = 0 To Data.Length - 1
                .TargetData(Pos) = Data(i) And &HFF
                .TargetData(Pos + 1) = ((Data(i) And &HFF00) / &H100) And &HFF
                Pos += 2
            Next
            .ReplaceStr = Str
            .Args = Args
        End With
    End Sub
    Private Sub LstFiles_DblClick(sender As Object, e As RoutedEventArgs) Handles LstFiles.MouseDoubleClick
        Dim Index As Integer = LstFiles.SelectedIndex
        If Index > -1 Then
            If Path.GetExtension(LCase(Files(Index).Name)) <> ".bin" Then
                MsgBox("Isso não é um arquivo de texto!" & vbCrLf & "Use um programa externo para edita-lo!", vbInformation)
                Exit Sub
            End If
            Dim Data() As Byte = LZSSDecomp(Files(Index).Data, Files(Index).Decomp_Size)
            Dim Texto As String = Nothing
            For i As Integer = 0 To Data.Length - 2
                Dim NoText As Boolean = False
                For j As Integer = 0 To Replaces.Length - 1
                    If CheckBytes(Data, i, Replaces(j).TargetData) Then
                        Texto &= Replaces(j).ReplaceStr
                        i += Replaces(j).TargetData.Length
                        If Replaces(j).Args > 0 Then
                            For k As Integer = 1 To Replaces(j).Args
                                Dim TmpVal As Integer = Data(i) + (Data(i + 1) * &H100)
                                Texto &= "<0x" & Hex(TmpVal) & ">"
                                i += 2
                            Next
                        End If
                        i -= 1
                        NoText = True
                        Exit For
                    End If
                Next
                If Not NoText Then
                    Dim ChrNum As Integer = Data(i) + (Data(i + 1) * &H100)
                    If (ChrNum < &HD800 Or ChrNum > &HDFFF) And ChrNum <> 0 Then
                        Texto &= Char.ConvertFromUtf32(ChrNum)
                    Else
                        'Caracteres Unicode inválidos para o .NET Framework
                        Texto &= "<0x" & Hex(ChrNum) & ">"
                    End If
                    i += 1
                End If
            Next
            Dim Window = New TextEdit()
            Window.Owner = Me
            Window.Show()
            Window.SelectedIndex = Index
            Window.TxtEdit.Text = Texto
        End If
    End Sub
    Private Sub BtnAbrePAK_Click(sender As Object, e As RoutedEventArgs) Handles BtnAbrePAK.Click
        Dim OpenDlg As New Microsoft.Win32.OpenFileDialog
        OpenDlg.Filter = "Arquivos PAK|*.pak"
        OpenDlg.ShowDialog()
        If File.Exists(OpenDlg.FileName) Then
            Lock_UI()
            OpenedFile = OpenDlg.FileName
            ThreadPool.QueueUserWorkItem(Sub() Open_File(OpenDlg.FileName))
        End If
    End Sub
    Private Sub Open_File(File_Name As String)
        Dim InFile As FileStream
        Try
            InFile = New FileStream(File_Name, FileMode.Open)
        Catch
            MsgBox("Não foi possível acessar o arquivo." & vbCrLf & "Tente executar a ferramenta como administrador.", vbExclamation)
            Me.Dispatcher.BeginInvoke(Sub() Unlock_UI())
            Exit Sub
        End Try
        InFile.Read(Header, 0, 12)

        'Limpa
        LstFiles.Dispatcher.BeginInvoke(New Action(Sub() LstFiles.Items.Clear()))
        ReDim Files(Endian32(Read32(Header, 4)) - 1)

        Dim Header_End As Integer
        Dim FileNum As Integer
        Do
            Dim Name As String = Read_File_Name(InFile)
            Dim Unknow As Integer = Read(InFile, 4)
            Dim Decomp_Size As Integer = Read(InFile, 4)
            Dim Comp_Size As Integer = Read(InFile, 4)
            Dim Offset As Integer = Read(InFile, 4)

            'Adiciona informações do arquivo
            With Files(FileNum)
                .Name = Name
                .Unknow = Unknow
                .Decomp_Size = Decomp_Size
                .Comp_Size = Comp_Size
                .Offset = Offset
                If FileNum = 0 Then Header_End = .Offset - 1
            End With
            FileNum += 1

            'Adiciona arquivos no ListView
            LstFiles.Dispatcher.BeginInvoke(New Action(Sub() _
                            LstFiles.Items.Add(New With { _
                            Key .Name = Name, _
                            Key .Offset = "0x" & Hex(Offset), _
                            Key .CompSize = GetBytes(Comp_Size), _
                            Key .DecompSize = GetBytes(Decomp_Size)})))
        Loop Until InFile.Position > Header_End

        'Carrega dados comprimidos do aruqivo para a Array
        ProcessedFiles = 0
        TotalFiles = Files.Length
        For Index As Integer = 0 To Files.Length - 1
            Dim Size As Integer = IIf(Files(Index).Comp_Size > 0, Files(Index).Comp_Size, Files(Index).Decomp_Size)
            InFile.Seek(Files(Index).Offset, SeekOrigin.Begin)
            ReDim Files(Index).Data(Size - 1)
            InFile.Read(Files(Index).Data, 0, Size)

            ProcessedFiles += 1
            Progresso.Dispatcher.BeginInvoke(New Action(Sub() UpdateProgress()))
        Next

        InFile.Close()
    End Sub
    Public Sub Atualiza_Lista()
        LstFiles.Items.Clear()
        For Each File As Pack_File In Files
            LstFiles.Dispatcher.BeginInvoke(New Action(Sub() _
                                    LstFiles.Items.Add(New With { _
                                    Key .Name = File.Name, _
                                    Key .Offset = "0x" & Hex(File.Offset), _
                                    Key .CompSize = GetBytes(File.Comp_Size), _
                                    Key .DecompSize = GetBytes(File.Decomp_Size)})))
        Next
    End Sub
    Private Function GetBytes(Bytes As Integer) As String
        If Bytes >= 1073741824 Then
            Return Format(Bytes / 1024 / 1024 / 1024, "#0.00") & " GB"
        ElseIf Bytes >= 1048576 Then
            Return Format(Bytes / 1024 / 1024, "#0.00") & " MB"
        ElseIf Bytes >= 1024 Then
            Return Format(Bytes / 1024, "#0.00") & " KB"
        ElseIf Bytes < 1024 Then
            Return Fix(Bytes) & " Bytes"
        Else
            Return Bytes
        End If
    End Function
    Private Sub BtnSalvaPAK_Click(sender As Object, e As RoutedEventArgs) Handles BtnSalvaPAK.Click
        If File.Exists(OpenedFile) Then
            Lock_UI()
            ProcessedFiles = 0
            TotalFiles = Files.Length
            ThreadPool.QueueUserWorkItem(Sub() Write_Pack(OpenedFile))
        Else
            If OpenedFile <> Nothing Then
                MsgBox("Arquivo não encontrado:" & vbCrLf & OpenedFile, vbCritical)
            Else
                MsgBox("Você precisa abrir um arquivo primeiro!", vbExclamation)
            End If
        End If
    End Sub
    Private Sub Write_Pack(File_Name As String)
        Dim OutFile As New FileStream(File_Name, FileMode.Create)
        OutFile.Write(Header, 0, Header.Length)
        For i As Integer = 0 To Files.Length - 1
            Write_File_Name(OutFile, Files(i).Name)
            Write(OutFile, Files(i).Unknow, 4)
            Write(OutFile, Files(i).Decomp_Size, 4)
            Write(OutFile, Files(i).Comp_Size, 4)
            Write(OutFile, Files(i).Offset, 4)
        Next

        For i As Integer = 0 To Files.Length - 1
            OutFile.Write(Files(i).Data, 0, Files(i).Data.Length)

            ProcessedFiles += 1
            Progresso.Dispatcher.BeginInvoke(New Action(Sub() UpdateProgress()))
        Next
    End Sub
    Private Sub BtnExtrair_Click(sender As Object, e As RoutedEventArgs) Handles BtnExtrair.Click
        If LstFiles.SelectedIndex > -1 Then
            Dim FolderDlg As New System.Windows.Forms.FolderBrowserDialog
            FolderDlg.ShowDialog()
            If Directory.Exists(FolderDlg.SelectedPath) Then
                Lock_UI()
                ProcessedFiles = 0
                TotalFiles = LstFiles.SelectedItems.Count
                For Each SelectedFile As Object In LstFiles.SelectedItems
                    Dim Index As Integer = LstFiles.Items.IndexOf(SelectedFile)
                    Dim FileName As String = FolderDlg.SelectedPath & "\" & Files(Index).Name
                    ThreadPool.QueueUserWorkItem(Sub() Extrai_Arquivo(FileName, Index))
                Next
            End If
        End If
    End Sub
    Private Sub Extrai_Arquivo(FileName As String, Index As Integer)
        If LCase(Path.GetExtension(Files(Index).Name)) = ".op2" Then
            'Arquivo de imagem
            FileName = Path.ChangeExtension(FileName, ".png")
            Dim Data() As Byte
            If Files(Index).Comp_Size > 0 Then
                Data = LZSSDecomp(Files(Index).Data, Files(Index).Decomp_Size)
            Else
                Data = Files(Index).Data
            End If

            Dim W As Integer = Endian32(Read32(Data, 4))
            Dim H As Integer = Endian32(Read32(Data, 8))
            Dim BPP As Integer = Endian32(Read32(Data, 12))
            Dim DataOffset As Integer = Endian32(Read32(Data, 20))
            Dim DIBSize As Integer = Endian32(Read32(Data, 24))

            Dim Stride As Integer = ((W * BPP + (BPP - 1)) And (Not (BPP - 1))) / 8
            Dim ImgData(Data.Length - DataOffset) As Byte
            Buffer.BlockCopy(Data, DataOffset, ImgData, 0, Data.Length - DataOffset)
            ImgData = LZSSDecomp(ImgData, DIBSize)

            Dim PF As PixelFormat
            Select Case BPP
                Case 8 : PF = PixelFormats.Gray8
                Case 16 : PF = PixelFormats.Gray16
                Case 32 : PF = PixelFormats.Bgra32
                Case Else : PF = PixelFormats.Bgr24
            End Select
            Dim Bmp As BitmapSource = BitmapSource.Create(W, H, 96.0F, 96.0F, PF, Nothing, ImgData, Stride)
            Dim Encoder As New PngBitmapEncoder
            Encoder.Frames.Add(BitmapFrame.Create(Bmp))
            Dim Stream As New FileStream(FileName, FileMode.Create)
            Encoder.Save(Stream)
            Stream.Close()
        Else
            If Files(Index).Comp_Size > 0 Then
                File.WriteAllBytes(FileName, LZSSDecomp(Files(Index).Data, Files(Index).Decomp_Size))
            Else
                File.WriteAllBytes(FileName, Files(Index).Data)
            End If
        End If

        ProcessedFiles += 1
        Progresso.Dispatcher.BeginInvoke(New Action(Sub() UpdateProgress()))
    End Sub
    Private Sub BtnInserir_Click(sender As Object, e As RoutedEventArgs) Handles BtnInserir.Click
        If OpenedFile = Nothing Then
            MsgBox("Você precisa abrir um arquivo primeiro!", vbExclamation)
            Exit Sub
        End If
        Dim OpenDlg As New Microsoft.Win32.OpenFileDialog
        OpenDlg.Filter = "Arquivo|*.*"
        OpenDlg.Multiselect = True
        OpenDlg.ShowDialog()
        If File.Exists(OpenDlg.FileName) Then
            Lock_UI()
            ProcessedFiles = 0
            TotalFiles = OpenDlg.FileNames.Count
            Dim ErrFiles As String = Nothing
            For Each FileName As String In OpenDlg.FileNames
                Dim OK As Boolean
                Dim Name As String = Path.GetFileName(FileName)
                If Path.GetExtension(Name) = ".png" Then Name = Path.ChangeExtension(Name, ".op2")
                For i As Integer = 0 To Files.Length - 1
                    If Files(i).Name = Name Then
                        Dim Index As Integer = i
                        ThreadPool.QueueUserWorkItem(Sub() Insere_Arquivo(FileName, Index))
                        OK = True
                        Exit For
                    End If
                Next
                If Not OK Then ErrFiles &= FileName & vbCrLf
            Next
            If ErrFiles <> Nothing Then
                MsgBox("Os seguintes arquivos não foram encontrados no PAK:" & vbCrLf & ErrFiles, vbExclamation)
                Progresso.Value = 0
                Unlock_UI()
                Atualiza_Lista()
            End If
        End If
    End Sub
    Private Sub Insere_Arquivo(FileName As String, Index As Integer)
        InsertMode = True
        Dim FileData() As Byte
        If Path.GetExtension(FileName) = ".png" Then
            Dim OriginalData() As Byte = IIf(Files(Index).Comp_Size > 0, LZSSDecomp(Files(Index).Data, Files(Index).Decomp_Size), Files(Index).Data)
            Dim Img As New BitmapImage(New Uri(FileName))
            Dim W As Integer = Img.Width
            Dim H As Integer = Img.Height
            Dim BPP As Integer = Endian32(Read32(OriginalData, 12)) 'IIf(Img.Format = PixelFormats.Bgra32, 32, 24)
            If BPP <> 24 And BPP <> 32 Then
                Erro = True
                ProcessedFiles += 1
                Progresso.Dispatcher.BeginInvoke(New Action(Sub() UpdateProgress()))
                Exit Sub
            End If
            Dim Stride As Integer = ((W * 32 + (32 - 1)) And (Not (32 - 1))) / 8
            Dim DIBSize As Integer = W * H * (BPP / 8)
            Dim TmpPixels((Stride * H) - 1) As Byte
            Img.CopyPixels(TmpPixels, Stride, 0)

            Dim Temp() As Byte
            If BPP = 32 Then
                Temp = LZSSComp(TmpPixels)
            Else
                'Converte um array Bgra32 para um array Bgr24
                Dim Pixels(DIBSize - 1) As Byte
                Dim k As Integer
                For j As Integer = 0 To TmpPixels.Length - 1 Step 4
                    Pixels(k) = TmpPixels(j)
                    Pixels(k + 1) = TmpPixels(j + 1)
                    Pixels(k + 2) = TmpPixels(j + 2)
                    k += 3
                Next
                Temp = LZSSComp(Pixels)
            End If

            Dim DataOffset As Integer = Endian32(Read32(OriginalData, 20))
            ReDim FileData(DataOffset + Temp.Length - 1)
            Buffer.BlockCopy(OriginalData, 0, FileData, 0, DataOffset) 'Copia o header original
            Write32(FileData, 4, Endian32(W)) 'Escreve a nova largura da imagem
            Write32(FileData, 8, Endian32(H)) 'Escreve a nova altura da imagem
            'Write32(Data, 12, Endian32(BPP)) 'Escreve novo BPP
            Write32(FileData, 24, Endian32(DIBSize)) 'Escreve novo tamanho da imagem
            Buffer.BlockCopy(Temp, 0, FileData, DataOffset, Temp.Length)
        Else
            FileData = File.ReadAllBytes(FileName)
        End If

        Files(Index).Decomp_Size = FileData.Length
        If Files(Index).Comp_Size > 0 Then
            Files(Index).Data = LZSSComp(FileData)
            Files(Index).Comp_Size = Files(Index).Data.Length
        Else
            Files(Index).Data = FileData
        End If

        RecalcAll()

        ProcessedFiles += 1
        Progresso.Dispatcher.BeginInvoke(New Action(Sub() UpdateProgress()))
    End Sub
    Private Sub UpdateProgress()
        If ProcessedFiles = TotalFiles Then
            Progresso.Value = 0
            GC.Collect() 'Força limpeza de RAM, já que terminamos tudo
            Unlock_UI()
            If InsertMode Then
                InsertMode = False
                Atualiza_Lista()
            End If
            If Erro Then
                Erro = False
                MsgBox("Ocorreram alguns erros no processo!", vbExclamation)
            End If
        Else
            Progresso.Value = (ProcessedFiles / TotalFiles) * 100
        End If
    End Sub
    Private Sub BtnModoComp_Click(sender As Object, e As RoutedEventArgs) Handles BtnModoComp.Click
        Fast_Compression = Not Fast_Compression
        If Fast_Compression Then CompText.Texto = "Normal" Else CompText.Texto = "Máxima"
    End Sub
    Private Sub About()
        Dim Msg As String = "DCGF-Tool rev. 4" & vbCrLf & vbCrLf
        Msg &= "Extrator e compressor para o jogo:" & vbCrLf & "D.C. Girl's Symphony ～ダ・カーポ～ ガールズシンフォニー" & vbCrLf & vbCrLf
        Msg &= "Feito por gdkchan - 2014"
        MsgBox(Msg, vbInformation)
    End Sub
    Private Sub Lock_UI()
        BtnAbrePAK.IsEnabled = False
        BtnSalvaPAK.IsEnabled = False
        BtnExtrair.IsEnabled = False
        BtnInserir.IsEnabled = False
    End Sub
    Private Sub Unlock_UI()
        BtnAbrePAK.IsEnabled = True
        BtnSalvaPAK.IsEnabled = True
        BtnExtrair.IsEnabled = True
        BtnInserir.IsEnabled = True
    End Sub
End Class
