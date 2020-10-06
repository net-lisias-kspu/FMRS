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
using System.Collections.Generic;

using Contracts;


namespace FMRS
{
    public partial class FMRS_Core : FMRS_Util, IFMRS
    {
        /*************************************************************************************************************************/
        public void save_landed_vessel(bool auto_recover_allowed, bool ForceRecover)
        {
            Game loadgame, savegame;
            Guid temp_guid;
            int ReferenceBodyIndex = 0;
            List<Guid> id_list = new List<Guid>();
            List<ProtoVessel> vessel_list = new List<ProtoVessel>();
            Dictionary<Guid, List<ProtoVessel>> vessel_dict = new Dictionary<Guid, List<ProtoVessel>>();
            string message;

            Log.PushStackInfo("FMRS_Core.save_landed_vessel", "entering save_landed_vessel(bool auto_recover_allowed) " + auto_recover_allowed.ToString());
            Log.dbg("save landed vessels");

            if (SwitchedToDropped == false)
            {
                Log.dbg("in Main Save, leaving save_landed_vessel");
                return;
            }

            //GamePersistence.SaveGame("FMRS_quicksave", HighLogic.SaveFolder + "/FMRS", SaveMode.OVERWRITE);
            FMRS_SAVE_Util.Instance.SaveGame("FMRS_Core.save_landed_vessel", "FMRS_quicksave", HighLogic.SaveFolder + "/FMRS", SaveMode.OVERWRITE);
            loadgame = GamePersistence.LoadGame("FMRS_quicksave", HighLogic.SaveFolder + "/FMRS", false, false);
            savegame = GamePersistence.LoadGame("FMRS_main_save", HighLogic.SaveFolder, false, false);

            foreach (Guid id in loaded_vessels)
            {
                if (FlightGlobals.Vessels.Find(v => v.id == id) == null)
                    if (!damaged_vessels.Contains(id))
                        damaged_vessels.Add(id);
            }

            foreach (Vessel v in FlightGlobals.Vessels.FindAll(v => v.loaded))
                id_list.Add(v.id);

            vessel_list = loadgame.flightState.protoVessels.FindAll(tpv => id_list.Contains(tpv.vesselID));

            foreach (Guid id in loaded_vessels)
                vessel_dict.Add(id, new List<ProtoVessel>());

            foreach (ProtoVessel pv in vessel_list)
            {
                if (loaded_vessels.Contains(pv.vesselID))
                    vessel_dict[pv.vesselID].Add(pv);
                else
                {
                    foreach (ProtoPartSnapshot ps in pv.protoPartSnapshots)
                    {
                        foreach (ProtoPartModuleSnapshot ppms in ps.modules)
                        {
                            if (ppms.moduleName == "FMRS_PM")
                            {
                                try
                                {
                                    temp_guid = new Guid(ppms.moduleValues.GetValue("parent_vessel"));
                                    if (loaded_vessels.Contains(temp_guid))
                                        vessel_dict[temp_guid].Add(pv);
                                }
                                catch (Exception)
                                {
                                    Log.info("Exception: save_landed_vessel: temp_guid = new Guid(ppms.moduleValues.GetValue(parent_vessel));");
                                }

                            }
                            break;
                        }
                    }
                }
                ReferenceBodyIndex = pv.orbitSnapShot.ReferenceBodyIndex;
            }

            id_list.Clear();
            foreach (KeyValuePair<Guid, List<ProtoVessel>> kvp in vessel_dict)
                id_list.Add(kvp.Key);
            foreach (Guid id in id_list)
                if (vessel_dict[id].Count == 0)
                    vessel_dict.Remove(id);
            id_list.Clear();
            foreach (KeyValuePair<Guid, List<ProtoVessel>> kvp in vessel_dict)
                id_list.Add(kvp.Key);

            foreach (Guid id in id_list)
            {
                if (id_list.Count == 1 && vessel_dict[id].Count == 0 && get_vessel_state(id) != vesselstate.RECOVERED)
                    set_vessel_state(id, vesselstate.DESTROYED);

                if (get_vessel_state(id) == vesselstate.RECOVERED)
                    vessel_dict.Remove(id);
            }

            if (vessel_dict.Count != 0)
            {
                Log.dbg("save landed vessel or recover");

                foreach (KeyValuePair<Guid, List<ProtoVessel>> kvp in vessel_dict)
                {
                    if ((_SETTING_Auto_Recover || ForceRecover) && auto_recover_allowed && ReferenceBodyIndex == 1)
                        savegame = recover_vessel(kvp.Key, kvp.Value, loadgame, savegame);
                    else
                    {
                        ProtoVessel temp_proto_del2 = savegame.flightState.protoVessels.Find(prtv => prtv.vesselID == kvp.Key);
                        if (temp_proto_del2 != null)
                            savegame.flightState.protoVessels.Remove(temp_proto_del2);

                        foreach (ProtoVessel pv in kvp.Value)
                        {
                            ProtoVessel temp_proto_del = savegame.flightState.protoVessels.Find(prtv => prtv.vesselID == pv.vesselID);
                            if (temp_proto_del != null)
                                savegame.flightState.protoVessels.Remove(temp_proto_del);

                            if (pv.landed || pv.splashed)
                            {
                                savegame.flightState.protoVessels.Add(pv);
                                set_vessel_state(kvp.Key, vesselstate.LANDED);
                            }
                        }
                    }
                }
            }

            if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
            {
                message = "";
                if (contract_complete.Count > 0)
                {
                    foreach (KeyValuePair<Guid, List<Contract>> kvp in contract_complete)
                    {
                        if (loaded_vessels.Contains(kvp.Key))
                        {
                            foreach (Contract c in kvp.Value)
                            {
                                set_recoverd_value("contract", "complete", c.ContractID.ToString());
                                message += c.Title + "@";
                            }
                        }
                    }
                    if (message != "")
                        set_recoverd_value("message", "FMRS: Completed Contracts", message);
                }
                foreach (KeyValuePair<Guid, List<ProtoVessel>> kvp in vessel_dict)
                {
                    if (contract_complete.ContainsKey(kvp.Key))
                        contract_complete.Remove(kvp.Key);
                }
            }

            foreach (killed_kerbal_str killed in killed_kerbals)
            {
                Log.dbg("Kerbal {0} killed on that flight?", killed.name);

                message = "";

                if (loaded_vessels.Contains(killed.vessel_id))
                {
                    Log.dbg("Kerbal {0} killed", killed.name);

                    foreach (ProtoCrewMember member in savegame.CrewRoster.Crew)
                    {
                        if (member.name == killed.name)
                        {
                            member.rosterStatus = ProtoCrewMember.RosterStatus.Dead;
                            set_recoverd_value("kerbal", "kill", killed.rep.ToString());
                            message += killed.name + " killed: " + Math.Round(killed.rep, 2).ToString() + " Reputation@";
                        }
                    }
                }
                if (message != "")
                    set_recoverd_value("message", "FMRS: Kerbal Killed", message);
            }

            if (damaged_buildings.Count > 0)
            {
                message = "";
                foreach (string build in damaged_buildings)
                {
                    set_recoverd_value("building", "destroyed", build);
                    message += build + "@";
                }
                if (message != "")
                    set_recoverd_value("message", "FMRS: Building Destroyed", message);
            }

            //GamePersistence.SaveGame(savegame, "FMRS_main_save", HighLogic.SaveFolder, SaveMode.OVERWRITE);
            FMRS_SAVE_Util.Instance.SaveGame("FMRS_Core.save_landed_vessel", savegame, "FMRS_main_save", HighLogic.SaveFolder, SaveMode.OVERWRITE);

            write_recover_file();
            write_save_values_to_file();

            Log.PopStackInfo("leaving save_landed_vessel(bool auto_recover_allowed)");
        }


