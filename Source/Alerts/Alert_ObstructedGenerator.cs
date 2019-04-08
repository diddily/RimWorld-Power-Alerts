using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Verse;
using UnityEngine;

namespace Power_Alerts.Alerts
{
    class Alert_ObstructedGenerator : Alert
    {
        private static FieldInfo windPathBlockedCellsField = AccessTools.Field(typeof(CompPowerPlantWind), "windPathBlockedCells");
        private static FieldInfo waterUsableField = AccessTools.Field(typeof(CompPowerPlantWater), "waterUsable");
        private static FieldInfo waterDoubleUsedField = AccessTools.Field(typeof(CompPowerPlantWater), "waterDoubleUsed");
        private bool IsObstructedGenerator(CompPowerTrader pct)
        {
            if (Power_Alerts.obstructedSolarEnabled)
            {
                CompPowerPlantSolar solar = pct as CompPowerPlantSolar;
                if (solar != null)
                {
                    foreach (IntVec3 current in pct.parent.OccupiedRect())
                    {
                        if (pct.parent.Map.roofGrid.Roofed(current))
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }

            if (Power_Alerts.obstructedWindEnabled)
            {
                CompPowerPlantWind wind = pct as CompPowerPlantWind;
                if (wind != null)
                {
                    return (windPathBlockedCellsField.GetValue(wind) as List<IntVec3>).Count() > 0;
                }
            }

            if (Power_Alerts.obstructedWaterEnabled)
            {
                CompPowerPlantWater water = pct as CompPowerPlantWater;
                if (water != null)
                {
                    return !((bool)waterUsableField.GetValue(water)) || ((bool)waterDoubleUsedField.GetValue(water));
                }
            }

            return false;
        }

        private IEnumerable<Building> GetObstructedGenerators()
        {
            return Find.Maps.FirstOrDefault(m => m.IsPlayerHome).powerNetManager.AllNetsListForReading.Where(pn => pn.batteryComps.Count() > 0 || pn.powerComps.Any(pc => pc != null && pc.Props.basePowerConsumption > 0.0f)).SelectMany(pn => pn.powerComps).Where(pct => IsObstructedGenerator(pct)).Select(pct => pct.parent as Building);
        }

        public Alert_ObstructedGenerator()
        {
            this.defaultLabel = "PA_Alert_ObstructedGenerator_Label".Translate();
            this.defaultExplanation = "PA_Alert_ObstructedGenerator_Description".Translate();
            this.defaultPriority = AlertPriority.High;
        }
        
        public override AlertReport GetReport()
        {
            if (!Power_Alerts.obstructedSolarEnabled && !Power_Alerts.obstructedWindEnabled && !Power_Alerts.obstructedWaterEnabled)
            {
                return false;
            }

            return AlertReport.CulpritsAre(GetObstructedGenerators());
        }
    }
}
