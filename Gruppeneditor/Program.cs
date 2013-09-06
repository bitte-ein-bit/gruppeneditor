using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Gruppeneditor
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            FormSplash.ShowSplashScreen();
            FormGuppeneditor mainForm = new FormGuppeneditor(); //this takes ages
            FormSplash.CloseForm();
            Application.Run(mainForm);
        }
    }
}
