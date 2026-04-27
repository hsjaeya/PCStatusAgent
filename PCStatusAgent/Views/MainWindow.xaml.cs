using System.Windows;
using PCStatusAgent.Services;

namespace PCStatusAgent.Views;

public partial class MainWindow : Window
{
    private readonly SupabaseService _supabase;

    public MainWindow(SupabaseService supabase)
    {
        InitializeComponent();
        _supabase = supabase;
        _ = StartAsync();
    }

    private async Task StartAsync()
    {
        StatusText.Text = "Supabase 연결 중...";

        // 백그라운드에서 실행
        _ = Task.Run(() => _supabase.StartListeningAsync());

        // 바로 대기 중으로 변경
        await Task.Delay(1000);
        Dispatcher.Invoke(() =>
        {
            StatusText.Text = "명령 대기 중...";
        });
    }

    private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }
}