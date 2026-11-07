#define NLOG_ALL
using System;
using AiUnity.Common.Patterns;
using AiUnity.NLog.Core;
using UnityEngine;

namespace AiUnity.NLog.Examples.Scripts;

[AddComponentMenu("AiUnity/NLog/Examples/NLogExample")]
public class NLogExample : MonoBehaviour
{
	private NLogger logger;

	public GameObject gameObjectContext;

	private void Awake()
	{
		Debug.Log("Standard Unity Debug log message, called from NLogExample Awake() method.\nPlease configure NLog GUI with the desired rule, target, and message level.");
		logger = Singleton<NLogManager>.Instance.GetLogger(this);
		logger.Info("Testing a NLog <i>Info</i> message from {0} Awake() method.", GetType().Name);
	}

	private void Start()
	{
		logger.Assert(false, "Testing a NLog <i>Assert</i> message from {0} Start() method.", GetType().Name);
		logger.Fatal("Testing a NLog <i>Fatal</i> message from {0} Start() method.", GetType().Name);
		logger.Error("Testing a NLog <i>Error</i> message from {0} Start() method.", GetType().Name);
		logger.Warn("Testing a NLog <i>Warn</i> message from {0} Start() method.", GetType().Name);
		logger.Info("Testing a NLog <i>Info</i> message from {0} Start() method.", GetType().Name);
		logger.Debug("Testing a NLog <i>Debug</i> message from {0} Start() method.", GetType().Name);
		logger.Trace("Testing a NLog <i>Trace</i> message from {0} Start() method.", GetType().Name);
		int num = 1;
		logger.Assert(num == 0, "Assertion fired because assertCondition={0}.", num);
		logger.Info(gameObjectContext, "Test NLog message with an explicit gameObject context");
		Exception exception;
		try
		{
			throw new Exception("Test Exception");
		}
		catch (Exception ex)
		{
			exception = ex;
		}
		logger.Info(exception, "Test NLog message with an exception");
	}

	private void Update()
	{
	}

	private void OnGUI()
	{
		GUILayout.BeginArea(new Rect(0f, 0f, Screen.width, Screen.height));
		GUILayout.BeginVertical();
		GUILayout.FlexibleSpace();
		GUILayout.Label("If messages missing on expected destination please configure NLog GUI with desired LogLevel (i.e. Everything) and target (i.e. UnityConsole/GameConsole).");
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}
}
