using System;
using System.Text;
using Newtonsoft.Json;

namespace XRL.World.Parts;

[Serializable]
public class Wire : IPart
{
	public string Material = "copper";

	public string NameColor;

	public string ColorString1;

	public string ColorString2;

	public int ColorStringThreshold;

	[NonSerialized]
	private static StringBuilder SB = new StringBuilder();

	[JsonIgnore]
	public int Length => ParentObject?.Count ?? 0;

	public override bool SameAs(IPart p)
	{
		Wire wire = p as Wire;
		if (wire.Material != Material)
		{
			return false;
		}
		if (wire.NameColor != NameColor)
		{
			return false;
		}
		if (wire.ColorString1 != ColorString1)
		{
			return false;
		}
		if (wire.ColorString2 != ColorString2)
		{
			return false;
		}
		if (wire.ColorStringThreshold != ColorStringThreshold)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AfterObjectCreatedEvent.ID && ID != PooledEvent<GetDisplayNameEvent>.ID)
		{
			return ID == PooledEvent<StackCountChangedEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(StackCountChangedEvent E)
	{
		CheckCount(E.NewValue);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterObjectCreatedEvent E)
	{
		CheckCount(ParentObject.Count);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!E.Single && E.Understood())
		{
			SB.Clear();
			if (!string.IsNullOrEmpty(NameColor))
			{
				SB.Append("{{").Append(NameColor).Append('|');
			}
			SB.Append('(').Append(ParentObject.Count).Append("')");
			if (!string.IsNullOrEmpty(NameColor))
			{
				SB.Append("}}");
			}
			E.AddClause(SB.ToString(), -60);
		}
		return base.HandleEvent(E);
	}

	private void CheckCount(int Count)
	{
		if (!string.IsNullOrEmpty(ColorString1) && !string.IsNullOrEmpty(ColorString2) && ParentObject?.Render != null)
		{
			if (Count >= ColorStringThreshold)
			{
				ParentObject.Render.ColorString = ColorString2;
			}
			else
			{
				ParentObject.Render.ColorString = ColorString1;
			}
			ParentObject.Render.IgnoreColorForStack = true;
		}
	}
}
