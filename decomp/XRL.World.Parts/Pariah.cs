using System;
using XRL.Language;
using XRL.Wish;

namespace XRL.World.Parts;

[Serializable]
[HasWishCommand]
public class Pariah : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

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
		MakePariah(ParentObject);
		return base.HandleEvent(E);
	}

	public static void MakePariah(GameObject go, bool AlterName = true, bool IsUnique = false)
	{
		if (go.Brain != null)
		{
			if (go.Brain.Allegiance.ContainsKey("Pariahs"))
			{
				return;
			}
			go.Brain.Allegiance.Clear();
			go.Brain.Allegiance.Add("Pariahs", 100);
			go.Brain.Allegiance.Hostile = false;
		}
		if (IsUnique)
		{
			HeroMaker.MakeHero(go);
			AlterName = false;
		}
		if (AlterName)
		{
			if (go.HasProperName)
			{
				go.RequirePart<SocialRoles>().RequireRole(Grammar.MakeTitleCase(go.GetBlueprint().DisplayName()) + " Pariah");
			}
			else
			{
				go.RequirePart<SocialRoles>().RequireRole("pariah to =pronouns.possessive= people");
			}
		}
	}

	[WishCommand("pariah", null)]
	public static void Wish(string Blueprint)
	{
		WishResult wishResult = WishSearcher.SearchForBlueprint(Blueprint);
		GameObject gameObject = GameObjectFactory.Factory.CreateObject(wishResult.Result, 0, 0, null, null, null, "Wish");
		MakePariah(gameObject);
		The.PlayerCell.getClosestEmptyCell().AddObject(gameObject);
	}
}
