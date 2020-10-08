/*
 * The MIT License (MIT)
 * 
 * Copyright (c) 2019 LinuxGuruGamer
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
