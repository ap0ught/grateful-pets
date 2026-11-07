using System;
using System.ComponentModel;
using System.Linq;
using AiUnity.Common.Attributes;
using AiUnity.Common.Extensions;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Log;
using AiUnity.Common.Patterns;
using AiUnity.NLog.Core.Common;
using AiUnity.NLog.Core.Layouts;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;

namespace AiUnity.NLog.Core.Targets;

[Target("GameConsole")]
[Preserve]
public sealed class GameConsoleTarget : UnityConsoleTarget
{
	private const bool ConsoleActiveDefault = true;

	private const int FontSizeDefault = 8;

	private const bool IconEnableDefault = true;

	private const string GameLayoutDefault = "<color=orange>[${level}] ${callsite}</color>${newline}<color=white>${message}</color><color=red>${exception}</color>";

	private const LogLevels LogLevelsFilterDefault = LogLevels.Warn | LogLevels.Error | LogLevels.Fatal | LogLevels.Assert;

	[RequiredParameter]
	[DefaultValue(true)]
	[Display("Start console", "Make Game Console Log window active at startup.", false, 0)]
	public bool ConsoleActive { get; set; }

	[RequiredParameter]
	[DefaultValue(8)]
	[Display("Font size", "Font size for game console", false, 0)]
	public int FontSize { get; set; }

	[RequiredParameter]
	[DefaultValue(true)]
	public bool IconEnable { get; set; }

	[RequiredParameter]
	[DefaultValue("<color=orange>[${level}] ${callsite}</color>${newline}<color=white>${message}</color><color=red>${exception}</color>")]
	[Display("Layout", "Specifies the layout and content of log message.  The + icon will present a list of variables that can be added.  For the Unity Console all excepted xml formating can be used (http://docs.unity3d.com/Manual/StyledText.html)", false, -100)]
	public override Layout Layout { get; set; }

	[RequiredParameter]
	[DefaultValue(LogLevels.Warn | LogLevels.Error | LogLevels.Fatal | LogLevels.Assert)]
	[Display("Filter levels", "Starting log level filter that is runtime adjustable.", false, 0)]
	public LogLevels LogLevelsFilter { get; set; }

	private static IInternalLogger Logger => Singleton<NLogInternalLogger>.Instance;

	private IGameConsoleController GameConsoleController { get; set; }

	public GameConsoleTarget()
	{
		if (Application.isPlaying)
		{
			Layout = "<color=orange>[${level}] ${callsite}</color>${newline}<color=white>${message}</color><color=red>${exception}</color>";
			LogLevelsFilter = LogLevels.Warn | LogLevels.Error | LogLevels.Fatal | LogLevels.Assert;
			FontSize = 8;
			IconEnable = true;
			ConsoleActive = true;
			Scene activeScene = SceneManager.GetActiveScene();
			UpdateNLogMessageTarget(activeScene);
			SceneManager.activeSceneChanged += (Scene s1, Scene s2) =>
			{
				UpdateNLogMessageTarget(s2);
			};
		}
	}

	protected override void Write(LogEventInfo logEvent)
	{
		if (Application.isPlaying && GameConsoleController != null)
		{
			string message = FixUnityConsoleXML(Layout.Render(logEvent) + Environment.NewLine);
			GameConsoleController.AddMessage((int)logEvent.Level, message, logEvent.LoggerName, logEvent.TimeStamp);
			if (logEvent.Level.Has(LogLevels.Assert) && Singleton<NLogManager>.Instance.AssertException)
			{
				Debug.Break();
				throw new AssertException(Layout.Render(logEvent), logEvent.Exception);
			}
		}
	}

	private void UpdateNLogMessageTarget(Scene scene)
	{
		GameConsoleController = scene.GetRootGameObjects().SelectMany((GameObject g) => g.GetComponentsInChildren<IGameConsoleController>()).FirstOrDefault();
		if (GameConsoleController != null)
		{
			GameConsoleController.SetIconEnable(IconEnable);
			GameConsoleController.SetConsoleActive(ConsoleActive);
			GameConsoleController.SetFontSize(FontSize);
			GameConsoleController.SetLogLevelFilter(LogLevelsFilter);
		}
		else
		{
			Logger.Warn("Unable to locate GameConsole GameObject.  Please place NLog prefab GameConsole in your hierarchy.");
		}
	}
}
