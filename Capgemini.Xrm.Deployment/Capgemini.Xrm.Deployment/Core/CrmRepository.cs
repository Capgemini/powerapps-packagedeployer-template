using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Capgemini.Xrm.Deployment.Core
{
    public class CrmRepository : ICrmRepository
    {
        protected readonly CrmAccess _crmAccess;

        public CrmRepository(CrmAccess crmAccess)
        {
            this._crmAccess = crmAccess;
        }

        public CrmAccess CurrentAccess
        {
            get
            {
                return this._crmAccess;
            }
        }

        public IOrganizationService CurrentOrganizationService
        {
            get
            {
                return this._crmAccess.CurrentServiceProxy;
            }
        }
    }

    public class CrmRepository<TCrmContext> : CrmRepository where TCrmContext : OrganizationServiceContext, ICrmRepository<TCrmContext>
    {
        public CrmRepository(CrmAccess<TCrmContext> crmAccess) : base(crmAccess)
        {
        }

        public new CrmAccess<TCrmContext> CurrentAccess
        {
            get
            {
                return (CrmAccess<TCrmContext>)this._crmAccess;
            }
        }

        public TCrmContext CurrentContext
        {
            get
            {
                return CurrentAccess.CurrentCrmContext;
            }
        }
    }
}