Imports System.Collections.ObjectModel
Imports System.Windows.Input
Imports CommunityToolkit.Mvvm.ComponentModel
Imports CommunityToolkit.Mvvm.Input

Namespace ViewModels

    ''' <summary>
    ''' Note: CommunityToolkit.Mvvm's [ObservableProperty]/[RelayCommand] source generators only run
    ''' for C# compilations, so properties/commands here are hand-written against the (fully
    ''' VB-compatible) ObservableObject/RelayCommand base classes instead of generated.
    ''' </summary>
    Public Class MainViewModel
        Inherits ObservableObject

        Private _statusText As String = "Waiting for command..."
        Private _progressValue As Integer
        Private _progressMaximum As Integer = 100

        Public Event RestoreRequested As EventHandler
        Public Event QuitRequested As EventHandler
        Public Event MinimizeToTrayRequested As EventHandler

        Public Sub New()
            LogEntries = New ObservableCollection(Of LogEntry)()
            RestoreCommand = New RelayCommand(Sub() RaiseEvent RestoreRequested(Me, EventArgs.Empty))
            QuitCommand = New RelayCommand(Sub() RaiseEvent QuitRequested(Me, EventArgs.Empty))
            MinimizeToTrayCommand = New RelayCommand(Sub() RaiseEvent MinimizeToTrayRequested(Me, EventArgs.Empty))
        End Sub

        Public Property StatusText As String
            Get
                Return _statusText
            End Get
            Set(value As String)
                SetProperty(_statusText, value)
            End Set
        End Property

        Public Property ProgressValue As Integer
            Get
                Return _progressValue
            End Get
            Set(value As Integer)
                SetProperty(_progressValue, value)
            End Set
        End Property

        Public Property ProgressMaximum As Integer
            Get
                Return _progressMaximum
            End Get
            Set(value As Integer)
                SetProperty(_progressMaximum, value)
            End Set
        End Property

        Public ReadOnly Property LogEntries As ObservableCollection(Of LogEntry)

        Public ReadOnly Property RestoreCommand As ICommand
        Public ReadOnly Property QuitCommand As ICommand
        Public ReadOnly Property MinimizeToTrayCommand As ICommand

        Public Sub UpdateStatus(statusText As String)
            Me.StatusText = statusText
        End Sub

        Public Sub AppendLog(logLine As String)
            LogEntries.Add(New LogEntry With {.Timestamp = DateTime.Now, .Text = logLine})
        End Sub

        Public Sub ClearLog()
            LogEntries.Clear()
        End Sub

        Public Sub SetProgress(current As Integer, maximum As Integer)
            ProgressValue = current
            ProgressMaximum = maximum
        End Sub

    End Class

End Namespace
