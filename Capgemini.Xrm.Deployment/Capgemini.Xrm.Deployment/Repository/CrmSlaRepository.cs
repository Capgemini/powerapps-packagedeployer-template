using Capgemini.Xrm.Deployment.Core;
using Capgemini.Xrm.Deployment.Core.Exceptions;
using Capgemini.Xrm.Deployment.Extensions;
using Capgemini.Xrm.Deployment.Model.Sla;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Capgemini.Xrm.Deployment.Repository
{
    public class CrmSlaRepository : CrmRepository, ICrmSlaRepository
    {
        public CrmSlaRepository(CrmAccess crmAccess) : base(crmAccess)
        {
        }

        public List<SlaEntity> GetAllSlas()
        {
            return GetAllSlasByFilter(null, null);
        }

        public SlaEntity GetSlaById(Guid slaId)
        {
            var sla = GetAllSlasByFilter(SlaFields.SLAId, slaId).FirstOrDefault();

            if (sla == null)
                throw new ValidationException("SLA with ID " + slaId.ToString() + " does not exist.");

            return sla;
        }

        public SlaEntity GetSlaByName(string name)
        {
            var sla = GetAllSlasByFilter(SlaFields.Name, name).FirstOrDefault();

            if (sla == null)
                throw new ValidationException("SLA with name " + name + " does not exist.");

            return sla;
        }

        public void SetSlaDefault(SlaEntity sla)
        {
            sla.ThrowIfNull();

            if (!sla.IsDefult)
            {
                var updateSLA = new Entity(SlaEntity.EntityName)
                {
                    Id = sla.SlaId
                };
                updateSLA.Attributes.Add(SlaFields.IsDefault, true);
                CurrentOrganizationService.Update(updateSLA);
            }
        }

        public void DeactivateSla(SlaEntity sla)
        {
            sla.ThrowIfNull();

            if (sla.SlaState == SlaState.Active && sla.SlaStatus == SlaStatusCode.Active)
            {
                Entity slaEnt = new Entity(SlaEntity.EntityName, sla.SlaId);
                CurrentAccess.SetEntityStatus(slaEnt, (int)SlaState.Draft, (int)SlaStatusCode.Draft);
            }
        }

        public void ActivateSla(SlaEntity sla)
        {
            sla.ThrowIfNull();

            if (sla.SlaState == SlaState.Draft && sla.SlaStatus == SlaStatusCode.Draft)
            {
                Entity slaEnt = new Entity(SlaEntity.EntityName, sla.SlaId);
                this.CurrentAccess.SetEntityStatus(slaEnt, (int)SlaState.Active, (int)SlaStatusCode.Active);
            }
        }

        private List<SlaEntity> GetAllSlasByFilter(string filterColumn, object filteValue)
        {
            var records = this.CurrentAccess.GetEntitiesByColumn(SlaEntity.EntityName, filterColumn, filteValue, new string[]
            {   SlaFields.SLAId,
                SlaFields.IsDefault,
                SlaFields.Name,
                SlaFields.StatusCode,
                SlaFields.StateCode,
                SlaFields.SLAType
            }, 100).Entities;

            return records.Select(e => new SlaEntity
            {
                SlaId = e.GetAttributeValue<Guid>(SlaFields.SLAId),
                IsDefult = e.Contains(SlaFields.IsDefault) ? e.GetAttributeValue<bool>(SlaFields.IsDefault) : false,
                Name = e.GetAttributeValue<string>(SlaFields.Name),
                SlaStatus = e.Contains(SlaFields.StatusCode) ? (SlaStatusCode)e.GetAttributeValue<OptionSetValue>(SlaFields.StatusCode).Value : (SlaStatusCode?)null,
                SlaState = e.Contains(SlaFields.StateCode) ? (SlaState)e.GetAttributeValue<OptionSetValue>(SlaFields.StateCode).Value : (SlaState?)null,
                SlaType = e.Contains(SlaFields.SLAType) ? (SlaType)e.GetAttributeValue<OptionSetValue>(SlaFields.SLAType).Value : (SlaType?)null
            }).ToList();
        }
    }
}