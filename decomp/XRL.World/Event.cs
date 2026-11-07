using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using XRL.Core;
using XRL.UI;
using XRL.World.AI.GoalHandlers;

namespace XRL.World;

[Serializable]
public class Event : IEvent
{
	[NonSerialized]
	private static int PeakEventPool = 0;

	[NonSerialized]
	private static int PeakStringPool = 0;

	[NonSerialized]
	private const int EventPoolSize = 4000;

	[NonSerialized]
	private const int StringPoolInitialSize = 1000;

	private static int pinGameObjectListLocation;

	private static int pinStringLocation;

	private static int pinLocation;

	[NonSerialized]
	public static List<StringBuilder> StringBuilderPool = null;

	[NonSerialized]
	public static int nStringBuilderPoolCounter = 0;

	[NonSerialized]
	public static List<List<GameObject>> GameObjectListPool = new List<List<GameObject>>();

	[NonSerialized]
	public static int nGameObjectListPoolCounter = 0;

	[NonSerialized]
	public static List<List<Cell>> CellListPool = new List<List<Cell>>();

	[NonSerialized]
	public static int nCellListPoolCounter = 0;

	public static bool bWarnedThisClear = false;

	public static Event[] EventPool = null;

	public static int nPoolCounter = 0;

	public static int nExceedCounter = 0;

	public static Dictionary<string, int> TopEvents = new Dictionary<string, int>(200);

	public string ID = "-";

	protected Dictionary<string, object> Parameters;

	protected Dictionary<string, string> StringParameters;

	protected Dictionary<string, int> IntParameters;

	public bool bSilent
	{
		get
		{
			return IsSilent();
		}
		set
		{
			SetSilent(value);
		}
	}

	private bool ExitInterfaceSignal
	{
		get
		{
			return IntParameters.ContainsKey("__ExitInterfaceSignal");
		}
		set
		{
			if (value)
			{
				IntParameters["__ExitInterfaceSignal"] = 1;
			}
			else
			{
				IntParameters.Remove("__ExitInterfaceSignal");
			}
		}
	}

	public static void PinCurrentPool()
	{
		pinLocation = nPoolCounter;
		pinStringLocation = nStringBuilderPoolCounter;
		pinGameObjectListLocation = nGameObjectListPoolCounter;
	}

	public static void ResetToPin()
	{
		for (int i = pinLocation; i < nPoolCounter; i++)
		{
			EventPool[i].Clear();
		}
		nPoolCounter = pinLocation;
		nStringBuilderPoolCounter = pinStringLocation;
		nGameObjectListPoolCounter = pinGameObjectListLocation;
	}

	public static void ResetPool(bool resetMinEventPools = true)
	{
		if (EventPool == null)
		{
			EventPool = new Event[4000];
			for (int i = 0; i < 4000; i++)
			{
				EventPool[i] = new Event(null, 3, 3, 3);
			}
		}
		if (nPoolCounter > PeakEventPool)
		{
			PeakEventPool = nPoolCounter;
		}
		for (int j = 0; j < nPoolCounter; j++)
		{
			EventPool[j].Clear();
		}
		nPoolCounter = 0;
		ResetStringbuilderPool();
		ResetGameObjectListPool();
		ResetCellListPool();
		if (resetMinEventPools)
		{
			MinEvent.ResetPools();
		}
		bWarnedThisClear = false;
		if (nExceedCounter > 0)
		{
			XRLCore.Log("Exceeded by " + nExceedCounter);
			nExceedCounter = 0;
		}
		if (XRLCore.Core != null && XRLCore.Core.Game != null)
		{
			XRLCore.Core.Game.lastFind = null;
			XRLCore.Core.Game.lastFindId = null;
		}
	}

	public static StringBuilder NewStringBuilder(StringBuilder Default)
	{
		if (!XRLCore.IsCoreThread)
		{
			return new StringBuilder().Append(Default);
		}
		if (StringBuilderPool == null)
		{
			StringBuilderPool = new List<StringBuilder>(1000);
			for (int i = 0; i < 1000; i++)
			{
				StringBuilderPool.Add(new StringBuilder(2048));
			}
		}
		while (StringBuilderPool.Count <= nStringBuilderPoolCounter)
		{
			StringBuilderPool.Add(new StringBuilder(2048));
		}
		StringBuilder stringBuilder = StringBuilderPool[nStringBuilderPoolCounter];
		nStringBuilderPoolCounter++;
		stringBuilder.Length = 0;
		if (Default != null)
		{
			stringBuilder.Append(Default);
		}
		return stringBuilder;
	}

	public static StringBuilder NewStringBuilder(string Default = null)
	{
		if (!XRLCore.IsCoreThread)
		{
			return new StringBuilder(Default);
		}
		if (StringBuilderPool == null)
		{
			StringBuilderPool = new List<StringBuilder>(1000);
			for (int i = 0; i < 1000; i++)
			{
				StringBuilderPool.Add(new StringBuilder(2048));
			}
		}
		while (StringBuilderPool.Count <= nStringBuilderPoolCounter)
		{
			StringBuilderPool.Add(new StringBuilder(2048));
		}
		StringBuilder stringBuilder = StringBuilderPool[nStringBuilderPoolCounter];
		nStringBuilderPoolCounter++;
		stringBuilder.Length = 0;
		if (Default != null)
		{
			stringBuilder.Append(Default);
		}
		return stringBuilder;
	}

	public static void ResetStringbuilderPool()
	{
		if (StringBuilderPool == null)
		{
			StringBuilderPool = new List<StringBuilder>(1000);
			for (int i = 0; i < 1000; i++)
			{
				StringBuilderPool.Add(new StringBuilder(2048));
			}
		}
		if (nStringBuilderPoolCounter > PeakStringPool)
		{
			PeakStringPool = nStringBuilderPoolCounter;
		}
		nStringBuilderPoolCounter = 0;
	}

