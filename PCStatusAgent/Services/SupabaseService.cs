using PCStatusAgent.Models;
using Supabase;
using Supabase.Realtime.PostgresChanges;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using static Supabase.Realtime.PostgresChanges.PostgresChangesOptions;

namespace PCStatusAgent.Services;

public class SupabaseService
{
    private const string URL = "https://xjopjgrraxjrfyidpnmj.supabase.co";
    private const string KEY = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Inhqb3BqZ3JyYXhqcmZ5aWRwbm1qIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NzcyNTQ2OTAsImV4cCI6MjA5MjgzMDY5MH0.dW47-fOvejSzcpzEyEOlPVZVV1rEYWdMVcknk_f9e3s";

    private Client? _client;

    public async Task<bool> LoginAsync(string email, string password)
    {
        _client = new Client(URL, KEY);
        await _client.InitializeAsync();

        var session = await _client.Auth.SignIn(email, password);
        return session?.User != null;
    }

    public async Task<bool> RegisterAsync(string email, string password)
    {
        _client = new Client(URL, KEY);
        await _client.InitializeAsync();

        var session = await _client.Auth.SignUp(email, password);
        return session?.User != null;
    }

    public async Task StartListeningAsync()
    {
        if (_client == null) return;

        System.Diagnostics.Debug.WriteLine("폴링 시작");

        _ = StartHeartbeatAsync();

        while (true)
        {
            try
            {
                var userId = _client.Auth.CurrentUser?.Id;
                if (userId == null)
                {
                    await Task.Delay(500);
                    continue;
                }

                var result = await _client.From<Command>()
                    .Where(c => c.UserId == userId && c.IsExecuted == false)
                    .Order(c => c.CreatedAt, Supabase.Postgrest.Constants.Ordering.Ascending)
                    .Limit(1)
                    .Get();

                var command = result.Models.FirstOrDefault();

                if (command != null)
                {
                    System.Diagnostics.Debug.WriteLine($"명령 감지: {command.CommandType}");

                    if (command.CommandType == "lock")
                    {
                        LockWorkStation();
                        await MarkExecutedAsync(command.Id);
                    }
                    else if (command.CommandType == "restart")
                    {
                        Process.Start("shutdown", "/r /t 0");
                        await MarkExecutedAsync(command.Id);
                    }
                    else if (command.CommandType == "shutdown")
                    {
                        Process.Start("shutdown", "/s /t 0");
                        await MarkExecutedAsync(command.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"폴링 오류: {ex.Message}");
            }

            await Task.Delay(2000);
        }
    }

    private readonly HardwareService _hardware = new();

    private async Task StartHeartbeatAsync()
    {
        while (true)
        {
            try
            {
                var userId = _client?.Auth.CurrentUser?.Id;
                if (userId != null)
                {
                    var cpu = _hardware.GetCpuPercent();
                    var temp = _hardware.GetCpuTemperature();
                    var (ramUsed, ramTotal, ramPercent) = _hardware.GetRamInfo();

                    // 프로세스 목록 수집
                    var processes = Process.GetProcesses()
                        .OrderByDescending(p => {
                            try { return p.WorkingSet64; } catch { return 0; }
                        })
                        .Take(10)
                        .Select(p => {
                            try
                            {
                                return new
                                {
                                    name = p.ProcessName,
                                    memory = p.WorkingSet64 / 1024 / 1024
                                };
                            }
                            catch
                            {
                                return new { name = p.ProcessName, memory = 0L };
                            }
                        })
                        .ToList();

                    var processesJson = JsonSerializer.Serialize(processes);

                    System.Diagnostics.Debug.WriteLine(
                        $"CPU: {cpu}%, 온도: {temp}°C, RAM: {ramUsed:F1}/{ramTotal:F1}GB ({ramPercent:F1}%)");

                    await _client!.From<PcOnline>()
                        .Upsert(new PcOnline
                        {
                            UserId = userId,
                            LastSeen = DateTime.UtcNow,
                            CpuPercent = cpu,
                            Temperature = temp,
                            RamUsed = ramUsed,
                            RamTotal = ramTotal,
                            RamPercent = ramPercent,
                            PcName = Environment.MachineName,
                            Processes = processesJson
                        });

                    System.Diagnostics.Debug.WriteLine("하트비트 전송");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Heartbeat 오류: {ex.Message}");
            }

            await Task.Delay(2000);
        }
    }

    private async Task MarkExecutedAsync(long commandId)
    {
        if (_client == null) return;

        await _client.From<Command>()
            .Where(c => c.Id == commandId)
            .Set(c => c.IsExecuted, true)
            .Update();
    }

    [DllImport("user32.dll")]
    private static extern bool LockWorkStation();
}