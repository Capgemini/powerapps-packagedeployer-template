using Capgemini.Xrm.Deployment.Model;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;

namespace Capgemini.Xrm.Deployment.Repository
{
    public interface ICrmWorkflowsRepository
    {
        void ActivateWorkflow(Guid id);

        void DeActivateWorkflow(Guid id);

        List<WorkflowEntity> GetAllWorkflows();

        List<Entity> GetAllCustomizableSDKSteps();

        void ActivateSDKStep(Guid id);

        void DeActivateSDKStep(Guid id);
    }
}