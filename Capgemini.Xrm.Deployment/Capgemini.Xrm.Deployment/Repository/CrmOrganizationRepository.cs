using Capgemini.Xrm.Deployment.Core;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

using System;
using System.Globalization;

namespace Capgemini.Xrm.Deployment.Repository
{
    public class CrmOrganizationRepository : CrmRepository, ICrmOrganizationRepository
    {
        public CrmOrganizationRepository(CrmAccess crmAccess) : base(crmAccess)
        {
        }

        public void SetOrganizationSetting(string name, string value, string valueType)
        {
            Entity org = GetOrganizationSetting(name);

            object currentValue = org.GetAttributeValue<object>(name);

            if (currentValue == null || currentValue.ToString() != value)
            {
                org.Attributes[name] = GetTypedObject(value, valueType);
                CurrentOrganizationService.Update(org);
            }
        }

        public Entity GetOrganizationSetting(string name)
        {
            Guid orgId = ((WhoAmIResponse)CurrentOrganizationService.Execute(new WhoAmIRequest())).OrganizationId;

            Entity org = this.CurrentAccess.GetEntity("organization", orgId, new string[] { "organizationid", name });

            return org;
        }

        private object GetTypedObject(string value, string valueType)
        {
            switch (valueType)
            {
                case "System.Boolean":
                    return Convert.ToBoolean(value,CultureInfo.InvariantCulture);

                case "System.String":
                    return value;

                default:
                    throw new Exception("Not supported type:" + valueType);
            }
        }
    }
}