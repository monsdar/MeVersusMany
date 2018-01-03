using Caliburn.Micro;
using System.Windows;

namespace MeVersusMany
{
    class MyBootstrapper : BootstrapperBase
    {
        public MyBootstrapper()
        {
            try
            {
                Initialize();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Unhandled Exception: " + ex.ToString());
                throw ex;
            }
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            try
            {
                DisplayRootViewFor<UI.ShellViewModel>();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Unhandled Exception: " + ex.ToString());
                throw ex;
            }
        }
    }
}
