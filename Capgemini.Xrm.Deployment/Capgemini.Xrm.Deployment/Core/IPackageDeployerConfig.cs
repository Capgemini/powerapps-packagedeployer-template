using Capgemini.Xrm.Deployment.Config;
using System;
using System.Collections.Generic;

namespace Capgemini.Xrm.Deployment.Core
{
    public interface IPackageDeployerConfig
    {
        /// <summary>
        /// Used by async import, sleep interval in miliseconds before checking progress
        /// </summary>
        int AsyncSleepIntervalMiliseconds { get; }

        /// <summary>
        /// Total Async operation timeout
        /// </summary>
        int AsyncTimeoutSeconds { get; }

        /// <summary>
        /// Path to Configuration Migration zip file
        /// </summary>
        string CrmMigdataImportFile { get; }

        /// <summary>
        /// List of SLAs which shodul be set to defaul after import is completed
        /// </summary>
        List<string> DefaultSLANames { get; }

        /// <summary>
        /// If True then SLA will be disabled before import
        /// </summary>
        bool DisableSlaBeforeImport { get; }

        /// <summary>
        /// If True SLAs will be activated after import
        /// </summary>
        bool EnableSlaAfterImport { get; }

        /// <summary>
        /// If True, no holding solutions are used, just simple update
        /// </summary>
        bool DontUseHoldingSulutions { get; }

        /// <summary>
        /// List of workflows which should be deactivated after import, all th eother will be activated
        /// </summary>
        List<string> ExcludedWorkflows { get; }

        /// <summary>
        /// List of SDK steps which should be deactivated after import
        /// </summary>
        List<Tuple<string, string>> SdkStepsToExclude { get; }

        /// <summary>
        /// If True, Ignores custom deployment logic - is using out of the box logic
        /// </summary>
        bool SkipCustomDeployment { get; }

        /// <summary>
        /// If True, Ignores post deployment actions as Activating SLAs, Capgemini Migraiton Engine packages import, deactivating/activating processes or SDK steps, importing word templates
        /// </summary>
        bool SkipPostDeploymentActions { get; }

        /// <summary>
        /// Path to solutionimport.xml file
        /// </summary>
        string SolutionConfigFilePath { get; }

        /// <summary>
        /// Settings for all CRM solutions
        /// </summary>
        List<SolutionImportSetting> SolutionImportSettings { get; }

        /// <summary>
        /// Folder path were CRM solutions shoudl be found
        /// </summary>
        string SolutionsFolder { get; }

        /// <summary>
        /// Enables Async Import but can be disabled on solution level as well in SolutionImportSettings
        /// </summary>
        bool UseAsyncImport { get; }

        /// <summary>
        /// Use Step For Upgrades instead of holding solutions
        /// </summary>
        bool UseNewApi { get; }

        /// <summary>
        /// Word templates to load after import
        /// </summary>
        List<string> WordTemplates { get; }
    }
}