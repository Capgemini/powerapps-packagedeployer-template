using Microsoft.Xrm.Sdk;
using System;

namespace Capgemini.Xrm.Deployment.Model
{
    public class WorkflowEntity
    {
        public WorkflowEntity(Guid id, string name, string rendererObjectTypeCode, OptionSetValue wfType, OptionSetValue category, OptionSetValue statuscode, OptionSetValue statecode)
        {
            Id = id;
            Name = name;
            RendererObjectTypeCode = rendererObjectTypeCode;
            WfType = wfType != null ? (WorkflowType)wfType.Value : (WorkflowType?)null;
            WfCategory = category != null ? (WorkflowCategory)category.Value : (WorkflowCategory?)null;
            WfStatus = statuscode != null ? (WorkflowStatusCode)statuscode.Value : (WorkflowStatusCode?)null;
            WfState = statecode != null ? (WorkflowState)statecode.Value : (WorkflowState?)null;
        }

        public Guid Id { get; set; }

        public string Name { get; set; }

        public string RendererObjectTypeCode { get; set; }

        public WorkflowState? WfState { get; set; }

        public WorkflowStatusCode? WfStatus { get; set; }

        public WorkflowCategory? WfCategory { get; set; }

        public WorkflowType? WfType { get; set; }
    }
}