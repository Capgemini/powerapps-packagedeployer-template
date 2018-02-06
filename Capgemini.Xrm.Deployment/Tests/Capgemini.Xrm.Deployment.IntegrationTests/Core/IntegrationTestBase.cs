using Capgemini.Xrm.Deployment.Core;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Configuration;

namespace Capgemini.Xrm.Deployment.IntegrationTests.Core
{
    public class IntegrationTestBase
    {
        private CrmAccessLogged _crmAccessLogged;
        private CrmAccess _crmAccess;

        protected CrmAccessLogged CrmAccessLogged
        {
            get
            {
                if (_crmAccessLogged == null)
                {
                    string connectionString = ConfigurationManager.ConnectionStrings[ConnStringKey].ConnectionString;
                    _crmAccessLogged = GetCrmAccessLogged(connectionString);
                }
                return _crmAccessLogged;
            }
        }

        protected CrmAccess CrmAccess
        {
            get
            {
                if (_crmAccess == null)
                {
                    string connectionString = ConfigurationManager.ConnectionStrings[ConnStringKey].ConnectionString;
                    _crmAccess = GetCrmAccess(connectionString);
                }
                return _crmAccess;
            }
        }

        public CrmAccessLogged GetCrmAccessLogged(string connString)
        {
            var conn = new CrmServiceClient(connString);

            if (conn.OrganizationServiceProxy != null)
                conn.OrganizationServiceProxy.Timeout = TimeSpan.FromMinutes(60);
            else if (conn.OrganizationWebProxyClient != null)
                conn.OrganizationWebProxyClient.InnerChannel.OperationTimeout = TimeSpan.FromMinutes(60);

            var orgService = (IOrganizationService)conn.OrganizationWebProxyClient ?? conn.OrganizationServiceProxy;

            return new CrmAccessLogged(orgService);
        }

        public CrmAccess GetCrmAccess(string connString)
        {
            var conn = new CrmServiceClient(connString);

            if (conn.OrganizationServiceProxy != null)
                conn.OrganizationServiceProxy.Timeout = TimeSpan.FromMinutes(60);
            else if (conn.OrganizationWebProxyClient != null)
                conn.OrganizationWebProxyClient.InnerChannel.OperationTimeout = TimeSpan.FromMinutes(60);

            var orgService = (IOrganizationService)conn.OrganizationWebProxyClient ?? conn.OrganizationServiceProxy;

            return new CrmAccess(orgService);
        }

        protected string ConnStringKey { get; set; } = "Crm";
    }
}