
using HugsLib;
using HugsLib.Utils;
using HugsLib.Settings;
using Verse;
using RimWorld;
using UnityEngine;

namespace Power_Alerts
{
    public class Power_Alerts : ModBase
    {
        public static Power_Alerts Instance { get; private set; }

        internal static SettingHandle<bool> disconnectedGeneratorEnabled;

        internal static SettingHandle<bool> lowFuelGeneratorEnabled;
        internal static SettingHandle<float> lowFuelGeneratorThresholdSeconds;

        internal static SettingHandle<bool> drainingBatteriesEnabled;
        internal static SettingHandle<float> drainingBatteriesThresholdSeconds;

        internal static SettingHandle<bool> shortCircuitRiskEnabled;


        public override string ModIdentifier
        {
            get { return "Power_Alerts"; }
        }

        public Power_Alerts()
        {
            Instance = this;
        }

        public override void DefsLoaded()
        {
            base.DefsLoaded();

            disconnectedGeneratorEnabled = Settings.GetHandle<bool>("disconnectedGeneratorEnabled", "PA_DisconnectedGeneratorEnabled_Title".Translate(), "PA_DisconnectedGeneratorEnabled_Description".Translate(), true);

            lowFuelGeneratorEnabled = Settings.GetHandle<bool>("lowFuelGeneratorEnabled", "PA_LowFuelGeneratorEnabled_Title".Translate(), "PA_LowFuelGeneratorEnabled_Description".Translate(), true);
            lowFuelGeneratorThresholdSeconds = Settings.GetHandle<float>("lowFuelGeneratorThresholdSeconds", "PA_LowFuelGeneratorThresholdSeconds_Title".Translate(), "PA_LowFuelGeneratorThresholdSeconds_Description".Translate(), 60.0f, Validators.FloatRangeValidator(0.0f, 6000.0f));

            drainingBatteriesEnabled = Settings.GetHandle<bool>("drainingBatteriesEnabled", "PA_DrainingBatteriesEnabled_Title".Translate(), "PA_DrainingBatteriesEnabled_Description".Translate(), true);
            drainingBatteriesThresholdSeconds = Settings.GetHandle<float>("drainingBatteriesThresholdSeconds", "PA_DrainingBatteriesThresholdSeconds_Title".Translate(), "PA_DrainingBatteriesThresholdSeconds_Description".Translate(), 60.0f, Validators.FloatRangeValidator(0.0f, 6000.0f));

            shortCircuitRiskEnabled = Settings.GetHandle<bool>("shortCircuitRiskEnabled", "PA_ShortCircuitRiskEnabled_Title".Translate(), "PA_ShortCircuitRiskEnabled_Description".Translate(), true);
        }

    }
}