        /*************************************************************************************************************************/
        public void fill_Vessels_list()
        {
           Log.PushStackInfo("FMRS_Core.fill_Vessels_list", "entering fill_Vessels_list()");

            foreach (Vessel temp_vessel in FlightGlobals.Vessels)
            {
                if (!Vessels.Contains(temp_vessel.id))
                {
                    Vessels.Add(temp_vessel.id);
                    Log.dbg("{0} Found", temp_vessel.vesselName);
                }
            }

            Log.PopStackInfo("leaving fill_Vessels_list()");
        }


        /*************************************************************************************************************************/
        public bool search_for_new_vessels(string save_file_name)
        {
            bool new_vessel_found = false, controllable = false;

            Log.PushStackInfo("FMRS_Core.search_for_new_vessels(string)", "entering search_for_new_vessels(string save_file_name) {0}", save_file_name);

            foreach (Vessel temp_vessel in FlightGlobals.Vessels)
            {
                controllable = false;

                //Check if the stage was claimed by another mod or by this mod
                string controllingMod = RecoveryControllerWrapper.ControllingMod(temp_vessel);                
                   
                bool FMRSIsControllingMod = false;
                if (controllingMod != null)
                {
                    Log.info("RecoveryControllerWrapper.ControllingMod for vessel: {0} : {1}", temp_vessel.name, controllingMod);

                    FMRSIsControllingMod = string.Equals(controllingMod, "FMRS", StringComparison.OrdinalIgnoreCase);
                }

                if (controllingMod == null || 
                    string.Equals(controllingMod, "auto", StringComparison.OrdinalIgnoreCase) ||
                   FMRSIsControllingMod)
                {
                    if (!Vessels.Contains(temp_vessel.id))
                    { 
                        if (FMRSIsControllingMod ||
                             (
                                ((temp_vessel.isCommandable && temp_vessel.IsControllable) || (_SETTING_Control_Uncontrollable && controllingMod == null)) &&
                                temp_vessel.vesselType != VesselType.EVA &&
                                temp_vessel.vesselType != VesselType.Flag &&
                                temp_vessel.vesselType != VesselType.SpaceObject &&
                                temp_vessel.vesselType != VesselType.Unknown
                              )
                           )

                            controllable = true;
                        else
                        {
                            foreach (ProtoPartSnapshot proto_part in temp_vessel.protoVessel.protoPartSnapshots)
                            {
                                List<ProtoPartModuleSnapshot> proto_modules = proto_part.modules;
                                ProtoPartModuleSnapshot module = null;

                                if (proto_modules != null && 
                                    (_SETTING_Parachutes && 
                                        ( (controllingMod != null && string.Equals(controllingMod, "FMRS", StringComparison.OrdinalIgnoreCase)) || 
                                        !_SETTING_Defer_Parachutes_to_StageRecovery || 
                                        !stageRecoveryInstalled)
                                        )
                                    )
                                {
                                    //
                                    module = proto_part.modules.Find(p => p.moduleName == "RealChuteModule" ||
                                                                            p.moduleName == "ModuleParachute" ||
                                                                            p.moduleName == "ModuleKrKerbalParachute" ||
                                                                            p.moduleName == "RealChuteFAR");
                                    if (module != null)
                                        controllable = true;
                                }

                                if (proto_part.protoCrewNames.Count > 0)
                                    controllable = true;
                            }
                        }
                        foreach (Part p in temp_vessel.Parts)
                        {
                            foreach (PartModule pm in p.Modules)
                            {
                                if (pm.moduleName == "FMRS_PM")
                                {
                                    if ((pm as FMRS_PM).parent_vessel != "00000000-0000-0000-0000-000000000000")
                                    {
                                        controllable = false;
                                        break;
                                    }
                                }
                            }
                            break;
                        }

                        if (controllable)
                        {
                            Log.dbg("{0} Found and will be added to the dicts", temp_vessel.vesselName);

                            Vessels_dropped.Add(temp_vessel.id, save_file_name);
                            Vessels_dropped_names.Add(temp_vessel.id, temp_vessel.vesselName);
                            Vessel_State.Add(temp_vessel.id, vesselstate.FLY);
                            foreach (Part p in temp_vessel.Parts)
                            {
                                foreach (PartModule pm in p.Modules)
                                    if (pm.moduleName == "FMRS_PM")
                                        pm.StartCoroutine("setid");
                            }

                            foreach (ProtoPartSnapshot part_snapshot in temp_vessel.protoVessel.protoPartSnapshots)
                            {
                                foreach (ProtoCrewMember member in part_snapshot.protoModuleCrew)
                                {
                                    if (!Kerbal_dropped.ContainsKey(member.name))
                                        Kerbal_dropped.Add(member.name, temp_vessel.id);
                                }
                            }
                            new_vessel_found = true;
                        }
                        Vessels.Add(temp_vessel.id);
                    }
                }
            }

            Log.PopStackInfo("leaving search_for_new_vessels(string save_file_name)");

            return (new_vessel_found);
        }


