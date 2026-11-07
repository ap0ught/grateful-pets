using System;
using System.Collections.Generic;

namespace XRL.World.Skills;

[Serializable]
public class SkillEntry : IBaseSkillEntry
{
	public List<PowerEntry> PowerList = new List<PowerEntry>();

	public Dictionary<string, PowerEntry> Powers = new Dictionary<string, PowerEntry>();

	public bool Initiatory
	{
		get
		{
			return Flags.HasBit(4);
		}
		set
		{
			Flags.SetBit(4, value);
		}
	}

	public bool Has(GameObject Object)
	{
		return Object?.HasPart(Class) ?? false;
	}

	public override bool MeetsRequirements(GameObject Object, bool ShowPopup = false)
	{
		foreach (PowerEntry power in PowerList)
		{
			if (power.Cost == 0 && !power.MeetsRequirements(Object, ShowPopup))
			{
				return false;
			}
		}
		return Generic.MeetsRequirements(Object, ShowPopup);
	}

	public void Add(PowerEntry Power)
	{
		PowerList.Add(Power);
		Powers.Add(Power.Name, Power);
	}

	public override void HandleXMLNode(XmlDataHelper Reader)
	{
		Initiatory = Reader.ParseAttribute("Initiatory", Initiatory);
		if (Initiatory)
		{
			Hidden = true;
		}
		base.HandleXMLNode(Reader);
	}
}
