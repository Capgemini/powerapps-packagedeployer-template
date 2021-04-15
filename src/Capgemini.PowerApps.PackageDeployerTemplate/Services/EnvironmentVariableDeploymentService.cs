namespace Capgemini.PowerApps.PackageDeployerTemplate.Services
{
  using System;
  using System.Linq;
  using Microsoft.Extensions.Logging;
  using Microsoft.Xrm.Sdk;
  using Microsoft.Xrm.Sdk.Query;
  using Capgemini.PowerApps.PackageDeployerTemplate.Adapters;

  /*
  *    Title: Development Hub
  *    Author: Max Ewing
  *    Date: 15 April 2021
  *    Code version: v0.2.25
  *    Availability: https://github.com/ewingjm/development-hub/blob/v0.2.25/deploy/EnvironmentVariableDeploymentService.cs
  */

  /// <summary>
  /// Deployment functionality related to environment variables.
  /// </summary>
  public class EnvironmentVariableDeploymentService
  {
    private readonly ILogger logger;
    private readonly ICrmServiceAdapter crmSvc;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentTemplateDeploymentService"/> class.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/>.</param>
    /// <param name="crmSvc">The <see cref="ICrmServiceAdapter"/>.</param>
    public EnvironmentVariableDeploymentService(ILogger logger, ICrmServiceAdapter crmSvc)
    {
      this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
      this.crmSvc = crmSvc ?? throw new ArgumentNullException(nameof(crmSvc));
    }

    /// <summary>
    /// Sets an environment variable on the target Common Data Service environment.
    /// </summary>
    /// <param name="key">Environment variable key.</param>
    /// <param name="value">Environment variable value.</param>
    public void SetEnvironmentVariable(string key, string value)
    {
      this.logger.LogInformation($"Setting {key} environment variable to {value}.");

      var definition = this.GetDefinitionByKey(key, new ColumnSet(false));
      if (definition == null)
      {
        throw new ArgumentException($"Environment variable {key} not found on target instance.");
      }

      var definitionReference = definition.ToEntityReference();
      this.logger.LogTrace($"Found environment variable on target instance: {definition.Id}");

      this.UpsertEnvironmentVariableValue(value, definitionReference);
    }

    private void UpsertEnvironmentVariableValue(string value, EntityReference definitionReference)
    {
      var existingValue = this.GetValueByDefinitionId(definitionReference, new ColumnSet("value"));
      if (existingValue != null)
      {
        existingValue["value"] = value;
        this.crmSvc.Update(existingValue);
      }
      else
      {
        this.SetValue(value, definitionReference);
      }
    }

    private Entity GetValueByDefinitionId(EntityReference definitionReference, ColumnSet columnSet)
    {
      var definitionQuery = new QueryExpression("environmentvariablevalue")
      {
        ColumnSet = columnSet,
        Criteria = new FilterExpression(),
      };
      definitionQuery.Criteria.AddCondition("environmentvariabledefinitionid", ConditionOperator.Equal, definitionReference.Id);

      return this.crmSvc.RetrieveMultiple(definitionQuery).Entities.FirstOrDefault();
    }

    private void SetValue(string value, EntityReference definition)
    {
      var val = new Entity("environmentvariablevalue")
      {
        Attributes = new AttributeCollection
                {
                    { "value", value },
                    { "environmentvariabledefinitionid", definition },
                },
      };

      this.crmSvc.Create(val);
    }

    private Entity GetDefinitionByKey(string key, ColumnSet columnSet)
    {
      var definitionQuery = new QueryExpression("environmentvariabledefinition")
      {
        ColumnSet = columnSet,
        Criteria = new FilterExpression(),
      };
      definitionQuery.Criteria.AddCondition("schemaname", ConditionOperator.Equal, key);

      return this.crmSvc.RetrieveMultiple(definitionQuery).Entities.FirstOrDefault();
    }

  }

}