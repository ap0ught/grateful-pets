using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using ConsoleLib.Console;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.EventsModels;
using UnityEngine;
using XRL;
using XRL.Core;
using XRL.UI;

public class MetricsManager
{
	public class TelemetryEntry
	{
		public bool isJSON;

		public object data;

		public string category;

		public string name;

		public ModInfo modInfo;

		public TelemetryEntry()
		{
		}

		public TelemetryEntry(string name, object data = null, string category = "uncategorized", bool json = false, ModInfo modInfo = null)
			: this()
		{
			this.name = name;
			this.data = data;
			this.category = category;
			isJSON = json;
			this.modInfo = modInfo;
		}

		public void Send()
		{
			if (isJSON)
			{
				_SendTelemetryJSON(name, (string)data, category, modInfo);
			}
			else
			{
				_SendTelemetry(name, data, category, modInfo);
			}
		}

		public void Clear()
		{
			name = null;
			isJSON = false;
			data = null;
			category = null;
		}
	}

	public static List<string> OptionsToIncludeInTelemetry = new List<string> { "OptionsPrereleaseInputManager", "OptionOverlayUI", "OptionOverlayPrereleaseStage" };

	public static bool bInit = false;

	public static GameObject ManagerObject = null;

	private static EntityTokenResponse entityTokenResponse = null;

	private static CleanQueue<TelemetryEntry> EventPool = new CleanQueue<TelemetryEntry>();

	private static CleanQueue<TelemetryEntry> Events = new CleanQueue<TelemetryEntry>();

	public static Dictionary<string, string> globalTags = null;

	private static int telemetryErrorLogged = 0;

	public static FieldInfo RandomItertorField = null;

	public static FieldInfo RandomItertorNextField = null;

	public static Dictionary<string, int> lastCheckpoint = new Dictionary<string, int>();

	public static HashSet<string> exceptionsSent = new HashSet<string>();

	public static void LogExceptionNow(Exception ex)
	{
		if (bInit)
		{
			_ = Globals.EnableMetrics;
		}
	}

	public static void LogExceptionNow(string ex)
	{
		if (bInit)
		{
			_ = Globals.EnableMetrics;
		}
	}

