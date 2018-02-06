using Capgemini.Xrm.Deployment.Core.Model;
using System;

namespace Capgemini.Xrm.Deployment.SolutionImport.Events
{
    public class ImportUpdateEventArgs : EventArgs
    {
        public SolutionDetails SolutionDetails { get; set; }

        public string Message { get; set; }

        public DateTime EventTime { get; set; }
    }
}