        /*************************************************************************************************************************/
        public List<ProtoVessel> search_for_new_vessels(Game loadgame, Game savegame)
        {
            List<ProtoVessel> return_list = new List<ProtoVessel>();

            Log.PushStackInfo("FMRS_Core.search_for_new_vessels(string, string)", "entering List<Guid> search_for_new_vessels(Game loadgame, Game savegame)");

            foreach (ProtoVessel vessel_load in loadgame.flightState.protoVessels)
            {
                if (vessel_load.landed || vessel_load.splashed)

                    if (savegame.flightState.protoVessels.Find(v => v.vesselID == vessel_load.vesselID) == null)
                    {
                        return_list.Add(vessel_load);
                        Log.dbg("{0} Found and added to list", vessel_load.vesselName);
                    }
            }

            Log.PopStackInfo("leaving List<Guid> search_for_new_vessels(Game loadgame, Game savegame)");

            return (return_list);
        }


        /*************************************************************************************************************************/
        public void jump_to_vessel(Guid vessel_id, bool save_landed)
        {
            int load_vessel;

            Log.PushStackInfo("FMRS_Core.jump_to_vessel(Guid, bool)", "entering jump_to_vessel(Guid vessel_id) {0} {1}", vessel_id, save_landed);
            Log.dbg("Jump to {0}", vessel_id);

            if (save_landed)
            {
                if (FlightGlobals.ActiveVessel.id == _SAVE_Main_Vessel)
                {
                    //GamePersistence.SaveGame("FMRS_main_save", HighLogic.SaveFolder, SaveMode.OVERWRITE);
                    FMRS_SAVE_Util.Instance.SaveGame("FMRS_Core.jump_to_vessel(Guid, bool)", "FMRS_main_save", HighLogic.SaveFolder, SaveMode.OVERWRITE);
                }
                save_landed_vessel(true, false);
            }

            Game loadgame = GamePersistence.LoadGame(get_save_value(save_cat.DROPPED, vessel_id.ToString()), HighLogic.SaveFolder + "/FMRS", false, false);

            if (loadgame != null && loadgame.compatible && loadgame.flightState != null)
            {
                Log.dbg("try to load gamefile {0}", get_save_value(save_cat.DROPPED, vessel_id.ToString()));

                // TODO: semantics refactor. load_vessel is the incentive
                for (load_vessel = 0; load_vessel < loadgame.flightState.protoVessels.Count && loadgame.flightState.protoVessels[load_vessel].vesselID != vessel_id; load_vessel++) ;
              
              
                if (load_vessel < loadgame.flightState.protoVessels.Count)
                {
                    Log.dbg("FMRS_save found, Vessel found, try to start");
                    
                    if (vessel_id != _SAVE_Main_Vessel)
                    {
                        _SAVE_Switched_To_Savefile = get_save_value(save_cat.DROPPED, vessel_id.ToString());
                        Log.info("sithilfe: {0}", _SAVE_Switched_To_Savefile);
                        _SAVE_Switched_To_Dropped = true;
                    }
                    else
                    {
                        _SAVE_Switched_To_Dropped = false;
                    }
                    Log.dbg("load_vessel: {0}", load_vessel);
                    Log.dbg("loadgame.flightState.protoVessels.Count: {0}", loadgame.flightState.protoVessels.Count);
                    Log.dbg("loadgame: {0}", loadgame);
                    Log.dbg("File loaded: {0}/FMRS/{1}", HighLogic.SaveFolder,  get_save_value(save_cat.DROPPED, vessel_id.ToString()));

                    Log.ShowStackInfo();
                    // detach_handlers();
                    FMRS_SAVE_Util.Instance.StartAndFocusVessel(loadgame, load_vessel);
//                    FlightDriver.StartAndFocusVessel(loadgame, load_vessel);
                    // attach_handlers();
                    return;
                }
            }
           
           Log.PopStackInfo("leaving jump_to_vessel(Guid vessel_id)");
        }


