using Prism.Unity;
using PultDecontominator.Views;
using System.Windows;

namespace PultDecontominator
{
    class Bootstrapper : UnityBootstrapper
    {
        protected override DependencyObject CreateShell()
        {
//            return Container.TryResolve<MainWindow>();
            return Container.TryResolve<Auth>();
        }

        protected override void InitializeShell()
        {
//            Application.Current.Auth.Show();
            Application.Current.MainWindow.Show();
        }
    }
}
