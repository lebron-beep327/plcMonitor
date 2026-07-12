using System.Windows.Media;

namespace PlcMonitorWpf.Models;

public sealed class PlcDevice : ObservableObject
{
    private readonly Queue<double> _temperatureSamples = new();
    private bool _isOnline;
    private double _temperature;
    private double _pressure;
    private string _motorStatus = "停止";
    private string _mode = "自动";
    private string _lastUpdated = "--";
    private PointCollection _trendPoints = new();

    private string _ipAddress;

    public PlcDevice(string name, string ipAddress, bool isOnline, double temperature, double pressure)
    {
        Name = name;
        _ipAddress = ipAddress;
        IsOnline = isOnline;
        Temperature = temperature;
        Pressure = pressure;
        for (var i = 0; i < 10; i++) AddTemperatureSample(temperature + i % 3 - 1);
    }

    public string Name { get; }
    public string IpAddress { get => _ipAddress; set => SetProperty(ref _ipAddress, value); }

    public bool IsOnline
    {
        get => _isOnline;
        set
        {
            if (!SetProperty(ref _isOnline, value)) return;
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(StatusBrush));
        }
    }

    public string StatusText => IsOnline ? "在线" : "离线";
    public Brush StatusBrush => IsOnline ? Brushes.MediumSeaGreen : Brushes.IndianRed;

    public double Temperature { get => _temperature; set => SetProperty(ref _temperature, value); }
    public double Pressure { get => _pressure; set => SetProperty(ref _pressure, value); }
    public string MotorStatus
    {
        get => _motorStatus;
        set
        {
            if (!SetProperty(ref _motorStatus, value)) return;
            OnPropertyChanged(nameof(MotorBrush));
            OnPropertyChanged(nameof(ValveStatus));
        }
    }
    public Brush MotorBrush => MotorStatus == "运行中" ? Brushes.MediumSeaGreen : Brushes.Orange;
    public string ValveStatus => MotorStatus == "运行中" ? "已开启" : "已关闭";
    public string Mode { get => _mode; set => SetProperty(ref _mode, value); }
    public string LastUpdated { get => _lastUpdated; set => SetProperty(ref _lastUpdated, value); }
    public PointCollection TrendPoints { get => _trendPoints; private set => SetProperty(ref _trendPoints, value); }

    public void AddTemperatureSample(double value)
    {
        _temperatureSamples.Enqueue(value);
        while (_temperatureSamples.Count > 14) _temperatureSamples.Dequeue();
        var samples = _temperatureSamples.ToArray();
        var min = Math.Min(20, samples.Min() - 2);
        var max = Math.Max(60, samples.Max() + 2);
        var points = new PointCollection();
        for (var i = 0; i < samples.Length; i++)
        {
            var x = samples.Length == 1 ? 0 : 250d * i / (samples.Length - 1);
            var y = 88d - (samples[i] - min) / (max - min) * 72d;
            points.Add(new System.Windows.Point(x, Math.Clamp(y, 10, 88)));
        }
        TrendPoints = points;
    }
}
