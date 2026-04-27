using LibreHardwareMonitor.Hardware;
using System.Collections;

namespace PCStatusAgent.Services;

public class HardwareService : IDisposable
{
    private readonly Computer _computer;

    public HardwareService()
    {
        _computer = new Computer
        {
            IsCpuEnabled = true,
            IsMemoryEnabled = true,
        };
        _computer.Open();
    }

    public float GetCpuPercent()
    {
        foreach (var hardware in _computer.Hardware)
        {
            if (hardware.HardwareType == HardwareType.Cpu)
            {
                hardware.Update();
                foreach (var sensor in hardware.Sensors)
                {
                    if (sensor.SensorType == SensorType.Load &&
                        sensor.Name == "CPU Total")
                    {
                        return sensor.Value ?? 0;
                    }
                }
            }
        }
        return 0;
    }

    public float GetCpuTemperature()
    {
        foreach (var hardware in _computer.Hardware)
        {
            if (hardware.HardwareType == HardwareType.Cpu)
            {
                hardware.Update();
                foreach (var sensor in hardware.Sensors)
                {
                    if (sensor.SensorType == SensorType.Temperature)
                    {
                        return sensor.Value ?? 0;
                    }
                }
            }
        }
        return 0;
    }

    public (float used, float total, float percent) GetRamInfo()
    {
        foreach (var hardware in _computer.Hardware)
        {
            if (hardware.HardwareType == HardwareType.Memory)
            {
                hardware.Update();
                float used = 0, available = 0;
                foreach (var sensor in hardware.Sensors)
                {
                    if (sensor.SensorType == SensorType.Data)
                    {
                        if (sensor.Name == "Memory Used")
                            used = sensor.Value ?? 0;
                        if (sensor.Name == "Memory Available")
                            available = sensor.Value ?? 0;
                    }
                }
                float total = used + available;
                float percent = total > 0 ? (used / total) * 100 : 0;
                return (used, total, percent);
            }
        }
        return (0, 0, 0);
    }

    public void Dispose()
    {
        _computer.Close();
    }
}