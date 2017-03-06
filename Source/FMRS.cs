/*
 * The MIT License (MIT)
 * 
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using UnityEngine;
using KSP.IO;
using KSP.UI.Screens;

namespace FMRS
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class FMRS : FMRS_Core
    {
        public static ApplicationLauncherButton Stock_Toolbar_Button = new ApplicationLauncherButton();

/*************************************************************************************************************************/
        public FMRS()
        {
            //nothing
        }


/*************************************************************************************************************************/
        void Awake()
        {
            Log.Info("Awake"); 
            
            FMRS_core_awake();

            //stb_texture = new Texture2D(38, 38);
            //stb_texture.LoadImage(System.IO.File.ReadAllBytes(Path.Combine(KSPUtil.ApplicationRootPath, "GameData/FMRS/icons/tb_st_di.png")));
            
            stb_texture = GameDatabase.Instance.GetTexture("FMRS/icons/tb_st_di", false);
            upArrow = GameDatabase.Instance.GetTexture("FMRS/Icons/up", false);
            downArrow = GameDatabase.Instance.GetTexture("FMRS/Icons/down", false);
            upContent = new GUIContent("", upArrow, "");
            downContent = new GUIContent("", downArrow, "");
            buttonContent = downContent;

            //if (Stock_Toolbar_Button != null)
            //    Stock_Toolbar_Button.SetTexture(stb_texture);

            GameEvents.onGUIApplicationLauncherReady.Add(add_toolbar_button);
            GameEvents.onGUIApplicationLauncherUnreadifying.Add(remove_toolbar_button);
            //if (ApplicationLauncher.Ready == true)
            //{
            //    add_toolbar_button();
            //}

            Log.Info("Version: " + mod_vers);
            
            _SAVE_SaveFolder = HighLogic.SaveFolder;
        }

        
/*************************************************************************************************************************/
        void Start()
        {
            //RenderingManager.AddToPostDrawQueue(3, new Callback(drawGUI));     
#if DEBUG
            //if (Debug_Level_1_Active)
                Log.PushStackInfo("FMRS.Start", "entering Start()");
#endif
            if (!_SAVE_Has_Launched)
                _SETTING_Enabled = false;

            if (FlightGlobals.ActiveVessel.situation == Vessel.Situations.PRELAUNCH && HighLogic.CurrentGame.Parameters.CustomParams<FMRS_Settings>().autoactiveAtLaunch)
            {
#if DEBUG
                if (Debug_Active)
                    Log.Info("ActiveVessel is prelaunch");
#endif
                
                _SETTING_Enabled = true;
                GameEvents.onLaunch.Add(launch_routine);
            }

            if (ToolbarManager.ToolbarAvailable)
            {
                Toolbar_Button = ToolbarManager.Instance.add("FMRS", "FMRSbutton");

                blz_toolbar_available = true;

                if (_SETTING_Enabled)
                    Toolbar_Button.TexturePath = "FMRS/icons/tb_blz_en";
                else
                    Toolbar_Button.TexturePath = "FMRS/icons/tb_blz_di";

                Toolbar_Button.ToolTip = "Flight Manager for Reusable Stages";
                Toolbar_Button.Visibility = new GameScenesVisibility(GameScenes.FLIGHT);
                Toolbar_Button.OnClick += (e) => toolbar_button_clicked();
            }

            if (_SETTING_Enabled)
            {
                flight_scene_start_routine();
                stb_texture = GameDatabase.Instance.GetTexture("FMRS/icons/tb_st_en", false);                             
            }
#if DEBUG
           // if (Debug_Level_1_Active)
                Log.PopStackInfo("leaving FMRS.Start ()");
#endif
        }

        private void OnGUI()
        {
            drawGUI();
        }


/*************************************************************************************************************************/
        void Update()
        {

            flight_scene_update_routine();

            if (ThrottleLogger != null)
#if DEBUG
                ThrottleLogger.Update(Debug_Active, Debug_Level_1_Active);
 #else
                 ThrottleLogger.Update();
 #endif
  		  
            if(ThrottleReplay != null)
 #if DEBUG
	                  ThrottleReplay.Update(Debug_Active, Debug_Level_1_Active);
#else
                 ThrottleReplay.Update();
#endif
            if (Input.GetKeyDown(KeyCode.F2))
                F2 = !F2;
            if (Input.GetKeyDown(KeyCode.F3) && F2)
                F2 = false;

        }


/*************************************************************************************************************************/
        void FixedUpdate()
        {
            if (ThrottleLogger != null)
                ThrottleLogger.LogThrottle(FlightGlobals.ActiveVessel.ctrlState.mainThrottle);

            //fixed_update_routine();
        }


