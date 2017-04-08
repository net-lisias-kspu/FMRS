using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KSP.UI.Screens;

namespace FMRS
{
    // Only need to do this once
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    class FMRS_Register:MonoBehaviour
    {
        void Start()
        {
            Log.Info("FMRS_Register.Start");
            if (RecoveryControllerWrapper.RecoveryControllerAvailable)
            {
                var o = RecoveryControllerWrapper.RegisterMod("FMRS");
                Log.Info("RecoveryControllerWrapper.RegisterMod: " + o.ToString());
            }
        }
    }
}
