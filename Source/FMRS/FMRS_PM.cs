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
using System;

namespace FMRS
{
    class FMRS_PM:PartModule
    {
        public string parent_vessel;


/*************************************************************************************************************************/
        public override void OnLoad(ConfigNode node)
        {
            try
            {
                parent_vessel = node.GetValue("parent_vessel");
            }
            catch (Exception)
            {
                parent_vessel = "00000000-0000-0000-0000-000000000000";
            }
        }


/*************************************************************************************************************************/
        public override void OnSave(ConfigNode node)
        {
            node.AddValue("MM_DYNAMIC", "true");
            node.AddValue("parent_vessel", parent_vessel);
        }

        
/*************************************************************************************************************************/
        public void setid()
        {
            parent_vessel = this.vessel.id.ToString();
        }


/*************************************************************************************************************************/
        public void resetid()
        {
            parent_vessel = "00000000-0000-0000-0000-000000000000";
        }
    }
}