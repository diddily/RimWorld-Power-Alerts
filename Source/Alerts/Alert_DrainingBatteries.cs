using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;

namespace Power_Alerts.Alerts
{
    class Alert_DrainingBatteries : Alert
    {
        private int FirstTick = 0;

        enum BatteryState
        {
            Normal,
            Draining,
            Brownout
        }

        private IEnumerable<Building> GetDrainingBatteries()
        {
            return Find.Maps.FirstOrDefault(m => m.IsPlayerHome).powerNetManager.AllNetsListForReading.Where(pn => GetBatteryState(pn) != BatteryState.Normal).SelectMany(pn => pn.batteryComps).Select(bc => bc.parent as Building);
        }

        private BatteryState GetBatteryState(PowerNet pn)
        {
            if (Find.TickManager.TicksGame - FirstTick > 60 && pn.batteryComps.Count() > 0)
            {
                float cegr = pn.CurrentEnergyGainRate();
                float cse = pn.CurrentStoredEnergy();
                //!pn.Map.gameConditionManager.ConditionIsActive(GameConditionDefOf.SolarFlare)
                // Avoid spamming the warning when browning out by checking for insufficient power and keeping the alert on.
                if (cse / (float)pn.batteryComps.Sum(bc => bc.Props.storedEnergyMax) < 0.05f && 
                    pn.powerComps.Any(pc => !pc.PowerOn && FlickUtility.WantsToBeOn(pc.parent) && !pc.parent.IsBrokenDown()))
                {
                    return BatteryState.Brownout;
                }

                if (cegr < 0)
                {
                    float timeLeft = (cse / cegr / -60f);
                    if (timeLeft <= Power_Alerts.drainingBatteriesThresholdSeconds)
                    {
                        return BatteryState.Draining;
                    }
                }


            }
            
            return BatteryState.Normal;
        }


        public Alert_DrainingBatteries()
        {
            this.defaultLabel = "PA_Alert_DrainingBatteries_Label".Translate();
            this.defaultPriority = AlertPriority.High;
        }

        public override TaggedString GetExplanation()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (PowerNet pn in Find.Maps.FirstOrDefault(m => m.IsPlayerHome).powerNetManager.AllNetsListForReading)
            {
                switch (GetBatteryState(pn))
                {
                    case BatteryState.Normal:
                        break;
                    case BatteryState.Draining:
                        stringBuilder.AppendLine(string.Format("PA_Alert_DrainingBatteries_Draining_Description".Translate(), (pn.CurrentStoredEnergy() / pn.CurrentEnergyGainRate() / -60f)));
                        break;
                    case BatteryState.Brownout:
                        stringBuilder.AppendLine("PA_Alert_DrainingBatteries_Brownout_Description".Translate());
                        break;
                }
            }

            return stringBuilder.ToString();
        }

        public override AlertReport GetReport()
        {
            if (FirstTick == 0)
            {
                FirstTick = Find.TickManager.TicksGame;
            }

            if (!Power_Alerts.drainingBatteriesEnabled)
            {
                return false;
            }

            return AlertReport.CulpritsAre(GetDrainingBatteries().Cast<Thing>().ToList());
        }
    }
}
