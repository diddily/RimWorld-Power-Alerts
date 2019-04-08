using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;

namespace Power_Alerts.Alerts
{
    class Alert_GeneratorWastingFuel : Alert
    {
        private int CurrentDayOfYear = -1;
        private float CurrentAverageGlow = 0;
        private List<Building> alertThings = new List<Building>();
        private List<PowerNet> batteryLow = new List<PowerNet>();
        private HashSet<Building> prevAlertThings = new HashSet<Building>();
        private HashSet<PowerNet> prevBatteryLow = new HashSet<PowerNet>();
        static float batteryHighToLowThreshold = 0.9f;
        static float batteryLowToHighThreshold = 0.98f;
        static float powerCompAlertOnToOffThreshold = 1.05f;
        static float powerCompAlertOffToOnThreshold = 1.25f;

        private IEnumerable<Building> GetWastingFuelGenerators()
        {
            prevAlertThings.Clear();
            prevAlertThings.Concat(alertThings);
            alertThings.Clear();
            prevBatteryLow.Clear();
            prevBatteryLow.Concat(batteryLow);
            batteryLow.Clear();
            Map map = Find.Maps.FirstOrDefault(m => m.IsPlayerHome);
            Vector2 longLat = Find.WorldGrid.LongLatOf(map.Tile);
            int gameTicks = Find.TickManager.TicksGame;
            int absTicks = Find.TickManager.TicksAbs;
            int dayOfYear = GenDate.DayOfYear((long)absTicks, longLat.x);
            if (dayOfYear != CurrentDayOfYear)
            {
                CurrentAverageGlow = GenCelestial.AverageGlow(longLat.y, dayOfYear);
                CurrentDayOfYear = dayOfYear;
            }
            float glow = CurrentAverageGlow;
            if (map.gameConditionManager.ConditionIsActive(GameConditionDefOf.Eclipse))
            {
                glow = 0.0f;
            }
            if (map.gameConditionManager.ConditionIsActive(GameConditionDefOf.VolcanicWinter))
            {
                glow *= 0.5f;
            }
            float averageWind = 0.55f;
            foreach (PowerNet powerNet in map.powerNetManager.AllNetsListForReading.Where(pn => pn.batteryComps.Count() > 0 || pn.powerComps.Any(pc => pc != null && pc.Props.basePowerConsumption > 0.0f)))
            {
                if (powerNet.batteryComps.Count() > 0)
                {
                    float batteryPercent = powerNet.batteryComps.Sum(bc => bc.StoredEnergy) / powerNet.batteryComps.Sum(bc => bc.Props.storedEnergyMax);
                    if (prevBatteryLow.Contains(powerNet))
                    {
                        if (batteryPercent < batteryLowToHighThreshold)
                        {
                            batteryLow.Add(powerNet);
                            continue;
                        }
                    }
                    else
                    {
                        if (batteryPercent < batteryHighToLowThreshold)
                        {
                            batteryLow.Add(powerNet);
                            continue;
                        }
                    }
                }
                IEnumerable<CompPowerTrader> generators = powerNet.powerComps.Where(pc => pc != null && pc.PowerOn && pc.Props.basePowerConsumption < 0.0f);
                IEnumerable<CompPowerPlantSolar> solarGens = generators.OfType<CompPowerPlantSolar>();
                IEnumerable<CompPowerPlantWind> windGens = generators.OfType<CompPowerPlantWind>();
                float deltaPower = solarGens.Sum(pc => glow * 1700 - pc.PowerOutput) + windGens.Sum(pc => averageWind * -pc.Props.basePowerConsumption - pc.PowerOutput);
                float excess = powerNet.CurrentEnergyGainRate() / CompPower.WattsToWattDaysPerTick + deltaPower;
                
                if (excess > 0)
                {
                    foreach (CompPowerTrader cpt in generators)
                    {
                        CompRefuelable cr = cpt.parent.GetComp<CompRefuelable>();
                        if (cr != null)
                        {
                            if (prevAlertThings.Contains(cpt.parent as Building))
                            {
                                if (excess < -cpt.Props.basePowerConsumption * powerCompAlertOnToOffThreshold)
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                if(excess < -cpt.Props.basePowerConsumption * powerCompAlertOffToOnThreshold)
                                {
                                    continue;
                                }
                            }

                            alertThings.Add(cpt.parent as Building);
                        }
                    }
                }
            }

            return alertThings;
        }
        
        public Alert_GeneratorWastingFuel()
        {
            this.defaultLabel = "PA_Alert_GeneratorWastingFuel_Label".Translate();
            this.defaultExplanation = "PA_Alert_GeneratorWastingFuel_Description".Translate();
            this.defaultPriority = AlertPriority.High;
        }

        public override AlertReport GetReport()
        {
            if (!Power_Alerts.wastingFuelGeneratorEnabled)
            {
                return false;
            }
                
            return AlertReport.CulpritsAre(GetWastingFuelGenerators());
        }
    }
}
