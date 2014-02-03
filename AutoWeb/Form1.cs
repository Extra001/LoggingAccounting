using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoWeb
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private List<IAutoWeb> autoWebList = new List<IAutoWeb>();

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            this.webBrowser1.Navigating += webBrowser1_Navigating;
            this.webBrowser1.DocumentCompleted += webBrowser1_DocumentCompleted;

            this.autoWebList.Add(new AutoCheckMoney());

            this.StepGo();
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.StepGo();
        }

        private void nextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.autoWebList[0].Next(this.webBrowser1);
            if (this.autoWebList[0].Completed)
            {
                this.notifyIcon1.ShowBalloonTip(10000, "Check Money", autoWebList[0].Message, ToolTipIcon.Info);
            }
        }

        private void autoToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void autoExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void test1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void test2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void test3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            this.Text = "Navigating...";

            this.textBox2.Text = e.Url.ToString();
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            this.Text = "Document Completed";
            
            if (this.autoToolStripMenuItem.Checked)
            {
                this.StepNext();
            }
        }
                
        private void notifyIcon1_BalloonTipClosed(object sender, EventArgs e)
        {
            if (this.autoExitToolStripMenuItem.Checked)
            {
                this.Close();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.StepNext();
        }

        private void StepGo()
        {
            this.autoWebList[0].Start(this.webBrowser1);
            this.timer1.Start();
        }

        private void StepNext()
        {
            if(this.webBrowser1.ReadyState != WebBrowserReadyState.Complete)
            {
                return;
            }

            this.autoWebList[0].Next(this.webBrowser1);
            if (this.autoWebList[0].Completed)
            {
                this.Text = "Auto Browsing";
                this.timer1.Stop();
                this.notifyIcon1.ShowBalloonTip(10000, "Check Money", autoWebList[0].Message, ToolTipIcon.Info);
            }
        }
    }
}
