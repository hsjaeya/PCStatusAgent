using System.Windows;
using PCStatusAgent.Views;

namespace PCStatusAgent;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var login = new LoginWindow();
        login.Show();
    }
}