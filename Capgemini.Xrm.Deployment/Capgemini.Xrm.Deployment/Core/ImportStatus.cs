using System;

namespace Capgemini.Xrm.Deployment.Core
{
    public class ImportStatus
    {
        public string SolutionName { get; set; }

        public string ImportState { get; set; }

        public Guid? ImportId { get; set; }

        public Guid? ImportAsyncId { get; set; }

        public string ImportMessage { get; set; }

        public int ImportStatusCode { get; internal set; }
    }
}