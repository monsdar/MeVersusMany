using Caliburn.Micro;
using System.Runtime.InteropServices;
using System.Windows;

namespace MeVersusMany
{
    //This class is needed to make SetThreadExecutionState accessible. Needed to prevent the system from falling asleep
    //See: https://stackoverflow.com/questions/6302185/how-to-prevent-windows-from-entering-idle-state
    internal static class NativeMethods
    {
        // Import SetThreadExecutionState Win32 API and necessary flags
        [DllImport("kernel32.dll")]
        public static extern uint SetThreadExecutionState(uint esFlags);
        public const uint ES_AWAYMODE_REQUIRED = 0x00000040;
        public const uint ES_CONTINUOUS = 0x80000000;
        public const uint ES_DISPLAY_REQUIRED = 0x00000002;
        public const uint ES_SYSTEM_REQUIRED = 0x00000001;
        // Legacy flag, should not be used.
        //public const uint ES_USER_PRESENT = 0x00000004
    }

    class MyBootstrapper : BootstrapperBase
    {
        private uint executionState = 0;

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

            //prevent system from falling asleep
            SetThreadState();
        }

        ~MyBootstrapper()
        {
            //Set the previous executionstate again when closing down
            NativeMethods.SetThreadExecutionState(executionState);
        }

        private void SetThreadState()
        {
            // Set new state to prevent system sleep
            executionState = NativeMethods.SetThreadExecutionState(NativeMethods.ES_CONTINUOUS | NativeMethods.ES_DISPLAY_REQUIRED);
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
