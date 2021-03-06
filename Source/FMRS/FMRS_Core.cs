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
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Contracts;

using File = KSPe.IO.File<FMRS.Startup>;

namespace FMRS
{
    public partial class FMRS_Core : FMRS_Util, IFMRS
    {
        //private string version_number = "1.0.3";
        public bool plugin_active = false;
        public double Time_Trigger_Staging, Time_Trigger_Start_Delay, Time_Trigger_Cuto;
        public bool timer_staging_active = false, timer_start_delay_active = false, timer_cuto_active = false;
        public float Timer_Stage_Delay = 0.2f;
        public double Timer_Start_Delay = 2;
        public double last_staging_event = 0;
        public string quicksave_file_name;
        public Guid anz_id;
        public bool separated_vessel, staged_vessel;
        public bool n_launchpad_preflight = false, flight_preflight = false;
        public bool main_ui_active = false;
        public bool revert_to_launch = false;
        public bool really_close = false;
        public bool show_setting = false;

        public string stockTexture;
        public string blizzyTexture;

        public static Texture2D upArrow;
        public static Texture2D downArrow;
        public GUIContent upContent;
        public GUIContent downContent;
        public GUIContent buttonContent;

        public bool can_restart, can_q_save_load;
        private int nr_save_files = 0;
        public Vector2 scroll_Vector = Vector2.zero;
        GUIStyle button_main, button_green, button_red, button_yellow, button_small, button_small_red, button_big;
        GUIStyle text_main, text_green, text_cyan, text_red, text_yellow, text_heading;
        GUIStyle area_style, scrollbar_stlye;
        private bool skin_init = false;
        private List<science_data_sent> science_sent = new List<science_data_sent>();
        private float current_rep, last_rep_change;
        private List<killed_kerbal_str> killed_kerbals = new List<killed_kerbal_str>();
        private Dictionary<Guid, List<Contract>> contract_complete = new Dictionary<Guid, List<Contract>>();
        private List<Guid> loaded_vessels = new List<Guid>();
        private List<Guid> damaged_vessels = new List<Guid>();
        private List<string> damaged_buildings = new List<string>();
        public FMRS_THL.FMRS_THL_Log ThrottleLogger;
        public FMRS_THL.FMRS_THL_Rep ThrottleReplay;
        public static bool HideFMRSUI = false;

#if DEBUG  //**************************
        public string[] debug_message = { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" };
        public Rect debug_windowPos;
#endif //**************************

#if BETA //**************************
        //string beta_version = " .01";
        public Rect beta_windowPos;
#endif   //**************************


        /*************************************************************************************************************************/
        public void FMRS_core_awake()
        {
            Log.PushStackInfo("FMRS_Core.FMRS_core_awake", "entering FMRS_core_awake()");

            Log.dbg("FMRS CODE AWAKE {0}", FILES.SETTINGS_FOLDER);
            if (!System.IO.Directory.Exists(FILES.SETTINGS_FOLDER)) System.IO.Directory.CreateDirectory(FILES.SETTINGS_FOLDER);
			init_Save_File_Content();
            load_save_file();

            Log.PopStackInfo("leaving FMRS_core_awake()");

            if (HighLogic.CurrentGame != null)
                Timer_Stage_Delay = HighLogic.CurrentGame.Parameters.CustomParams<FMRS_Settings>().Timer_Stage_Delay;
        }


        /*************************************************************************************************************************/
        public void flight_scene_start_routine()
        {
            Log.PushStackInfo("FMRS_Core.flight_scene_start_routine", "entering flight_scene_start_routine()");
            Log.dbg("FMRS flight_scene_start_routine");

            plugin_active = true;
            if (FlightGlobals.ActiveVessel == null)
                Log.info("ActiveVessel is null");
            else
            {
                if (FlightGlobals.ActiveVessel.situation == Vessel.Situations.PRELAUNCH || n_launchpad_preflight || flight_preflight)
                {
                    Log.dbg("FMRS Vessel is prelaunch");

                    delete_dropped_vessels();
                    _SETTING_Enabled = true;
                    _SAVE_Switched_To_Dropped = false;
                    _SAVE_Kick_To_Main = false;
                    _SAVE_Main_Vessel = FlightGlobals.ActiveVessel.id;
                    _SAVE_Launched_At = 0;
                    _SAVE_Has_Launched = false;

                    if (flight_preflight)
                        _SAVE_Flight_Reset = true;

                    foreach (Part p in FlightGlobals.ActiveVessel.Parts)
                    {
                        foreach (PartModule pm in p.Modules)
                            if (pm.moduleName == "FMRS_PM")
                                pm.StartCoroutine("resetid");
                    }

                    recover_values.Clear();

                    StartCoroutine(save_game_pre_flight());
                }
            }
            
            can_restart = HighLogic.CurrentGame.Parameters.Flight.CanRestart;
            if (HighLogic.CurrentGame.Parameters.Flight.CanQuickLoad && HighLogic.CurrentGame.Parameters.Flight.CanQuickSave)
                can_q_save_load = true;
            else
                can_q_save_load = false;

            if (FlightGlobals.ActiveVessel.id == _SAVE_Main_Vessel)
            {
                _SAVE_Switched_To_Dropped = false;
                _SAVE_Kick_To_Main = false;
            }
            else
                _SAVE_Switched_To_Dropped = true;
            

            Time_Trigger_Start_Delay = Planetarium.GetUniversalTime();
            timer_start_delay_active = true;

            if (_SAVE_Switched_To_Dropped)
            {
                foreach (KeyValuePair<Guid, string> kvp in Vessels_dropped)
                {
                    if (kvp.Value == _SAVE_Switched_To_Savefile)
                    {
                        if (get_vessel_state(kvp.Key) == vesselstate.FLY)
                        {
                            loaded_vessels.Add(kvp.Key);
                        }
                    }
                }

                Log.dbg("loaded_vessels: {0}", loaded_vessels.Count);

                if (_SETTING_Throttle_Log)
                {
                    ThrottleReplay = new FMRS_THL.FMRS_THL_Rep();

                    foreach (Vessel v in FlightGlobals.Vessels)
                    {
                        if (v.id == _SAVE_Main_Vessel)
                        {
                            //if (v.loaded)
                            {
                                Log.info("appling flybywire callback to main vessel");
                                v.OnFlyByWire += new FlightInputCallback(ThrottleReplay.flybywire);
                            }
                        }
                    }
                }
            }

            if ((windowPos.x == 0) && (windowPos.y == 0))
            {
                windowPos = new Rect(Screen.width / 2, Screen.height / 2, 50, 50);
                write_save_values_to_file();
            }

            Vessels.Clear();
            fill_Vessels_list();

            timer_staging_active = false;
            really_close = false;
            revert_to_launch = false;
            separated_vessel = false;
            staged_vessel = false;

            current_rep = Reputation.CurrentRep;
            last_rep_change = 0;

            attach_handlers();

            if (!main_ui_active)
            {
                main_ui_active = true;

                Log.dbg("activate drawMainGUI");
            }

            Log.PopStackInfo("leaving flight_scene_start_routine()");
        }


        /*************************************************************************************************************************/
        public void flight_scene_update_routine()
        {
            Log.PushStackInfo("FMRS_Core.flight_scene_update_routine", "entering flight_scene_update_routine()");
            Instance = this;
            if (_SETTING_Enabled)
            {
                if (timer_start_delay_active)
                    if ((Timer_Start_Delay + Time_Trigger_Start_Delay) <= Planetarium.GetUniversalTime())
                    {
                        if (!_SAVE_Switched_To_Dropped)
                        {
                            write_recovered_values_to_save();
                        }
                        else
                        {
                            List<Guid> temp_guid_list = new List<Guid>();
                            foreach (Guid id in loaded_vessels)
                            {
                                if (FlightGlobals.Vessels.Find(v => v.id == id) == null)
                                    temp_guid_list.Add(id);
                            }
                            foreach (Guid id in temp_guid_list)
                            {
                                Log.dbg("loaded_vessels: removing " + id.ToString());
                                loaded_vessels.Remove(id);
                            }
                            Log.dbg("loaded_vessels: " + loaded_vessels.Count.ToString());
                        }
                        timer_start_delay_active = false;
                    }

                if (timer_staging_active)
                {
                    if (timer_cuto_active && _SETTING_Auto_Cut_Off)
                    {
                        //if ((Time_Trigger_Cuto + 0.1) <= Planetarium.GetUniversalTime())
                        //if (_SETTING_Auto_Cut_Off)
                        {
                            Log.dbg("auto thrust cut off");
                            foreach (Vessel temp_vessel in FlightGlobals.Vessels)
                            {
                                if (!Vessels.Contains(temp_vessel.id))
                                {
                                   
                                        temp_vessel.ctrlState.mainThrottle = 0;
                                }
                            }
                            timer_cuto_active = false;
                        }
                    }

                    if ((Time_Trigger_Staging + Timer_Stage_Delay) <= Planetarium.GetUniversalTime())
                    {
                        Log.dbg("Has Staged Delayed");

                        last_staging_event = Planetarium.GetUniversalTime();

                        timer_staging_active = false;

                        quicksave_file_name = FILES.GAMESAVE_NAME + FlightGlobals.ActiveVessel.currentStage.ToString();

                        if (Vessels_dropped.ContainsValue(quicksave_file_name) || (separated_vessel && !staged_vessel))
                        {
                            int nr_save_file = 0;

                            foreach (KeyValuePair<Guid, string> temp_keyvalues in Vessels_dropped)
                            {
                                if (temp_keyvalues.Value.Contains("separated_"))
                                    if (nr_save_file <= Convert.ToInt16(temp_keyvalues.Value.Substring(20)))
                                        nr_save_file = Convert.ToInt16(temp_keyvalues.Value.Substring(20)) + 1;
                            }

                            quicksave_file_name = FILES.GAMESAVE_NAME + "separated_" + nr_save_file;
                        }

                        separated_vessel = false;
                        staged_vessel = false;

                        if (search_for_new_vessels(quicksave_file_name))
                        {
                            // GamePersistence.SaveGame(quicksave_file_name, HighLogic.SaveFolder + "/FMRS", SaveMode.OVERWRITE);
                            FMRS_SAVE_Util.Instance.SaveGame("FMRS_Core.flight_scene_update_routine", quicksave_file_name, HighLogic.SaveFolder + "/FMRS", SaveMode.OVERWRITE);

                            if (_SAVE_Main_Vessel != FlightGlobals.ActiveVessel.id && !_SAVE_Switched_To_Dropped)
                                main_vessel_changed(quicksave_file_name);

                            set_save_value(save_cat.SAVEFILE, quicksave_file_name, Planetarium.GetUniversalTime().ToString());
                            write_save_values_to_file();
                        }
                    }
                }

                if (n_launchpad_preflight && !FlightGlobals.ActiveVessel.Landed)
                {
                    EventReport dummy_event = null;
                    Log.dbg("non launchpad launch");
                    n_launchpad_preflight = false;
                    launch_routine(dummy_event);
                }
            }

            Log.PopStackInfo("leaving flight_scene_update_routine");
        }


        /*************************************************************************************************************************/
        private IEnumerator save_game_pre_flight()
        {
            while (!FlightGlobals.ActiveVessel.protoVessel.wasControllable)
                yield return 0;

            FMRS_SAVE_Util.Instance.SaveGame("FMRS_Core.save_game_pre_flight", "before_launch", HighLogic.SaveFolder + "/FMRS", SaveMode.OVERWRITE);
            FMRS_SAVE_Util.Instance.SaveGame("FMRS_Core.save_game_pre_flight", "FMRS_main_save", HighLogic.SaveFolder, SaveMode.OVERWRITE);
        }


        /*************************************************************************************************************************/
        public void toolbar_button_clicked()
        {
            Log.PushStackInfo("FMRS_Core.toolbar_button_clicked", "enter toolbar_button_clicked()");
            Log.dbg("Toolbar Button Clicked");

            if (_SETTING_Enabled)
            {
                if (!_SAVE_Has_Launched)
                    close_FMRS();
                else
                {
                    if (really_close)
                        really_close = false;
                    else
                        really_close = true;
                }
            }
            else
                toolbar_open();

            write_save_values_to_file();

            Log.PopStackInfo("leave toolbar_button_clicked()");
        }


        /*************************************************************************************************************************/
        public void toolbar_open()
        {
            bool arm_save = false;

            Log.PushStackInfo("FMRS_Core.toolbar_open", "enter toolbar_open()");
            Log.dbg("enable plugin form toolbar");

            stockTexture = "tb_st_en";
            blizzyTexture = "tb_blz_en";
            FMRS.toolbarControl.SetTexture(
                    File.Asset.Solve("icons", stockTexture),
                    File.Asset.Solve("icons", blizzyTexture)
                );
            Log.info("SetTexture 2, stockTexture: {0},   blizzyTexture {1}", stockTexture, blizzyTexture);

            _SETTING_Enabled = true;

            if (FlightGlobals.ActiveVessel.situation == Vessel.Situations.PRELAUNCH)
            {
                Log.dbg("start plugin on launchpad");
                GameEvents.onLaunch.Add(launch_routine);
                flight_scene_start_routine();
            }
            else if (FlightGlobals.ActiveVessel.Landed)
            {
                Log.dbg("start plugin not on launchpad");
                n_launchpad_preflight = true;
                flight_scene_start_routine();
            }
            else
            {
                Log.dbg("start plugin during flight");
                if (!_SETTING_Armed)
                {
                    _SETTING_Armed = true;
                    arm_save = true;
                }
                flight_preflight = true;
                flight_scene_start_routine();
                EventReport dummy_event = null;
                launch_routine(dummy_event);
                if (arm_save)
                    _SETTING_Armed = false;
                flight_preflight = false;
            }
            Log.PopStackInfo("leave toolbar_open()");
        }


        /*************************************************************************************************************************/
        public void close_FMRS()
        {
            Log.PushStackInfo("FMRS_Core.close_FMRS", "enter close_FMRS()");
            Log.dbg("close plugin");

            _SETTING_Enabled = false;
            _SAVE_Has_Launched = false;
            delete_dropped_vessels();
            really_close = false;
            _SAVE_Flight_Reset = false;

            FMRS.toolbarControl.SetTexture(
                    File.Asset.Solve("icons", stockTexture),
                    File.Asset.Solve("icons", blizzyTexture)
                );

            if (_SAVE_Has_Launched && _SAVE_Switched_To_Dropped)
                jump_to_vessel("Main");

            destroy_FMRS();

            Log.PopStackInfo("leave close_FMRS()");
        }


        /*************************************************************************************************************************/
        public void destroy_FMRS()
        {
            Log.PushStackInfo("FMRS_Core.destroy_FMRS", "enter destroy_FMRS()");

            plugin_active = false;

            if (!_SETTING_Enabled)
                _SAVE_Has_Launched = false;

            if (ThrottleLogger != null)
                ThrottleLogger.EndLog();

            if (ThrottleReplay != null)
                ThrottleReplay.EndReplay();

            write_save_values_to_file();
            write_recover_file();

            if (main_ui_active)
            {
                main_ui_active = false;
                Log.dbg("close drawMainGUI");
            }

            detach_handlers();

            n_launchpad_preflight = false;

            if (ThrottleLogger != null)
                ThrottleLogger.EndLog();
            Log.PopStackInfo("leave destroy_FMRS()");
        }
    }
}
