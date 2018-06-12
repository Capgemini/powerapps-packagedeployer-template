using Capgemini.Xrm.Deployment.Core;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Configuration;

namespace Capgemini.Xrm.Deployment.PackageDeployer.Core
{
    public class CrmAccessClient : CrmAccess
    {
        public CrmAccessClient(ConnectionStringSettings connectionStringName, int timeoutMinutes) : this(connectionStringName.ConnectionString, timeoutMinutes)
        {
        }

        public CrmAccessClient(string connectionString, int timeoutMinutes) : base(null, timeoutMinutes)
        {
            if (!connectionString.ToUpper().Contains("REQUIRENEWINSTANCE=TRUE"))
                connectionString = "RequireNewInstance=True; " + connectionString;

            ServiceClient = new CrmServiceClient(connectionString);

            _serviceProxy = (IOrganizationService)ServiceClient.OrganizationWebProxyClient ?? ServiceClient.OrganizationServiceProxy;
        }

        public CrmServiceClient ServiceClient { get; set; }

    }
}