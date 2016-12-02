using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Executor
{
    static class Program
    {

        [STAThread]
        static void Main()
        {
            Application.Run(new Window());
        }
    }
}