	public static void Init()
	{
		if (Globals.EnableMetrics)
		{
			if (ManagerObject == null)
			{
				ManagerObject = new GameObject();
				ManagerObject.name = "PlayFab Dummy";
			}
			UnityEngine.Debug.Log("PlayFab player id: " + GetAnonymousID());
			PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest
			{
				CustomId = GetAnonymousID(),
				CreateAccount = true,
				TitleId = "9C73C"
			}, OnLoginSuccess, OnLoginFailure);
		}
	}

	private static string GetAnonymousID()
	{
		string text = PlayerPrefs.GetString("_PlayfabID", null);
		if (string.IsNullOrEmpty(text))
		{
			text = Guid.NewGuid().ToString();
			PlayerPrefs.SetString("_PlayfabID", text);
		}
		return text;
	}

	private static void OnLoginSuccess(LoginResult result)
	{
		UnityEngine.Debug.Log("PlayFab login success.");
		entityTokenResponse = result.EntityToken;
		DateTime? tokenExpiration = entityTokenResponse.TokenExpiration;
		UnityEngine.Debug.Log("PlayFab token expires " + tokenExpiration.ToString());
	}

	private static void OnLoginFailure(PlayFabError error)
	{
		UnityEngine.Debug.LogError("PlayFab login failure:" + error.GenerateErrorReport());
	}

	public static void Update()
	{
		if (Events.Count <= 0)
		{
			return;
		}
		lock (Events)
		{
			lock (EventPool)
			{
				while (Events.Count > 0)
				{
					TelemetryEntry telemetryEntry = Events.Dequeue();
					telemetryEntry.Send();
					telemetryEntry.Clear();
					EventPool.Enqueue(telemetryEntry);
				}
			}
		}
	}

	[Conditional("UNITY_EDITOR")]
	public static void LogEditorInfo(string Message)
	{
	}

	public static void LogModInfo(ModInfo mod, object Message)
	{
		string text = ((mod != null) ? ("[" + mod.DisplayTitleStripped + "]") : "");
		mod?.Settings?.Warnings?.Add(Message.ToString());
		UnityEngine.Debug.Log("MODINFO " + text + " - " + Message);
	}

	public static void LogAssemblyWarning(Type Type, object Message)
	{
		LogAssemblyWarning(Type?.Assembly, Message);
	}

	public static void LogAssemblyWarning(MemberInfo Info, object Message)
	{
		LogAssemblyWarning(Info?.DeclaringType?.Assembly, Message);
	}

	public static void LogAssemblyWarning(Assembly Assembly, object Message)
	{
		ModInfo mod = ModManager.GetMod(Assembly);
		if (mod != null)
		{
			mod.Warn(Message);
		}
		else
		{
			LogWarning(Message);
		}
	}

	public static void LogModWarning(ModInfo mod, object Message)
	{
		string text = ((mod != null) ? ("[" + mod.DisplayTitleStripped + "]") : "");
		mod?.Settings?.Warnings?.Add(Message.ToString());
		UnityEngine.Debug.LogWarning("MODWARN " + text + " - " + Message);
	}

	public static void LogWarning(string Message)
	{
		UnityEngine.Debug.LogWarning("WARN - " + Message);
	}

	public static void LogWarning(object Message)
	{
		UnityEngine.Debug.LogWarning("WARN - " + Message.ToString());
	}

	[Conditional("UNITY_EDITOR")]
	public static void LogEditorWarning(string Message)
	{
	}

	public static void LogFileWarning(DataFile File, object Message, IXmlLineInfo Line = null)
	{
		string text = ((Line != null) ? $"File: {File.Path}, Line: {Line.LineNumber}:{Line.LinePosition} " : ("File: " + File.Path + " "));
		if (File.IsMod)
		{
			LogModWarning(File.Mod, text + Message);
		}
		else
		{
			LogWarning(text + Message);
		}
	}

	[Conditional("UNITY_EDITOR")]
	public static void LogEditorError(string Message)
	{
	}

	[Conditional("UNITY_EDITOR")]
	public static void LogEditorError(string Context, string Message)
	{
	}

	public static void LogError(object Message)
	{
		LogError(Message.ToString());
	}

	public static void LogError(object Context, object Message)
	{
		LogError(Context.ToString(), Message.ToString());
	}

	public static void LogCallingModError(object Message)
	{
		if (ModManager.TryGetCallingMod(out var Mod, out var Frame))
		{
			MethodBase method = Frame.GetMethod();
			Mod.Error(Message?.ToString() + " in " + method.DeclaringType.FullName + "." + method.Name);
		}
		else
		{
			LogModError(null, Message);
		}
	}

	public static void LogAssemblyError(Type Type, object Message)
	{
		LogAssemblyError(Type?.Assembly, Message);
	}

	public static void LogAssemblyError(MemberInfo Info, object Message)
	{
		LogAssemblyError(Info?.DeclaringType?.Assembly, Message);
	}

	public static void LogAssemblyError(Assembly Assembly, object Message)
	{
		ModInfo mod = ModManager.GetMod(Assembly);
		if (mod != null)
		{
			mod.Error(Message);
		}
		else
		{
			LogError(Message);
		}
	}

	public static void LogPotentialModError(ModInfo Mod, object Message)
	{
		if (Mod != null)
		{
			LogModError(Mod, Message);
		}
		else
		{
			LogError(Message);
		}
	}

	public static void LogModError(ModInfo mod, object Message)
	{
		string text = ((mod != null) ? (" [" + mod.DisplayTitleStripped + "]") : "");
		mod?.Settings?.Errors?.Add(Message.ToString());
		UnityEngine.Debug.LogError("MODERROR" + text + " - " + Message);
		if (Options.ShowErrorPopups)
		{
			string message = ConsoleLib.Console.ColorUtility.EscapeFormatting(Message.ToString());
			string prompt = (mod?.DisplayTitle ?? "{{R|Mod}}") + " {{R|Error}}";
			Popup.ShowBlockPrompt(message, prompt);
		}
		if (Globals.EnableMetrics && mod != null)
		{
			string text2 = Message.ToString();
			if (!exceptionsSent.Contains(text2))
			{
				exceptionsSent.Add(text2);
				SendTelemetry("mod_error", "diagnostics.moderror", new Dictionary<string, string>
				{
					{ "data", text2 },
					{ "mod", mod.ID }
				}, jsonPayload: false, mod);
			}
		}
	}

	public static void LogFileError(DataFile File, object Message, IXmlLineInfo Line = null)
	{
		string text = ((Line != null) ? $"File: {File.Path}, Line: {Line.LineNumber}:{Line.LinePosition} " : ("File: " + File.Path + " "));
		if (File.IsMod)
		{
			LogModError(File.Mod, text + Message);
		}
		else
		{
			LogError(text + Message);
		}
	}

	public static void LogKeybinding(GameCommand binding)
	{
		LogKeybinding(binding.ID);
	}

	public static void LogKeybinding(string binding)
	{
		string text = CommandBindingManager.GetSerializedBindingsForCommand(binding).Aggregate("", (string a, string b) => (!string.IsNullOrEmpty(a)) ? (a + ", " + b) : b);
		LogInfo("Changed Keybinding " + binding + " to " + text);
		if (Globals.EnableMetrics)
		{
			SendTelemetry("keybind", "diagnostics.keybind", new Dictionary<string, string>
			{
				{ "command", binding },
				{ "binding", text }
			});
		}
	}

	public static void LogError(string Message)
	{
		if (ModManager.TryGetCallingMod(out var Mod, out var _))
		{
			Mod.Error(Message);
			return;
		}
		UnityEngine.Debug.LogError("ERROR  - " + Message);
		if (Options.ShowErrorPopups)
		{
			Popup.ShowBlockPrompt(Message + "\n" + new StackTrace(1).ToString(), "{{R|Error}}");
		}
		if (Globals.EnableMetrics)
		{
			if (!exceptionsSent.Contains(Message))
			{
				exceptionsSent.Add(Message);
				SendTelemetry("game_error", "diagnostics.exceptions", Message + "\n" + new StackTrace(1).ToString());
			}
		}
	}

	public static void LogError(string Context, string Message)
	{
		if (ModManager.TryGetCallingMod(out var Mod, out var _))
		{
			Mod.Error(Context + "@" + Message);
			return;
		}
		UnityEngine.Debug.LogError("ERROR - " + Context + "@" + Message);
		if (Options.ShowErrorPopups)
		{
			Popup.ShowBlockPrompt(Context + " - " + Message + "\n" + new StackTrace(1).ToString(), "{{R|Error}}");
		}
		if (Globals.EnableMetrics)
		{
			string text = Context + " - " + Message;
			if (!exceptionsSent.Contains(text))
			{
				exceptionsSent.Add(text);
				SendTelemetry("game_error", "diagnostics.exceptions", text);
			}
		}
	}

	public static void LogInfo(string Message)
	{
		UnityEngine.Debug.Log("INFO - " + Message);
	}

	public static void LogInfo(string Context, string Message)
	{
		UnityEngine.Debug.Log("INFO - at " + Context + ": " + Message);
	}

	public static void LogError(string Context, Exception x)
	{
		if (ModManager.TryGetCallingMod(out var Mod, out var _))
		{
			Mod.Error($"{Context}: {x}");
			return;
		}
		UnityEngine.Debug.LogError("ERROR - " + Context + " :" + x.ToString());
		if (Options.ShowErrorPopups)
		{
			Popup.ShowBlockPrompt(Context + ":\n" + x.ToString(), "{{R|Error}}");
		}
	}

	public static void SendTelemetryWithPayload(string name, string category = "unassigned", Dictionary<string, string> payload = null)
	{
		SendTelemetry(name, category, payload);
	}

	public static void SendTelemetry(string name, string category = "unassigned", string payload = null)
	{
		SendTelemetry(name, category, new Dictionary<string, string> { { "data", payload } });
	}

	public static void SendTelemetryJSON(string name, string category = "unassigned", string payload = "{}")
	{
		SendTelemetry(name, category, payload, jsonPayload: true);
	}

	public static void SendTelemetry(string name, string category, object payload, bool jsonPayload = false, ModInfo modInfo = null)
	{
		if (payload == null)
		{
			payload = new Dictionary<string, string> { { "data", "none" } };
		}
		if (!Globals.EnableMetrics)
		{
			return;
		}
		lock (Events)
		{
			lock (EventPool)
			{
				TelemetryEntry telemetryEntry = ((EventPool.Count <= 0) ? new TelemetryEntry() : EventPool.Dequeue());
				telemetryEntry.name = name;
				telemetryEntry.category = category;
				telemetryEntry.data = payload;
				telemetryEntry.isJSON = jsonPayload;
				telemetryEntry.modInfo = modInfo;
				Events.Enqueue(telemetryEntry);
			}
		}
	}

	private static void _SendTelemetry(string name, object payload, string category = "unassigned", ModInfo modInfo = null, bool jsonPayload = false)
	{
		try
		{
			if (payload == null)
			{
				payload = new Dictionary<string, string> { { "data", "none" } };
			}
			WriteEventsRequest writeEventsRequest = new WriteEventsRequest();
			EventContents eventContents = new EventContents();
			writeEventsRequest.Events = new List<EventContents> { eventContents };
			eventContents.Name = name;
			eventContents.EventNamespace = "custom.player." + category;
			eventContents.OriginalTimestamp = DateTime.UtcNow;
			if (jsonPayload)
			{
				eventContents.Payload = null;
				eventContents.PayloadJSON = (string)payload;
			}
			else
			{
				eventContents.PayloadJSON = null;
				eventContents.Payload = payload;
			}
			if (globalTags == null)
			{
				globalTags = new Dictionary<string, string>();
				globalTags.Add("version", XRLGame.CoreVersion.ToString());
			}
			eventContents.CustomTags = new Dictionary<string, string>(globalTags);
			foreach (string item in OptionsToIncludeInTelemetry)
			{
				eventContents.CustomTags.Add(item, Options.GetOption(item));
			}
			StringBuilder modList = new StringBuilder();
			ModManager.ForEachMod((ModInfo mod) =>
			{
				modList.AppendFormat("{0}:{1};", mod.ID, mod.Manifest.Version.ToString());
			});
			if (modList.Length > 0)
			{
				if (modList.Length > 128)
				{
					modList.Remove(119, modList.Length - 120);
					modList.Append("; (more)");
				}
				eventContents.CustomTags.Add("mods", modList.ToString());
			}
			if (entityTokenResponse != null)
			{
				PlayFabEventsAPI.WriteTelemetryEvents(writeEventsRequest, OnWriteTelemetrySuccess, OnWriteTelemetryFailure);
			}
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError("Telemetry exception: " + ex.ToString());
		}
	}

	private static void _SendTelemetryJSON(string name, string jsonpayload, string category = "unassigned", ModInfo modInfo = null)
	{
		_SendTelemetry(name, jsonpayload, category, modInfo, jsonPayload: true);
	}

	private static void OnWriteTelemetrySuccess(WriteEventsResponse request)
	{
	}

	private static void OnWriteTelemetryFailure(PlayFabError error)
	{
		if (telemetryErrorLogged <= 0)
		{
			telemetryErrorLogged++;
			UnityEngine.Debug.LogError("Telemetry error: " + error.ToString());
		}
	}

	public static void rngCheckpoint(string location)
	{
	}

	public static void LogException(string Context, Exception x, string category = "game_exception")
	{
		string text = Context + ": " + x;
		if (ModManager.TryGetStackMod(x, out var Mod, out var _))
		{
			Mod.Error(text);
			return;
		}
		UnityEngine.Debug.LogError(text);
		if (Options.ShowErrorPopups)
		{
			Popup.ShowBlockPrompt(text, "{{R|Error}}");
		}
		try
		{
			if (Globals.EnableMetrics && !exceptionsSent.Contains(text))
			{
				exceptionsSent.Add(text);
				SendTelemetry(category, "diagnostics.exceptions", text);
			}
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError("Metrics exception: " + ex.ToString());
		}
	}

	public static void LogEvent(string EventName)
	{
		try
		{
			_ = bInit;
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError("Metrics exception: " + ex.ToString());
		}
	}

	public static void LogEvent(string EventName, float Value)
	{
		try
		{
			_ = bInit;
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError("Metrics exception: " + ex.ToString());
		}
	}
}
