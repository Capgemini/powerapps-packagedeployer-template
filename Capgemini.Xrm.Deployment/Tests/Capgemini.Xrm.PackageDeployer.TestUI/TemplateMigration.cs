using Capgemini.Xrm.Deployment.DocTemplates;
using Capgemini.Xrm.Deployment.PackageDeployer.Core;
using Capgemini.Xrm.Deployment.Repository;
using System;
using System.Windows.Forms;

namespace Capgemini.Xrm.PackageDeployer.TestUI
{
    public partial class TemplateMigration : Form
    {
        public TemplateMigration()
        {
            InitializeComponent();
        }

        private void btExport_Click(object sender, EventArgs e)
        {
            var crmAccess = new CrmAccessClient(tbSourceConnection.Text, 60);
            var repository = new CrmTemplatesRepository(crmAccess);
            var templManager = new DocTemplateManager(repository);

            templManager.SaveTemplateToFile(tbTemplateName.Text, tbFilePath.Text);
        }

        private void btImport_Click(object sender, EventArgs e)
        {
            var crmAccess = new CrmAccessClient(tbTargetConnection.Text, 60);
            var repository = new CrmTemplatesRepository(crmAccess);
            var templManager = new DocTemplateManager(repository);

            templManager.ImportTemplateFromFile(tbTargetTemplateName.Text, tbSurcePath.Text);
        }
    }
}