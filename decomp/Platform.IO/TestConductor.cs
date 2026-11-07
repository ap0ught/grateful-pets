using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using LaundryBear;
using UnityEngine;
using UnityEngine.UIElements;

namespace Platform.IO;

public static class TestConductor
{
	public class TestResult
	{
		public string name;

		public bool passed;

		public string baseline = string.Empty;

		public string candidate = string.Empty;

		public bool IsMissing(string test)
		{
			return string.IsNullOrEmpty(test);
		}

		public TestResult(string name, string baseline, string candidate)
		{
			this.name = name;
			this.baseline = baseline;
			this.candidate = candidate;
			passed = baseline == candidate && !IsMissing(baseline) && !IsMissing(candidate);
		}

		public string Details(bool includeOnlyManualHash = false)
		{
			string empty = string.Empty;
			string text = (passed ? "(Passed)" : "(Failed)");
			empty = empty + "Test: " + name + " " + text + "\n\n";
			if (passed)
			{
				empty += "Baseline & Candidate:\n";
				return empty + getTestText(baseline);
			}
			if (IsMissing(baseline))
			{
				empty += "Baseline: Missing";
			}
			else
			{
				empty += "Baseline:\n";
				empty += getTestText(baseline);
			}
			empty += "\n\n";
			if (IsMissing(candidate))
			{
				return empty + "Candidate: Missing";
			}
			empty += "Candidate:\n";
			return empty + getTestText(candidate);
			string getTestText(string testSerialized)
			{
				if (!includeOnlyManualHash)
				{
					return testSerialized;
				}
				int num = testSerialized.IndexOf("Directory State:", StringComparison.Ordinal);
				if (num == -1)
				{
					UnityEngine.Debug.LogWarning("Could not find directory state");
					return testSerialized;
				}
				return testSerialized.Substring(0, num);
			}
		}

		public string Summary()
		{
			string text = "passed";
			if (!passed)
			{
				text = "failed";
				if (IsMissing(baseline))
				{
					text += " (baseline snapshot missing)";
				}
				if (IsMissing(candidate))
				{
					text += " (candidate snapshot missing)";
				}
			}
			return "Test " + name + ": " + text;
		}

		public override string ToString()
		{
			return Summary();
		}
	}

	public class TestResults
	{
		public List<TestResult> tests = new List<TestResult>();

		public string errorMsg = string.Empty;

		public int numberPassed;

		public bool AllPassed()
		{
			return numberPassed == tests.Count;
		}

		public int FailedTestsCount()
		{
			return tests.Count - numberPassed;
		}

		public void LogResults()
		{
			LogSummary();
			string text = "\n========\n======\n=====\n===\n";
			foreach (TestResult test in tests)
			{
				if (!test.passed)
				{
					UnityEngine.Debug.LogError(test.Details() + text);
				}
			}
			foreach (TestResult test2 in tests)
			{
				if (test2.passed)
				{
					UnityEngine.Debug.Log(test2.Details() + text);
				}
			}
		}

		public void LogSummary()
		{
			if (!string.IsNullOrEmpty(errorMsg))
			{
				UnityEngine.Debug.LogError("IO Parity Test Could not be performed due to an error: " + errorMsg);
				return;
			}
			if (AllPassed())
			{
				UnityEngine.Debug.Log($"Passed all IO Parity Tests ({tests.Count})");
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"Not all IO Parity Tests Passed: ({numberPassed}/{tests.Count})");
			foreach (TestResult test in tests)
			{
				if (!test.passed)
				{
					stringBuilder.Append(test.Summary());
					stringBuilder.Append('\n');
				}
			}
			UnityEngine.Debug.Log(stringBuilder.ToString());
		}

