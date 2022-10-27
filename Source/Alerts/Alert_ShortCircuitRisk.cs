using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;

namespace Power_Alerts.Alerts
{
    class Alert_ShortCircuitRisk : Alert
    {
        private IEnumerable<Building> GetAtRiskBuildings()
        {
            return Find.Maps.FirstOrDefault(m => m.IsPlayerHome).powerNetManager.AllNetsListForReading.SelectMany(pn => pn.powerComps.Cast<CompPower>().Union(pn.batteryComps.Cast<CompPower>())).Where(pc => pc.Props.shortCircuitInRain && ((pc is CompPowerTrader) && ((CompPowerTrader)pc).PowerOn)).Select(bc => bc.parent as Building).Where(b => (b.Faction?.IsPlayer ?? false) && !b.Map.roofGrid.Roofed(b.Position));
        }

      
        public Alert_ShortCircuitRisk()
        {
            this.defaultLabel = "PA_Alert_ShortCircuitRisk_Label".Translate();
            this.defaultPriority = AlertPriority.High;
        }

        public override TaggedString GetExplanation()
        {
            int count = GetAtRiskBuildings().Count();
            if (count == 1)
            {
                return "PA_Alert_ShortCircuitRisk_Description".Translate();
            }
            else
            {
                return string.Format("PA_Alert_ShortCircuitRisks_Description".Translate(), count);
            }
        }

        public override AlertReport GetReport()
        {
            if (!Power_Alerts.shortCircuitRiskEnabled)
            {
                return false;
            }

            return AlertReport.CulpritsAre(GetAtRiskBuildings().Cast<Thing>().ToList());
        }
    }
}
