using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileAssocDefender.Models;
using FileAssocDefender.Services;

namespace FileAssocDefender.ViewModels;

public partial class LogViewModel : ObservableObject
{
    private readonly LogService _logService;

    public LogViewModel(LogService logService)
    {
        _logService = logService;
        Entries = new ObservableCollection<LogEntry>(_logService.Entries);
        _logService.EntriesChanged += OnEntriesChanged;
    }

    public ObservableCollection<LogEntry> Entries { get; }

    [RelayCommand]
    private void Clear()
    {
        _logService.Clear();
    }

    private void OnEntriesChanged()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            Entries.Clear();
            foreach (var entry in _logService.Entries)
            {
                Entries.Add(entry);
            }
        });
    }
}
