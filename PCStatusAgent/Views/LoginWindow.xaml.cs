using System.Windows;
using PCStatusAgent.Services;

namespace PCStatusAgent.Views;

public partial class LoginWindow : Window
{
    private readonly SupabaseService _supabase = new();

    public LoginWindow()
    {
        InitializeComponent();
    }

    private async void LoginBtn_Click(object sender, RoutedEventArgs e)
    {
        var email = EmailBox.Text.Trim();
        var password = PasswordBox.Password;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            MessageBox.Show("이메일과 비밀번호를 입력하세요.");
            return;
        }

        LoginBtn.IsEnabled = false;
        LoginBtn.Content = "로그인 중...";

        var success = await _supabase.LoginAsync(email, password);

        if (success)
        {
            var main = new MainWindow(_supabase);
            main.Show();
            Close();
        }
        else
        {
            MessageBox.Show("로그인 실패. 이메일 또는 비밀번호를 확인하세요.");
            LoginBtn.IsEnabled = true;
            LoginBtn.Content = "로그인";
        }
    }

    private async void RegisterBtn_Click(object sender, RoutedEventArgs e)
    {
        var email = EmailBox.Text.Trim();
        var password = PasswordBox.Password;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            MessageBox.Show("이메일과 비밀번호를 입력하세요.");
            return;
        }

        var success = await _supabase.RegisterAsync(email, password);

        if (success)
            MessageBox.Show("회원가입 완료! 로그인하세요.");
        else
            MessageBox.Show("회원가입 실패. 다시 시도하세요.");
    }
}