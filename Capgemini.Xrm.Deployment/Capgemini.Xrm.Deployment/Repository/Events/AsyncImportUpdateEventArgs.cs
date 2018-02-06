using System;

namespace Capgemini.Xrm.Deployment.Repository.Events
{
    public class AsyncImportUpdateEventArgs : EventArgs
    {
        public string ImportState { get; set; }

        public string Message { get; set; }

        public int ImportStatusCode { get; set; }

        public DateTime EventTime { get; set; }
    }
}