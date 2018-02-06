using Capgemini.Xrm.Deployment.Core;
using Microsoft.Xrm.Sdk;
using System;

namespace Capgemini.Xrm.Deployment.Repository
{
    public interface ICrmTemplatesRepository : ICrmRepository
    {
        Entity GetDocumentTemplateByName(string name);

        Guid SetTemplate(string templateName, string entityName, OptionSetValue templateType, byte[] content);
    }
}