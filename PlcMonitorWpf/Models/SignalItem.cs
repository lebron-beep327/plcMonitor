using System.Windows.Media;

namespace PlcMonitorWpf.Models;

public sealed class SignalItem : ObservableObject
{
    private string _value;
    private bool _isNormal;

    public SignalItem(string name, string signalType, string value, bool isNormal)
    {
        Name = name;
        SignalType = signalType;
        _value = value;
        _isNormal = isNormal;
    }

    public string Name { get; }
    public string SignalType { get; }
    public string Value { get => _value; set => SetProperty(ref _value, value); }
    public bool IsNormal
    {
        get => _isNormal;
        set
        {
            if (!SetProperty(ref _isNormal, value)) return;
            OnPropertyChanged(nameof(StatusBrush));
        }
    }
    public Brush StatusBrush => IsNormal ? Brushes.MediumSeaGreen : Brushes.Orange;
}
