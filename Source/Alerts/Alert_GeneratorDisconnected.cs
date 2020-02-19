using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Power_Alerts.Alerts
{
    class Alert_GeneratorDisconnected : Alert
    {
        private IEnumerable<Building> GetDisconnectedGenerators()
        {
            return Find.Maps.FirstOrDefault(m => m.IsPlayerHome).powerNetManager.AllNetsListForReading.Where(pn => !pn.powerComps.Any(pc => pc.Props.basePowerConsumption > 0.0f) && !pn.transmitters.Any(t => t.parent.AllComps.Any(c => c is CompShipPart))).SelectMany(pn => pn.powerComps.Where(pc => pc.PowerOutput > 0.0f)).Select(pc => pc.parent as Building);
        }

        public Alert_GeneratorDisconnected()
        {
            this.defaultLabel = "PA_Alert_DisconnectedGenerator_Label".Translate();
            this.defaultPriority = AlertPriority.High;
        }
        
        public override TaggedString GetExplanation()
        {
            int count = GetDisconnectedGenerators().Count();
            if (count == 1)
            {
                return "PA_Alert_DisconnectedGenerator_Description".Translate();
            }
            else
            {
                return string.Format("PA_Alert_DisconnectedGenerators_Description".Translate(), count);
            }
        }

        public override AlertReport GetReport()
        {
            if (!Power_Alerts.disconnectedGeneratorEnabled)
            {
                return false;
            }

            return AlertReport.CulpritsAre(GetDisconnectedGenerators().Cast<Thing>().ToList());
        }
    }
}