		public string Details()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (TestResult test in tests)
			{
				stringBuilder.Append("\n\n========\n========\n\n");
				stringBuilder.Append(test.Details());
			}
			return stringBuilder.ToString();
		}
	}

	public static VisualElement? ResultsDisplayContainerEl;

	public const string ShouldTestInEditorKey = "Should Platform-System Parity Test";

	private const string elementDelimeter = "\n====\n====\n";

	private const string keyValueDelimiter = "::::\n";

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Init()
	{
		ServiceLocator.ManagersInitializedEvent -= OnInitialized;
		ServiceLocator.ManagersInitializedEvent += OnInitialized;
	}

	private static bool ShouldTest()
	{
		if (!Singleton<ServiceLocator>.HasInstance)
		{
			return false;
		}
		if (ServiceLocator.IsInitialized != ServiceLocator.ServiceInitializationStatus.Ready)
		{
			return false;
		}
		if (!ServiceLocator.TryGetService<PlatformService>(out var service))
		{
			return false;
		}
		if (service.InitializationStatus != ServiceLocator.ServiceInitializationStatus.Ready)
		{
			UnityEngine.Debug.LogWarning($"PlatformService is not present but {service.InitializationStatus}. So we cannot test IO parity.");
			return false;
		}
		return false;
	}

	private static async void OnInitialized()
	{
		await Awaitable.NextFrameAsync();
		if (ShouldTest())
		{
			PerformParityTest(out TestResults results);
			if (ResultsDisplayContainerEl != null)
			{
				GenerateUI(ResultsDisplayContainerEl, results);
			}
			results.LogResults();
		}
	}

	private static void PerformParityTest(out TestResults results)
	{
		results = new TestResults();
		if (!Singleton<ServiceLocator>.HasInstance)
		{
			results.errorMsg = "ServiceLocator not initialized, please start the game and let it init before running";
			return;
		}
		if (!LoadSnapshotBaseline(out TestGenerator.Snapshot snapshot))
		{
			results.errorMsg = "Baseline Snapshot not found at " + GetPathSnapshotBaseline() + " so testing cannot be performed";
			return;
		}
		if (!TestGenerator.GenerateTestSnapshot(TestGenerator.GenerateSnapshotCandidate__GENERATED, out var exOut, out var snapshot2))
		{
			results.errorMsg = string.Format("{0} should had an exception thrown at an unexpected point, the rest of the tests are invalidated\n\"{1}\"", "GenerateSnapshotCandidate__GENERATED", exOut);
			return;
		}
		HashSet<string> hashSet = new HashSet<string>();
		hashSet.UnionWith(snapshot.tests.Keys);
		hashSet.UnionWith(snapshot2.tests.Keys);
		foreach (string item in hashSet)
		{
			TestResult testResult = new TestResult(item, snapshot.tests.GetValueOrDefault(item), snapshot2.tests.GetValueOrDefault(item));
			if (testResult.passed)
			{
				results.numberPassed++;
			}
			results.tests.Add(testResult);
		}
	}

	private static void GenerateUI(VisualElement containerEl, TestResults results)
	{
		if (!string.IsNullOrEmpty(results.errorMsg))
		{
			containerEl.Add(new HelpBox(results.errorMsg, HelpBoxMessageType.Error));
			return;
		}
		if (results.AllPassed())
		{
			containerEl.Add(new HelpBox("All Tests passed", HelpBoxMessageType.Info));
		}
		else
		{
			containerEl.Add(new HelpBox($"Not all Tests passed:  ({results.numberPassed}/{results.tests.Count})", HelpBoxMessageType.Warning));
		}
		foreach (TestResult test in results.tests)
		{
			VisualElement visualElement = new VisualElement();
			visualElement.style.marginTop = 2f;
			visualElement.style.marginTop = 2f;
			visualElement.style.marginLeft = 2f;
			visualElement.style.flexDirection = FlexDirection.Row;
			visualElement.tooltip = test.Summary();
			VisualElement visualElement2 = new VisualElement();
			visualElement2.style.width = 32f;
			visualElement2.style.height = 20f;
			visualElement2.style.backgroundColor = (test.passed ? Color.green : Color.red);
			visualElement2.style.marginRight = 2f;
			Label child = new Label(test.Summary());
			Button button = new Button(() =>
			{
				UnityEngine.Debug.Log(test.Details());
			});
			button.text = "Log Results";
			button.tooltip = "This is the entire state of the test. Including full directory state.";
			Button button2 = new Button(() =>
			{
				UnityEngine.Debug.Log(test.Details(includeOnlyManualHash: true));
			});
			button2.text = "Log Manual Hash";
			button2.tooltip = "This excludes full directory state";
			visualElement.Add(visualElement2);
			visualElement.Add(child);
			visualElement.Add(button);
			visualElement.Add(button2);
			containerEl.Add(visualElement);
		}
	}

	public static bool LoadSnapshotBaseline(out TestGenerator.Snapshot? snapshot)
	{
		TestGenerator.Snapshot snapshotClosure = null;
		ExecuteAndDisplayIfErrored(() =>
		{
			string pathSnapshotBaseline = GetPathSnapshotBaseline();
			if (!Deserialize(System.IO.File.ReadAllText(pathSnapshotBaseline), out snapshotClosure))
			{
				throw new Exception("Could not deserialize snapshot from " + pathSnapshotBaseline);
			}
		});
		snapshot = snapshotClosure;
		return snapshot != null;
	}

	private static bool ExecuteAndDisplayIfErrored(Action action)
	{
		try
		{
			action();
			return true;
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError(ex);
			if (ResultsDisplayContainerEl != null)
			{
				ResultsDisplayContainerEl.Add(new HelpBox(ex.Message, HelpBoxMessageType.Error));
			}
			return false;
		}
	}

	private static string Serialize(TestGenerator.Snapshot snapshot)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(snapshot.lastSaved.ToLongDateString());
		stringBuilder.Append("\n====\n====\n");
		foreach (KeyValuePair<string, string> test in snapshot.tests)
		{
			stringBuilder.Append(test.Key);
			stringBuilder.Append("::::\n");
			stringBuilder.Append(test.Value);
			stringBuilder.Append("\n====\n====\n");
		}
		return stringBuilder.ToString();
	}

	private static bool Deserialize(string hashTxt, out TestGenerator.Snapshot snapshot)
	{
		snapshot = new TestGenerator.Snapshot();
		snapshot.tests = new Dictionary<string, string>();
		string[] array = hashTxt.Split("\n====\n====\n", StringSplitOptions.RemoveEmptyEntries);
		if (array.Length < 1)
		{
			return false;
		}
		if (!DateTime.TryParse(array[0], out var result))
		{
			return false;
		}
		snapshot.lastSaved = result;
		for (int i = 1; i < array.Length; i++)
		{
			string[] array2 = array[i].Split("::::\n", 2);
			if (array2.Length != 2)
			{
				UnityEngine.Debug.LogWarning($"Could not parse line {i} of hash text: {array[i]}");
				continue;
			}
			string key = array2[0];
			string value = array2[1];
			snapshot.tests[key] = value;
		}
		return true;
	}

	private static string GetPathTestsFolder()
	{
		return Path.Combine(Application.dataPath, "LaundryBear", "Tests");
	}

	private static string GetPathSnapshotFolder()
	{
		return Path.Combine(Application.streamingAssetsPath, "ParityTests");
	}

	public static string GetPathSnapshotBaseline()
	{
		return Path.Combine(GetPathSnapshotFolder(), "ParityTestSnapshotBaseline.txt");
	}

	public static string GetPathSourceCandidate()
	{
		return Path.Combine(GetPathTestsFolder(), "GenerateSnapshotsCandidate.cs");
	}

	public static string GetPathSourceBaseline()
	{
		return Path.Combine(GetPathTestsFolder(), "GenerateSnapshotsBaseline.cs");
	}

	[Conditional("UNITY_EDITOR")]
	public static void PerformStaticTests()
	{
		TestMatch();
	}

	private static void TestMatch()
	{
		string path = "a.json";
		string path2 = "amal/option.xml";
		string path3 = "amal/option.text";
		string path4 = "bam/option.txt";
		string path5 = "vabam/option.txt";
		string searchPattern = "*option*.xml";
		string searchPattern2 = "*option*";
		string searchPattern3 = "bam/option.txt";
		assertFalse(Path.IsMatch("C:/Users/charl/Desktop/Development/Work/Freehold/Repos/caves-of-qud-console-fork-2/Assets/LaundryBear/Wrappers/Platform.IO.Tests.cs", searchPattern));
		assertFalse(Path.IsMatch(path, searchPattern));
		assertTrue(Path.IsMatch(path2, searchPattern));
		assertFalse(Path.IsMatch(path3, searchPattern));
		assertFalse(Path.IsMatch(path4, searchPattern));
		assertFalse(Path.IsMatch("C:/Users/charl/Desktop/Development/Work/Freehold/Repos/caves-of-qud-console-fork-2/Assets/LaundryBear/Wrappers/Platform.IO.Tests.cs", searchPattern2));
		assertFalse(Path.IsMatch(path, searchPattern2));
		assertTrue(Path.IsMatch(path2, searchPattern2));
		assertTrue(Path.IsMatch(path3, searchPattern2));
		assertTrue(Path.IsMatch(path4, searchPattern2));
		assertFalse(Path.IsMatch("C:/Users/charl/Desktop/Development/Work/Freehold/Repos/caves-of-qud-console-fork-2/Assets/LaundryBear/Wrappers/Platform.IO.Tests.cs", searchPattern3));
		assertFalse(Path.IsMatch(path, searchPattern3));
		assertFalse(Path.IsMatch(path2, searchPattern3));
		assertFalse(Path.IsMatch(path3, searchPattern3));
		assertTrue(Path.IsMatch(path4, searchPattern3));
		assertFalse(Path.IsMatch(path5, searchPattern3));
		static void assertFalse(bool conditional)
		{
			if (conditional)
			{
				UnityEngine.Debug.LogError("Assert False failed");
			}
		}
		static void assertTrue(bool conditional)
		{
			if (!conditional)
			{
				UnityEngine.Debug.LogError("Assert True failed");
			}
		}
	}
}
