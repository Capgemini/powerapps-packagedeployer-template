using Capgemini.Xrm.Deployment.Core;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;

namespace Capgemini.Xrm.Deployment.Repository
{
    public class CrmTemplatesRepository : CrmRepository, ICrmTemplatesRepository
    {
        public CrmTemplatesRepository(CrmAccess crmAccess) : base(crmAccess)
        {
        }

        public Entity GetDocumentTemplateByName(string name)
        {
            var results = this.CurrentAccess.GetEntitiesByColumn("documenttemplate", "name", name,
                new string[] { "documenttemplateid", "documenttype", "name", "associatedentitytypecode", "content" }, 10);

            return results.Entities.FirstOrDefault();
        }

        public Guid SetTemplate(string templateName, string entityName, OptionSetValue templateType, byte[] content)
        {
            var documentTemplate = this.GetDocumentTemplateByName(templateName);

            if (documentTemplate == null)
            {
                documentTemplate = new Entity("documenttemplate");
                documentTemplate["name"] = templateName;
            }

            documentTemplate["associatedentitytypecode"] = entityName;
            documentTemplate["documenttype"] = templateType;
            documentTemplate["content"] = Convert.ToBase64String(content);

            if (documentTemplate.Id == Guid.Empty)
                documentTemplate.Id = this.CurrentOrganizationService.Create(documentTemplate);
            else
                this.CurrentOrganizationService.Update(documentTemplate);

            return documentTemplate.Id;
        }
    }
}