        /*************************************************************************************************************************/
        public void jump_to_vessel(string main)
        {
            Game loadgame;
            int load_vessel;

            Log.PushStackInfo("FMRS_Core.jump_to_vessel(string)", "entering jump_to_vessel(string main)");
            Log.dbg("Jump to Main");

            if (!_SAVE_Switched_To_Dropped)
                return;

            save_landed_vessel(true, false);

            loadgame = GamePersistence.LoadGame("FMRS_main_save", HighLogic.SaveFolder, false, false);

            if (loadgame != null && loadgame.compatible && loadgame.flightState != null)
            {
                Log.dbg("try to load gamefile FMRS_main_save");

                for (load_vessel = 0; load_vessel < loadgame.flightState.protoVessels.Count && loadgame.flightState.protoVessels[load_vessel].vesselID != _SAVE_Main_Vessel; load_vessel++) ;
                if (load_vessel < loadgame.flightState.protoVessels.Count)
                {
                    Log.dbg("FMRS_main_save found, main vessel found, try to load");
                    _SAVE_Switched_To_Savefile = "FMRS_main_save";
                    _SAVE_Switched_To_Dropped = false;
                    FMRS_SAVE_Util.Instance.StartAndFocusVessel(loadgame, load_vessel);
//                    FlightDriver.StartAndFocusVessel(loadgame, load_vessel);
                }
                else
                    Log.dbg("main vessel not found in savefile");
            }
            else
                Log.error("Loading gamefile FMRS_main_save failed");

            Log.PopStackInfo("leaving jump_to_vessel(string main)");
        }


