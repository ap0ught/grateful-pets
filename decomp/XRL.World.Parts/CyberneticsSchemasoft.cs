using System;
using System.Collections.Generic;
using XRL.Language;
using XRL.World.Tinkering;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsSchemasoft : IPart
{
	public int MaxTier = 3;

	public string RecipesAdded;

	public string Category;

	public bool AddOn;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetCyberneticsBehaviorDescriptionEvent>.ID && ID != ImplantedEvent.ID && ID != ObjectCreatedEvent.ID)
		{
			return ID == UnimplantedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetCyberneticsBehaviorDescriptionEvent E)
	{
		if (!Category.IsNullOrEmpty())
		{
			if (AddOn)
			{
				E.Add("You gain access to every schematic of " + GetTierDisplay() + " " + Category + ".");
			}
			else
			{
				E.Description = "You gain access to every schematic of " + GetTierDisplay() + " " + Category + ".";
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		GameObject implantee = E.Implantee;
		if (implantee != null && implantee.IsActivePlayerBody())
		{
			RecipesAdded = "";
			E.Implantee.PlayWorldOrUISound("sfx_characterMod_tinkerSchematic_learn");
			foreach (TinkerData tinkerRecipe in TinkerData.TinkerRecipes)
			{
				if (tinkerRecipe.Category == Category && tinkerRecipe.Tier <= MaxTier && !TinkerData.KnownRecipes.Contains(tinkerRecipe))
				{
					if (RecipesAdded != "")
					{
						RecipesAdded += ",";
					}
					RecipesAdded += tinkerRecipe.Blueprint;
					TinkerData.KnownRecipes.Add(tinkerRecipe);
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		GameObject implantee = E.Implantee;
		if (implantee != null && implantee.IsActivePlayerBody() && !RecipesAdded.IsNullOrEmpty())
		{
			string[] array = RecipesAdded.Split(',');
			foreach (string text in array)
			{
				if (!text.IsNullOrEmpty())
				{
					TinkerData.UnlearnRecipe(text);
				}
			}
			RecipesAdded = "";
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		InitChip();
		return base.HandleEvent(E);
	}

	public override void AddedAfterCreation()
	{
		InitChip(ChangeName: false);
		base.AddedAfterCreation();
	}

	public static string GetTierDisplay(int MaxTier, bool Capitalized = false)
	{
		if (MaxTier >= 6)
		{
			if (!Capitalized)
			{
				return "high tier";
			}
			return "High Tier";
		}
		if (MaxTier >= 4)
		{
			if (!Capitalized)
			{
				return "mid tier";
			}
			return "Mid Tier";
		}
		if (!Capitalized)
		{
			return "low tier";
		}
		return "Low Tier";
	}

	public string GetTierDisplay(bool Capitalized = false)
	{
		return GetTierDisplay(MaxTier, Capitalized);
	}

	public void InitChip(bool ChangeName = true)
	{
		if (Category != null)
		{
			return;
		}
		List<string> list = new List<string>(new string[9] { "ammo and energy cells", "pistols", "rifles", "melee weapons", "grenades", "tonics", "utility", "armor", "heavy weapons" });
		Category = list.GetRandomElement();
		bool flag = false;
		foreach (TinkerData tinkerRecipe in TinkerData.TinkerRecipes)
		{
			if (tinkerRecipe.Category == Category && tinkerRecipe.Type == "Build" && tinkerRecipe.Tier <= MaxTier)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			MetricsManager.LogError("generated empty schemasoft, " + Category + " max tier " + MaxTier);
		}
		if (ChangeName && ParentObject != null)
		{
			ParentObject.Render.DisplayName = "{{Y|Schemasoft [{{C|" + Grammar.MakeTitleCase(Category) + ", " + GetTierDisplay(Capitalized: true) + "}}]}}";
		}
	}
}
