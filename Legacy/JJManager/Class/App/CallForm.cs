using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JJManager.Class.App
{
    internal class CallForm
    {
        MaterialForm form = null;
        Thread thread = null;

        public void Show(MaterialForm parent)
        {
            thread = new Thread(new ParameterizedThreadStart(LoadingProcess));
            thread.TrySetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Name = "Call_Form";

            thread.Start(parent);
        }

        public void Close()
        {
            if (form != null)
            {
                form.BeginInvoke(new System.Threading.ThreadStart(form.Close));
                form = null;
                thread = null;
            }
        }

        private void LoadingProcess(object parent)
        {
            MaterialForm parent1 = parent as MaterialForm;
            if (parent1.InvokeRequired)
            {
                parent1.BeginInvoke((MethodInvoker)delegate
                {
                    form = parent1;
                    form.Show();
                });
            }
            else
            {
                form = parent1;
                form.Show();
            }
        }
    }
}
