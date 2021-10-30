/*
	This file is part of Flight Manager for Reusable Stages [FMRS] /L Unleashed
		© 2021 Lisias T : http://lisias.net <support@lisias.net>
		© 2017-2018 LinuxGuruGamer
		© 2014-2015 SIT89

	FMRS /L is double licensed, as follows:
		* SKL 1.0 : https://ksp.lisias.net/SKL-1_0.txt
		* GPL 2.0 : https://www.gnu.org/licenses/gpl-2.0.txt

	And you are allowed to choose the License that better suit your needs.

	FMRS /L Unleashed is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

	You should have received a copy of the SKL Standard License 1.0
	along with FMRS /L Unleashed. If not, see <https://ksp.lisias.net/SKL-1_0.txt>.

	You should have received a copy of the GNU General Public License 2.0
	along with FMRS /L Unleashed. If not, see <https://www.gnu.org/licenses/>.

*/
using System.Collections;
using System.Reflection;

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
