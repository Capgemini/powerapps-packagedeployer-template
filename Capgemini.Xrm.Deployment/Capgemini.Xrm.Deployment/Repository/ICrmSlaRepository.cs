using Capgemini.Xrm.Deployment.Model.Sla;
using System;
using System.Collections.Generic;

namespace Capgemini.Xrm.Deployment.Repository
{
    public interface ICrmSlaRepository
    {
        List<SlaEntity> GetAllSlas();

        SlaEntity GetSlaById(Guid slaId);

        SlaEntity GetSlaByName(string name);

        void SetSlaDefault(SlaEntity sla);

        void DeactivateSla(SlaEntity sla);

        void ActivateSla(SlaEntity sla);
    }
}