        /*************************************************************************************************************************/
        public void jump_to_vessel(Guid vessel_id, string save_file)
        {
            Game loadgame;
            int load_vessel;

            Log.PushStackInfo("FMRS_Core.jump_to_vessel(Guid, string)", "entering jump_to_vessel(string vessel_id,string save_file {0} {1}", vessel_id, save_file);
            Log.dbg("Jump to {0}", save_file);

            loadgame = GamePersistence.LoadGame(save_file, HighLogic.SaveFolder + "/FMRS", false, false);

            if (loadgame != null && loadgame.compatible && loadgame.flightState != null)
            {
                Log.dbg("try to load gamefile {0}", save_file);

                for (load_vessel = 0; load_vessel < loadgame.flightState.protoVessels.Count && loadgame.flightState.protoVessels[load_vessel].vesselID != vessel_id; load_vessel++) ;
                if (load_vessel < loadgame.flightState.protoVessels.Count)
                {
                    if (vessel_id != _SAVE_Main_Vessel)
                    {
                        _SAVE_Switched_To_Savefile = save_file;
                        _SAVE_Switched_To_Dropped = true;
                    }
                    else
                        _SAVE_Switched_To_Dropped = false;
                    FMRS_SAVE_Util.Instance.StartAndFocusVessel(loadgame, load_vessel);
//                    FlightDriver.StartAndFocusVessel(loadgame, load_vessel);
                }
            }
            else
                Log.error("Loading gamefile {0} failed", save_file);

            Log.PopStackInfo("leaving jump_to_vessel(string vessel_id,string save_file)");
        }


        /*************************************************************************************************************************/
        public void main_vessel_changed(string save_file)
        {
            Log.PushStackInfo("FMRS_Core.main_vessel_changed", "enter main_vessel_changed(string save_file) {0}", save_file);
            Log.dbg("switching main vessel");

            ProtoVessel temp_proto;
            Game loadgame = GamePersistence.LoadGame(save_file, HighLogic.SaveFolder + "/FMRS", false, false);

            if (loadgame != null && loadgame.compatible && loadgame.flightState != null)
            {
                temp_proto = loadgame.flightState.protoVessels.Find(p => p.vesselID == _SAVE_Main_Vessel);
                if (temp_proto != null)
                {
                    if (Vessels_dropped.ContainsKey(temp_proto.vesselID))
                        delete_dropped_vessel(temp_proto.vesselID);

                    Vessels_dropped.Add(temp_proto.vesselID, quicksave_file_name);
                    Vessels_dropped_names.Add(temp_proto.vesselID, temp_proto.vesselName);
                    Vessels.Add(temp_proto.vesselID);
                }
                else
                    Log.dbg("main vessel not found");

                if (Vessels_dropped.ContainsKey(FlightGlobals.ActiveVessel.id))
                    delete_dropped_vessel(FlightGlobals.ActiveVessel.id);

                _SAVE_Main_Vessel = FlightGlobals.ActiveVessel.id;
            }
            else
                Log.dbg("unable to load savefile");

            Log.PopStackInfo("leaving main_vessel_changed(string save_file)");
        }


