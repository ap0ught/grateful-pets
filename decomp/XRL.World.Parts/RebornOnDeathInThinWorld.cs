using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class RebornOnDeathInThinWorld : IPart
{
	public bool Reborn;

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EnteredCell");
		Registrar.Register("BeforeTakeAction");
		Registrar.Register("BeforeDie");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeTakeAction" || E.ID == "EnteredCell")
		{
			if (ParentObject.CurrentZone != null && ParentObject.CurrentZone.ZoneWorld != "ThinWorld")
			{
				ParentObject.Obliterate();
			}
			else if (Reborn)
			{
				Reborn = false;
				if (ParentObject.Brain != null)
				{
					ParentObject.Brain.Goals.Clear();
					ParentObject.Brain.Opinions.Clear();
				}
			}
		}
		else if (E.ID == "BeforeDie")
		{
			ParentObject.RestorePristineHealth();
			ParentObject.DilationSplat();
			ParentObject.SmallTeleportSwirl(null, "&C", Voluntary: true);
			if (ParentObject.IsPlayer())
			{
				Popup.Show("Death has no meaning here.");
			}
			else if (ParentObject.Brain != null)
			{
				ParentObject.Brain.Goals.Clear();
				ParentObject.Brain.Opinions.Clear();
				Reborn = true;
			}
			DidX("continue", "being", null, null, null, ParentObject);
			return false;
		}
		return base.FireEvent(E);
	}
}
