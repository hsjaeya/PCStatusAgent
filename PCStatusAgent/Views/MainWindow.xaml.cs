using System.ComponentModel;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using PCStatusAgent.Services;

namespace PCStatusAgent.Views;

public partial class MainWindow : Window
{
    private readonly SupabaseService _supabase;
    private TaskbarIcon? _trayIcon;

    public MainWindow(SupabaseService supabase)
    {
        InitializeComponent();
        _supabase = supabase;
        InitTrayIcon();
        _ = StartAsync();
    }

    private void InitTrayIcon()
    {
        _trayIcon = (TaskbarIcon)System.Windows.Application.Current.Resources["TrayIcon"];

        var menu = new System.Windows.Controls.ContextMenu();

        var screenWidth = SystemParameters.PrimaryScreenWidth;
        var screenHeight = SystemParameters.PrimaryScreenHeight;
        Left = screenWidth - Width - 10;
        Top = screenHeight - Height - 50;

        var openItem = new System.Windows.Controls.MenuItem { Header = "열기" };
        openItem.Click += (s, e) => ShowWindow();

        var exitItem = new System.Windows.Controls.MenuItem { Header = "종료" };
        exitItem.Click += (s, e) => ExitApp();

        menu.Items.Add(openItem);
        menu.Items.Add(exitItem);

        _trayIcon.ContextMenu = menu;
        _trayIcon.TrayMouseDoubleClick += (s, e) => ShowWindow();
    }

    private void ShowWindow()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    private void ExitApp()
    {
        _trayIcon?.Dispose();
        System.Windows.Application.Current.Shutdown();
    }

    protected override void OnStateChanged(EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
            Hide();
        base.OnStateChanged(e);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }

    private async Task StartAsync()
    {
        StatusText.Text = "Supabase 연결 중...";
        _ = Task.Run(() => _supabase.StartListeningAsync());
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