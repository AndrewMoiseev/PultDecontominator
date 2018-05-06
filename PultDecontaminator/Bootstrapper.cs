using Prism.Unity;
using PultDecontominator.Views;
using System.Windows;

namespace PultDecontominator
{
    class Bootstrapper : UnityBootstrapper
    {
        protected override DependencyObject CreateShell()
        {
            return Container.TryResolve<MainWindow>();
        }

        protected override void InitializeShell()
        {
            //Application.Current.MainWindow.Show();
        }
    }
}
