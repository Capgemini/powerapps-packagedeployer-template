using System;

namespace Capgemini.Xrm.Deployment.Core.Model
{
    public class ConfigurationParameter
    {
        internal const string ExecuteMultipleRequestsKey = "ExecuteMultipleRequests";

        public Guid Id { set; get; }
        public String Value { set; get; }
    }
}