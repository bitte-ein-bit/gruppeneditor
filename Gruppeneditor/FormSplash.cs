using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Gruppeneditor
{
    public partial class FormSplash : Form
    {
        public FormSplash()
        {
            InitializeComponent();
        }

        //Delegate for cross thread call to close
        private delegate void CloseDelegate();

        //The type of form to be displayed as the splash screen.
        private static FormSplash splashForm;
        private static int progress;

        static public void ShowSplashScreen()
        {
            // Make sure it is only launched once.

            if (splashForm != null)
                return;
            Thread thread = new Thread(new ThreadStart(FormSplash.ShowForm));
            thread.IsBackground = true;
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        static private void ShowForm()
        {
            splashForm = new FormSplash();
            Application.Run(splashForm);
        }

        static public void CloseForm()
        {   
            try
            {
                //splashForm.Invoke(new CloseDelegate(FormSplash.CloseFormInternal));
            }
            catch (Exception e)
            {
            }
        }

        static private void CloseFormInternal()
        {
            splashForm.Close();
        }

        static public void setProgress(int i)
        {
            progress = i;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (progressBar1.Value < progress)
            {
                progressBar1.Value += 5;
                if (progressBar1.Value >= 100)
                {
                    timer1.Enabled = false;
                    timer1.Tick -= timer1_Tick;
                    timer1.Tick += timer1_Tick2;
                    timer1.Interval = 1000;
                    timer1.Enabled = true;
                }
            }
        }

        private void timer1_Tick2(object sender, EventArgs e)
        {
            this.Close();
        }


    }
}
