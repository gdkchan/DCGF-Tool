Imports System.IO
Module LZSS
    Public Fast_Compression As Boolean = True
    Public Function LZSSDecomp(InData() As Byte, Decomp_Size As Integer) As Byte()
        Dim Data(Decomp_Size - 1) As Byte
        Dim Dic(&HFFF) As Byte
        Dim LenOffset As Integer = 3

        Dim SrcPtr, DstPtr, BitCount As Integer
        Dim DicPtr As Integer = 4078
        Dim BitFlags As Byte
        While DstPtr < Decomp_Size And SrcPtr + 2 < InData.Length
            If BitCount = 0 Then
                BitFlags = InData(SrcPtr)
                SrcPtr += 1
                BitCount = 8
            End If

            If BitFlags And 1 Then
                Dic(DicPtr) = InData(SrcPtr)
                SrcPtr += 1
                Data(DstPtr) = Dic(DicPtr)
                DicPtr = (DicPtr + 1) And &HFFF
                DstPtr += 1
            Else
                Dim Byte_Low As Byte = InData(SrcPtr)
                Dim Byte_High As Byte = InData(SrcPtr + 1)
                SrcPtr += 2

                '2 bytes -> 12 bits = endereço do dicionário, 4 bits tamanho
                Dim Back As Integer = ((Byte_High And &HF0) << 4) Or Byte_Low
                Dim Len As Integer = (Byte_High And &HF) + LenOffset

                If DstPtr + Len > Decomp_Size Then Len = Decomp_Size - DstPtr
                For j As Integer = 0 To Len - 1
                    Dic(DicPtr) = Dic(Back)
                    Data(DstPtr) = Dic(DicPtr)
                    DicPtr = (DicPtr + 1) And &HFFF
                    Back = (Back + 1) And &HFFF
                    DstPtr += 1
                Next
            End If
            BitFlags >>= 1
            BitCount -= 1
        End While

        Return Data
    End Function
    Public Function LZSSComp(InData() As Byte) As Byte()
        Dim Dic(&HFFF) As Byte
        Dim DicPtr As Integer = 4078

        Dim Data(InData.Length - 1 + ((InData.Length - 1) / 8)) As Byte
        Dim SrcPtr, DstPtr, BitCount As Integer
        Dim BitsPtr As Integer

        While SrcPtr < InData.Length
            If BitCount = 0 Then
                BitsPtr = DstPtr
                DstPtr += 1
                BitCount = 8
            End If

            Dim Back As Integer = 0
            Dim Found_Data As Integer = 0
            Dim Compressed_Data As Boolean = False
            Dim Index As Integer = Array.IndexOf(Dic, InData(SrcPtr))
            If Index <> -1 Then
                If Fast_Compression Then
                    Found_Data = 0
                    For j As Integer = 0 To 17
                        If SrcPtr + j >= InData.Length Then Exit For
                        If Dic((Index + j) And &HFFF) = InData(SrcPtr + j) Then
                            Found_Data += 1
                        Else
                            Exit For
                        End If
                    Next
                    If Found_Data >= 3 Then
                        If Index + Found_Data < DicPtr Or Index > DicPtr + Found_Data Then
                            Compressed_Data = True
                            Back = Index
                        End If
                    End If
                Else
                    Do
                        Dim DataSize As Integer = 0
                        For j As Integer = 0 To 17
                            If SrcPtr + j >= InData.Length Then Exit For
                            If Dic((Index + j) And &HFFF) = InData(SrcPtr + j) Then
                                DataSize += 1
                            Else
                                Exit For
                            End If
                        Next
                        If DataSize >= 3 Then
                            If Index + DataSize < DicPtr Or Index > DicPtr + DataSize Then
                                If DataSize > Found_Data Then
                                    Compressed_Data = True
                                    Found_Data = DataSize
                                    Back = Index
                                End If
                            End If
                        End If
                        Index = Array.IndexOf(Dic, InData(SrcPtr), Index + 1)
                        If Index = -1 Then Exit Do
                    Loop
                End If
            End If
            If Compressed_Data Then
                Data(DstPtr) = Back And &HFF
                Data(DstPtr + 1) = ((Found_Data - 3) And &HF) + ((Back And &HF00) >> 4)
                DstPtr += 2
                For j As Integer = 0 To Found_Data - 1
                    Dic(DicPtr) = InData(SrcPtr)
                    DicPtr = (DicPtr + 1) And &HFFF
                    SrcPtr += 1
                Next
            Else
                Data(BitsPtr) = Data(BitsPtr) Or (2 ^ (8 - BitCount)) 'Não comprimido, define bit
                Data(DstPtr) = InData(SrcPtr)
                Dic(DicPtr) = Data(DstPtr)
                DicPtr = (DicPtr + 1) And &HFFF
                DstPtr += 1
                SrcPtr += 1
            End If

            BitCount -= 1
        End While

        ReDim Preserve Data(DstPtr)

        Return Data
    End Function
End Module
