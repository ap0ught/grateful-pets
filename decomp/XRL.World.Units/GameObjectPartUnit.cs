using System;
using ConsoleLib.Console;

namespace XRL.World.Units;

[Serializable]
public class GameObjectPartUnit : GameObjectUnit
{
	public IPart Part;

	public string Description;

	public override void Apply(GameObject Object)
	{
		Object.AddPart(Part.DeepCopy(Object));
	}

	public override void Remove(GameObject Object)
	{
		Object.RemovePart(Part.Name);
	}

	public override void Reset()
	{
		base.Reset();
		Part = null;
	}

	public override bool CanInscribe()
	{
		return !Part.WantEvent(GetShortDescriptionEvent.ID, MinEvent.CascadeLevel);
	}

	public override string GetDescription(bool Inscription = false)
	{
		if (Description == null)
		{
			try
			{
				if (Part.WantEvent(GetShortDescriptionEvent.ID, MinEvent.CascadeLevel))
				{
					GetShortDescriptionEvent E = GetShortDescriptionEvent.FromPool();
					E.AsIfKnown = true;
					E.NoStatus = true;
					Part.HandleEvent(E);
					Description = ColorUtility.StripFormatting(E.Postfix.ToString()).Trim();
					GetShortDescriptionEvent.ResetTo(ref E);
				}
			}
			catch (Exception x)
			{
				Description = $"[DescriptionError: {Part?.GetType()}]";
				MetricsManager.LogException("GameObjectPartUnit.GetShortDescriptionEvent", x);
			}
		}
		return Description;
	}
}
