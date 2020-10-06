/*
 * The MIT License (MIT)
 * 
 * Copyright (c) 2018-2020 LisiasT
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
using KSPe.Util.Log;
using System.Diagnostics;

#if DEBUG
using System.Collections;
using System.Collections.Generic;
#endif

namespace FMRS
{
	public static class Log
	{
		private static readonly Logger log = Logger.CreateForType<Startup>();

		internal static void force(string msg, params object[] @params)
		{
			log.force(msg, @params);
		}

		internal static void info(string msg, params object[] @params)
		{
			Log.info(msg, @params);
		}

		internal static void warn(string msg, params object[] @params)
		{
			log.warn(msg, @params);
		}

		internal static void detail(string msg, params object[] @params)
		{
			log.detail(msg, @params);
		}

		internal static void error(Exception e, object offended)
		{
			log.error(offended, e);
		}

		internal static void error(string msg, params object[] @params)
		{
			log.error(msg, @params);
		}

		[ConditionalAttribute("DEBUG")]
		internal static void dbg(string msg, params object[] @params)
		{
			log.trace(msg, @params);
		}

#if DEBUG
		private static readonly HashSet<string> DBG_SET = new HashSet<string>();
		static Stack funcStack = new Stack();
#endif

		[ConditionalAttribute("DEBUG")]
		internal static void dbgOnce(string msg, params object[] @params)
		{
			string new_msg = string.Format(msg, @params);
#if DEBUG
			if (DBG_SET.Contains(new_msg)) return;
			DBG_SET.Add(new_msg);
#endif
			log.trace(new_msg);
		}

		[ConditionalAttribute("DEBUG")]
		public static void ShowStackInfo()
		{
#if DEBUG
			int cnt = 0;
			Log.detail("Stack size: {0}", funcStack.Count);
			foreach(var obj in funcStack)
			{
				Log.detail("Stack[{0}] = {1}", cnt, obj);
				cnt++;
			}
#endif
		}

		[ConditionalAttribute("DEBUG")]
		public static void PushStackInfo(string funcName, string msg, params object[] @params)
		{
#if DEBUG
			funcStack.Push(funcName);
#endif
			Log.detail(msg, @params);
		}

		public static void PopStackInfo(string msg)
		{
#if DEBUG
			if (funcStack.Count > 0)
			{
				string f = (string)funcStack.Pop();
			}
			else
				Log.warn("Pop failed, no values on stack");
#endif
			Log.detail(msg);
		}
	}
}
