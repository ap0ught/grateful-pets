using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class CannotBeInfluenced : IActivePart
{
	public string Messages;

	public CannotBeInfluenced()
	{
		WorksOnSelf = true;
		IsBreakageSensitive = false;
		IsRustSensitive = false;
	}

	public override bool SameAs(IPart p)
	{
		if ((p as CannotBeInfluenced).Messages != Messages)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CanBeInfluenced");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanBeInfluenced" && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			if (!string.IsNullOrEmpty(Messages))
			{
				string stringParameter = E.GetStringParameter("Type");
				Dictionary<string, string> dictionary = Messages.CachedDictionaryExpansion();
				string value = null;
				if (stringParameter != null && dictionary.TryGetValue(stringParameter, out value))
				{
					E.SetParameter("Message", value);
				}
				else if (dictionary.TryGetValue("default", out value))
				{
					E.SetParameter("Message", value);
				}
			}
			return false;
		}
		return base.FireEvent(E);
	}
}
