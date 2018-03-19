using Capgemini.Xrm.Deployment.Core;
using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase;
using System;
using System.Diagnostics;

namespace Capgemini.Xrm.Deployment.PackageDeployer.Core
{
    public class Logger : ILogger, Capgemini.DataMigration.Core.ILogger
    {
        private IPackageTemplate _impExtension;

        public Logger(IPackageTemplate impExtension)
        {
            _impExtension = impExtension;
        }

        private void WriteLogMessage(string message, TraceEventType eventType, Exception ex, ProgressPanelItemStatus panelStatus)
        {
            try
            {
                if (eventType == TraceEventType.Start)
                {
                    _impExtension.CreateProgressItem(message);
                }

                if (eventType == TraceEventType.Error || eventType == TraceEventType.Critical)
                {
                    if (ex == null) ex = new Exception(message);

                    _impExtension.PackageLog.Log("Error:" + message, eventType, ex);

                    try
                    {
                        if (_impExtension.RootControlDispatcher != null)
                        {
                            _impExtension.RootControlDispatcher.Invoke((Action)(() =>
                            {
                                _impExtension.RaiseFailEvent(message, ex);
                            }));
                        }
                        else
                        {
                            _impExtension.RaiseFailEvent(message, ex);
                        }
                    }
                    catch (Exception)
                    {
                        _impExtension.PackageLog.Log("RaiseFailEvent issue");
                    }
                }
                else
                {
                    if ((int)eventType <= 8)
                        _impExtension.PackageLog.Log(eventType.ToString() + ":" + message, eventType);

                    try
                    {
                        if (_impExtension.RootControlDispatcher != null)
                        {
                            _impExtension.RootControlDispatcher.Invoke((Action)(() =>
                            {
                                _impExtension.RaiseUpdateEvent(message, panelStatus);
                            }));
                        }
                        else
                        {
                            _impExtension.RaiseUpdateEvent(message, panelStatus);
                        }
                    }
                    catch (Exception)
                    {
                        _impExtension.PackageLog.Log("RaiseUpdateEvent issue");
                    }
                }
            }
            catch (Exception exw)
            {
                _impExtension?.PackageLog?.Log($"Logging issue, message:{message}, type:{eventType}, {(ex != null ? ex.ToString() : "")} : {exw.ToString()}", TraceEventType.Warning);
            }
        }

        public void WriteLogMessage(string message, TraceEventType eventType, Exception ex)
        {
            WriteLogMessage(message, eventType, ex, ProgressPanelItemStatus.Failed);
        }

        public void WriteLogMessage(string message, TraceEventType eventType)
        {
            ProgressPanelItemStatus pnlStatus = ProgressPanelItemStatus.Working;

            switch (eventType)
            {
                case TraceEventType.Critical:
                case TraceEventType.Error:
                    pnlStatus = ProgressPanelItemStatus.Failed;
                    break;

                case TraceEventType.Stop:
                    pnlStatus = ProgressPanelItemStatus.Complete;
                    break;

                case TraceEventType.Warning:
                    pnlStatus = ProgressPanelItemStatus.Warning;
                    break;

                default:
                    pnlStatus = ProgressPanelItemStatus.Working;
                    break;
            }

            WriteLogMessage(message, eventType, null, pnlStatus);
        }

        public void WriteLogMessage(string message)
        {
            WriteLogMessage(message, TraceEventType.Information, null, ProgressPanelItemStatus.Working);
        }

        public void Error(string message)
        {
            WriteLogMessage(message, TraceEventType.Error);
        }

        public void Error(string message, Exception ex)
        {
            WriteLogMessage(message, TraceEventType.Error, ex);
        }

        public void Info(string message)
        {
            WriteLogMessage(message, TraceEventType.Information);
        }

        public void Verbose(string message)
        {
            WriteLogMessage(message, TraceEventType.Verbose);
        }

        public void Warning(string message)
        {
            WriteLogMessage(message, TraceEventType.Warning);
        }
    }
}