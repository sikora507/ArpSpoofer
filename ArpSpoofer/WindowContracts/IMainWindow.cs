using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArpSpoofer.WindowContracts
{
    public interface IMainWindow
    {
        void Show();
        void Hide();
        event CancelEventHandler Closing;
    }
}
