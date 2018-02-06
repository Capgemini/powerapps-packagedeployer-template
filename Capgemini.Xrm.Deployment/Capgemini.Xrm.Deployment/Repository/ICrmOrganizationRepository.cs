using Microsoft.Xrm.Sdk;

namespace Capgemini.Xrm.Deployment.Repository
{
    public interface ICrmOrganizationRepository
    {
        Entity GetOrganizationSetting(string name);

        void SetOrganizationSetting(string name, string value, string valueType);
    }
}