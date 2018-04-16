using UnityEngine;
using ToolbarControl_NS;

namespace FMRS
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RegisterToolbar : MonoBehaviour
    {
        void Start()
        {
            ToolbarControl.RegisterMod(FMRS.MODID, FMRS.MODNAME);
        }
    }
}