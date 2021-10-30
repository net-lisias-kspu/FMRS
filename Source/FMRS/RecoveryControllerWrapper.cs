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
using System.Linq;
using System.Reflection;

using UnityEngine;

//
// RecoveryControllerWrapper
//
// usage
//  Copy the RecoveryControllerWrapper.cs file into your project.
//  Edit the namespace in RecoveryControllerWrapper.cs to match your plugin's namespace.
//  Use the RecoveryControllerWrapper Plugin's API
//  You can use RecoveryControllerWrapper.RecoveryControllerAvailable to check if the RecoveryControllerWrapper Plugin is actually available. 
//  Note that you must not call any other RecoveryControllerWrapper Plugin API methods if this returns false.
//
// Functions available
//
//      string RecoveryControllerWrapper.RecoveryControllerAvailable()
//      string RecoveryControllerWrapper.RegisterMod(string modName)
//      string RecoveryControllerWrapper.UnRegisterMod(string modName)
//      string RecoveryControllerWrapper.ControllingMod(Vessel v)
//
//  To use:
//
//      Your mod must register itself first, passing in your modname.  This MUST be done before going into either the editor or flight
//      You can unregister you rmod if you like
//      Call the ControllingMod with the vessel to find out which mod is registered to have control of it.

// TODO: Change to your plugin's namespace here.
namespace FMRS
{

    /**********************************************************\
    *          --- DO NOT EDIT BELOW THIS COMMENT ---          *
    *                                                          *
    * This file contains classes and interfaces to use the     *
    * Toolbar Plugin without creating a hard dependency on it. *
    *                                                          *
    * There is nothing in this file that needs to be edited    *
    * by hand.                                                 *
    *                                                          *
    *          --- DO NOT EDIT BELOW THIS COMMENT ---          *
    \**********************************************************/


    class RecoveryControllerWrapper
    {
        private static bool? recoveryControllerAvailable;
        private static Type calledType;

        public static bool RecoveryControllerAvailable
        {
            get
            {
                if (recoveryControllerAvailable == null)
                {
                    recoveryControllerAvailable = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "RecoveryController");
                    calledType = Type.GetType("RecoveryController.RecoveryController,RecoveryController");
                }
                return recoveryControllerAvailable.GetValueOrDefault();
            }
        }

        static object CallRecoveryController(string func, object modName)
        {
            if (!RecoveryControllerAvailable)
                return null;
            try
            {

                if (calledType != null)
                {
                    MonoBehaviour rcRef = (MonoBehaviour)UnityEngine.Object.FindObjectOfType(calledType); //assumes only one instance of class Historian exists as this command returns first instance found, also must inherit MonoBehavior for this command to work. Getting a reference to your Historian object another way would work also.
                    if (rcRef != null)
                    {
                        MethodInfo myMethod = calledType.GetMethod(func, BindingFlags.Instance | BindingFlags.Public);

                        if (myMethod != null)
                        {
                            object magicValue;
                            if (modName != null)
                                magicValue = myMethod.Invoke(rcRef, new object[] { modName });
                            else
                                magicValue = myMethod.Invoke(rcRef, null);
                            return magicValue;
                        }
                        else
                        {
                            Log.detail("{0} not available in RecoveryController", func);
                        }
                    }
                    else
                    {
                        Log.warn("{0} failed", func);
                        return null;
                    }
                }
                Log.warn("calledtype failed");
                return null;
            }
            catch (Exception e)
            {
                Log.error("Error calling type: {0}", e);
                return null;
            }
        }

        public static bool RegisterMod(string modName)
        {
            if (!RecoveryControllerAvailable)
            {
                return false;
            }
            var s = CallRecoveryController("RegisterMod", modName);
            if (s == null)
                return false;
            return (bool)s;
        }

        public static bool UnRegisterMod(string modName)
        {
            if (!RecoveryControllerAvailable)
            {
                return false;
            }
            var s = CallRecoveryController("UnRegisterMod", modName);
            if (s == null)
                return false;
            return (bool)s;
        }

        public static string ControllingMod(Vessel v)
        {
            if (!RecoveryControllerAvailable)
            {
                return null;
            }
            if (v.name.StartsWith("Ast."))
                return "";
            var s = CallRecoveryController("ControllingMod", v);
            if (s != null)
                return (string)s;
            return null;
        }
    }
}
