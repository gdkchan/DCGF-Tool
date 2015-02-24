Imports System.IO
Module Utils
    Public Function Read_File_Name(InFile As FileStream) As String
        Dim Nome As String = Nothing
        For i As Integer = 1 To 30
            Dim ChrNum As Integer = Read(InFile, 2)
            If ChrNum <> 0 Then Nome &= Char.ConvertFromUtf32(ChrNum)
        Next
        Return Nome.Trim
    End Function
    Public Function Read(InFile As FileStream, Bytes As Integer) As Long
        Dim Val As Long
        Dim Mult As Long = 1
        For i As Integer = 1 To Bytes
            Dim CurrByte As Byte = InFile.ReadByte
            Val += (((CurrByte And &HF0) / &H10) * Mult)
            Mult *= &H10
            Val += ((CurrByte And &HF) * Mult)
            Mult *= &H10
        Next
        Return Val
    End Function
    Public Function Endian32(Address As Integer) As Integer
        Return ((Address And &HFF) << 24) + _
            ((Address And &HFF00) << 8) + _
            ((Address And &HFF0000) >> 8) + _
            ((Address >> 24) And &HFF)
    End Function
    Public Function Endian16(Address As Integer) As Integer
        Return ((Address And &HFF) << 8) + _
            ((Address And &HFF00) >> 8)
    End Function
    Public Function Read32(Data() As Byte, Address As Integer) As Integer
        Return (Data(Address + 3) And &HFF) + _
            ((Data(Address + 2) And &HFF) << 8) + _
            ((Data(Address + 1) And &HFF) << 16) + _
            ((Data(Address) And &HFF) << 24)
    End Function
    Public Function Read16(Data() As Byte, Address As Integer) As Integer
        Return (Data(Address + 1) And &HFF) + _
            ((Data(Address) And &HFF) << 8)
    End Function
    Public Sub Write32(Data() As Byte, Address As Integer, Value As Integer)
        Data(Address) = ((Value And &HFF000000) >> 24) And &HFF
        Data(Address + 1) = ((Value And &HFF0000) >> 16) And &HFF
        Data(Address + 2) = ((Value And &HFF00) >> 8) And &HFF
        Data(Address + 3) = Value And &HFF
    End Sub
    Public Sub Write16(Data() As Byte, Address As Integer, Value As Integer)
        Data(Address) = ((Value And &HFF00) >> 8) And &HFF
        Data(Address + 1) = Value And &HFF
    End Sub
    Public Function CheckBytes(InData() As Byte, Offset As Integer, BytesToCheck() As Byte) As Boolean
        Dim Fail As Boolean
        Dim i As Integer
        If Offset + BytesToCheck.Length > InData.Length Then Return False
        For Pos As Integer = Offset To Offset + (BytesToCheck.Length - 1)
            If InData(Pos) <> BytesToCheck(i) Then
                Fail = True
                Exit For
            End If
            i += 1
        Next
        Return Not Fail
    End Function
    Public Sub Write_File_Name(OutFile As FileStream, Nome As String)
        For i As Integer = 0 To 29
            If i < Len(Nome) Then
                Write(OutFile, Char.ConvertToUtf32(Nome, i), 2)
            Else
                Write(OutFile, 0, 2)
            End If
        Next
    End Sub
    Public Sub Write(OutFile As FileStream, Value As Integer, Bytes As Integer)
        Dim Val, RealVal As Byte
        Dim Mult1 As Long = 1
        Dim Mult2 As Long = &HFF
        For i As Integer = 1 To Bytes
            Val = ((Value And Mult2) / Mult1)
            Mult1 *= &H100
            Mult2 *= &H100
            RealVal = ((Val And &HF0) / &H10) + ((Val And &HF) * &H10)
            OutFile.WriteByte(RealVal)
        Next
    End Sub
End Module
