using Capgemini.Xrm.Deployment.Config;
using Capgemini.Xrm.Deployment.Core;
using Capgemini.Xrm.Deployment.Model;
using Capgemini.Xrm.Deployment.Repository;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Capgemini.Xrm.DeploymentHelpers
{
    public class ProcessesActivator
    {
        private readonly PackageDeployerConfigReader _config;
        private readonly ILogger _packageLog;
        private readonly CrmWorkflowsRepository _workflowRepo;

        public ProcessesActivator(PackageDeployerConfigReader configReader, ILogger packageLog, CrmAccess crmAccess)
        {
            _config = configReader;
            _packageLog = packageLog;
            _workflowRepo = new CrmWorkflowsRepository(crmAccess);
        }

        public bool DeactivatePluginSteps()
        {
            bool areSDKDeActivated = true;
            _packageLog.WriteLogMessage("DeactivatePluginSteps", TraceEventType.Start);

            try
            {
                if (_config.SdkStepsToExclude != null && _config.SdkStepsToExclude.Any())
                    areSDKDeActivated = DeactivateRequiredPluginSteps(_config.SdkStepsToExclude);
            }
            catch (Exception ex)
            {
                _packageLog.WriteLogMessage("DeactivatePluginSteps", TraceEventType.Error, ex);
                return false;
            }

            if (areSDKDeActivated)
            {
                _packageLog.WriteLogMessage("DeactivatePluginSteps", TraceEventType.Stop);
            }
            else
            {
                _packageLog.WriteLogMessage("DeactivatePluginSteps Not all SDK steps deactivated, please see log for more details.", TraceEventType.Warning);
            }

            return areSDKDeActivated;
        }

        public bool ActivateRequiredWorkflows()
        {
            bool areWorkflowActivated = true;
            _packageLog.WriteLogMessage("ActivateRequiredWorkflows", TraceEventType.Start);

            try
            {
                if (_config.ExcludedWorkflows != null && _config.ExcludedWorkflows.Any())
                    areWorkflowActivated = ActivateAllWorkflows(_config.ExcludedWorkflows);
            }
            catch (Exception ex)
            {
                _packageLog.WriteLogMessage("ActivateRequiredWorkflows", TraceEventType.Error, ex);
                return false;
            }

            if (areWorkflowActivated)
            {
                _packageLog.WriteLogMessage("ActivateRequiredWorkflows", TraceEventType.Stop);
                _packageLog.WriteLogMessage("All required workflows activated or deactivated OK", TraceEventType.Information);
            }
            else
            {
                _packageLog.WriteLogMessage("ActivateRequiredWorkflows Not all the workflows are activated, please chek log for more details.", TraceEventType.Warning);
            }

            return areWorkflowActivated;
        }

        private bool ActivateAllWorkflows(List<string> excluded)
        {
            var workflowlist = _workflowRepo.GetAllWorkflows();
            bool allActivated = true;

            foreach (var item in workflowlist)
            {
                try
                {
                    if (item.WfType == WorkflowType.Definition && item.RendererObjectTypeCode == null)
                    {
                        if (item.Name.StartsWith("DRAFT", StringComparison.Ordinal) && item.WfState == WorkflowState.Activated)
                        {
                            _workflowRepo.DeActivateWorkflow(item.Id);
                        }
                        else if (!excluded.Contains(item.Name) && item.WfState == WorkflowState.Draft)
                        {
                            _workflowRepo.ActivateWorkflow(item.Id);
                        }
                        else if (excluded.Contains(item.Name) && item.WfState == WorkflowState.Activated)
                        {
                            _workflowRepo.DeActivateWorkflow(item.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    allActivated = false;
                    _packageLog.WriteLogMessage($"Error activating/deactivating workflow {item.Name} id: {item.Id}", TraceEventType.Warning, ex);
                }
            }

            return allActivated;
        }

        private bool DeactivateRequiredPluginSteps(List<Tuple<string, string>> excluded)
        {
            var sdkSteps = _workflowRepo.GetAllCustomizableSDKSteps();
            bool activatedAll = true;

            foreach (var item in sdkSteps)
            {
                var eventhandler = item.GetAttributeValue<EntityReference>("eventhandler");
                var name = item.GetAttributeValue<string>("name");
                var status = item.GetAttributeValue<OptionSetValue>("statuscode");

                try
                {
                    if (eventhandler != null)
                    {
                        var sdkStep = excluded.FirstOrDefault(p => p.Item1 == name && p.Item2 == eventhandler.Name);
                        if (sdkStep != null)
                        {
                            if (status.Value == (int)SDKSepStatusCode.Enabled)
                            {
                                _workflowRepo.DeActivateSDKStep(item.Id);
                                _packageLog.Info($"SDKStep:{name}, eventHandler:{eventhandler} has been deactivated.");
                            }
                            else
                            {
                                _packageLog.Info($"SDKStep:{name}, eventHandler:{eventhandler} is already deactivated.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _packageLog.WriteLogMessage($"Error deactivating SDK Step {name} id: {item.Id}, handler:{eventhandler.Name}", TraceEventType.Warning, ex);
                    activatedAll = false;
                }
            }

            return activatedAll;
        }
    }
}