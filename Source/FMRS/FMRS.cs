﻿/*
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
using System.Collections.Generic;

using UnityEngine;

using KSP.UI.Screens;

using Asset = KSPe.IO.Asset<FMRS.Startup>;
using File = KSPe.IO.File<FMRS.Startup>;
using ToolbarControl_NS;

namespace FMRS
{
	public static class FILES
	{
		public static readonly string SETTINGS_FOLDER   = System.IO.Path.GetFullPath(File.Data.Solve(".")); // Something in need to be revised on KSPe...
		public static readonly string SAVE_TXT          = File.Data.Solve("save.txt");
		public static readonly string RECOVER_TXT       = File.Data.Solve("recover.txt");
		public static readonly string RECORD_TXT        = File.Data.Solve("record.txt");

		public const string GAMESAVE_NAME = "FMRS_save_";
	}

	[KSPAddon(KSPAddon.Startup.Flight, false)]
    public class FMRS : FMRS_Core
    {
        //public static ApplicationLauncherButton Stock_Toolbar_Button = new ApplicationLauncherButton();
        public static ToolbarControl toolbarControl;
        internal const string MODID = "FMRS_NS";
        internal const string MODNAME = "Flight Manager for Reusable Stages";

        /*************************************************************************************************************************/
        public FMRS()
        {
            //nothing
        }


/*************************************************************************************************************************/
        void Awake()
        {
            Log.dbg("Awake"); 
            
            FMRS_core_awake();

            //stb_texture = new Texture2D(38, 38);
            //stb_texture.LoadImage(System.IO.File.ReadAllBytes(Path.Combine(KSPUtil.ApplicationRootPath, "GameData/FMRS/icons/tb_st_di.png")));

            //stb_texture = GameDatabase.Instance.GetTexture("FMRS/icons/tb_st_di", false);
            stockTexture = "tb_st_di";
            blizzyTexture = "tb_blz_di";

            upArrow   = Asset.Texture2D.LoadFromFile(2, 2, false, "icons", "up");   // GameDatabase.Instance.GetTexture("FMRS/Icons/up", false);
            downArrow = Asset.Texture2D.LoadFromFile(2, 2, false, "icons", "down"); // GameDatabase.Instance.GetTexture("FMRS/Icons/down", false);

            upContent = new GUIContent("", upArrow, "");
            downContent = new GUIContent("", downArrow, "");
            buttonContent = downContent;

            add_toolbar_button();
            //if (ApplicationLauncher.Ready == true)
            //{
            //    add_toolbar_button();
            //}

            _SAVE_SaveFolder = HighLogic.SaveFolder;
           
        }

        
/*************************************************************************************************************************/
        void Start()
        {
            //RenderingManager.AddToPostDrawQueue(3, new Callback(drawGUI));     
            Log.PushStackInfo("FMRS.Start", "entering Start()");

            if (!_SAVE_Has_Launched)
                _SETTING_Enabled = false;

            if (FlightGlobals.ActiveVessel.situation == Vessel.Situations.PRELAUNCH && HighLogic.CurrentGame.Parameters.CustomParams<FMRS_Settings>().autoactiveAtLaunch)
            {
                Log.dbg("ActiveVessel is prelaunch");
                
                _SETTING_Enabled = true;
                GameEvents.onLaunch.Add(launch_routine);
            }

            if (_SETTING_Enabled)
            {
                flight_scene_start_routine();
                //stb_texture = GameDatabase.Instance.GetTexture("FMRS/icons/tb_st_en", false);                             
                stockTexture = "tb_st_en";
                blizzyTexture = "tb_blz_en";
            }
            else
            {
                stockTexture = "tb_st_di";
                blizzyTexture = "tb_blz_di";
            }
            Log.info("SetTexture 1, stockTexture: {0},   blizzyTexture: {1}", stockTexture, blizzyTexture);
             if (toolbarControl != null)
                toolbarControl.SetTexture(
                    File.Asset.Solve("icons", stockTexture),
                    File.Asset.Solve("icons", blizzyTexture)
                );
            Log.PopStackInfo("leaving FMRS.Start ()");
            GameEvents.onShowUI.Add(ShowUI);
            GameEvents.onHideUI.Add(HideUI);

        }
        private void ShowUI()
        {
            HideFMRSUI = false;
        }
        void HideUI()
        {
            HideFMRSUI = true;
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
                 ThrottleLogger.Update();
  		  
            if(ThrottleReplay != null)
                 ThrottleReplay.Update();

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
            Log.PushStackInfo("FMRS.OnDestroy", "enter OnDestroy()");

            destroy_FMRS();

            remove_toolbar_button();

            //RenderingManager.RemoveFromPostDrawQueue(3, new Callback(drawGUI));
            Log.PopStackInfo("leave OnDestroy()");
        }

    
        /*************************************************************************************************************************/
        public void add_toolbar_button()
        {
            toolbarControl = gameObject.AddComponent<ToolbarControl>();
            toolbarControl.AddToAllToolbars(toolbar_button_clicked, toolbar_button_clicked,
                ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW,
                MODID,
                "fmrsButton",
                File.Asset.Solve("icons", "tb_st_di"),
                File.Asset.Solve("icons", "tb_blz_di"),
                MODNAME
            );

        }

        public void remove_toolbar_button()
        {
            toolbarControl.OnDestroy();
            Destroy(toolbarControl);
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
            Log.dbg("FMRS_Space_Center On Awake");

            FMRS_core_awake();
            _SAVE_SaveFolder = HighLogic.SaveFolder;

            ReloadSettings();
        }


