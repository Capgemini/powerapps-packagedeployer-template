using Capgemini.Xrm.Deployment.Config;
using Capgemini.Xrm.Deployment.Core;
using Capgemini.Xrm.Deployment.DocTemplates;
using Capgemini.Xrm.Deployment.Repository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Capgemini.Xrm.DeploymentHelpers
{
    public class DeploymentActivities
    {
        private readonly PackageDeployerConfigReader _config;
        private readonly ILogger _packageLog;
        private readonly CrmTemplatesRepository _crmTemplatesRepo;
        private readonly CrmSlaRepository _slaRepo;

        public DeploymentActivities(PackageDeployerConfigReader configReader, ILogger packageLog, CrmAccess crmAccess)
        {
            _config = configReader;
            _packageLog = packageLog;
            _crmTemplatesRepo = new CrmTemplatesRepository(crmAccess);
            _slaRepo = new CrmSlaRepository(crmAccess);
        }

        public void LoadTemplates()
        {
            _packageLog.WriteLogMessage("LoadTemplates", TraceEventType.Start);
            try
            {
                List<string> wordTemplates = _config.WordTemplates;
                if (wordTemplates != null && wordTemplates.Any())
                {
                    DocTemplateManager templManager = new DocTemplateManager(_crmTemplatesRepo);
                    foreach (var wordTemplatePath in wordTemplates)
                    {
                        FileInfo fi = new FileInfo(Path.Combine(_config.SolutionsFolder, wordTemplatePath));
                        string templateName = fi.Name.Replace(fi.Extension, "");
                        try
                        {
                            templManager.ImportTemplateFromFile(templateName, fi.FullName);
                            _packageLog.WriteLogMessage($"Template {templateName} has been imported");
                        }
                        catch (Exception exw)
                        {
                            _packageLog.WriteLogMessage($"Error Importing Template {templateName}", TraceEventType.Warning, exw);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _packageLog.WriteLogMessage("LoadTemplates Error", TraceEventType.Warning, ex);
                return;
            }
            _packageLog.WriteLogMessage("LoadTemplates", TraceEventType.Stop);
        }

        public void DeactivateAllSLAs()
        {
            _packageLog.WriteLogMessage("DeactivateAllSLAs", TraceEventType.Start);

            if (_config.DisableSlaBeforeImport)
            {
                try
                {
                    _packageLog.Info("Getting list of SLAs to deactivate");
                    var slas = _slaRepo.GetAllSlas();
                    if (slas != null && slas.Any())
                    {
                        slas.ForEach(sla =>
                        {
                            _packageLog.Info($"Deactivating SLA:{sla.Name}");
                            try
                            {
                                _slaRepo.DeactivateSla(sla);
                                _packageLog.Info($"Sla {sla.Name} has been deactivated");
                            }
                            catch (Exception exw)
                            {
                                _packageLog.WriteLogMessage($"Error deactivating SLA {sla.Name}", TraceEventType.Warning, exw);
                            }
                        });
                    }
                    else
                    {
                        _packageLog.Info("No SLAs to deactivate");
                    }
                }
                catch (Exception ex)
                {
                    _packageLog.WriteLogMessage($"DeactivateAllSLAs: Error:{ex.Message}, StackTrace:{ex.StackTrace}", TraceEventType.Warning);
                    return;
                }
            }
            else
            {
                _packageLog.WriteLogMessage("DeActivating SLA disabled in importconfig.xml");
            }

            _packageLog.WriteLogMessage("DeactivateAllSLAs", TraceEventType.Stop);
        }

        public void ActivateAllSLAs()
        {
            _packageLog.WriteLogMessage("ActivateAllSLAs", TraceEventType.Start);

            if (_config.EnableSlaAfterImport)
            {
                try
                {
                    _packageLog.Info("Getting list of SLAs to activate");
                    var slas = _slaRepo.GetAllSlas();

                    if (slas != null && slas.Any())
                    {
                        slas.ForEach(sla =>
                        {
                            _packageLog.Info($"Activating SLA:{sla.Name}");
                            try
                            {
                                _slaRepo.ActivateSla(sla);
                                _packageLog.Info($"Sla {sla.Name} has been activated");
                            }
                            catch (Exception exw)
                            {
                                _packageLog.WriteLogMessage($"Error activating SLA {sla.Name}", TraceEventType.Warning, exw);
                            }
                        });

                        if (_config.DefaultSLANames != null && _config.DefaultSLANames.Any())
                        {
                            _config.DefaultSLANames.ForEach(slaName =>
                          {
                              var sla = _slaRepo.GetSlaByName(slaName.Trim());
                              _packageLog.Info($"Making SLA default:{sla.Name}");
                              try
                              {
                                  _slaRepo.SetSlaDefault(sla);
                                  _packageLog.Info($"Sla {sla.Name} has been set to default");
                              }
                              catch (Exception exw)
                              {
                                  _packageLog.WriteLogMessage($"Error making SLA {sla.Name} default", TraceEventType.Warning, exw);
                              }
                          });
                        }
                    }
                    else
                    {
                        _packageLog.Info("No SLAs to activate");
                    }
                }
                catch (Exception ex)
                {
                    _packageLog.WriteLogMessage($"ActivateAllSLAs: Error:{ex.Message} StackTrace: {ex.StackTrace}", TraceEventType.Warning);
                    return;
                }
            }
            else
            {
                _packageLog.WriteLogMessage("Activating SLA disabled in importconfig.xml");
            }
            _packageLog.WriteLogMessage("ActivateAllSLAs", TraceEventType.Stop);
        }
    }
}