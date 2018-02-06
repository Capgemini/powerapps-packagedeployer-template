using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;

namespace Capgemini.Xrm.Deployment.IntegrationTests.Helpers
{
    public static class EntityMockHelper
    {
        public static List<Entity> EntitiesToCreate
        {
            get
            {
                List<Entity> ent = new List<Entity>();

                int idx = 50;

                while (idx > 0)
                {
                    ent.Add(GetContactEntity());
                    idx--;
                }

                return ent;
            }
        }

        public static List<Entity> EntitiesToCreateConfigurationParatmers
        {
            get
            {
                List<Entity> ent = new List<Entity>();

                int idx = 10;
                while (idx > 0)
                {
                    ent.Add(GetConfigurationParameters());
                    idx--;
                }
                return ent;
            }
        }

        private static Entity GetConfigurationParameters()
        {
            Entity ent = new Entity("cap_configurationparameter", Guid.NewGuid());
            Guid configurationParam = Guid.NewGuid();
            ent.Id = configurationParam;
            ent["cap_value"] = "Test";
            return ent;
        }

        private static Entity GetContactEntity()
        {
            Entity ent = new Entity("contact", Guid.NewGuid());
            Guid id = Guid.NewGuid();
            ent.Id = id;
            ent["firstname"] = "Test";
            ent["lastname"] = id.ToString();
            return ent;
        }
    }
}