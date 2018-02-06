using Capgemini.Xrm.Deployment.Core;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using System.Configuration;

namespace Capgemini.Xrm.PackageDeployer.TestUI
{
    public class CrmAccessClient : CrmAccess
    {
        public CrmAccessClient(ConnectionStringSettings connectionStringName) : this(connectionStringName.ConnectionString)
        {
        }

        public CrmAccessClient(string connectionString) : base(null)
        {
            if (!connectionString.ToUpper().Contains("REQUIRENEWINSTANCE=TRUE"))
                connectionString = "RequireNewInstance=True; " + connectionString;

            ServiceClient = new CrmServiceClient(connectionString);

            _serviceProxy = (IOrganizationService)ServiceClient.OrganizationWebProxyClient ?? ServiceClient.OrganizationServiceProxy;
        }

        public CrmServiceClient ServiceClient { get; set; }

        public override IOrganizationService CurrentServiceProxy
        {
            get
            {
                if (ServiceClient.OrganizationWebProxyClient != null)
                {
                    var service = ServiceClient.OrganizationWebProxyClient;
                    service.InnerChannel.OperationTimeout = new System.TimeSpan(1, 0, 0);
                    return service;
                }

                if (ServiceClient.OrganizationServiceProxy != null)
                {
                    var service = ServiceClient.OrganizationServiceProxy;
                    service.Timeout = new System.TimeSpan(1, 0, 0);
                    return service;
                }

                throw new System.Exception("Cannot get IOrganizationService");
            }
        }
    }
}