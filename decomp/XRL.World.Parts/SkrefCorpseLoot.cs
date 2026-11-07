using System;
using Qud.API;
using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class SkrefCorpseLoot : IPart
{
	public bool bCreated;

	public bool bSeen;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EnteredCell");
		base.Register(Object, Registrar);
	}

	public override bool Render(RenderEvent E)
	{
		if (!bSeen)
		{
			bSeen = true;
			Popup.ShowSpace("You stumble upon some flattened remains.");
			JournalAPI.AddAccomplishment("You stumbled upon some flattened remains.", "On the " + Calendar.GetDay() + " of " + Calendar.GetMonth() + ", a fallen cherub gifted his broken wings to the traveler =name=.", "On the " + Calendar.GetDay() + " of " + Calendar.GetMonth() + ", =name= stumbled upon the flatted remains of a vainglorious humanoid. From that day forth, =name= always kept some mechanical wings on " + The.Player.GetPronounProvider().PossessiveAdjective + " person.", null, "general", MuralCategory.FindsObject, MuralWeight.High, null, -1L);
			Achievement.FIND_APPRENTICE.Unlock();
			JournalAPI.GetMapNote("$skrefcorpse")?.Reveal();
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell")
		{
			if (bCreated)
			{
				return true;
			}
			bCreated = true;
			Cell cell = ParentObject.CurrentCell;
			GameObject gameObject = GameObject.Create("Mechanical Wings");
			gameObject.ApplyEffect(new Broken());
			cell.AddObject(gameObject);
			cell.AddObject(GameObject.Create("Wire Strand 50"));
			if (25.in100())
			{
				cell.AddObject(GameObject.Create("Wire Strand 50"));
			}
			if (25.in100())
			{
				cell.AddObject(GameObject.Create("Wire Strand 20"));
			}
			if (25.in100())
			{
				cell.AddObject(GameObject.Create("Wire Strand 10"));
			}
			int i = 0;
			for (int num = Stat.Random(1, 2); i < num; i++)
			{
				cell.AddObject(PopulationManager.CreateOneFrom("Junk 3"));
			}
			cell.AddObject(PopulationManager.CreateOneFrom("Armor 2"));
			cell.AddObject(PopulationManager.CreateOneFrom("Armor 1"));
			cell.AddObject(PopulationManager.CreateOneFrom("Melee Weapons 2"));
			ParentObject.UnregisterPartEvent(this, "EnteredCell");
			ParentObject.Bloodsplatter();
		}
		return base.FireEvent(E);
	}
}
