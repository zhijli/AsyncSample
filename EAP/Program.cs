using AsyncOperationManagerExample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EAP
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            Application.Run(new PrimeNumberCalculatorMain());
        }
    }
}
