using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Capgemini.Xrm.Deployment.Core
{
    public interface ICrmRepository
    {
        IOrganizationService CurrentOrganizationService { get; }

        CrmAccess CurrentAccess { get; }
    }

    public interface ICrmRepository<TCrmContext> : ICrmRepository where TCrmContext : OrganizationServiceContext
    {
        new CrmAccess<TCrmContext> CurrentAccess { get; }

        TCrmContext CurrentContext { get; }
    }
}