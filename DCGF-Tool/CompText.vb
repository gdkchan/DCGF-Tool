Imports System.ComponentModel
Public Class CompText
    Implements INotifyPropertyChanged
    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
    Private Sub NotifyPropertyChanged(ByVal Info As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(Info))
    End Sub
    Dim Txt As String = "Normal"
    Public Property Texto As String
        Get
            Return Txt
        End Get
        Set(Value As String)
            Txt = Value
            NotifyPropertyChanged("Texto")
        End Set
    End Property
End Class
