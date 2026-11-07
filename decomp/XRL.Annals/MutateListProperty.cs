using System;
using System.Collections.Generic;
using System.Linq;
using HistoryKit;

namespace XRL.Annals;

[Serializable]
public class MutateListProperty : HistoricEvent
{
	public string name;

	[NonSerialized]
	public Func<string, string> mutation;

	[NonSerialized]
	public HistoricEntitySnapshot snapshot;

	public MutateListProperty(string _name, Func<string, string> _mutation, HistoricEntitySnapshot _snapshot)
	{
		name = _name;
		mutation = _mutation;
		snapshot = _snapshot;
	}

	public override void Generate()
	{
		List<string> list = snapshot.GetList(name);
		list = list.Select((string s) => mutation(s)).ToList();
		ChangeListProperty(name, snapshot.GetList(name), list);
		duration = 0L;
	}
}
