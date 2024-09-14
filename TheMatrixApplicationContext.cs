using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TheMatrix
{
    internal class TheMatrixApplicationContext : ApplicationContext
    {
        private int formCount;
        MatrixForm[] forms;

        public TheMatrixApplicationContext(int formCount)
        {
            this.formCount = formCount;

            forms = new MatrixForm[formCount];

            for (int i = 0; i < formCount; i++)
            {
                forms[i] = new MatrixForm(i);
                forms[i].Name = $"The Matrix - Monitor {i}";
                forms[i].Bounds = Screen.AllScreens[i].Bounds; // Does not work from constructor
                forms[i].Show();
                forms[i].Closed += new EventHandler(OnFormClosed);
            }
        }

        private void OnFormClosed(object sender, EventArgs e)
        {
            ExitThread();
        }
    }
}
