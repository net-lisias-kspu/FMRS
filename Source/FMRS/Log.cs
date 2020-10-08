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
#if UNITY2019
		private static readonly Logger log = Logger.CreateForType<Startup>();
#endif
		internal static void force(string msg, params object[] @params)
		{
#if UNITY2019
			log.force(msg, @params);
#else
			if (null != @params && @params.Length > 0) UnityEngine.Debug.LogFormat("[FMRS] "+msg, @params);
			else UnityEngine.Debug.Log("[FMRS] "+msg);
#endif
		}

		[ConditionalAttribute("DEBUG")]
		internal static void info(string msg, params object[] @params)
		{
#if UNITY2019
			Log.info(msg, @params);
#endif
		}

		[ConditionalAttribute("DEBUG")]
		internal static void warn(string msg, params object[] @params)
		{
#if UNITY2019
			log.warn(msg, @params);
#endif
		}

		[ConditionalAttribute("DEBUG")]
		internal static void detail(string msg, params object[] @params)
		{
#if UNITY2019
			log.detail(msg, @params);
#endif
		}

		[ConditionalAttribute("DEBUG")]
		internal static void error(Exception e, object offended)
		{
#if UNITY2019
			log.error(offended, e);
#else
			UnityEngine.Debug.LogException(e);
#endif
		}

		[ConditionalAttribute("DEBUG")]
		internal static void error(string msg, params object[] @params)
		{
#if UNITY2019
			log.error(msg, @params);
#else
			if (null != @params && @params.Length > 0) UnityEngine.Debug.LogErrorFormat(msg, @params);
			else UnityEngine.Debug.LogError(msg);
#endif
		}

		[ConditionalAttribute("DEBUG")]
		internal static void dbg(string msg, params object[] @params)
		{
#if UNITY2019
			log.trace(msg, @params);
#endif
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
#if UNITY2019
			log.trace(new_msg);
#endif
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
			Log.detail(msg);
#endif
		}
	}
}
