using MaterialSkin.Controls;
using System.Threading;
using System.Windows.Forms;

namespace JJManager.Class.App
{
    internal class WaitForm
    {
        JJManager.Pages.App.WaitForm wait;
        Thread loadthread;

        public void Show()
        {
            loadthread = new Thread(new ThreadStart(LoadingProcess));
            loadthread.Name = "Load_Form";
            loadthread.TrySetApartmentState(ApartmentState.STA);
            loadthread.IsBackground = true;
            loadthread.Start();
        }

        public void Show(MaterialForm parent)
        {
            loadthread = new Thread(new ParameterizedThreadStart(LoadingProcess));
            loadthread.TrySetApartmentState(ApartmentState.STA);
            loadthread.Name = "Load_Form";
            loadthread.IsBackground = true;
            loadthread.Start(parent);
        }

        public void Close()
        {
            if (wait != null)
            {
                wait.BeginInvoke(new System.Threading.ThreadStart(wait.CloseWaitForm));
                wait = null;
                loadthread = null;
            }
        }

        private void LoadingProcess()
        {
            wait = new JJManager.Pages.App.WaitForm();
            if (wait.InvokeRequired)
            {
                wait.BeginInvoke((MethodInvoker)delegate
                {
                    wait = new JJManager.Pages.App.WaitForm();
                    wait.Show();
                });
            }
            else
            {
                wait = new JJManager.Pages.App.WaitForm();
                wait.Show();
            }
        }

        private void LoadingProcess(object parent)
        {
            MaterialForm parent1 = parent as MaterialForm;
            if (parent1.InvokeRequired)
            {
                parent1.BeginInvoke((MethodInvoker)delegate
                {
                    wait = new JJManager.Pages.App.WaitForm(parent1);
                    wait.Show();
                });
            }
            else
            {
                wait = new JJManager.Pages.App.WaitForm(parent1);
                wait.Show();
            }
        }
    }
}
