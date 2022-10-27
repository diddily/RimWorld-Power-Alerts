using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Power_Alerts.Alerts
{
    class Alert_GeneratorFuelLow : Alert
    {
        private IEnumerable<CompPowerTrader> GetLowFuelGenerators()
        {
            return Find.Maps.FirstOrDefault(m => m.IsPlayerHome).powerNetManager.AllNetsListForReading.Where(pn => pn.batteryComps.Count() > 0 || pn.powerComps.Any(pc => pc != null && pc.Props.PowerConsumption > 0.0f)).SelectMany(pn => pn.powerComps.Where(pc => (pc?.parent?.Faction?.IsPlayer ?? false) && pc.PowerOn && pc.Props.PowerConsumption < 0.0f && IsFuelLow(pc.parent.TryGetComp<CompRefuelable>())));
        }
        
        private bool IsFuelLow(CompRefuelable cr)
        {
            return cr != null && (cr.Props.fuelConsumptionRate > 0.0f && (!cr.HasFuel || (cr.Fuel / cr.Props.fuelConsumptionRate * 1000f) <= Power_Alerts.lowFuelGeneratorThresholdSeconds));
        }

        public Alert_GeneratorFuelLow()
        {
            this.defaultLabel = "PA_Alert_LowFuelGenerator_Label".Translate();
            this.defaultPriority = AlertPriority.High;
        }
        
        public override TaggedString GetExplanation()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (CompRefuelable cr in GetLowFuelGenerators().Select(pc => pc.parent.GetComp<CompRefuelable>()))
            {
                if (!cr.HasFuel)
                {
                    stringBuilder.AppendLine("PA_Alert_LowFuelGenerator_Empty_Description".Translate());
                }
                else
                {
                    stringBuilder.AppendLine(string.Format("PA_Alert_LowFuelGenerator_Low_Description".Translate(), (cr.Fuel / cr.Props.fuelConsumptionRate * 1000f)));
                }
            }

            return stringBuilder.ToString();
        }

        public override AlertReport GetReport()
        {
            if (!Power_Alerts.lowFuelGeneratorEnabled)
            {
                return false;
            }

            return AlertReport.CulpritsAre(GetLowFuelGenerators().Select(pc => pc.parent as Building).Cast<Thing>().ToList());
        }
    }
}
