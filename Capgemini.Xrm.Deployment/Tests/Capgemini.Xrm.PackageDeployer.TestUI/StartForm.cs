using System;
using System.Windows.Forms;

namespace Capgemini.Xrm.PackageDeployer.TestUI
{
    public partial class StartForm : Form
    {
        public StartForm()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            TemplateMigration tm = new TemplateMigration();
            tm.ShowDialog();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Import imp = new Import();
            imp.ShowDialog();
        }
    }
}