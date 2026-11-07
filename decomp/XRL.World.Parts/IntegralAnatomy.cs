using System;
using XRL.World.Anatomy;

namespace XRL.World.Parts;

[Serializable]
public class IntegralAnatomy : IPart
{
	public string IncludeTypes;

	public string ExcludeTypes;

	public bool MortalOnly;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		foreach (BodyPart item in E.Object.Body.LoopParts())
		{
			if (IsApplicable(item))
			{
				item.Integral = true;
			}
		}
		return base.HandleEvent(E);
	}

	public bool IsApplicable(BodyPart Part)
	{
		if (Part.Integral || (MortalOnly && !Part.Mortal))
		{
			return false;
		}
		if (!IncludeTypes.IsNullOrEmpty())
		{
			if (Part.Type == null)
			{
				return false;
			}
			if (!IncludeTypes.CachedCommaExpansion().Contains(Part.Type))
			{
				return false;
			}
		}
		if (Part.Type != null && !ExcludeTypes.IsNullOrEmpty() && ExcludeTypes.CachedCommaExpansion().Contains(Part.Type))
		{
			return false;
		}
		return true;
	}
}
