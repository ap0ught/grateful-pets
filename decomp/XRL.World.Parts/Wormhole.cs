using System;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class Wormhole : IPart
{
	public bool bSeen;

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EnteredCell");
		Registrar.Register("ObjectEnteredCell");
		base.Register(Object, Registrar);
	}

	public override bool Render(RenderEvent E)
	{
		if (!bSeen)
		{
			if (ParentObject.CurrentCell != null && ParentObject.CurrentCell.IsVisible())
			{
				ParentObject.DilationSplat();
				bSeen = true;
			}
			return true;
		}
		if (Stat.RandomCosmetic(1, 60) < 3)
		{
			string text = "&C";
			if (Stat.RandomCosmetic(1, 3) == 1)
			{
				text = "&W";
			}
			if (Stat.RandomCosmetic(1, 3) == 1)
			{
				text = "&R";
			}
			if (Stat.RandomCosmetic(1, 3) == 1)
			{
				text = "&B";
			}
			XRLCore.ParticleManager.AddRadial(text + "ù", ParentObject.CurrentCell.X, ParentObject.CurrentCell.Y, Stat.RandomCosmetic(0, 7), Stat.RandomCosmetic(5, 10), 0.01f * (float)Stat.RandomCosmetic(4, 6), -0.01f * (float)Stat.RandomCosmetic(3, 7));
		}
		switch (Stat.RandomCosmetic(0, 4))
		{
		case 0:
			E.ColorString = "&B^k";
			break;
		case 1:
			E.ColorString = "&R^k";
			break;
		case 2:
			E.ColorString = "&C^k";
			break;
		case 3:
			E.ColorString = "&W^k";
			break;
		case 4:
			E.ColorString = "&K^k";
			break;
		}
		switch (Stat.RandomCosmetic(0, 3))
		{
		case 0:
			E.RenderString = "\t";
			break;
		case 1:
			E.RenderString = "é";
			break;
		case 2:
			E.RenderString = "\u0015";
			break;
		case 3:
			E.RenderString = "\u000f";
			break;
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ObjectEnteredCell")
		{
			ParentObject?.PlayWorldSound("Sounds/Abilities/sfx_ability_spacetimeVortex_interact_in");
		}
		else if (E.ID == "EnteredCell")
		{
			ParentObject?.PlayWorldSound("Sounds/Abilities/sfx_ability_spacetimeVortex_interact_out");
		}
		return base.FireEvent(E);
	}
}
