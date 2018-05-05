using Microsoft.Practices.Unity;
using Prism.Unity;
using System.Windows;
using PultDecontominator.Views;

namespace PultDecontominator
{
    class Bootstrapper : UnityBootstrapper
    {
        protected override DependencyObject CreateShell()
        {
           return Container.Resolve<MainWindow>();
        }

        protected override void InitializeShell()
        {
            Application.Current.MainWindow.Show();
        }
    }
}
