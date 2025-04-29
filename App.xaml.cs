using EkzamenADO;
using System.Windows;

namespace EkzamenADO
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            new LoginWindow().Show();
        }
    }
}
