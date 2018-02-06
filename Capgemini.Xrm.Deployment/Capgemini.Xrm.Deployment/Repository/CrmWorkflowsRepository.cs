using Capgemini.Xrm.Deployment.Core;
using Capgemini.Xrm.Deployment.Model;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Capgemini.Xrm.Deployment.Repository
{
    public class CrmWorkflowsRepository : CrmRepository, ICrmWorkflowsRepository
    {
        public CrmWorkflowsRepository(CrmAccess crmAccess) : base(crmAccess)
        {
        }

        public void ActivateWorkflow(Guid id)
        {
            var activateRequest = new SetStateRequest
            {
                EntityMoniker = new EntityReference("workflow", id),
                State = new OptionSetValue((int)WorkflowState.Activated),
                Status = new OptionSetValue((int)WorkflowStatusCode.Activated)
            };

            this.CurrentOrganizationService.Execute(activateRequest);
        }

        public void DeActivateWorkflow(Guid id)
        {
            var activateRequest = new SetStateRequest
            {
                EntityMoniker = new EntityReference("workflow", id),
                State = new OptionSetValue((int)WorkflowState.Draft),
                Status = new OptionSetValue((int)WorkflowStatusCode.Draft)
            };

            this.CurrentOrganizationService.Execute(activateRequest);
        }

        public List<WorkflowEntity> GetAllWorkflows()
        {
            List<WorkflowEntity> processes = new List<WorkflowEntity>();

            QueryExpression query = new QueryExpression { EntityName = "workflow", ColumnSet = new ColumnSet("name", "statuscode", "statecode", "type", "category", "rendererobjecttypecode") };

            EntityCollection results = this.CurrentAccess.GetDataByQuery(query, 5000);

            processes = results.Entities.Select(p =>
            new WorkflowEntity(
                p.Id,
                p.GetAttributeValue<string>("name"),
                p.GetAttributeValue<string>("rendererobjecttypecode"),
                p.GetAttributeValue<OptionSetValue>("type"),
                p.GetAttributeValue<OptionSetValue>("category"),
                p.GetAttributeValue<OptionSetValue>("statuscode"),
                p.GetAttributeValue<OptionSetValue>("statecode"))).ToList();

            return processes;
        }

        public List<Entity> GetAllCustomizableSDKSteps()
        {
            QueryExpression query = new QueryExpression { EntityName = "sdkmessageprocessingstep", ColumnSet = new ColumnSet(true) };
            query.Criteria = new FilterExpression(LogicalOperator.And);
            query.Criteria.AddCondition(new ConditionExpression("ishidden", ConditionOperator.Equal, false));
            query.Criteria.AddCondition(new ConditionExpression("iscustomizable", ConditionOperator.Equal, true));

            EntityCollection results = this.CurrentAccess.GetDataByQuery(query, 5000);

            return results.Entities.ToList();
        }

        public void ActivateSDKStep(Guid id)
        {
            var activateRequest = new SetStateRequest
            {
                EntityMoniker = new EntityReference("sdkmessageprocessingstep", id),
                State = new OptionSetValue((int)SDKStepState.Enabled),
                Status = new OptionSetValue((int)SDKSepStatusCode.Enabled)
            };

            this.CurrentOrganizationService.Execute(activateRequest);
        }

        public void DeActivateSDKStep(Guid id)
        {
            var activateRequest = new SetStateRequest
            {
                EntityMoniker = new EntityReference("sdkmessageprocessingstep", id),
                State = new OptionSetValue((int)SDKStepState.Disabled),
                Status = new OptionSetValue((int)SDKSepStatusCode.Disabled)
            };

            this.CurrentOrganizationService.Execute(activateRequest);
        }
    }
}