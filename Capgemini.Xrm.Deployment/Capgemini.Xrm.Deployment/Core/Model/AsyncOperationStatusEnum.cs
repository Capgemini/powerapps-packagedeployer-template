using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capgemini.Xrm.Deployment.Core.Model
{
    public enum  AsyncOperationStatusEnum
    {
        WaitingForResources = 0,
        Waiting = 10,
        InProgress = 20,
        Pausing = 21,
        Canceling = 22,
        Succeeded = 30,
        Failed = 31,
        Canceled = 32
    }
}