/*************************************************************************************************************************/
        void OnDestroy()
        {
#if DEBUG
            //if (Debug_Level_1_Active)
                Log.PushStackInfo("FMRS.OnDestroy", "enter OnDestroy()");
#endif
            destroy_FMRS();

            if (ToolbarManager.ToolbarAvailable)
                Toolbar_Button.Destroy();

            GameEvents.onGUIApplicationLauncherReady.Remove(add_toolbar_button);
            GameEvents.onGUIApplicationLauncherUnreadifying.Remove(remove_toolbar_button);

            //RenderingManager.RemoveFromPostDrawQueue(3, new Callback(drawGUI));
#if DEBUG
            //if (Debug_Level_1_Active)
                Log.PopStackInfo("leave OnDestroy()");
#endif
        }


/*************************************************************************************************************************/
        public void add_toolbar_button()
        {
            Stock_Toolbar_Button = ApplicationLauncher.Instance.AddModApplication(
                toolbar_button_clicked,
                toolbar_button_clicked,
                null, null, null, null,
                ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW,
                stb_texture);
        }

        public void remove_toolbar_button(GameScenes scene)
        {
            ApplicationLauncher.Instance.RemoveModApplication(Stock_Toolbar_Button);
        }
    }





/*************************************************************************************************************************/
/*************************************************************************************************************************/
/*************************************************************************************************************************/
/*************************************************************************************************************************/
/*************************************************************************************************************************/
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class FMRS_Space_Center : FMRS_Core
    {
        private float delay = 35;


/*************************************************************************************************************************/
        public FMRS_Space_Center()
        {
            //nothing
        }


/*************************************************************************************************************************/
        void Awake()
        {
            FMRS_core_awake();
#if DEBUG
            Log.Info("FMRS Version: " + mod_vers);
#endif
            _SAVE_SaveFolder = HighLogic.SaveFolder;

#if DEBUG
            if (Debug_Active)
                Log.Info("FMRS_Space_Center On Awake");
#endif
        }


/*************************************************************************************************************************/
        void Start()
        {
#if DEBUG
            if (Debug_Active)
                Log.Info("FMRS_Space_Center On Start");


            Debug_Active = HighLogic.CurrentGame.Parameters.CustomParams<FMRS_Settings>().Debug_Active;
            Debug_Level_1_Active = HighLogic.CurrentGame.Parameters.CustomParams<FMRS_Settings>().Debug_Level_1_Active;
            Debug_Level_2_Active = HighLogic.CurrentGame.Parameters.CustomParams<FMRS_Settings>().Debug_Level_2_Active;

#endif
            _SETTING_Messages = HighLogic.CurrentGame.Parameters.CustomParams<FMRS_Settings>()._SETTING_Messages;
            _SETTING_Auto_Cut_Off = HighLogic.CurrentGame.Parameters.CustomParams<FMRS_Settings>()._SETTING_Auto_Cut_Off;
            _SETTING_Auto_Recover = HighLogic.CurrentGame.Parameters.CustomParams<FMRS_Settings>()._SETTING_Auto_Recover;
            _SETTING_Throttle_Log = HighLogic.CurrentGame.Parameters.CustomParams<FMRS_Settings>()._SETTING_Throttle_Log;
        }


        /*************************************************************************************************************************/
        void Update()
        {
            //nothing
        }


/*************************************************************************************************************************/
        void FixedUpdate()
        {
            Game savegame;
            ProtoVessel temp_proto;
            int load_vessel;

            if (_SAVE_Kick_To_Main)
            {
                if (delay > 0)
                    delay--;
                else
                {
#if DEBUG
                    if (Debug_Active)
                        Log.Info("FMRS_Space_Center kick to main");
#endif
                    _SAVE_Kick_To_Main = false;
                    write_save_values_to_file();

                    savegame = GamePersistence.LoadGame("FMRS_main_save", HighLogic.SaveFolder, false, false);
                    temp_proto = savegame.flightState.protoVessels.Find(p => p.vesselID == _SAVE_Main_Vessel);

                    if (temp_proto != null)
                    {
                        for (load_vessel = 0; load_vessel < savegame.flightState.protoVessels.Count && savegame.flightState.protoVessels[load_vessel].vesselID.ToString() != temp_proto.vesselID.ToString(); load_vessel++) ;

                        if (load_vessel  < savegame.flightState.protoVessels.Count)
                            FMRS_SAVE_Util.Instance.StartAndFocusVessel(savegame, load_vessel);
//                        FlightDriver.StartAndFocusVessel(savegame, load_vessel);
                    }

                    write_save_values_to_file();
                    write_recover_file();
                }
            }
        }


/*************************************************************************************************************************/
        void OnDestroy()
        {
        }
    }




