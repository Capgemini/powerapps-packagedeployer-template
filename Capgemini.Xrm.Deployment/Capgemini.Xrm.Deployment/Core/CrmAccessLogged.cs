using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Capgemini.Xrm.Deployment.Core
{
    public class CrmAccessLogged : CrmAccess
    {
        private IOrganizationService _loggedServiceProxy;

        public CrmAccessLogged(IOrganizationService service) : base(service)
        {
        }

        public override IOrganizationService CurrentServiceProxy
        {
            get
            {
                if (_loggedServiceProxy == null)
                {
                    _loggedServiceProxy = new SoapLoggerOrganizationService(base.CurrentServiceProxy);
                }

                return _loggedServiceProxy;
            }
        }
    }

    public abstract class CrmAccessLogged<TCrmContext> : CrmAccess<TCrmContext>
    where TCrmContext : OrganizationServiceContext
    {
        private IOrganizationService _loggedServiceProxy;

        protected CrmAccessLogged(IOrganizationService service)
            : base(service)
        { }

        public override IOrganizationService CurrentServiceProxy
        {
            get
            {
                if (_loggedServiceProxy == null)
                {
                    _loggedServiceProxy = new SoapLoggerOrganizationService(base.CurrentServiceProxy);
                }

                return _loggedServiceProxy;
            }
        }
    }
}