using System.Collections.ObjectModel;
using System.Net;
using System.Text.Json;
using System.Windows.Input;
using System.Windows.Threading;
using PlcMonitorWpf.Models;

namespace PlcMonitorWpf.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly DispatcherTimer _timer;
    private readonly Random _random = new();
    private PlcDevice _selectedPlc;
    private string _currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    private string _cameraIp = string.Empty;
    private readonly string _settingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PlcMonitorWpf", "settings.json");

    public MainViewModel()
    {
        PlcDevices = new ObservableCollection<PlcDevice>
        {
            new("PLC-01 生产线", "192.168.1.10", true, 36.5, 0.42) { MotorStatus = "运行中" },
            new("PLC-02 包装机", "192.168.1.11", true, 31.8, 0.35) { MotorStatus = "停止" },
            new("PLC-03 空压机", "192.168.1.12", true, 42.1, 0.61) { MotorStatus = "运行中" },
            new("PLC-04 备用设备", "192.168.1.13", false, 0, 0) { Mode = "--" }
        };
        _selectedPlc = PlcDevices[0];
        var settings = LoadSettings();
        if (!string.IsNullOrWhiteSpace(settings.PlcIp)) _selectedPlc.IpAddress = settings.PlcIp;
        _cameraIp = settings.CameraIp ?? string.Empty;
        Alarms = new ObservableCollection<AlarmEntry>
        {
            new("PLC-04 通讯中断，请检查网线或电源。"),
            new("PLC-01 运行正常。")
        };
        PlcSignals = new ObservableCollection<SignalItem>
        {
            new("加速允许", "PLC 开关量", "ON", true),
            new("减速允许", "PLC 开关量", "OFF", false),
            new("运行速度", "PLC 模拟量", "18.6 m/min", true),
            new("系统压力", "PLC 模拟量", "0.42 MPa", true)
        };
        SqlChartBars = new ObservableCollection<ChartBar>
        {
            new("08:00", "12", 48), new("09:00", "18", 72), new("10:00", "15", 60),
            new("11:00", "23", 96), new("12:00", "20", 82)
        };
        DatabaseRecords = new ObservableCollection<ProductionRecord>
        {
            new("TR-20260712-001", "作业中", "18:42", false),
            new("TR-20260712-000", "已完成", "18:31", true),
            new("TR-20260711-092", "已完成", "17:58", true),
            new("TR-20260711-091", "已完成", "17:24", true)
        };

        ToggleRunCommand = new RelayCommand(_ => ToggleRun());
        ResetAlarmCommand = new RelayCommand(_ => ResetAlarm());
        SavePlcIpCommand = new RelayCommand(_ => SavePlcIp());
        SaveCameraIpCommand = new RelayCommand(_ => SaveCameraIp());
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (_, _) => RefreshSimulation();
        _timer.Start();
    }

    public ObservableCollection<PlcDevice> PlcDevices { get; }
    public ObservableCollection<AlarmEntry> Alarms { get; }
    public ObservableCollection<SignalItem> PlcSignals { get; }
    public ObservableCollection<ChartBar> SqlChartBars { get; }
    public ObservableCollection<ProductionRecord> DatabaseRecords { get; }
    public ICommand ToggleRunCommand { get; }
    public ICommand ResetAlarmCommand { get; }
    public ICommand SavePlcIpCommand { get; }
    public ICommand SaveCameraIpCommand { get; }
    public int AlarmCount => Alarms.Count;

    public PlcDevice SelectedPlc
    {
        get => _selectedPlc;
        set => SetProperty(ref _selectedPlc, value);
    }

    public string CurrentTime { get => _currentTime; private set => SetProperty(ref _currentTime, value); }
    public string CameraIp { get => _cameraIp; set => SetProperty(ref _cameraIp, value); }
    public string CurrentVehicleNumber => "TR-20260712-001";
    public string CurrentWorkStatus => "装载中";
    public string EquipmentStatus => "运行正常";
    public string CompletedVehicleCount => "128";
    public string PendingVehicleCount => "6";

    private void RefreshSimulation()
    {
        CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        foreach (var plc in PlcDevices.Where(x => x.IsOnline))
        {
            plc.Temperature = Math.Clamp(plc.Temperature + (_random.NextDouble() - 0.5) * 1.2, 22, 58);
            plc.Pressure = Math.Clamp(plc.Pressure + (_random.NextDouble() - 0.5) * 0.04, 0.2, 0.9);
            plc.LastUpdated = DateTime.Now.ToString("HH:mm:ss");
            plc.AddTemperatureSample(plc.Temperature);
        }
        // 后续接入 PLC 后，在此处用实际 Tag 数据更新 PlcSignals。
        PlcSignals[2].Value = $"{15 + _random.NextDouble() * 6:F1} m/min";
    }

    private void ToggleRun()
    {
        if (!SelectedPlc.IsOnline)
        {
            AddAlarm($"{SelectedPlc.Name} 当前离线，无法执行控制。");
            return;
        }
        SelectedPlc.MotorStatus = SelectedPlc.MotorStatus == "运行中" ? "停止" : "运行中";
        AddAlarm($"{SelectedPlc.Name} 已{(SelectedPlc.MotorStatus == "运行中" ? "启动" : "停止")}。");
    }

    private void ResetAlarm()
    {
        AddAlarm($"{SelectedPlc.Name} 已执行报警复位。");
    }

    private void SavePlcIp()
    {
        if (!IPAddress.TryParse(SelectedPlc.IpAddress, out _))
        {
            AddAlarm("PLC IP 地址格式不正确，未保存。");
            return;
        }
        SaveSettings();
        AddAlarm($"PLC IP 已保存：{SelectedPlc.IpAddress}");
    }

    private void SaveCameraIp()
    {
        if (!IPAddress.TryParse(CameraIp, out _))
        {
            AddAlarm("摄像头 IP 地址格式不正确，未保存。");
            return;
        }
        SaveSettings();
        AddAlarm($"摄像头 IP 已保存：{CameraIp}");
    }

    private AppSettings LoadSettings()
    {
        try
        {
            return File.Exists(_settingsPath)
                ? JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(_settingsPath)) ?? new AppSettings()
                : new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    private void SaveSettings()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
            var settings = new AppSettings { PlcIp = SelectedPlc.IpAddress, CameraIp = CameraIp };
            File.WriteAllText(_settingsPath, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch
        {
            AddAlarm("配置文件保存失败，请检查本机权限。");
        }
    }

    private void AddAlarm(string message)
    {
        Alarms.Insert(0, new AlarmEntry(message));
        while (Alarms.Count > 20) Alarms.RemoveAt(Alarms.Count - 1);
        OnPropertyChanged(nameof(AlarmCount));
    }
}
