namespace PlcMonitorWpf.Models;

public sealed class ChartBar
{
    public ChartBar(string label, string valueText, double barHeight)
    {
        Label = label;
        ValueText = valueText;
        BarHeight = barHeight;
    }

    public string Label { get; }
    public string ValueText { get; }
    public double BarHeight { get; }
}
