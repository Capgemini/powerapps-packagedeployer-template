using System;

namespace Capgemini.Xrm.Deployment.Core.Model
{
    public class SolutionDetails
    {
        public string SolutionName { get; set; }

        public string HoldingSolutionName { get; set; }

        public Version SolutionVersion { get; set; }

        public string SolutionFilePath { get; set; }

        public string HoldingSolutionFilePath { get; set; }

        public bool ForceUpdate { get; set; }

        public override string ToString()
        {
            return string.Format("SolutionName:{0}, HoldingSolutionName:{1}, SolutionVersion:{2}, ForceUpdate:{3}", SolutionName, HoldingSolutionName, SolutionVersion, ForceUpdate);
        }
    }
}