namespace PlcMonitorWpf.Models;

public sealed class AlarmEntry
{
    public AlarmEntry(string message) => Message = message;
    public string Message { get; }
    public string Time { get; } = DateTime.Now.ToString("HH:mm:ss");
}
