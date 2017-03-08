using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;



namespace FMRS
{
    // http://forum.kerbalspaceprogram.com/index.php?/topic/147576-modders-notes-for-ksp-12/#comment-2754813
    // search for "Mod integration into Stock Settings

    public class FMRS_Settings : GameParameters.CustomParameterNode
    {
        public override string Title { get { return ""; } } // column heading
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "FMRS"; } }
        public override int SectionOrder { get { return 1; } }
        public override bool HasPresets { get { return false; } }


        [GameParameters.CustomParameterUI("FMRS Enabled")]
        public bool enabled = true;

        [GameParameters.CustomParameterUI("Auto-Active at launch")]
        public bool autoactiveAtLaunch = true;


        [GameParameters.CustomFloatParameterUI("Stage Delay", minValue = 0.2f, maxValue = 5.0f, asPercentage = false, displayFormat = "0.0",
                   toolTip = "How long after staging before saves are taken")]
        public float Timer_Stage_Delay = 0.2f;

        [GameParameters.CustomParameterUI("Messaging System")]
        public bool _SETTING_Messages = true;

        [GameParameters.CustomParameterUI("Auto Cut Off Engines")]
        public bool _SETTING_Auto_Cut_Off = true;

        [GameParameters.CustomParameterUI("Auto Recover Landed Crafts")]
        public bool _SETTING_Auto_Recover = true;

        [GameParameters.CustomParameterUI("Throttle Logger WIP")]
        public bool _SETTING_Throttle_Log = true;

        [GameParameters.CustomParameterUI("Parachutes are controllable",
            toolTip = "If enabled, any stage with a parachute will be treated as controllable by the mod")]
        public bool _SETTING_Parachutes = true;

        [GameParameters.CustomParameterUI("Defer parachute-only stages to Stage-Recovery (if installed)",
            toolTip = "If Stage Recovery is installed, do not control stages which only have parachutes")]
        public bool _SETTING_Defer_Parachutes_to_StageRecovery = true;



#if DEBUG
        [GameParameters.CustomParameterUI("Debug mode (spams the log file")]
        public  bool Debug_Active = true;
        [GameParameters.CustomParameterUI("Debug mode 1 initial")]
        public  bool Debug_Level_1_Active = true;
        [GameParameters.CustomParameterUI("Debug mode 2 initial")]
        public  bool Debug_Level_2_Active = true;
#endif

#if false
        public override void SetDifficultyPreset(GameParameters.Preset preset)
        {
            switch (preset)
            {
                case GameParameters.Preset.Easy:
                    toolbarEnabled = true;
                    toolbarPopupsEnabled = true;
                    editorMenuPopupEnabled = true;
                    hoverTimeout = 0.5f;
                    break;

                case GameParameters.Preset.Normal:
                    toolbarEnabled = true;
                    toolbarPopupsEnabled = true;
                    editorMenuPopupEnabled = true;
                    hoverTimeout = 0.5f;
                    break;

                case GameParameters.Preset.Moderate:
                    toolbarEnabled = true;
                    toolbarPopupsEnabled = true;
                    editorMenuPopupEnabled = true;
                    hoverTimeout = 0.5f;
                    break;

                case GameParameters.Preset.Hard:
                    toolbarEnabled = true;
                    toolbarPopupsEnabled = true;
                    editorMenuPopupEnabled = true;
                    hoverTimeout = 0.5f;
                    break;
            }
        }
#endif

        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {
            if (member.Name == "enabled")
                return true;

            return enabled; //otherwise return true
        }

        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {

            return true;
            //            return true; //otherwise return true
        }

        public override IList ValidValues(MemberInfo member)
        {
            return null;
        }

    }
}
