/*
 * The MIT License (MIT)
 * 
 * Copyright (c) 2018-2020 LisiasT
 * Copyright (c) 2015 SIT89
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
*/


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
        public override string DisplaySection { get { return "FMRS"; } }
        public override int SectionOrder { get { return 1; } }
        public override bool HasPresets { get { return false; } }


        [GameParameters.CustomParameterUI("FMRS Enabled")]
        public bool enabled = true;

        [GameParameters.CustomParameterUI("Auto-Active at launch")]
        public bool autoactiveAtLaunch = true;

        [GameParameters.CustomParameterUI("Include Undocking events as staging events",
            toolTip = "useful when staging isn't available (ie: after docking two ships, can't make the ports a stage)")]
        public bool _SETTING_Include_Undock = false;


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
            toolTip = "If Stage Recovery is installed, do not control stages which only have parachutes.  Note that using RecoveryController to specify a mod to control the stage will override this.")]
        public bool _SETTING_Defer_Parachutes_to_StageRecovery = true;

        [GameParameters.CustomParameterUI("Uncontrolled stages are controllable",
           toolTip = "Ignored if RecoveryController is active.  If enabled, any stage will be treated as controllable by the mod, even if you have no control over it.")]
        public bool _SETTING_Control_Uncontrollable = false;


#if false
        [GameParameters.CustomParameterUI("Default all stages to Stage-Recovery (if installed)",
           toolTip = "If Stage Recovery is installed, it will control the recovery unless changed in the Decoupler")]
        public bool _SETTING_Default_to_StageRecovery = true;
#endif

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
