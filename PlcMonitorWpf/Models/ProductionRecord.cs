using System.Windows.Media;

namespace PlcMonitorWpf.Models;

public sealed class ProductionRecord
{
    public ProductionRecord(string vehicleNumber, string status, string recordTime, bool completed)
    {
        VehicleNumber = vehicleNumber;
        Status = status;
        RecordTime = recordTime;
        StatusBrush = completed ? Brushes.MediumSeaGreen : Brushes.Orange;
    }

    public string VehicleNumber { get; }
    public string Status { get; }
    public string RecordTime { get; }
    public Brush StatusBrush { get; }
}
