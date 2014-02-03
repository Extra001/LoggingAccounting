using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoWeb
{
    interface IAutoWeb
    {
        bool Completed
        {
            get;
        }

        string Message
        {
            get;
        }

        void Start(WebBrowser webBrowser1);

        void Next(WebBrowser webBrowser1);
    }
}
