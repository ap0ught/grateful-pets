using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsPropertyModifier : IPart
{
	public string Props;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ImplantedEvent.ID && ID != UnimplantedEvent.ID && ID != PooledEvent<GetJumpingBehaviorEvent>.ID)
		{
			return ID == PooledEvent<GetPropertyModDescription>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetPropertyModDescription E)
	{
		if (E.Actor == ParentObject.Implantee && ParseProps(Props).TryGetValue(E.Property, out var value))
		{
			E.AddLinearBonusModifier(value, ParentObject.BaseDisplayName);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetJumpingBehaviorEvent E)
	{
		if (E.Actor == ParentObject.Implantee && E.Stats != null && ParseProps(Props).TryGetValue("JumpRangeModifier", out var value))
		{
			E.Stats.AddLinearBonusModifier("Range", value, ParentObject.BaseDisplayName);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		foreach (KeyValuePair<string, int> item in ParseProps(Props))
		{
			E.Implantee.ModIntProperty(item.Key, item.Value, RemoveIfZero: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		foreach (KeyValuePair<string, int> item in ParseProps(Props))
		{
			E.Implantee.ModIntProperty(item.Key, -item.Value, RemoveIfZero: true);
		}
		return base.HandleEvent(E);
	}

	public Dictionary<string, int> ParseProps(string Props)
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		string[] array = Props.Split(';');
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(':');
			dictionary.Add(array2[0], Convert.ToInt32(array2[1]));
		}
		return dictionary;
	}
}