        /*************************************************************************************************************************/
        public void vessel_create_routine(Vessel input)
        {
            Log.PushStackInfo("FMRS_Core.vessel_create_routine", "enter vessel_create_routine(Vessel input) {0}", input.id);
            Log.dbg("Vessel created");

            if (!staged_vessel)
            {
                Time_Trigger_Staging = Planetarium.GetUniversalTime() + 1;
                separated_vessel = true;
                timer_staging_active = true;
                timer_cuto_active = true;
                Time_Trigger_Cuto = Planetarium.GetUniversalTime();
            }

            Log.PopStackInfo("leaving vessel_create_routine(Vessel input)");
        }


        /*************************************************************************************************************************/
        void vessel_on_rails(Vessel vessel)
        {
            List<ProtoCrewMember> member_list = new List<ProtoCrewMember>();

            Log.PushStackInfo("FMRS_Core.vessel_on_rails", "enter vessel_on_rails(Vessel vessel) {0}", vessel.id);
            Log.detail("Vessel will be on rails: {0}", vessel.vesselName);

            if (Vessels_dropped.ContainsKey(vessel.id) && !_SAVE_Switched_To_Dropped)
            {
                Log.dbg("this vessel is listed in dropped dict");

                foreach (ProtoPartSnapshot part_snapshot in vessel.protoVessel.protoPartSnapshots)
                {
                    foreach (ProtoCrewMember member in part_snapshot.protoModuleCrew)
                        member_list.Add(member);
                    foreach (ProtoCrewMember member in member_list)
                    {
                        Log.dbg("remove crew member {0}", member.name);
                        part_snapshot.RemoveCrew(member);
                    }
                    member_list.Clear();
                }
            }

            if (_SAVE_Switched_To_Dropped)
            {
                if (loaded_vessels.Contains(vessel.id) && vessel.id != FlightGlobals.ActiveVessel.id && !vessel.Landed && !vessel.Splashed)
                {
                    Log.detail("loaded_vessels: removing {0}", vessel.id);
                    loaded_vessels.Remove(vessel.id);
                    Log.detail("vessel {0} removed from loaded_vessels", vessel.name);
                }

                if (vessel.id == _SAVE_Main_Vessel)
                {
                    if (ThrottleReplay != null)
                    {
                        vessel.OnFlyByWire -= new FlightInputCallback(ThrottleReplay.flybywire);
                        ThrottleReplay.EndReplay();
                    }
                }
            }

            Log.PopStackInfo("leave vessel_on_rails(Vessel vessel)");
        }


        /*************************************************************************************************************************/
        void vessel_off_rails(Vessel vessel)
        {
            Log.PushStackInfo("FMRS_Core.vessel_off_rails", "enter vessel_off_rails(Vessel vessel) {0}", vessel.id);
            Log.dbg("Vessel will be off rails: {0}", vessel.vesselName);

            if (_SAVE_Switched_To_Dropped)
            {
                Vessel temp_vessel;
                temp_vessel = FlightGlobals.Vessels.Find(v => v.id == vessel.id);
                if (temp_vessel != null)
                    if (temp_vessel.loaded)
                        if (Vessels_dropped.ContainsKey(vessel.id))
                            if (Vessels_dropped[vessel.id] == _SAVE_Switched_To_Savefile)
                                if (!loaded_vessels.Contains(vessel.id))
                                {
                                    Log.dbg("Vessel will be off rails: adding to loaded_vessels");
                                    loaded_vessels.Add(vessel.id);
                                }

                if (vessel.id == FlightGlobals.ActiveVessel.id)
                {
                    List<Vessel> temp_vessel_list = new List<Vessel>();
                    temp_vessel_list = FlightGlobals.Vessels.FindAll(v => Vessels_dropped.ContainsKey(v.id) && v.loaded);
                    foreach (Vessel v in temp_vessel_list)
                        if (Vessels_dropped[v.id] == _SAVE_Switched_To_Savefile)
                            if (!loaded_vessels.Contains(v.id))
                            {
                                Log.detail("Vessel will be off rails: adding to loaded_vessels");
                                loaded_vessels.Add(v.id);
                            }
                }

                Log.dbg("loaded_vessels: {0}", loaded_vessels.Count);
            }
            Log.PopStackInfo("leave vessel_off_rails(Vessel vessel)");
        }
    }
}
