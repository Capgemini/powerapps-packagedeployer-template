using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase;
using System;
using System.Windows.Threading;

namespace Capgemini.Xrm.Deployment.PackageDeployer.Core
{
    public interface IPackageTemplate
    {
        TraceLogger PackageLog { get; set; }

        string GetImportPackageDataFolderName { get; }

        Dispatcher RootControlDispatcher { get; set; }

        void RaiseFailEvent(string message, Exception ex);

        void RaiseUpdateEvent(string message, Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase.ProgressPanelItemStatus panelStatus);

        void CreateProgressItem(string message);
    }
}