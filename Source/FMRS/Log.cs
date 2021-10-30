/*
	This file is part of Flight Manager for Reusable Stages [FMRS] /L Unleashed
		© 2018-21 Lisias T : http://lisias.net <support@lisias.net>

	THIS FILE is licensed to you under:

		* WTFPL - http://www.wtfpl.net
			* Everyone is permitted to copy and distribute verbatim or modified
 				copies of this license document, and changing it is allowed as long
				as the name is changed.

	THIS FILE is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
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
