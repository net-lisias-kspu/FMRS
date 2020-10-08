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

using IO = System.IO;
using Log = FMRS.Log;

namespace FMRS_THL
{
    public class FMRS_THL_Log : FMRS_THL_Util
    {
        public List<entry> Throttle_Log_Buffer = new List<entry>();
        public List<entry> temp_buffer = new List<entry>();
        public bool writing = false;
        public bool started = false;


        /*************************************************************************************************************************/
        public FMRS_THL_Log()
        {
            FMRS.Log.info("### FMRS_THL_Log: constructor");
        }

        
/*************************************************************************************************************************/
        public void StartLog()
        {
            Log.PushStackInfo("FMRS_THL.StartLog", "FMRS_THL_Log: entering StartLog()");
            Log.dbg("FMRS_THL_Log: Start Log");

            if (!init_done)
                init();

            started = true;

            FMRS.Log.PopStackInfo("FMRS_THL_Log: leave StartLog()");
        }


        /*************************************************************************************************************************/
        public void EndLog()
        {
            Log.PushStackInfo("FMRS_THL.EndLog", "FMRS_THL_Log: entering EndLog()");
            Log.dbg("FMRS_THL_Log: End Log");

            if (!started)
                return;
            started = false;
            write_record_file();

           Log.PopStackInfo("FMRS_THL_Log: leave EndLog()");
        }