/*************************************************************************************************************************/
/*************************************************************************************************************************/
/*************************************************************************************************************************/
/*************************************************************************************************************************/
/*************************************************************************************************************************/
    [KSPAddon(KSPAddon.Startup.TrackingStation, false)]
    public class FMRS_TrackingStation : FMRS_Core
    {
        private float delay = 35;


/*************************************************************************************************************************/
        public FMRS_TrackingStation()
        {
            //nothing
        }


/*************************************************************************************************************************/
        void Awake()
        {
            FMRS_core_awake();

#if DEBUG
            Log.Info("FMRS Version: " + mod_vers);
#endif
            _SAVE_SaveFolder = HighLogic.SaveFolder;
#if DEBUG
            if (Debug_Active)
                Log.Info("TrackingStation On Awake");
#endif
        }


/*************************************************************************************************************************/
        void Start()
        {
#if DEBUG
            if (Debug_Active)
                Log.Info("TrackingStation On Start");
#endif
        }

/*************************************************************************************************************************/
        void Update()
        {
            //nothing
        }


/*************************************************************************************************************************/
        void FixedUpdate()
        {
            Game savegame;
            ProtoVessel temp_proto;
            int load_vessel;

            if (_SAVE_Kick_To_Main)
            {
                if (delay > 0)
                    delay--;
                else
                {
#if DEBUG
                    if (Debug_Active)
                        Log.Info("TrackingStation kick to main");
#endif
                    _SAVE_Kick_To_Main = false;
                    write_save_values_to_file();

                    savegame = GamePersistence.LoadGame("FMRS_main_save", HighLogic.SaveFolder, false, false);
                    temp_proto = savegame.flightState.protoVessels.Find(p => p.vesselID == _SAVE_Main_Vessel);

                    if (temp_proto != null)
                    {
                        for (load_vessel = 0; load_vessel < savegame.flightState.protoVessels.Count && savegame.flightState.protoVessels[load_vessel].vesselID.ToString() != temp_proto.vesselID.ToString(); load_vessel++) ;

                        if (load_vessel < savegame.flightState.protoVessels.Count)
                            FMRS_SAVE_Util.Instance.StartAndFocusVessel(savegame, load_vessel);
//                        FlightDriver.StartAndFocusVessel(savegame, load_vessel);
                    }
                }
            }
        }


/*************************************************************************************************************************/
        void OnDestroy()
        {
            destroy_FMRS();
        }
    }




/*************************************************************************************************************************/
/*************************************************************************************************************************/
/*************************************************************************************************************************/
/*************************************************************************************************************************/
/*************************************************************************************************************************/
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class FMRS_Main_Menu : FMRS_Core
    {
/*************************************************************************************************************************/
        public FMRS_Main_Menu()
        {
            //nothing
        }


/*************************************************************************************************************************/
        void Awake()
        {
            FMRS_core_awake();
#if DEBUG
            Log.Info("FMRS Version: " + mod_vers);

            if (Debug_Active)
                Log.Info("FMRS_MainMenu On Awake");
#endif
        }


/*************************************************************************************************************************/
        void Start()
        {
#if DEBUG
            if (Debug_Active)
                Log.Info("FMRS_MainMenu On Start");
#endif

            if (_SETTING_Enabled)
            {
                _SETTING_Enabled = false;
                Log.Info("FMRS_MainMenu _SAVE_Has_Closed");
            }
            if (_SAVE_Has_Launched)
            {
                _SAVE_Has_Launched = false;
                Log.Info("FMRS_MainMenu _SAVE_Has_Launched");
            }
            if(_SAVE_Switched_To_Dropped)
            {
                Log.Info("FMRS_MainMenu _SAVE_Switched_To_Dropped");
                Game loadgame = GamePersistence.LoadGame("FMRS_main_save", _SAVE_SaveFolder, false, false);
                // GamePersistence.SaveGame(loadgame, "persistent", _SAVE_SaveFolder, SaveMode.OVERWRITE);
                FMRS_SAVE_Util.Instance.SaveGame("FMRS.Start", loadgame, "persistent", _SAVE_SaveFolder, SaveMode.OVERWRITE);
                _SAVE_Switched_To_Dropped = false;
            }

            write_save_values_to_file();
            write_recover_file();
        }


/*************************************************************************************************************************/
        void Update()
        {
            //nothing
        }


/*************************************************************************************************************************/
        void FixedUpdate()
        {
            //nothing
        }


/*************************************************************************************************************************/
        void OnDestroy()
        {
            //nothing
        }
    }
}