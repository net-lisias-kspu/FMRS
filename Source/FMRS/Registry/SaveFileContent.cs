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
using IO = System.IO;

using save_cat = FMRS.FMRS_Util.save_cat;

namespace FMRS.Registry
{
	internal class SaveFileContent
	{
        internal readonly Dictionary<save_cat, Dictionary<string, string>> content = new Dictionary<save_cat, Dictionary<string, string>>();

		private readonly object MUTEX = new object();

		internal static SaveFileContent Instance = new SaveFileContent(); 
		private SaveFileContent(){ }

		internal void Init()
		{
			lock (MUTEX)
			{
				foreach (save_cat sc in (save_cat[]) Enum.GetValues(typeof(save_cat)))
					if (!this.content.ContainsKey(sc))
						this.content.Add(sc, new Dictionary<string, string>());
			}
		}

		internal void Set(save_cat cat, string key, string value)
		{
			lock (MUTEX)
			{
				if (this.content[cat].ContainsKey(key))
				{
					this.content[cat][key] = value;
				}
				else
				{
					this.content[cat].Add(key, value);
				}
			}
		}

		internal bool ContainsKey(save_cat key)
		{
			// No lock on this, as we know that we never remove a category from the dictionary once it's there.
			return this.content.ContainsKey(key);
		}

		internal void Clear()
		{
			lock (MUTEX)
			{
				foreach (KeyValuePair<save_cat, Dictionary<string, string>> content in this.content)
					this.content[content.Key].Clear();
			}
		}

		internal void Clear(save_cat key)
		{
			lock (MUTEX)
			{
				this.content.Clear();
			}
		}

		internal string Get(save_cat cat, string key)
		{
			lock (MUTEX)
			{
				if (this.content.ContainsKey(cat))
					return (this.content[cat][key]);
				else
					return (false.ToString());
			}
		}

		internal Dictionary<string,string> Get(save_cat cat)
		{
			lock (MUTEX)
			{
				if (this.content.ContainsKey(cat))
					return new Dictionary<string,string>(this.content[cat]);
				else
					return new Dictionary<string,string>();
			}
		}

		internal void DumpTo(IO.TextWriter file)
		{
			lock (MUTEX)
			{
				foreach (KeyValuePair<save_cat, Dictionary<string, string>> save_cat_block in this.content)
					foreach (KeyValuePair<string, string> writevalue in save_cat_block.Value)
					   file.WriteLine(save_cat_toString(save_cat_block.Key) + "=" + writevalue.Key + "=" + writevalue.Value);
			}
		}

		internal void DumpToLog()
		{
			Log.dbg("DumpToLog");
			lock (MUTEX)
			{
				Log.dbg("MUTEX {0}", this.content.Count);
				foreach (KeyValuePair<save_cat, Dictionary<string, string>> temp_keyvalue in this.content)
					foreach (KeyValuePair<string, string> readvalue in temp_keyvalue.Value)
						Log.info("{0} = {1} = {2}" + temp_keyvalue.Key, readvalue.Key, readvalue.Value);
			}
		}

		private string save_cat_toString(save_cat cat)
		{
			Log.dbg("enter string save_cat_toString(save_cat cat) {0} NO LEAVE MESSAGE", cat);

			switch (cat)
			{
				case save_cat.SETTING:			return "SETTING";
				case save_cat.SAVE:				return "SAVE";
				case save_cat.SAVEFILE:			return "SAVEFILE";
				case save_cat.SUBSAVEFILE:		return "SUBSAVEFILE";
				case save_cat.DROPPED:			return "VESSEL_DROPPED";
				case save_cat.NAME:				return "VESSEL_NAME";
				case save_cat.STATE:			return "VESSEL_STATE";
				case save_cat.KERBAL_DROPPED:	return "KERBAL_DROPPED";

				default:						return "UNDEF";
			}
		}
	}
}