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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using IO = System.IO;

namespace FMRS
{

    [KSPAddon(KSPAddon.Startup.Flight, true)]
    public class FMRS_SAVE_Util : MonoBehaviour
    {
        bool saveInProgress = false;
        bool readyToLoad = false;
        Game gameToLoad = null;
        int vesselToFocus = 0;

        public bool jumpInProgress { get; private set; }

        public static FMRS_SAVE_Util Instance;

        public void Wait(float seconds, Action action)
        {
            Log.info("Wait, seconds: {0}", seconds);
            jumpInProgress = true;
            StartCoroutine(_wait(seconds, action));
        }

        IEnumerator _wait(float time, Action callback)
        {
            yield return new WaitForSeconds(time);
            jumpInProgress = false;
            callback();
        }

         private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            Instance = this;
            jumpInProgress = false;
            GameEvents.onGameStateSaved.Add(OnGameStateSaved);
            jumpInProgress = false;
            DontDestroyOnLoad(this);
        }

        private void OnDestroy()
        {
            GameEvents.onGameStateSaved.Remove(OnGameStateSaved);
        }

        public string SaveGame(string func, Game game, string saveFileName, string saveFolder, SaveMode saveMode)
        {
            saveInProgress = true;
            Log.info("Save: SaveGame 1: {0}", func);
            return GamePersistence.SaveGame(game, saveFileName, saveFolder, saveMode);
        }

        public string SaveGame(string func, string saveFileName, string saveFolder, SaveMode saveMode)
        {
            saveInProgress = true;
            Log.info("Save: SaveGame 2: {0}", func);

            return GamePersistence.SaveGame(saveFileName, saveFolder, saveMode);
        }


        public void StartAndFocusVessel(Game stateToLoad, int vesselToFocusIdx)
        {
            Log.info("Save: StartAndFocusVessel, saveInprogress: {0}", saveInProgress);
           if (saveInProgress)
         // if (false)
            {
                readyToLoad = true;
                gameToLoad = stateToLoad;
                vesselToFocus = vesselToFocusIdx;
            }
            else
            {
                gameToLoad = stateToLoad;
                vesselToFocus = vesselToFocusIdx;
               // detach_handlers();
                Wait(1, () => {
                    doStartAndFocusVessel();
                   // Debug.Log("1 second is lost forever");
                });
                //Log.info("Save: Calling FlightDriver.StartAndFocusVessel 1");
                //FlightDriver.StartAndFocusVessel(stateToLoad, vesselToFocusIdx);
            }
        }

        void doStartAndFocusVessel()
        {
            Log.info("Save: doStartAndFocusVessel, Calling FlightDriver.StartAndFocusVessel 1");
            FlightDriver.StartAndFocusVessel(gameToLoad, vesselToFocus);
            readyToLoad = false;
            gameToLoad = null;
            //attach_handlers();
        }

