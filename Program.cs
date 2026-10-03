using System;
using System.Windows.Forms;
using SecretariaServer;

namespace SecretariaServer
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            var secretariaForm = new SecretariaForm();
            var inspetorForm = new InspetorForm();

            secretariaForm.Show();
            inspetorForm.Show();

            Application.Run(secretariaForm);
        }
    }
}