	public static void ResetTo(StringBuilder SB)
	{
		if (StringBuilderPool == null || !XRLCore.IsCoreThread)
		{
			return;
		}
		while (nStringBuilderPoolCounter > 0)
		{
			StringBuilder stringBuilder = StringBuilderPool[--nStringBuilderPoolCounter].Clear();
			if (SB == stringBuilder)
			{
				break;
			}
		}
	}

	public static string FinalizeString(StringBuilder SB)
	{
		string result = SB.ToString();
		ResetTo(SB);
		return result;
	}

	public static List<GameObject> NewGameObjectList()
	{
		while (GameObjectListPool.Count <= nGameObjectListPoolCounter)
		{
			GameObjectListPool.Add(new List<GameObject>(16));
		}
		List<GameObject> list = GameObjectListPool[nGameObjectListPoolCounter];
		nGameObjectListPoolCounter++;
		if (list.Count > 0)
		{
			list.Clear();
		}
		return list;
	}

	public static List<GameObject> NewGameObjectList(List<GameObject> List)
	{
		List<GameObject> list = NewGameObjectList();
		list.AddRange(List);
		return list;
	}

	public static List<GameObject> NewGameObjectList(List<GameObject> List, Predicate<GameObject> Filter)
	{
		if (Filter == null)
		{
			return NewGameObjectList(List);
		}
		List<GameObject> list = NewGameObjectList();
		foreach (GameObject item in List)
		{
			if (Filter(item))
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static List<GameObject> NewGameObjectList(IEnumerable<GameObject> List)
	{
		List<GameObject> list = NewGameObjectList();
		list.AddRange(List);
		return list;
	}

	public static void ResetGameObjectListPool()
	{
		for (int i = 0; i < nGameObjectListPoolCounter; i++)
		{
			if (GameObjectListPool[i].Count > 0)
			{
				GameObjectListPool[i].Clear();
			}
		}
		nGameObjectListPoolCounter = 0;
	}

	public static List<Cell> NewCellList()
	{
		while (CellListPool.Count <= nCellListPoolCounter)
		{
			CellListPool.Add(new List<Cell>(16));
		}
		List<Cell> list = CellListPool[nCellListPoolCounter];
		nCellListPoolCounter++;
		if (list.Count > 0)
		{
			list.Clear();
		}
		return list;
	}

	public static List<Cell> NewCellList(Cell Cell)
	{
		List<Cell> list = NewCellList();
		list.Add(Cell);
		return list;
	}

	public static List<Cell> NewCellList(IEnumerable<Cell> Cells)
	{
		List<Cell> list = NewCellList();
		list.AddRange(Cells);
		return list;
	}

	public static void ResetCellListPool()
	{
		for (int i = 0; i < nCellListPoolCounter; i++)
		{
			if (CellListPool[i].Count > 0)
			{
				CellListPool[i].Clear();
			}
		}
		nCellListPoolCounter = 0;
	}

	public static void ShowTopEvents()
	{
		StringBuilder stringBuilder = NewStringBuilder();
		foreach (KeyValuePair<string, int> item in TopEvents.OrderByDescending((KeyValuePair<string, int> entry) =>
		{
			KeyValuePair<string, int> keyValuePair = entry;
			return keyValuePair.Value;
		}).Take(20).ToDictionary((KeyValuePair<string, int> pair) => pair.Key, (KeyValuePair<string, int> pair) => pair.Value))
		{
			stringBuilder.Append(item.Key).Append(": ").Append(item.Value)
				.Append("\n");
		}
		stringBuilder.Append("(total tracked: ").Append(TopEvents.Count).Append(")");
		Popup.Show(stringBuilder.ToString());
	}

	public static Event New(string ID, int ObjParams = 0, int StrParams = 0, int IntParams = 0)
	{
		if (EventPool == null)
		{
			ResetPool();
		}
		if (TopEvents.ContainsKey(ID))
		{
			TopEvents[ID]++;
		}
		else
		{
			TopEvents.Add(ID, 0);
		}
		if (nPoolCounter >= 4000)
		{
			if (!bWarnedThisClear)
			{
				bWarnedThisClear = true;
				XRLCore.Log("exceeded event pool size " + 4000 + "\n" + new StackTrace(fNeedFileInfo: true).ToString());
			}
			nExceedCounter++;
			return new Event(ID, ObjParams, StrParams, IntParams);
		}
		Event obj = EventPool[nPoolCounter];
		obj.ID = ID;
		if (obj.Parameters.Count > 0)
		{
			obj.Parameters.Clear();
		}
		nPoolCounter++;
		return obj;
	}

	public static Event New(string ID, string Name1, string Value1)
	{
		Event obj = New(ID, 0, 1);
		obj.SetParameterInternal(Name1, Value1);
		return obj;
	}

	public static Event New(string ID, string Name1, string Value1, string Name2, string Value2)
	{
		Event obj = New(ID, 0, 2);
		obj.SetParameterInternal(Name1, Value1);
		obj.SetParameterInternal(Name2, Value2);
		return obj;
	}

	public static Event New(string ID, string Name1, string Value1, string Name2, string Value2, string Name3, string Value3)
	{
		Event obj = New(ID, 0, 3);
		obj.SetParameterInternal(Name1, Value1);
		obj.SetParameterInternal(Name2, Value2);
		obj.SetParameterInternal(Name3, Value3);
		return obj;
	}

	public static Event New(string ID, string Name1, int Value1)
	{
		Event obj = New(ID, 0, 0, 1);
		obj.SetParameterInternal(Name1, Value1);
		return obj;
	}

	public static Event New(string ID, string Name1, int Value1, string Name2, int Value2)
	{
		Event obj = New(ID, 0, 0, 2);
		obj.SetParameterInternal(Name1, Value1);
		obj.SetParameterInternal(Name2, Value2);
		return obj;
	}

	public static Event New(string ID, string Name1, int Value1, string Name2, int Value2, string Name3, int Value3)
	{
		Event obj = New(ID, 0, 0, 3);
		obj.SetParameterInternal(Name1, Value1);
		obj.SetParameterInternal(Name2, Value2);
		obj.SetParameterInternal(Name3, Value3);
		return obj;
	}

	public static Event New(string ID, string Name1, int Value1, string Name2, int Value2, string Name3, int Value3, string Name4, int Value4)
	{
		Event obj = New(ID, 0, 0, 4);
		obj.SetParameterInternal(Name1, Value1);
		obj.SetParameterInternal(Name2, Value2);
		obj.SetParameterInternal(Name3, Value3);
		obj.SetParameterInternal(Name4, Value4);
		return obj;
	}

	public static Event New(string ID, string Name1, object Value1)
	{
		Event obj = New(ID, 1);
		obj.SetParameterInternal(Name1, Value1);
		return obj;
	}

	public static Event New(string ID, string Name1, object Value1, string Name2, object Value2)
	{
		Event obj = New(ID, 2);
		obj.SetParameterInternal(Name1, Value1);
		obj.SetParameterInternal(Name2, Value2);
		return obj;
	}

	public static Event New(string ID, string Name1, object Value1, string Name2, object Value2, string Name3, object Value3)
	{
		Event obj = New(ID, 3);
		obj.SetParameterInternal(Name1, Value1);
		obj.SetParameterInternal(Name2, Value2);
		obj.SetParameterInternal(Name3, Value3);
		return obj;
	}

	public static Event New(string ID, string Name1, object Value1, string Name2, object Value2, string Name3, object Value3, string Name4, object Value4)
	{
		Event obj = New(ID, 4);
		obj.SetParameterInternal(Name1, Value1);
		obj.SetParameterInternal(Name2, Value2);
		obj.SetParameterInternal(Name3, Value3);
		obj.SetParameterInternal(Name4, Value4);
		return obj;
	}

	public static Event New(string ID, string Name1, object Value1, string Name2, object Value2, string Name3, object Value3, string Name4, int Value4)
	{
		Event obj = New(ID, 4);
		obj.SetParameterInternal(Name1, Value1);
		obj.SetParameterInternal(Name2, Value2);
		obj.SetParameterInternal(Name3, Value3);
		obj.SetParameterInternal(Name4, Value4);
		return obj;
	}

	public static Event New(string ID, string Name1, object Value1, string Name2, object Value2, string Name3, object Value3, string Name4, object Value4, string Name5, object Value5)
	{
		Event obj = New(ID, 5);
		obj.SetParameterInternal(Name1, Value1);
		obj.SetParameterInternal(Name2, Value2);
		obj.SetParameterInternal(Name3, Value3);
		obj.SetParameterInternal(Name4, Value4);
		obj.SetParameterInternal(Name5, Value5);
		return obj;
	}

	public static Event New(string ID, string Name1, string Value1, string Name2, int Value2)
	{
		Event obj = New(ID, 0, 1, 1);
		obj.SetParameterInternal(Name1, Value1);
		obj.SetParameterInternal(Name2, Value2);
		return obj;
	}

	public static Event New(string ID, string Name1, int Value1, string Name2, string Value2)
	{
		Event obj = New(ID, 0, 1, 1);
		obj.SetParameterInternal(Name1, Value1);
		obj.SetParameterInternal(Name2, Value2);
		return obj;
	}

	public static Event New(string ID, string Name1, string Value1, string Name2, object Value2)
	{
		Event obj = New(ID, 1, 1);
		obj.SetParameterInternal(Name1, Value1);
		obj.SetParameterInternal(Name2, Value2);
		return obj;
	}

	public static Event New(string ID, string Name1, object Value1, string Name2, string Value2)
	{
		Event obj = New(ID, 1, 1);
		obj.SetParameterInternal(Name1, Value1);
		obj.SetParameterInternal(Name2, Value2);
		return obj;
	}

	public static Event New(string ID, string Name1, int Value1, string Name2, object Value2)
	{
		Event obj = New(ID, 1, 0, 1);
		obj.SetParameterInternal(Name1, Value1);
		obj.SetParameterInternal(Name2, Value2);
		return obj;
	}

	public static Event New(string ID, string Name1, object Value1, string Name2, int Value2)
	{
		Event obj = New(ID, 1, 0, 1);
		obj.SetParameterInternal(Name1, Value1);
		obj.SetParameterInternal(Name2, Value2);
		return obj;
	}

	public static Event New(string ID, string Name1, string Value1, string Name2, int Value2, string Name3, int Value3)
	{
		Event obj = New(ID, 0, 1, 2);
		obj.SetParameterInternal(Name1, Value1);
		obj.SetParameterInternal(Name2, Value2);
		obj.SetParameterInternal(Name3, Value3);
		return obj;
	}

	public static Event New(string ID, string Name1, string Value1, string Name2, int Value2, string Name3, string Value3)
	{
		Event obj = New(ID, 0, 2, 1);
		obj.SetParameterInternal(Name1, Value1);
		obj.SetParameterInternal(Name2, Value2);
		obj.SetParameterInternal(Name3, Value3);
		return obj;
	}

	public static Event New(string ID, string Name1, string Value1, string Name2, string Value2, string Name3, int Value3)
	{
		Event obj = New(ID, 0, 2, 1);
		obj.SetParameterInternal(Name1, Value1);
		obj.SetParameterInternal(Name2, Value2);
		obj.SetParameterInternal(Name3, Value3);
		return obj;
	}

	public static Event New(string ID, string Name1, int Value1, string Name2, string Value2, string Name3, string Value3)
	{
		Event obj = New(ID, 0, 2, 1);
		obj.SetParameterInternal(Name1, Value1);
		obj.SetParameterInternal(Name2, Value2);
		obj.SetParameterInternal(Name3, Value3);
		return obj;
	}

	public static Event New(string ID, string Name1, int Value1, string Name2, int Value2, string Name3, string Value3)
	{
		Event obj = New(ID, 0, 1, 2);
		obj.SetParameterInternal(Name1, Value1);
		obj.SetParameterInternal(Name2, Value2);
		obj.SetParameterInternal(Name3, Value3);
		return obj;
	}

	public static Event New(string ID, string Name1, int Value1, string Name2, string Value2, string Name3, int Value3)
	{
		Event obj = New(ID, 0, 1, 2);
		obj.SetParameterInternal(Name1, Value1);
		obj.SetParameterInternal(Name2, Value2);
		obj.SetParameterInternal(Name3, Value3);
		return obj;
	}

	public static Event New(string ID, string Name1, string Value1, string Name2, object Value2, string Name3, object Value3)
	{
		Event obj = New(ID, 2, 1);
		obj.SetParameterInternal(Name1, Value1);
		obj.SetParameterInternal(Name2, Value2);
		obj.SetParameterInternal(Name3, Value3);
		return obj;
	}

	public static Event New(string ID, string Name1, string Value1, string Name2, string Value2, string Name3, object Value3)
	{
		Event obj = New(ID, 1, 2);
		obj.SetParameterInternal(Name1, Value1);
		obj.SetParameterInternal(Name2, Value2);
		obj.SetParameterInternal(Name3, Value3);
		return obj;
	}

	public static Event New(string ID, string Name1, string Value1, string Name2, string Value2, string Name3, object Value3, string Name4, int Value4)
	{
		Event obj = New(ID, 1, 2, 1);
		obj.SetParameterInternal(Name1, Value1);
		obj.SetParameterInternal(Name2, Value2);
		obj.SetParameterInternal(Name3, Value3);
		obj.SetParameterInternal(Name4, Value4);
		return obj;
	}

	public static Event New(string ID, string Name1, string Value1, string Name2, object Value2, string Name3, string Value3)
	{
		Event obj = New(ID, 1, 2);
		obj.SetParameterInternal(Name1, Value1);
		obj.SetParameterInternal(Name2, Value2);
		obj.SetParameterInternal(Name3, Value3);
		return obj;
	}

	public static Event New(string ID, string Name1, int Value1, string Name2, object Value2, string Name3, object Value3)
	{
		Event obj = New(ID, 2, 0, 1);
		obj.SetParameterInternal(Name1, Value1);
		obj.SetParameterInternal(Name2, Value2);
		obj.SetParameterInternal(Name3, Value3);
		return obj;
	}

	public static Event New(string ID, string Name1, int Value1, string Name2, int Value2, string Name3, object Value3)
	{
		Event obj = New(ID, 1, 0, 2);
		obj.SetParameterInternal(Name1, Value1);
		obj.SetParameterInternal(Name2, Value2);
		obj.SetParameterInternal(Name3, Value3);
		return obj;
	}

	public static Event New(string ID, string Name1, int Value1, string Name2, object Value2, string Name3, int Value3)
	{
		Event obj = New(ID, 1, 0, 2);
		obj.SetParameterInternal(Name1, Value1);
		obj.SetParameterInternal(Name2, Value2);
		obj.SetParameterInternal(Name3, Value3);
		return obj;
	}

	public static Event New(string ID, string Name1, object Value1, string Name2, int Value2, string Name3, int Value3)
	{
		Event obj = New(ID, 1, 0, 2);
		obj.SetParameterInternal(Name1, Value1);
		obj.SetParameterInternal(Name2, Value2);
		obj.SetParameterInternal(Name3, Value3);
		return obj;
	}

	public static Event New(string ID, string Name1, object Value1, string Name2, object Value2, string Name3, int Value3)
	{
		Event obj = New(ID, 2, 0, 1);
		obj.SetParameterInternal(Name1, Value1);
		obj.SetParameterInternal(Name2, Value2);
		obj.SetParameterInternal(Name3, Value3);
		return obj;
	}

	public static Event New(string ID, string Name1, object Value1, string Name2, int Value2, string Name3, object Value3)
	{
		Event obj = New(ID, 2, 0, 1);
		obj.SetParameterInternal(Name1, Value1);
		obj.SetParameterInternal(Name2, Value2);
		obj.SetParameterInternal(Name3, Value3);
		return obj;
	}

	public static Event New(string ID, string Name1, string Value1, string Name2, int Value2, string Name3, object Value3)
	{
		Event obj = New(ID, 1, 1, 1);
		obj.SetParameterInternal(Name1, Value1);
		obj.SetParameterInternal(Name2, Value2);
		obj.SetParameterInternal(Name3, Value3);
		return obj;
	}

	public static Event New(string ID, string Name1, int Value1, string Name2, string Value2, string Name3, object Value3)
	{
		Event obj = New(ID, 1, 1, 1);
		obj.SetParameterInternal(Name1, Value1);
		obj.SetParameterInternal(Name2, Value2);
		obj.SetParameterInternal(Name3, Value3);
		return obj;
	}

	public static Event New(string ID, string Name1, object Value1, string Name2, int Value2, string Name3, string Value3)
	{
		Event obj = New(ID, 1, 1, 1);
		obj.SetParameterInternal(Name1, Value1);
		obj.SetParameterInternal(Name2, Value2);
		obj.SetParameterInternal(Name3, Value3);
		return obj;
	}

	public static Event New(string ID, string Name1, object Value1, string Name2, string Value2, string Name3, int Value3)
	{
		Event obj = New(ID, 1, 1, 1);
		obj.SetParameterInternal(Name1, Value1);
		obj.SetParameterInternal(Name2, Value2);
		obj.SetParameterInternal(Name3, Value3);
		return obj;
	}

	public static Event New(string ID, string Name1, int Value1, string Name2, object Value2, string Name3, string Value3)
	{
		Event obj = New(ID, 1, 1, 1);
		obj.SetParameterInternal(Name1, Value1);
		obj.SetParameterInternal(Name2, Value2);
		obj.SetParameterInternal(Name3, Value3);
		return obj;
	}

	public static Event New(string ID, string Name1, int Value1, string Name2, object Value2, string Name3, string Value3, string Name4, object Value4)
	{
		Event obj = New(ID, 2, 1, 1);
		obj.SetParameterInternal(Name1, Value1);
		obj.SetParameterInternal(Name2, Value2);
		obj.SetParameterInternal(Name3, Value3);
		obj.SetParameterInternal(Name4, Value4);
		return obj;
	}

	public static Event New(string ID, string Name1, int Value1, string Name2, object Value2, string Name3, object Value3, string Name4, string Value4)
	{
		Event obj = New(ID, 2, 1, 1);
		obj.SetParameterInternal(Name1, Value1);
		obj.SetParameterInternal(Name2, Value2);
		obj.SetParameterInternal(Name3, Value3);
		obj.SetParameterInternal(Name4, Value4);
		return obj;
	}

	public static Event New(string ID, string Name1, string Value1, string Name2, object Value2, string Name3, int Value3)
	{
		Event obj = New(ID, 1, 1, 1);
		obj.SetParameterInternal(Name1, Value1);
		obj.SetParameterInternal(Name2, Value2);
		obj.SetParameterInternal(Name3, Value3);
		return obj;
	}

	public static Event New(string ID, string Name1, string Value1, string Name2, string Value2, string Name3, string Value3, string Name4, string Value4)
	{
		Event obj = New(ID, 0, 4);
		obj.SetParameterInternal(Name1, Value1);
		obj.SetParameterInternal(Name2, Value2);
		obj.SetParameterInternal(Name3, Value3);
		obj.SetParameterInternal(Name4, Value4);
		return obj;
	}

	public static Event New(string ID, string Name1, object Value1, string Name2, object Value2, string Name3, int Value3, string Name4, string Value4)
	{
		Event obj = New(ID, 2, 1, 1);
		obj.SetParameterInternal(Name1, Value1);
		obj.SetParameterInternal(Name2, Value2);
		obj.SetParameterInternal(Name3, Value3);
		obj.SetParameterInternal(Name4, Value4);
		return obj;
	}

	public static Event NewSilent(string ID, string Name1, string Value1)
	{
		Event obj = New(ID, 0, 1);
		obj.SetParameterInternal(Name1, Value1);
		obj.bSilent = true;
		return obj;
	}

	public static Event NewSilent(string ID, string Name1, int Value1)
	{
		Event obj = New(ID, 0, 0, 1);
		obj.SetParameterInternal(Name1, Value1);
		obj.bSilent = true;
		return obj;
	}

	public static Event NewSilent(string ID, string Name1, object Value1)
	{
		Event obj = New(ID, 1);
		obj.SetParameterInternal(Name1, Value1);
		obj.bSilent = true;
		return obj;
	}

	public Event(string _ID)
	{
		if (Parameters == null)
		{
			Parameters = new Dictionary<string, object>();
		}
		if (StringParameters == null)
		{
			StringParameters = new Dictionary<string, string>();
		}
		if (IntParameters == null)
		{
			IntParameters = new Dictionary<string, int>();
		}
		ID = _ID;
	}

	public Event(string _ID, int ObjParams, int StrParams, int IntParams)
	{
		if (Parameters == null)
		{
			Parameters = new Dictionary<string, object>(ObjParams);
		}
		if (StringParameters == null)
		{
			StringParameters = new Dictionary<string, string>(StrParams);
		}
		if (IntParameters == null)
		{
			IntParameters = new Dictionary<string, int>(IntParams);
		}
		ID = _ID;
	}

	public Event(string _ID, string ParameterName, string Parameter)
		: this(_ID, 0, 1, 0)
	{
		SetParameterInternal(ParameterName, Parameter);
	}

	public Event(string _ID, string ParameterName, int Parameter)
		: this(_ID, 0, 0, 1)
	{
		SetParameterInternal(ParameterName, Parameter);
	}

	public Event(string _ID, string ParameterName, object Parameter)
		: this(_ID, 1, 0, 0)
	{
		SetParameterInternal(ParameterName, Parameter);
	}

	public Event(string _ID, string ParameterName1, string Parameter1, string ParameterName2, object Parameter2)
		: this(_ID, 1, 1, 0)
	{
		SetParameterInternal(ParameterName1, Parameter1);
		SetParameterInternal(ParameterName2, Parameter2);
	}

	public Event(string _ID, string ParameterName1, string Parameter1, string ParameterName2, int Parameter2)
		: this(_ID, 1, 0, 1)
	{
		SetParameterInternal(ParameterName1, Parameter1);
		SetParameterInternal(ParameterName2, Parameter2);
	}

	public Event(string _ID, string ParameterName1, object Parameter1, string ParameterName2, string Parameter2)
		: this(_ID, 1, 1, 0)
	{
		SetParameterInternal(ParameterName1, Parameter1);
		SetParameterInternal(ParameterName2, Parameter2);
	}

	public Event(string _ID, string ParameterName1, string Parameter1, string ParameterName2, string Parameter2)
		: this(_ID, 0, 2, 0)
	{
		SetParameterInternal(ParameterName1, Parameter1);
		SetParameterInternal(ParameterName2, Parameter2);
	}

	public Event(string _ID, string ParameterName1, int Parameter1, string ParameterName2, string Parameter2)
		: this(_ID, 0, 1, 1)
	{
		SetParameterInternal(ParameterName1, Parameter1);
		SetParameterInternal(ParameterName2, Parameter2);
	}

	public Event(string _ID, string ParameterName1, int Parameter1, string ParameterName2, object Parameter2)
		: this(_ID, 1, 0, 1)
	{
		SetParameterInternal(ParameterName1, Parameter1);
		SetParameterInternal(ParameterName2, Parameter2);
	}

	public Event(string _ID, string ParameterName1, object Parameter1, string ParameterName2, int Parameter2)
		: this(_ID, 1, 0, 1)
	{
		SetParameterInternal(ParameterName1, Parameter1);
		SetParameterInternal(ParameterName2, Parameter2);
	}

	public Event(string _ID, string ParameterName1, int Parameter1, string ParameterName2, int Parameter2)
		: this(_ID, 0, 0, 2)
	{
		SetParameterInternal(ParameterName1, Parameter1);
		SetParameterInternal(ParameterName2, Parameter2);
	}

	public Event(string _ID, string ParameterName1, int Parameter1, string ParameterName2, int Parameter2, string ParameterName3, int Parameter3)
		: this(_ID, 0, 0, 3)
	{
		SetParameterInternal(ParameterName1, Parameter1);
		SetParameterInternal(ParameterName2, Parameter2);
		SetParameterInternal(ParameterName3, Parameter3);
	}

	public Event(string _ID, string ParameterName1, int Parameter1, string ParameterName2, int Parameter2, string ParameterName3, int Parameter3, string ParameterName4, int Parameter4)
		: this(_ID, 0, 0, 4)
	{
		SetParameterInternal(ParameterName1, Parameter1);
		SetParameterInternal(ParameterName2, Parameter2);
		SetParameterInternal(ParameterName3, Parameter3);
		SetParameterInternal(ParameterName4, Parameter4);
	}

	public Event(string _ID, string ParameterName1, int Parameter1, string ParameterName2, string Parameter2, string ParameterName3, object Parameter3)
		: this(_ID, 1, 1, 1)
	{
		SetParameterInternal(ParameterName1, Parameter1);
		SetParameterInternal(ParameterName2, Parameter2);
		SetParameterInternal(ParameterName3, Parameter3);
	}

	public Event(string _ID, string ParameterName1, int Parameter1, string ParameterName2, object Parameter2, string ParameterName3, object Parameter3)
		: this(_ID, 2, 0, 1)
	{
		SetParameterInternal(ParameterName1, Parameter1);
		SetParameterInternal(ParameterName2, Parameter2);
		SetParameterInternal(ParameterName3, Parameter3);
	}

	public Event(string _ID, string ParameterName1, object Parameter1, string ParameterName2, object Parameter2)
		: this(_ID, 2, 0, 0)
	{
		SetParameterInternal(ParameterName1, Parameter1);
		SetParameterInternal(ParameterName2, Parameter2);
	}

	public Event(string _ID, string ParameterName1, object Parameter1, string ParameterName2, object Parameter2, string ParameterName3, string Parameter3)
		: this(_ID, 2, 1, 0)
	{
		SetParameterInternal(ParameterName1, Parameter1);
		SetParameterInternal(ParameterName2, Parameter2);
		SetParameterInternal(ParameterName3, Parameter3);
	}

	public Event(string _ID, string ParameterName1, object Parameter1, string ParameterName2, string Parameter2, string ParameterName3, object Parameter3)
		: this(_ID, 2, 1, 0)
	{
		SetParameterInternal(ParameterName1, Parameter1);
		SetParameterInternal(ParameterName2, Parameter2);
		SetParameterInternal(ParameterName3, Parameter3);
	}

	public Event(string _ID, string ParameterName1, object Parameter1, string ParameterName2, int Parameter2, string ParameterName3, int Parameter3)
		: this(_ID, 1, 0, 2)
	{
		SetParameterInternal(ParameterName1, Parameter1);
		SetParameterInternal(ParameterName2, Parameter2);
		SetParameterInternal(ParameterName3, Parameter3);
	}

	public Event(string _ID, string ParameterName1, object Parameter1, string ParameterName2, object Parameter2, string ParameterName3, object Parameter3)
		: this(_ID, 3, 0, 0)
	{
		SetParameterInternal(ParameterName1, Parameter1);
		SetParameterInternal(ParameterName2, Parameter2);
		SetParameterInternal(ParameterName3, Parameter3);
	}

	public Event(string _ID, string ParameterName1, object Parameter1, string ParameterName2, object Parameter2, string ParameterName3, object Parameter3, string ParameterName4, object Parameter4)
		: this(_ID, 4, 0, 0)
	{
		SetParameterInternal(ParameterName1, Parameter1);
		SetParameterInternal(ParameterName2, Parameter2);
		SetParameterInternal(ParameterName3, Parameter3);
		SetParameterInternal(ParameterName4, Parameter4);
	}

	public Event(string _ID, string ParameterName1, object Parameter1, string ParameterName2, string Parameter2, string ParameterName3, int Parameter3, string ParameterName4, int Parameter4)
		: this(_ID, 1, 1, 2)
	{
		SetParameterInternal(ParameterName1, Parameter1);
		SetParameterInternal(ParameterName2, Parameter2);
		SetParameterInternal(ParameterName3, Parameter3);
		SetParameterInternal(ParameterName4, Parameter4);
	}

	public virtual bool HasFlag(string Name)
	{
		if (IntParameters.TryGetValue(Name, out var value))
		{
			return value > 0;
		}
		return false;
	}

	public virtual Event SetFlag(string Name, bool State)
	{
		int value2;
		if (State)
		{
			if (IntParameters.TryGetValue(Name, out var value))
			{
				if (value <= 0)
				{
					IntParameters[Name] = 1;
				}
			}
			else
			{
				IntParameters.Add(Name, 1);
			}
		}
		else if (IntParameters.TryGetValue(Name, out value2) && value2 > 0)
		{
			IntParameters.Remove(Name);
		}
		return this;
	}

	public virtual Event SetSilent(bool Silent)
	{
		return SetFlag("IsSilent", Silent);
	}

	public bool IsSilent()
	{
		return HasFlag("IsSilent");
	}

	public virtual void Clear()
	{
		Parameters.Clear();
		StringParameters.Clear();
		IntParameters.Clear();
	}

	public bool HasParameter(string Name)
	{
		if (Parameters.ContainsKey(Name))
		{
			return true;
		}
		if (StringParameters.ContainsKey(Name))
		{
			return true;
		}
		if (IntParameters.ContainsKey(Name))
		{
			return true;
		}
		return false;
	}

	public bool HasObjectParameter(string Name)
	{
		return Parameters.ContainsKey(Name);
	}

	public bool HasStringParameter(string Name)
	{
		return StringParameters.ContainsKey(Name);
	}

	public bool HasIntParameter(string Name)
	{
		return IntParameters.ContainsKey(Name);
	}

	public int GetIntParameter(string Name, int Default = 0)
	{
		if (!IntParameters.TryGetValue(Name, out var value))
		{
			if (Parameters.TryGetValue(Name, out var value2))
			{
				return Convert.ToInt32(value2);
			}
			if (StringParameters.TryGetValue(Name, out var value3))
			{
				return Convert.ToInt32(value3);
			}
			return Default;
		}
		return value;
	}

	public bool TryGetStringParameter(string Key, out string Value)
	{
		return StringParameters.TryGetValue(Key, out Value);
	}

	public string GetStringParameter(string Name, string Default = null)
	{
		if (!StringParameters.TryGetValue(Name, out var value))
		{
			if (Parameters.TryGetValue(Name, out var value2))
			{
				return (string)value2;
			}
			if (IntParameters.TryGetValue(Name, out var value3))
			{
				return value3.ToString();
			}
			return Default;
		}
		return value;
	}

	public float GetFloatParameter(string Name, float Default = 0f)
	{
		if (!IntParameters.TryGetValue(Name, out var value))
		{
			if (Parameters.TryGetValue(Name, out var value2))
			{
				return Convert.ToSingle(value2);
			}
			if (StringParameters.TryGetValue(Name, out var value3))
			{
				return Convert.ToSingle(value3);
			}
			return Default;
		}
		return BitConverter.Int32BitsToSingle(value);
	}

	public GameObject GetGameObjectParameter(string Name)
	{
		if (Parameters.TryGetValue(Name, out var value) && value is GameObject result)
		{
			return result;
		}
		return null;
	}

	public object GetParameter(string Name)
	{
		if (Parameters.TryGetValue(Name, out var value))
		{
			return value;
		}
		if (StringParameters.TryGetValue(Name, out var value2))
		{
			return value2;
		}
		if (IntParameters.TryGetValue(Name, out var value3))
		{
			return value3;
		}
		return null;
	}

	public T GetParameter<T>()
	{
		string name = typeof(T).Name;
		if (Parameters.TryGetValue(name, out var value) && value is T)
		{
			return (T)value;
		}
		if (StringParameters.TryGetValue(name, out var value2) && value2 is T)
		{
			return (T)(object)((value2 is T) ? value2 : null);
		}
		if (IntParameters.TryGetValue(name, out var value3) && value3 is T)
		{
			return (T)((((object)value3) is T) ? ((object)value3) : null);
		}
		return default;
	}

	public T GetParameter<T>(string Name)
	{
		if (Parameters.TryGetValue(Name, out var value) && value is T)
		{
			return (T)value;
		}
		if (StringParameters.TryGetValue(Name, out var value2) && value2 is T)
		{
			return (T)(object)((value2 is T) ? value2 : null);
		}
		if (IntParameters.TryGetValue(Name, out var value3) && value3 is T)
		{
			return (T)((((object)value3) is T) ? ((object)value3) : null);
		}
		return default;
	}

	public T GetParameter<T>(string Name, T Default)
	{
		if (Parameters.TryGetValue(Name, out var value) && value is T)
		{
			return (T)value;
		}
		if (StringParameters.TryGetValue(Name, out var value2) && value2 is T)
		{
			return (T)(object)((value2 is T) ? value2 : null);
		}
		if (IntParameters.TryGetValue(Name, out var value3) && value3 is T)
		{
			return (T)((((object)value3) is T) ? ((object)value3) : null);
		}
		return Default;
	}

	protected Event SetParameterInternal(string Name, string Value)
	{
		StringParameters[Name] = Value;
		return this;
	}

	protected Event SetParameterInternal(string Name, int Value)
	{
		IntParameters[Name] = Value;
		return this;
	}

	protected Event SetParameterInternal(string Name, object Value)
	{
		if (Value is int)
		{
			SetParameterInternal(Name, (int)Value);
		}
		if (Value is string)
		{
			SetParameterInternal(Name, (string)Value);
		}
		Parameters[Name] = Value;
		return this;
	}

	public virtual Event SetParameter(string Name, object Value)
	{
		Parameters[Name] = Value;
		return this;
	}

	public virtual Event SetParameter(string Name, string Value)
	{
		StringParameters[Name] = Value;
		return this;
	}

	public virtual Event SetParameter(string Name, int Value)
	{
		IntParameters[Name] = Value;
		return this;
	}

	public virtual Event SetFloatParameter(string Name, float Value)
	{
		IntParameters[Name] = BitConverter.SingleToInt32Bits(Value);
		return this;
	}

	public virtual Event AddParameter(string Name, string Value)
	{
		StringParameters[Name] = Value;
		return this;
	}

	public virtual Event AddParameter(string Name, int Value)
	{
		IntParameters[Name] = Value;
		return this;
	}

	public virtual Event AddParameter(string Name, object Value)
	{
		Parameters[Name] = Value;
		return this;
	}

	public virtual Event ModParameter(string Name, string Value)
	{
		StringParameters[Name] = GetStringParameter(Name, "") + Value;
		return this;
	}

	public virtual Event ModParameter(string Name, int Value)
	{
		IntParameters[Name] = GetIntParameter(Name) + Value;
		return this;
	}

	public virtual Event ModParameter(string Name, float Value)
	{
		SetFloatParameter(Name, GetFloatParameter(Name) + Value);
		return this;
	}

	public Event Copy(string _ID = null)
	{
		Event obj = New(_ID ?? ID);
		foreach (KeyValuePair<string, object> parameter in Parameters)
		{
			obj.Parameters.Add(parameter.Key, parameter.Value);
		}
		foreach (KeyValuePair<string, string> stringParameter in StringParameters)
		{
			obj.StringParameters.Add(stringParameter.Key, stringParameter.Value);
		}
		foreach (KeyValuePair<string, int> intParameter in IntParameters)
		{
			obj.IntParameters.Add(intParameter.Key, intParameter.Value);
		}
		return obj;
	}

	public virtual void RequestInterfaceExit()
	{
		ExitInterfaceSignal = true;
	}

	public bool InterfaceExitRequested()
	{
		return ExitInterfaceSignal;
	}

	public void PreprocessChildEvent(IEvent E)
	{
		if (ID == "TakeDamage" && E is IDamageEvent damageEvent && HasFlag("WillUseOutcomeMessageFragment"))
		{
			damageEvent.WillUseOutcomeMessageFragment = true;
			damageEvent.OutcomeMessageFragment = GetStringParameter("OutcomeMessageFragment");
		}
	}

	public void ProcessChildEvent(IEvent E)
	{
		if (E.InterfaceExitRequested())
		{
			ExitInterfaceSignal = true;
		}
		if (ID == "TakeDamage" && E is IDamageEvent { WillUseOutcomeMessageFragment: not false, OutcomeMessageFragment: not null } damageEvent)
		{
			SetParameter("OutcomeMessageFragment", damageEvent.OutcomeMessageFragment);
		}
	}

	public void ProcessChildEvent(Event E)
	{
		if (E.ExitInterfaceSignal)
		{
			ExitInterfaceSignal = true;
		}
		SetFlag("DidSpecialEffect", E.HasFlag("DidSpecialEffect"));
	}

	public void AddAICommand(string Command, int Priority = 1, GameObject Object = null, bool Inv = false, bool Self = false, GameObject TargetOverride = null)
	{
		if (!(GetParameter("List") is List<AICommandList> list))
		{
			UnityEngine.Debug.Log("AddAICommand() called on " + ID + " event with no List parameter");
			return;
		}
		AICommandList item = new AICommandList(Command, Priority, Object, Inv, Self, TargetOverride);
		if (Priority > list.Capacity)
		{
			list.Capacity += Priority;
		}
		for (int i = 0; i < Priority; i++)
		{
			list.Add(item);
		}
	}

	public bool AddInventoryAction(string Name, string Display = null, string Command = null, char Key = ' ', bool FireOnActor = false, int Default = 0, bool Override = false, bool WorksAtDistance = false, bool WorksTelekinetically = false, bool WorksTelepathically = false, bool AsMinEvent = false, GameObject FireOn = null)
	{
		if (!(GetParameter("Actions") is EventParameterGetInventoryActions eventParameterGetInventoryActions))
		{
			UnityEngine.Debug.Log("AddInventoryAction() called on " + ID + " event with no Actions parameter");
			return false;
		}
		return eventParameterGetInventoryActions.AddAction(Name, Key, FireOnActor, Display, Command, null, Default, 0, Override, WorksAtDistance, WorksTelekinetically, WorksTelepathically, AsMinEvent, FireOn);
	}

	public bool AddInventoryAction(string Name, char Key, bool FireOnActor, string Display, string Command, int Default = 0, bool Override = false, bool WorksAtDistance = false, bool WorksTelekinetically = false, bool WorksTelepathically = false, bool AsMinEvent = false, GameObject FireOn = null)
	{
		return AddInventoryAction(Name, Display, Command, Key, FireOnActor, Default, Override, WorksAtDistance, WorksTelekinetically, WorksTelepathically, AsMinEvent, FireOn);
	}

	public void AppendToStringBuilder(string Name, string Text)
	{
		if (!(GetParameter(Name) is StringBuilder stringBuilder))
		{
			UnityEngine.Debug.Log("AppendToStringBuilder() trying to append to " + Name + ", not found");
		}
		else
		{
			stringBuilder.Append(Text);
		}
	}

	public bool ActuateOn(GameObject obj)
	{
		return obj.FireEvent(this);
	}
}