        void OnGameStateSaved(Game game)
        {
            Log.info("Save: OnGameStateSaved, readyToLoad: {0}", readyToLoad);
            saveInProgress = false;
            if (gameToLoad != null && readyToLoad)
            {
                if (gameToLoad != null)
                {
                    Wait(1, () => {
                        doStartAndFocusVessel();
                       // Debug.Log("1 second is lost forever");
                    });
                    Log.info("Save: Calling FlightDriver.StartAndFocusVessel 2");

                   // FlightDriver.StartAndFocusVessel(gameToLoad, vesselToFocus);
                   // readyToLoad = false;
                   // gameToLoad = null;
                }
            }
        }
    }

    public class FMRS_Util : MonoBehaviour
    {
        public struct recover_value
        {
            public string cat;
            public string key;
            public string value;
        }

        public enum save_cat : int { SETTING = 1, SAVE, SAVEFILE, SUBSAVEFILE, DROPPED, NAME, STATE, KERBAL_DROPPED, UNDEF };
        public enum vesselstate : int {NONE = 1, FLY, LANDED, DESTROYED, RECOVERED }

        public List<Guid> Vessels = new List<Guid>();
        public Dictionary<Guid, string> Vessels_dropped = new Dictionary<Guid, string>();
        public Dictionary<Guid, string> Vessels_dropped_names = new Dictionary<Guid, string>();
        public Dictionary<Guid, string> Vessel_sub_save = new Dictionary<Guid, string>();
        public Dictionary<Guid, vesselstate> Vessel_State = new Dictionary<Guid, vesselstate>();
        public Dictionary<String, Guid> Kerbal_dropped = new Dictionary<string, Guid>();
        public List<recover_value> recover_values = new List<recover_value>();

        public bool bflush_save_file = false;
        
        public Dictionary<save_cat, Dictionary<string, string>> Save_File_Content = new Dictionary<save_cat, Dictionary<string, string>>();
        public Rect windowPos;
        public Guid _SAVE_Main_Vessel;
        public string _SAVE_Switched_To_Savefile, _SAVE_SaveFolder;
        public bool _SETTING_Enabled, _SETTING_Armed, _SETTING_Minimize, _SAVE_Has_Launched, _SAVE_Flight_Reset, _SAVE_Kick_To_Main, _SAVE_Switched_To_Dropped;
        public static bool _SETTING_Messages, _SETTING_Auto_Cut_Off, _SETTING_Auto_Recover, _SETTING_Throttle_Log, _SETTING_Parachutes, _SETTING_Defer_Parachutes_to_StageRecovery,
            _SETTING_Control_Uncontrollable;
        public static bool stageRecoveryInstalled = false;

        public double _SAVE_Launched_At;

        /*************************************************************************************************************************/
        private void Start()
        {
		}

    public void set_save_value(save_cat cat, string key, string value)
        {
            Log.detail("entering set_save_value(int cat, string key, string value)");

            if (Save_File_Content[cat].ContainsKey(key))
            {
                Save_File_Content[cat][key] = value;
            }
            else
            {
                Save_File_Content[cat].Add(key, value);
            }

            Log.info("set_save_value: {0} = {1}", key, value);
            Log.detail("leaving set_save_value(int cat, string key, string value)");
        }


/*************************************************************************************************************************/
        public string get_save_value(save_cat cat, string key)
        {
            Log.dbg("entering get_save_value(int cat, string key) #### FMRS: NO LEAVE MESSAGE");

            if (Save_File_Content[cat].ContainsKey(key))
                return (Save_File_Content[cat][key]);
            else
                return (false.ToString());
        }


/*************************************************************************************************************************/
        public void write_save_values_to_file()
        {
            Log.PushStackInfo("FMRS_Util.write_save_values_to_file", "entering write_save_values_to_file()");

            set_save_value(save_cat.SETTING, "Window_X", Convert.ToInt32(windowPos.x).ToString());
            set_save_value(save_cat.SETTING, "Window_Y", Convert.ToInt32(windowPos.y).ToString());
            set_save_value(save_cat.SETTING, "Armed", _SETTING_Armed.ToString());
            set_save_value(save_cat.SETTING, "Minimized", _SETTING_Minimize.ToString());
            set_save_value(save_cat.SETTING, "Enabled", _SETTING_Enabled.ToString());
            //set_save_value(save_cat.SETTING, "Messages", _SETTING_Messages.ToString());
            //set_save_value(save_cat.SETTING, "Auto_Cut_Off", _SETTING_Auto_Cut_Off.ToString());
            //set_save_value(save_cat.SETTING, "Auto_Recover", _SETTING_Auto_Recover.ToString());
            //set_save_value(save_cat.SETTING, "Throttle_Log", _SETTING_Throttle_Log.ToString());
            set_save_value(save_cat.SAVE, "Main_Vessel", _SAVE_Main_Vessel.ToString());
            set_save_value(save_cat.SAVE, "Has_Launched", _SAVE_Has_Launched.ToString());
            set_save_value(save_cat.SAVE, "Launched_At", _SAVE_Launched_At.ToString());
            set_save_value(save_cat.SAVE, "Flight_Reset", _SAVE_Flight_Reset.ToString());
            set_save_value(save_cat.SAVE, "Kick_To_Main", _SAVE_Kick_To_Main.ToString());
            set_save_value(save_cat.SAVE, "Switched_To_Dropped", _SAVE_Switched_To_Dropped.ToString());
            set_save_value(save_cat.SAVE, "Switched_To_Savefile", _SAVE_Switched_To_Savefile);
            set_save_value(save_cat.SAVE, "SaveFolder", _SAVE_SaveFolder);

            write_vessel_dict_to_Save_File_Content();
            
            IO.TextWriter file = IO.File.CreateText(FILES.SAVE_TXT);
            file.Flush();
            file.Close();
            file = IO.File.CreateText(FILES.SAVE_TXT);
            foreach (KeyValuePair<save_cat, Dictionary<string, string>> save_cat_block in Save_File_Content)
            {
                foreach (KeyValuePair<string, string> writevalue in save_cat_block.Value)
                {
                   file.WriteLine(save_cat_toString(save_cat_block.Key) + "=" + writevalue.Key + "=" + writevalue.Value);
                }
            }
            file.Close();

            Log.dbg("Save File written in private void write_save_values_to_file()");
            Log.PopStackInfo("leaving save_values_to_file()");
        }


/*************************************************************************************************************************/
        public void write_vessel_dict_to_Save_File_Content()
        {
            List<string> delete_values = new List<string>();

            Log.PushStackInfo("FMRS_Util.write_vessel_dict_to_Save_File_Content", "entering write_vessel_dict_to_Save_File_Content()");

            Save_File_Content[save_cat.DROPPED].Clear();
            Save_File_Content[save_cat.NAME].Clear();
            Save_File_Content[save_cat.STATE].Clear();
            
            foreach (KeyValuePair<Guid, string> write_keyvalue in Vessels_dropped)
            {
                set_save_value(save_cat.DROPPED, write_keyvalue.Key.ToString(), write_keyvalue.Value);
                Log.dbg("write {0} to Save_File_Content DROPPED", write_keyvalue.Key);
            }

            foreach (KeyValuePair<Guid, string> write_keyvalue in Vessels_dropped_names)
            {
                set_save_value(save_cat.NAME, write_keyvalue.Key.ToString(), write_keyvalue.Value);
                Log.dbg("write NAME {0} to Save_File_Content NAME", write_keyvalue.Key);
            }

            foreach (KeyValuePair<Guid,vesselstate> st  in Vessel_State)
            {
                set_save_value(save_cat.STATE, st.Key.ToString(), st.Value.ToString());
                Log.dbg("write {0}: {1} to Save_File_Content STATE", st.Key, st.Value);
            }

            foreach (KeyValuePair<Guid, string> st in Vessel_sub_save)
            {
                set_save_value(save_cat.SUBSAVEFILE, st.Key.ToString(), st.Value.ToString());
                Log.dbg("write {0}: {1} to Save_File_Content SUBSAVEFILE", st.Key, st.Value);
            }

            foreach (KeyValuePair<string, Guid> write_keyvalue in Kerbal_dropped)
            {
                set_save_value(save_cat.KERBAL_DROPPED, write_keyvalue.Key.ToString(), write_keyvalue.Value.ToString());
                Log.dbg("write KERBAL_DROPPED {0} to Save_File_Content KERBAL_DROPPED", write_keyvalue.Key);
            }

            Log.PopStackInfo("leaving write_vessel_dict_to_Save_File_Content()");

        }


/*************************************************************************************************************************/
        public string get_time_string(double dtime)
        {
            Int16 s, min, ds;
            Int32 time;
            string return_string;

            dtime *= 100;

            time = Convert.ToInt32(dtime);

            s = 0;
            ds = 0;
            min = 0;

            if (time >= 6000)
            {
                while (time >= 6000)
                {
                    min++;
                    time -= 6000;
                }
            }

            if (time >= 100)
            {
                while (time >= 100)
                {
                    s++;
                    time -= 100;
                }
            }

            if (time >= 10)
            {
                while (time >= 10)
                {
                    ds++;
                    time -= 10;
                }
            }

            if (min < 10)
                return_string = "0" + min.ToString();
            else
                return_string = min.ToString();

            return_string += ":";

            if (s < 10)
                return_string += "0" + s.ToString();
            else
                return_string += s.ToString();

            return_string += "." + ds.ToString();

            return (return_string);
        }


/*************************************************************************************************************************/
        public void flush_save_file()
        {
            int anz_lines;

            Log.PushStackInfo("FMRS_Util.flush_save_file", "enter flush_save_file()");
            Log.dbg("flush save file");

            string[] lines = IO.File.ReadAllLines(FILES.SAVE_TXT);
            anz_lines = lines.Length;

            Log.dbg("delete {0} lines", anz_lines);

            IO.TextWriter file = IO.File.CreateText(FILES.SAVE_TXT);
            while (anz_lines != 0)
            {
                file.WriteLine("");
                anz_lines--;
            }
            file.Close();

            foreach (KeyValuePair<save_cat, Dictionary<string, string>> content in Save_File_Content)
                Save_File_Content[content.Key].Clear();

            bflush_save_file = false;
            init_save_file();
            read_save_file();
#if DEBUG
           // if (Debug_Level_1_Active)
                Log.PopStackInfo("leave flush_save_file()");
#endif
        }


/*************************************************************************************************************************/
        public void read_save_file()
        {
            double temp_double;
            save_cat temp_cat;

            Log.PushStackInfo("FMRS_Util.read_save_file", "enter read_save_file()");
			Log.dbg("read save file");

            foreach(KeyValuePair<save_cat,Dictionary<string,string>> content in Save_File_Content)
                Save_File_Content[content.Key].Clear();

			string[] lines = IO.File.ReadAllLines(FILES.SAVE_TXT);

            foreach (string value_string in lines)
            {
                if (value_string != "")
                {
                    string[] line = value_string.Split('=');
                    temp_cat = save_cat_parse(line[0]);

                    try
                    {
                        Save_File_Content[temp_cat].Add(line[1].Trim(), line[2].Trim());
                    }
                    catch (Exception)
                    {
                        Log.info("inconsistent save file, flush save file");
                        bflush_save_file = true;
                        break;
                    }
                }
            }
            if (bflush_save_file)
                return;

            try
            {
                temp_double = Convert.ToDouble(get_save_value(save_cat.SETTING, "Window_X"));
                windowPos.x = Convert.ToInt32(temp_double);
                temp_double = Convert.ToDouble(get_save_value(save_cat.SETTING, "Window_Y"));
                windowPos.y = Convert.ToInt32(temp_double);
                Debug.Log("windowPos loaded from file");
            }
            catch (Exception)
            {
#if false
                // DEBUG
                Debug_Active = true;
                Debug_Level_1_Active = true;
#endif
                Log.info("invalid save file, flush save file");
                bflush_save_file = true;
            }
#if DEBUG
            foreach (KeyValuePair<save_cat, Dictionary<string, string>> temp_keyvalue in Save_File_Content)
                foreach (KeyValuePair<string, string> readvalue in temp_keyvalue.Value)
                    Log.info("{0} = {1} = {2}" + temp_keyvalue.Key, readvalue.Key, readvalue.Value);
#endif

            if (bflush_save_file)
                return;
#if false
            // DEBUG
            if (get_save_value(save_cat.SETTING,"Debug_Level") == "1" && Debug_Active)
                Debug_Level_1_Active = true;

            if (get_save_value(save_cat.SETTING,"Debug_Level") == "2" && Debug_Active)
            {
                Debug_Level_1_Active = true;
                Debug_Level_2_Active = true;
            }
#endif
            try
            {
                _SETTING_Armed = Convert.ToBoolean(get_save_value(save_cat.SETTING, "Armed"));
                _SETTING_Minimize = Convert.ToBoolean(get_save_value(save_cat.SETTING, "Minimized"));
                _SETTING_Enabled = Convert.ToBoolean(get_save_value(save_cat.SETTING, "Enabled"));
                //_SETTING_Messages = Convert.ToBoolean(get_save_value(save_cat.SETTING, "Messages"));
                //_SETTING_Auto_Cut_Off = Convert.ToBoolean(get_save_value(save_cat.SETTING, "Auto_Cut_Off"));
                //_SETTING_Auto_Recover = Convert.ToBoolean(get_save_value(save_cat.SETTING, "Auto_Recover"));
                //_SETTING_Throttle_Log = Convert.ToBoolean(get_save_value(save_cat.SETTING, "Throttle_Log"));
                _SAVE_Main_Vessel = new Guid(get_save_value(save_cat.SAVE, "Main_Vessel"));
                _SAVE_Has_Launched = Convert.ToBoolean(get_save_value(save_cat.SAVE, "Has_Launched"));
                _SAVE_Launched_At = Convert.ToDouble(get_save_value(save_cat.SAVE, "Launched_At"));
                _SAVE_Flight_Reset = Convert.ToBoolean(get_save_value(save_cat.SAVE, "Flight_Reset"));
                _SAVE_Kick_To_Main = Convert.ToBoolean(get_save_value(save_cat.SAVE, "Kick_To_Main"));
                _SAVE_Switched_To_Dropped = Convert.ToBoolean(get_save_value(save_cat.SAVE, "Switched_To_Dropped"));
                _SAVE_Switched_To_Savefile = get_save_value(save_cat.SAVE, "Switched_To_Savefile");
                _SAVE_SaveFolder = get_save_value(save_cat.SAVE, "SaveFolder");
            }
            catch (Exception)
            {
                Log.info("invalid save file, flush save file");
                bflush_save_file = true;
            }
            Log.PopStackInfo("leave read_save_file()");
        }


/*************************************************************************************************************************/
        public void init_save_file()
        {
            Log.PushStackInfo("FMRS_Util.init_save_file", "enter init_save_file()");
            Log.dbg("init save file");

            foreach (KeyValuePair<save_cat, Dictionary<string, string>> content in Save_File_Content)
                Save_File_Content[content.Key].Clear();

            set_save_value(save_cat.SETTING, "Window_X", Convert.ToInt32(windowPos.x).ToString());
            set_save_value(save_cat.SETTING, "Window_Y", Convert.ToInt32(windowPos.y).ToString());
            set_save_value(save_cat.SETTING, "Enabled", true.ToString());
            set_save_value(save_cat.SETTING, "Armed", true.ToString());
            set_save_value(save_cat.SETTING, "Minimized", false.ToString());
            set_save_value(save_cat.SETTING, "Messages", true.ToString());
            set_save_value(save_cat.SETTING, "Auto_Cut_Off", false.ToString());
            set_save_value(save_cat.SETTING, "Auto_Recover", false.ToString());
            set_save_value(save_cat.SETTING, "Throttle_Log", false.ToString());
            set_save_value(save_cat.SETTING, "Debug", false.ToString());
            set_save_value(save_cat.SETTING, "Debug_Level", "0");
            set_save_value(save_cat.SAVE, "Main_Vessel", new Guid().ToString());          
            set_save_value(save_cat.SAVE, "Has_Launched", false.ToString());
            set_save_value(save_cat.SAVE, "Launched_At", "null");
            set_save_value(save_cat.SAVE, "Flight_Reset", false.ToString());
            set_save_value(save_cat.SAVE, "Switched_To_Dropped", false.ToString());
            set_save_value(save_cat.SAVE, "Kick_To_Main", false.ToString());
            set_save_value(save_cat.SAVE, "Switched_To_Savefile", "");
            set_save_value(save_cat.SAVE, "SaveFolder", HighLogic.SaveFolder);

            IO.TextWriter file = IO.File.CreateText(FILES.SAVE_TXT);

            foreach (KeyValuePair<save_cat, Dictionary<string, string>> writecat in Save_File_Content)
                foreach (KeyValuePair<string, string> writevalue in writecat.Value)
                    file.WriteLine(writecat.Key.ToString() + "=" + writevalue.Key + "=" + writevalue.Value);

            file.Close();
#if DEBUG
           // if (Debug_Level_1_Active)
                Log.PopStackInfo("leave init_save_file()");
#endif
        }


/*************************************************************************************************************************/
        public void load_save_file()
        {
			if (!IO.File.Exists(FILES.SAVE_TXT))
                init_save_file();
            read_save_file();

#if false
            if (!bflush_save_file)
                if (get_save_value(save_cat.SETTING, "Version") != mod_version)
                {
                    Log.info("diferent version, flush save file");
                    bflush_save_file = true;
                }
#endif
            if (bflush_save_file)
                flush_save_file();

            if (!IO.File.Exists(FILES.RECOVER_TXT))
                init_recover_file();
            read_recover_file();

            get_dropped_vessels();
        }


/*************************************************************************************************************************/
        public void get_dropped_vessels()
        {
#if DEBUG
           // if (Debug_Level_1_Active)
                Log.PushStackInfo("FMRS_Util.get_dropped_vessels", "entering get_dropped_vessels()");
#endif

            foreach (KeyValuePair<save_cat, Dictionary<string, string>> savecat in Save_File_Content)
            {
                if (savecat.Key == save_cat.DROPPED)
                    foreach(KeyValuePair<string,string> save_value in savecat.Value)
                    {
                        Vessels_dropped.Add(new Guid(save_value.Key), save_value.Value);
                        Log.dbg(" #### FMRS: {0} set to {1} in Vessels_dropped", save_value.Key, save_value.Value);
                    }

                if (savecat.Key == save_cat.NAME)
                    foreach (KeyValuePair<string, string> save_value in savecat.Value)
                    {
                        Vessels_dropped_names.Add(new Guid(save_value.Key), save_value.Value);
                        Log.dbg(" #### FMRS: {0} set to {1} in Vessels_dropped_names", save_value.Key, save_value.Value);
                    }

                if (savecat.Key == save_cat.STATE)
                    foreach (KeyValuePair<string, string> save_value in savecat.Value)
                    {
                        Vessel_State.Add(new Guid(save_value.Key), parse_vesselstate(save_value.Value));
                        Log.dbg(" #### FMRS: {0} set to {1} in Vessels_dropped_landed", save_value.Key, save_value.Value);
                    }

                if (savecat.Key == save_cat.SUBSAVEFILE)
                    foreach (KeyValuePair<string, string> save_value in savecat.Value)
                    {
                        Vessel_sub_save.Add(new Guid(save_value.Key), save_value.Value);
                        Log.dbg(" #### FMRS: {0} set to {1} in Vessel_sub_save", save_value.Key, save_value.Value);
                    }

                if (savecat.Key == save_cat.KERBAL_DROPPED)
                    foreach (KeyValuePair<string, string> save_value in savecat.Value)
                    {
                        Kerbal_dropped.Add(save_value.Key, new Guid(save_value.Value));
                        Log.dbg(" #### FMRS: {0} set to {1} in Kerbal_dropped", save_value.Key, save_value.Value);
                    }
            }

            Log.PopStackInfo("leaving get_dropped_vessels()");
        }


/*************************************************************************************************************************/
        public void init_recover_file()
        {
            Log.PushStackInfo("FMRS_Util.init_recover_file", "enter init_recover_file()");
            Log.dbg("init recover file");

            IO.TextWriter file = IO.File.CreateText(FILES.RECOVER_TXT);
            file.Close();

            Log.PopStackInfo("leave init_recover_file()");
        }


/*************************************************************************************************************************/
        public void flush_recover_file()
        {
            int anz_lines;

            Log.PushStackInfo("fMRS_Util.flush_recover_file", "enter flush_recover_file()");
            Log.dbg("flush recover file");

            string[] lines = IO.File.ReadAllLines(FILES.RECOVER_TXT);
            anz_lines = lines.Length;

            IO.TextWriter file = IO.File.CreateText(FILES.RECOVER_TXT);
            while (anz_lines != 0)
            {
                file.WriteLine("");
                anz_lines--;
            }
            file.Close();

            Log.PopStackInfo("leave flush_recover_file()");
        }


/*************************************************************************************************************************/
        public void read_recover_file()
        {
            recover_value temp_value;

            Log.PushStackInfo("FMRS_Util.read_recover_file", "enter read_recover_file()");
            Log.dbg("read recover file");

            recover_values.Clear();
            string[] lines = IO.File.ReadAllLines(FILES.RECOVER_TXT);

            foreach (string value_string in lines)
            {
                if (value_string != "")
                {
                    string[] line = value_string.Split('=');
                    temp_value.cat = line[0].Trim();
                    temp_value.key = line[1].Trim();
                    temp_value.value = line[2].Trim();
                    recover_values.Add(temp_value);
                }
            }
#if DEBUG
            foreach (recover_value temp in recover_values)
                Log.info("recover value: {0} = {1} = {2}", temp.cat, temp.key, temp.value);

            Log.PopStackInfo("leave read_recover_file()");
#endif
        }


/*************************************************************************************************************************/
        public void write_recover_file()
        {
            Log.PushStackInfo("FMRS_Util.write_recover_file", "enter write_recover_file()");

            flush_recover_file();

            Log.dbg("write recover file");

            IO.TextWriter file = IO.File.CreateText(FILES.RECOVER_TXT);

            foreach (recover_value writevalue in recover_values)
            {
                file.WriteLine(writevalue.cat + "=" + writevalue.key + "=" + writevalue.value);
            }
            file.Close();

            Log.PopStackInfo("leave write_recover_file()");
        }


/*************************************************************************************************************************/
        public void set_recoverd_value(string cat, string key, string value)
        {
            recover_value temp_value;

            Log.dbg("enter set_recoverd_value(string key, string value)");
            Log.dbg("add to recover file: {0} = {1} = {2}", cat, key, value);

            temp_value.cat = cat;
            temp_value.key = key;
            temp_value.value = value;
            recover_values.Add(temp_value);

            Log.dbg("set_recoverd_value(string key, string value)");
        }


/*************************************************************************************************************************/
        public save_cat save_cat_parse(string in_string)
        {
            Log.dbg("enter save_cat_parse(string in_string) {0} NO LEAVE MESSAGE", in_string);

            switch (in_string)
            {
                case "SETTING":
                    return save_cat.SETTING;
                case "SAVE":
                    return save_cat.SAVE;
                case "SAVEFILE":
                    return save_cat.SAVEFILE;
                case "SUBSAVEFILE":
                    return save_cat.SUBSAVEFILE;
                case "VESSEL_DROPPED":
                    return save_cat.DROPPED;
                case "VESSEL_NAME":
                    return save_cat.NAME;
                case "VESSEL_STATE":
                    return save_cat.STATE;
                case "KERBAL_DROPPED":
                    return save_cat.KERBAL_DROPPED;

                default:
                    return save_cat.UNDEF;
            }
        }


/*************************************************************************************************************************/
        public string save_cat_toString(save_cat cat)
        {
            Log.dbg("enter string save_cat_toString(save_cat cat) {0} NO LEAVE MESSAGE", cat);

            switch (cat)
            {
                case save_cat.SETTING:
                    return "SETTING";
                case save_cat.SAVE:
                    return "SAVE";
                case save_cat.SAVEFILE:
                    return "SAVEFILE";
                case save_cat.SUBSAVEFILE:
                    return "SUBSAVEFILE";
                case save_cat.DROPPED:
                    return "VESSEL_DROPPED";
                case save_cat.NAME:
                    return "VESSEL_NAME";
                case save_cat.STATE:
                    return "VESSEL_STATE";
                case save_cat.KERBAL_DROPPED:
                    return "KERBAL_DROPPED";

                default:
                    return "UNDEF";
            }
        }

        
/*************************************************************************************************************************/
        public void delete_dropped_vessels()
        {
            List<string> temp_list = new List<string>();

            Log.PushStackInfo("FMRS_Util.delete_dropped_vessels", "entering delete_dropped_vessels()");

            Vessels_dropped.Clear();
            Vessels_dropped_names.Clear();
            Vessel_State.Clear();
            Vessel_sub_save.Clear();
            Kerbal_dropped.Clear();
            Save_File_Content[save_cat.SAVEFILE].Clear();

            write_save_values_to_file();

            Log.PopStackInfo("leaving delete_dropped_vessels()");
        }


/*************************************************************************************************************************/
        public void delete_dropped_vessel(Guid vessel_guid)
        {
            List<string> temp_list = new List<string>();
            string temp_string = null;

            Log.PushStackInfo("FMRS_Util.delete_dropped_vessel", "entering delete_dropped_vessel(Guid vessel_guid) {0}", vessel_guid);
            Log.dbg("remove vessel {0}", vessel_guid);

            if (Vessels_dropped.ContainsKey(vessel_guid))
                Vessels_dropped.Remove(vessel_guid);
            if (Vessels_dropped_names.ContainsKey(vessel_guid))
                Vessels_dropped_names.Remove(vessel_guid);
            if (Vessel_State.ContainsKey(vessel_guid))
                Vessel_State.Remove(vessel_guid);
            if (Vessel_sub_save.ContainsKey(vessel_guid))
                Vessel_sub_save.Remove(vessel_guid);

            foreach (KeyValuePair<string, Guid> Kerbal in Kerbal_dropped)
                if (Kerbal.Value == vessel_guid)
                    temp_string = Kerbal.Key;
            if(temp_string!=null)
                Kerbal_dropped.Remove(temp_string);
 

            write_save_values_to_file();

            Log.PopStackInfo("leaving delete_dropped_vessel(Guid vessel_guid)");
        }


/*************************************************************************************************************************/
        public void init_Save_File_Content()
        {
#if DEBUG
           // if (Debug_Level_1_Active)
                Log.PushStackInfo("FMRS_Util.init_Save_File_Content", "entering init_Save_File_Content()");
#endif
            if (!Save_File_Content.ContainsKey(save_cat.SETTING))
                Save_File_Content.Add(save_cat.SETTING, new Dictionary<string, string>());

            if (!Save_File_Content.ContainsKey(save_cat.SAVE))
                Save_File_Content.Add(save_cat.SAVE, new Dictionary<string, string>());

            if (!Save_File_Content.ContainsKey(save_cat.SAVEFILE))
                Save_File_Content.Add(save_cat.SAVEFILE, new Dictionary<string, string>());

            if (!Save_File_Content.ContainsKey(save_cat.SUBSAVEFILE))
                Save_File_Content.Add(save_cat.SUBSAVEFILE, new Dictionary<string, string>());

            if (!Save_File_Content.ContainsKey(save_cat.DROPPED))
                Save_File_Content.Add(save_cat.DROPPED, new Dictionary<string, string>());

            if (!Save_File_Content.ContainsKey(save_cat.NAME))
                Save_File_Content.Add(save_cat.NAME, new Dictionary<string, string>());

            if (!Save_File_Content.ContainsKey(save_cat.STATE))
                Save_File_Content.Add(save_cat.STATE, new Dictionary<string, string>());

            if (!Save_File_Content.ContainsKey(save_cat.KERBAL_DROPPED))
                Save_File_Content.Add(save_cat.KERBAL_DROPPED, new Dictionary<string, string>());

            Log.PopStackInfo("leaving init_Save_File_Content()");
        }

      
        /*************************************************************************************************************************/
        public string vesselstate_toString(vesselstate vs)
        {
            Log.dbg("entering vesselstate_toString(vesselstate vs) {0} NO LEAVE ENTRY", vs);

            switch (vs)
            {
                case vesselstate.DESTROYED:
                    return "DESTROYED";
                case vesselstate.FLY:
                    return "FLY";
                case vesselstate.LANDED:
                    return "LANDED";
                case vesselstate.RECOVERED:
                    return "RECOVERED";
                default:
                    return "NONE";
            }
        }


/*************************************************************************************************************************/
        public vesselstate parse_vesselstate (string str)
        {
            Log.dbg("entering  parse_vesselstate (string str) {0} NO LEAVE ENTRY", str);

            switch (str)
            {
                case "DESTROYED":
                    return vesselstate.DESTROYED;
                case "FLY":
                    return vesselstate.FLY;
                case "LANDED":
                    return vesselstate.LANDED;
                case "RECOVERED":
                    return vesselstate.RECOVERED;
                default:
                    return vesselstate.NONE;
            }
        }


/*************************************************************************************************************************/
        public vesselstate get_vessel_state(Guid id)
        {
            Log.dbg("entering  vesselstate get_vessel_state(Guid id) {0} NO LEAVE ENTRY", id);
            if (Vessel_State.ContainsKey(id))
                return Vessel_State[id];
            else
            {
                Log.dbg("{0} NOT IN Vessel_State", id);
                return vesselstate.NONE;
            }
        }


/*************************************************************************************************************************/
        public bool set_vessel_state(Guid id, vesselstate state)
        {
            Log.dbg("entering  bool set_vessel_state(Guid id, vesselstate state) {0} {1} NO LEAVE ENTRY", id, state);

            if (Vessel_State.ContainsKey(id))
            {
                Vessel_State[id] = state;
                return true;
            }
            else
            {
                Log.dbg("{0} NOT IN Vessel_State", id);
                return false;
            }
        }
    }
}