/*************************************************************************************************************************/
        void Start()
        {
            GameEvents.OnGameSettingsApplied.Add(ReloadSettings);
        }

        void ReloadSettings()
        {
            Log.dbg("FMRS_Space_Center On Start");

            _SETTING_Messages = HighLogic.CurrentGame.Parameters.CustomParams<FMRS_Settings>()._SETTING_Messages;
            _SETTING_Auto_Cut_Off = HighLogic.CurrentGame.Parameters.CustomParams<FMRS_Settings>()._SETTING_Auto_Cut_Off;
            _SETTING_Auto_Recover = HighLogic.CurrentGame.Parameters.CustomParams<FMRS_Settings>()._SETTING_Auto_Recover;
            _SETTING_Throttle_Log = HighLogic.CurrentGame.Parameters.CustomParams<FMRS_Settings>()._SETTING_Throttle_Log;
            _SETTING_Parachutes = HighLogic.CurrentGame.Parameters.CustomParams<FMRS_Settings>()._SETTING_Parachutes;
            _SETTING_Defer_Parachutes_to_StageRecovery = HighLogic.CurrentGame.Parameters.CustomParams<FMRS_Settings>()._SETTING_Defer_Parachutes_to_StageRecovery;
            _SETTING_Control_Uncontrollable = HighLogic.CurrentGame.Parameters.CustomParams<FMRS_Settings>()._SETTING_Control_Uncontrollable;

            if (hasMod("StageRecovery"))
            {
                stageRecoveryInstalled = true;

            }
        }

        public static List<String> installedMods = new List<String>();
        public void buildModList()
        {
            Log.info("buildModList");
            //https://github.com/Xaiier/Kreeper/blob/master/Kreeper/Kreeper.cs#L92-L94 <- Thanks Xaiier!
            foreach (AssemblyLoader.LoadedAssembly a in AssemblyLoader.loadedAssemblies)
            {
                string name = a.name;
                Log.info(string.Format("Loading assembly: {0}", name));
                installedMods.Add(name);
            }
        }
        public bool hasMod(string modIdent)
        {
            if (installedMods.Count == 0)
                buildModList();
            return installedMods.Contains(modIdent);
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
                    Log.dbg("FMRS_Space_Center kick to main");

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
            Log.dbg("FMRS_Space_Center OnDestroy()");
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

            _SAVE_SaveFolder = HighLogic.SaveFolder;
            Log.dbg("TrackingStation On Awake");
        }


/*************************************************************************************************************************/
        void Start()
        {
            Log.dbg("TrackingStation On Start");
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
                    Log.dbg("TrackingStation kick to main");

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
            Log.dbg("FMRS_TrackingStation OnDestroy()");
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
            Log.dbg("FMRS_MainMenu On Awake");
            FMRS_core_awake();
        }

/*************************************************************************************************************************/
        void Start()
        {
            Log.dbg("FMRS_MainMenu On Start");

            if (_SETTING_Enabled)
            {
                _SETTING_Enabled = false;
                Log.info("FMRS_MainMenu _SAVE_Has_Closed");
            }
            if (_SAVE_Has_Launched)
            {
                _SAVE_Has_Launched = false;
                Log.info("FMRS_MainMenu _SAVE_Has_Launched");
            }
            if(_SAVE_Switched_To_Dropped)
            {
                Log.info("FMRS_MainMenu _SAVE_Switched_To_Dropped");
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
            Log.dbg("FMRS_Main_Menu OnDestroy()");
            //nothing
        }
    }
}