        /*************************************************************************************************************************/
        public void Update()
        {
            if (started)
            {
                if (Throttle_Log_Buffer.Count > 1000)
                {
                    write_record_file();
                }
            }
        }


/*************************************************************************************************************************/
        public void LogThrottle(float in_throttle)
        {
            if(started)
            {
                if(!writing)
                    Throttle_Log_Buffer.Add(new entry(Planetarium.GetUniversalTime(),in_throttle));
                else
                    temp_buffer.Add(new entry(Planetarium.GetUniversalTime(), in_throttle));
            }
        }


/*************************************************************************************************************************/
        public void init()
        {
            Log.PushStackInfo("FMRS_THL.init", "FMRS_THL_Log: entering linit()");
            Log.dbg("FMRS_THL_Log: init");

            if (init_done)
                return;

            if (!IO.File.Exists(FMRS.FILES.RECORD_TXT))
            {
                IO.TextWriter file = IO.File.CreateText(FMRS.FILES.RECORD_TXT);
                file.Close();
            }
            init_done = true;

           Log.PopStackInfo("FMRS_THL_Log: leave linit()");
        }


/*************************************************************************************************************************/
        public void flush_record_file()
        {
            Log.PushStackInfo("FMRS_THL.flush_record_file", "FMRS_THL_Log: entering flush_record_file()");
            Log.dbg("FMRS_THL_Log: flush record file");

            IO.TextWriter writer = IO.File.CreateText(FMRS.FILES.RECORD_TXT);
            writer.Flush();
            writer.Close();

            Throttle_Log_Buffer.Clear();
            temp_buffer.Clear();

            Log.PopStackInfo("FMRS_THL_Log: leave flush_record_file()");
        }


/*************************************************************************************************************************/
        public void write_record_file()
        {
            Log.PushStackInfo("FMRS_THL.write_record_file", "FMRS_THL_Log: entering write_record_file()");
            Log.dbg("FMRS_THL_Log: write to record file");

            writing = true;

            Throttle_Log_Buffer.Sort(delegate(entry x, entry y)
            {
                if (x.time > y.time)
                    return 1;
                else
                    return -1;
            });

			IO.TextWriter writer = null;
			try
			{
				writer = IO.File.AppendText(FMRS.FILES.RECORD_TXT);
				writer.WriteLine("##########################################");
				foreach (entry temp in Throttle_Log_Buffer)
					writer.WriteLine(temp.ToString());

				if (!started)
					writer.WriteLine("####EOF####");
			}
			finally
			{
				writer?.Close();
			}

            Throttle_Log_Buffer.Clear();
            writing = false;

            foreach (entry temp in temp_buffer)
                Throttle_Log_Buffer.Add(temp);
            temp_buffer.Clear();

            FMRS.Log.PopStackInfo("FMRS_THL_Log: leave write_record_file()");
        }

    }



/*************************************************************************************************************************/
/*************************************************************************************************************************/
/*************************************************************************************************************************/
/*************************************************************************************************************************/
    public class FMRS_THL_Rep : FMRS_THL_Util
    {
        public Queue<entry> Throttle_Replay = new Queue<entry>();
        private bool replay = false;    
		public IO.TextReader reader;
        public bool EOF = false;
        public string debug_message = "nv";


/*************************************************************************************************************************/
        public FMRS_THL_Rep(bool Debug_Active = false, bool Debug_Level_1_Active = false)
        {
            Log.detail("### FMRS_THL_Rep: constructor");
        }


/*************************************************************************************************************************/
        public void EndReplay()
        {
            Log.PushStackInfo("FMRS_THL.EndReply", "FMRS_THL_Rep: entering end_replay()");

            if (!replay)
                return;

            Log.dbg("FMRS_THL_Rep: End Replay");

            try { reader.Close(); }
                catch(Exception){}
            
            replay = false;
            debug_message = "replay ended";

            Log.PopStackInfo("FMRS_THL_Rep: leave end_replay()");
        }


/*************************************************************************************************************************/
        public void flybywire(FlightCtrlState state)
        {
            entry temp_entry;
            Log.info("flybywire, state: " + state.ToString());
            if (!replay && !EOF)
                start_replay();

            while(true)
            {
                if (Throttle_Replay.Count < 10 && !EOF)
                    read_throttle_values();
                if (Throttle_Replay.Count < 1)
                {
                    EndReplay();
                    break;
                }                  
                temp_entry = Throttle_Replay.Dequeue();
                if (temp_entry.time < Planetarium.GetUniversalTime())
                    continue;
                state.mainThrottle = temp_entry.value;
                debug_message = Math.Round(temp_entry.value,2).ToString() + " : " + Math.Round(temp_entry.time,2).ToString() + " @ " + Math.Round(Planetarium.GetUniversalTime(),2).ToString() + " left " + Throttle_Replay.Count.ToString();
                break;
            }
        }


/*************************************************************************************************************************/
        public void start_replay()
        {
            entry temp_entry;
            string temp_string;
            Log.info("start_replay");

            Log.PushStackInfo("FMS_THL.start_replay", "FMRS_THL_Rep: entering start_replay()");
            Log.dbg("FMRS_THL_Rep: Start Replay");

            if (replay || EOF)
                return;

            Throttle_Replay.Clear();

            reader = IO.File.OpenText(FMRS.FILES.RECORD_TXT);

            while (true)
            {
                temp_string = reader.ReadLine();    //sithilfe check data /end of file
                if (temp_string.Contains("EOF"))
                {
                    Log.dbg("FMRS_THL_Rep: EOF");

                    EOF = true;
                    break;
                }
                if (temp_string.Contains("#"))
                    continue;
                temp_entry = new entry(temp_string);
                if (temp_entry.time > Planetarium.GetUniversalTime())
                {
                    Log.dbg("FMRS_THL_Rep: start time found: {0}", temp_entry.time);

                    read_throttle_values();
                    replay = true;                
                    break;
                }
            }

            FMRS.Log.PopStackInfo("FMRS_THL_Rep: leave start_replay()");
        }


/*************************************************************************************************************************/
        public void Update(bool Debug_Active = false, bool Debug_Level_1_Active = false)
        {
            if (replay)
            {
                if (!EOF)
                {
                    if (Throttle_Replay.Count < 25)
                    {
                        Log.dbg("FMRS_THL_Rep: update read_throttle_values() call");
                        read_throttle_values();
                    }
                }
                else
                {
                    if (Throttle_Replay.Count == 0)
                    {
                        Log.dbg("FMRS_THL_Rep: update EndReplay() call");
                        EndReplay();
                    }
                }
            }
        }


/*************************************************************************************************************************/
        public void read_throttle_values()
        {
            string temp_string;

            Log.PushStackInfo("FMRS_THL.read_throttle_values", "FMRS_THL_Rep: entering read_throttle_values()");
            Log.dbg("FMRS_THL_Rep: read throttle values");

            for (int i = 0; i < 1000; i++)
            {
                temp_string = reader.ReadLine();
                if (temp_string.Contains("EOF"))
                {
                    EOF = true;
                    Log.dbg("FMRS_THL_Rep: EOF");
                    break;
                }
                else if (temp_string.Contains("#"))
                    continue;

                Throttle_Replay.Enqueue(new entry(temp_string));
            }

            Log.dbg("FMRS_THL_Rep: buffer size = {0}" + Throttle_Replay.Count);
            Log.PopStackInfo("FMRS_THL_Rep: leave read_throttle_values()");
        }
    }


/*************************************************************************************************************************/
/*************************************************************************************************************************/
/*************************************************************************************************************************/
/*************************************************************************************************************************/
    public class FMRS_THL_Util
    {
        public bool init_done = false;
    }



/*************************************************************************************************************************/
/*************************************************************************************************************************/
/*************************************************************************************************************************/
/*************************************************************************************************************************/
    public class entry
    {
        public double time;
        public float value;


/*************************************************************************************************************************/
        public entry() { }


/*************************************************************************************************************************/
        public entry(double time, float value) 
        {
            this.time = time;
            this.value = value;
        }


/*************************************************************************************************************************/
        public entry(string entry)
        {
            string[] line = entry.Split('@');
            try
            {
                time = Convert.ToDouble(line[0]);
                value = float.Parse(line[1]);
            }
            catch (Exception) 
            {
                time = 0;
                value = 0;
            }
        }


/*************************************************************************************************************************/
        public new string ToString()
        {
            string temp;
            temp = time.ToString();
            temp += "@";
            temp += value.ToString();
            return temp;
        }

    }
}
