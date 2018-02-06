using System;

namespace Capgemini.Xrm.Deployment.Model.Sla
{
    public class SlaEntity
    {
        public static string EntityName = "sla";

        public Guid SlaId { get; set; }

        public string Name { get; set; }

        public SlaState? SlaState { get; set; }

        public SlaStatusCode? SlaStatus { get; set; }

        public SlaType? SlaType { get; set; }
        public bool IsDefult { get; internal set; }
    }
}