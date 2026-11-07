using System;
using System.Collections.Generic;
using System.Linq;
using Qud.API;
using XRL.Rules;
using XRL.UI;
using XRL.World.Anatomy;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class FungalSporeInfection : Effect, ITierInitialized
{
	public string InfectionObject = "LuminousInfection";

	public bool Fake;

	public int TurnsLeft;

	public int Damage = 2;

	public GameObject Owner;

	public bool bSpawned;

	public FungalSporeInfection()
	{
		DisplayName = "{{w|itchy skin}}";
		Duration = 1;
	}

	public FungalSporeInfection(int Duration, string infection)
		: this()
	{
		TurnsLeft = Duration;
		InfectionObject = infection;
	}

	public void Initialize(int Tier)
	{
		Duration = Stat.Random(20, 30) * 120;
		TurnsLeft = Duration;
		InfectionObject = SporePuffer.InfectionObjectList.GetRandomElement();
	}

	public override int GetEffectType()
	{
		return 117456900;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		return "Skin flakes and itches. Is something growing there?";
	}

	public override bool Apply(GameObject Object)
	{
		return Object.FireEvent("ApplyFungalSporeInfection");
	}

	public override void Remove(GameObject Object)
	{
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == SingletonEvent<GeneralAmnestyEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GeneralAmnestyEvent E)
	{
		Owner = null;
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ApplyFungalSporeInfection");
		Registrar.Register("EndTurn");
		base.Register(Object, Registrar);
	}

	public static bool BodyPartSuitableForFungalInfection(BodyPart Part, bool IgnoreBodyPartCategory = false)
	{
		if (Part.Abstract)
		{
			return false;
		}
		if (Part.Extrinsic)
		{
			return false;
		}
		if (!Part.Contact)
		{
			return false;
		}
		if (!IgnoreBodyPartCategory && Part.Category != 1 && Part.Category != 2 && Part.Category != 3 && Part.Category != 4 && Part.Category != 5 && Part.Category != 9 && Part.Category != 12 && Part.Category != 13 && Part.Category != 14 && Part.Category != 16 && Part.Category != 7 && Part.Category != 8 && Part.Category != 19 && Part.Category != 18 && Part.Category != 20 && Part.Category != 21)
		{
			return false;
		}
		if (Part.Equipped != null)
		{
			if (Part.Equipped == Part.Cybernetics)
			{
				return false;
			}
			if (!Part.Equipped.CanBeUnequipped(null, null, Forced: false, SemiForced: true))
			{
				return false;
			}
		}
		return true;
	}

	public static bool BodyPartPreferableForFungalInfection(BodyPart Part)
	{
		if (!BodyPartSuitableForFungalInfection(Part))
		{
			return false;
		}
		return Part.Type == "Fungal Outcrop";
	}

	public static bool ChooseLimbForInfection(string FungusName, out BodyPart Target, out string Name)
	{
		return ChooseLimbForInfection(The.Player.Body.GetParts(), FungusName, out Target, out Name);
	}

	public static bool ChooseLimbForInfection(List<BodyPart> Parts, string FungusName, out BodyPart Target, out string Name, bool IgnoreBodyPartCategory = false)
	{
		Target = null;
		Name = null;
		List<BodyPart> infectable = Parts.Where(BodyPartPreferableForFungalInfection).ToList();
		infectable.AddRange(Parts.Where((BodyPart x) => !infectable.Contains(x) && BodyPartSuitableForFungalInfection(x, IgnoreBodyPartCategory)));
		if (infectable.Count == 0)
		{
			Popup.Show("You have no infectable body parts.");
			return false;
		}
		string[] array = infectable.Select((BodyPart x) => x.GetOrdinalName()).ToArray();
		int num = Popup.PickOption("Choose a limb to infect with " + FungusName + ".", null, "", "Sounds/UI/ui_notification", array, null, null, null, null, null, null, 1, 75, 0, -1, AllowEscape: true);
		if (num < 0)
		{
			return false;
		}
		Target = infectable[num];
		Name = array[num];
		return true;
	}

	public static bool ApplyFungalInfection(GameObject Object, string InfectionBlueprint, BodyPart SelectedPart = null)
	{
		if (Object.HasTagOrProperty("ImmuneToFungus"))
		{
			return false;
		}
		if (InfectionBlueprint == "PaxInfection" && (Object.IsPlayer() || Object.HasIntProperty("HasPax")))
		{
			return true;
		}
		Body body = Object.Body;
		if (body == null)
		{
			return true;
		}
		List<BodyPart> list;
		if (SelectedPart == null)
		{
			list = body.GetParts();
			list.ShuffleInPlace();
		}
		else
		{
			list = new List<BodyPart>();
			list.Add(SelectedPart);
		}
		BodyPart bodyPart = null;
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			if (BodyPartPreferableForFungalInfection(list[i]) && (list[i].Equipped == null || list[i].TryUnequip(Silent: false, SemiForced: true)))
			{
				bodyPart = list[i];
				break;
			}
		}
		if (bodyPart == null)
		{
			int j = 0;
			for (int count2 = list.Count; j < count2; j++)
			{
				if (BodyPartSuitableForFungalInfection(list[j]) && (list[j].Equipped == null || list[j].TryUnequip(Silent: false, SemiForced: true)))
				{
					bodyPart = list[j];
					break;
				}
			}
		}
		if (bodyPart == null)
		{
			return false;
		}
		GameObject gameObject = GameObject.Create(InfectionBlueprint);
		gameObject.UsesSlots = bodyPart.VariantType;
		if (bodyPart.SupportsDependent != null)
		{
			foreach (BodyPart part2 in Object.Body.GetParts())
			{
				if (part2 != bodyPart && part2.DependsOn == bodyPart.SupportsDependent && BodyPartSuitableForFungalInfection(part2))
				{
					GameObject equipped = part2.Equipped;
					if (equipped == null || !equipped.HasPropertyOrTag("FungalInfection"))
					{
						gameObject.UsesSlots = gameObject.UsesSlots + "," + part2.VariantType;
					}
					break;
				}
			}
		}
		if (bodyPart.Type == "Hand")
		{
			MeleeWeapon part = gameObject.GetPart<MeleeWeapon>();
			if (InfectionBlueprint == "WaxInfection")
			{
				part.BaseDamage = "2d3";
				part.Skill = "Cudgel";
				part.PenBonus = 0;
				part.MaxStrengthBonus = 999;
			}
			else
			{
				part.BaseDamage = "1d4";
				part.Skill = "Cudgel";
				part.PenBonus = 0;
				part.MaxStrengthBonus = 999;
			}
			gameObject.SetIntProperty("ShowMeleeWeaponStats", 1);
		}
		else if (bodyPart.Type == "Body")
		{
			if (InfectionBlueprint == "WaxInfection")
			{
				gameObject.GetPart<Armor>().AV = 6;
				gameObject.GetPart<Armor>().DV = -12;
				gameObject.GetPart<Armor>().SpeedPenalty = 4;
			}
			else
			{
				gameObject.GetPart<Armor>().AV = 3;
			}
		}
		else if (bodyPart.Type == "Feet" || bodyPart.Type == "Head")
		{
			if (InfectionBlueprint == "WaxInfection")
			{
				gameObject.GetPart<Armor>().AV = 3;
				gameObject.GetPart<Armor>().DV = -3;
				gameObject.GetPart<Armor>().SpeedPenalty = 2;
			}
			else
			{
				gameObject.GetPart<Armor>().AV = 1;
			}
		}
		else if (bodyPart.Type == "Hand" || bodyPart.Type == "Hands")
		{
			if (InfectionBlueprint == "WaxInfection")
			{
				gameObject.GetPart<Armor>().AV = 2;
				gameObject.GetPart<Armor>().DV = -2;
				gameObject.GetPart<Armor>().SpeedPenalty = 2;
			}
			else
			{
				gameObject.GetPart<Armor>().AV = 1;
			}
		}
		else if (InfectionBlueprint == "WaxInfection")
		{
			gameObject.GetPart<Armor>().AV = 2;
			gameObject.GetPart<Armor>().DV = -4;
			gameObject.GetPart<Armor>().SpeedPenalty = 2;
		}
		else
		{
			gameObject.GetPart<Armor>().AV = 1;
		}
		if (!bodyPart.Equip(gameObject, 0, Silent: true, ForDeepCopy: false, Forced: false, SemiForced: true))
		{
			gameObject.Destroy();
			return false;
		}
		if (Object.IsPlayer())
		{
			Object?.PlayWorldSound("sfx_fungalInfection_gain");
			JournalAPI.AddAccomplishment("You contracted " + gameObject.DisplayNameOnly + " on your " + bodyPart.GetOrdinalName() + ", endearing " + Object.itself + " to fungi across Qud.", "Bless the " + Calendar.GetDay() + " of " + Calendar.GetMonth() + ", when =name= cemented a historic alliance with fungi by contracting " + gameObject.ShortDisplayName + " on " + The.Player.GetPronounProvider().PossessiveAdjective + " " + bodyPart.GetOrdinalName() + "!", "While traveling around " + The.Player.CurrentZone.GetTerrainDisplayName() + ", =name= discovered " + JournalAPI.GetLandmarkNearestPlayer().Text + ". There " + The.Player.GetPronounProvider().Subjective + " befriened fungi and contracted " + gameObject.DisplayNameOnly + " on " + The.Player.GetPronounProvider().PossessiveAdjective + " " + bodyPart.GetOrdinalName() + ".", null, "general", MuralCategory.BodyExperienceNeutral, MuralWeight.Medium, null, -1L);
			Popup.Show("You've contracted " + gameObject.DisplayNameOnly + " on your " + bodyPart.GetOrdinalName() + ".");
		}
		if (InfectionBlueprint == "PaxInfection")
		{
			Object.SetIntProperty("HasPax", 1);
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn")
		{
			GameObject.Validate(ref Owner);
			if (Object != null && !Object.FireEvent("ApplySpores"))
			{
				Duration = 0;
				return true;
			}
			if (TurnsLeft % 300 == 0 && !bSpawned && Object != null && Object.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("Your skin itches.");
			}
			if (TurnsLeft > 0)
			{
				TurnsLeft--;
			}
			if (TurnsLeft <= 0 && !bSpawned && Object != null)
			{
				Duration = 0;
				bSpawned = true;
				if (!Fake)
				{
					ApplyFungalInfection(Object, InfectionObject);
				}
			}
		}
		else if (E.ID == "ApplyFungalSporeInfection")
		{
			return false;
		}
		return true;
	}
}
