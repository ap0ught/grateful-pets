using System;
using System.Collections.Generic;
using System.Linq;
using XRL.Collections;
using XRL.Language;

namespace XRL.World.Parts;

[Serializable]
public class PreparedCookingIngredient : IPart
{
	public string type;

	public string descriptionPostfix;

	public int charges;

	[NonSerialized]
	private static List<string> RandomTypeList = new List<string>(256);

	public override bool SameAs(IPart p)
	{
		if (!(p is PreparedCookingIngredient preparedCookingIngredient))
		{
			return false;
		}
		if (type != preparedCookingIngredient.type)
		{
			return false;
		}
		if (charges != preparedCookingIngredient.charges)
		{
			return false;
		}
		if (descriptionPostfix != preparedCookingIngredient.descriptionPostfix)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != GetShortDescriptionEvent.ID)
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (ParentObject.HasTag("SpecialCookingIngredient"))
		{
			if (charges == 1)
			{
				E.AddTag("{{y|[{{C|1}} cooking serving]}}");
			}
			else if (charges > 1)
			{
				E.AddTag("{{y|[{{C|" + charges + "}} cooking servings]}}");
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(descriptionPostfix);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		List<string> list = type.CachedCommaExpansion();
		List<string> list2 = new List<string>(list.Count);
		foreach (string item in list)
		{
			string tag = GameObjectFactory.Factory.Blueprints["ProceduralCookingIngredient_" + item].GetTag("Description");
			if (!string.IsNullOrEmpty(tag) && !list2.Contains(tag))
			{
				list2.Add(tag);
			}
		}
		descriptionPostfix = "Adds " + Grammar.MakeOrList(list2) + " effects to cooked meals.";
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("SelfRenderStackerDisplay");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "SelfRenderStackerDisplay")
		{
			int num = 0;
			int number = ParentObject.Stacker.Number;
			num = charges;
			if (number > 1)
			{
				if (num == 1)
				{
					E.SetParameter("Display", "[{{C|1 serving] x" + number);
				}
				else
				{
					E.SetParameter("Display", "[{{C|" + num + "}} servings] x" + number);
				}
			}
			else if (num == 1)
			{
				E.SetParameter("Display", "[{{C|1 serving]");
			}
			else
			{
				E.SetParameter("Display", "[{{C|" + num + "}} servings]");
			}
		}
		return base.FireEvent(E);
	}

	public static IReadOnlyList<string> GetRandomTypeList()
	{
		return GetRandomTypeListInternal("RandomWeight");
	}

	public static void GetRandomTypeList(IList<string> FillList)
	{
		GetRandomTypeListInternal("RandomWeight", FillList);
	}

	public static IReadOnlyList<string> GetRandomHighTierTypeList()
	{
		return GetRandomTypeListInternal("RandomHighTierWeight");
	}

	public static void GetRandomHighTierTypeList(IList<string> FillList)
	{
		GetRandomTypeListInternal("RandomHighTierWeight", FillList);
	}

	private static IReadOnlyList<string> GetRandomTypeListInternal(string Tag, IList<string> FillList = null, IReadOnlyList<string> Skip = null)
	{
		IList<string> list = FillList ?? RandomTypeList;
		if (FillList == null)
		{
			list.Clear();
		}
		foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
		{
			if (!blueprint.InheritsFrom("IngredientMapping"))
			{
				continue;
			}
			int num = (int.TryParse(blueprint.GetTag(Tag), out var result) ? result : 0);
			string text = blueprint.GetTag("CookingDomain", null) ?? blueprint.Name.Split('_')[^1];
			if (Skip == null || !Skip.Contains(text))
			{
				while (num > 0)
				{
					list.Add(text);
					num--;
				}
			}
		}
		if (FillList == null)
		{
			return RandomTypeList;
		}
		return null;
	}

	public static string GetRandomType()
	{
		return GetRandomTypeList().GetRandomElement();
	}

	public static string GetRandomHighTierType()
	{
		return GetRandomHighTierTypeList().GetRandomElement();
	}

	[Obsolete("Use GetTypeOptions(ListToFill)")]
	public IReadOnlyList<string> GetTypeOptions()
	{
		if (type == "random")
		{
			return GetRandomTypeList();
		}
		if (type == "randomHighTier")
		{
			return GetRandomHighTierTypeList();
		}
		if (type.Contains(","))
		{
			return new List<string>(type.Split(','));
		}
		return new List<string>(new string[1] { type });
	}

	public void GetTypeOptions(IList<string> FillList)
	{
		if (type == "random")
		{
			GetRandomTypeList(FillList);
		}
		else if (type == "randomHighTier")
		{
			GetRandomHighTierTypeList(FillList);
		}
		else if (type.Contains(","))
		{
			FillList.AddRange(type.Split(','));
		}
		else
		{
			FillList.Add(type);
		}
	}

	public string GetTypeInstance()
	{
		if (type == "random")
		{
			return GetRandomType();
		}
		if (type == "randomHighTier")
		{
			return GetRandomHighTierType();
		}
		if (type.Contains(","))
		{
			return type.Split(',').GetRandomElement();
		}
		return type;
	}

	public string GetTypeInstanceAvoidingList(IReadOnlyList<string> Skip)
	{
		if (type == "random")
		{
			RandomTypeList.Clear();
			GetRandomTypeListInternal("RandomWeight", RandomTypeList, Skip);
			return RandomTypeList.GetRandomElement();
		}
		if (type == "randomHighTier")
		{
			RandomTypeList.Clear();
			GetRandomTypeListInternal("RandomHighTierWeight", RandomTypeList, Skip);
			return RandomTypeList.GetRandomElement();
		}
		if (type.Contains(","))
		{
			using (ScopeDisposedList<string> collection = ScopeDisposedList<string>.GetFromPoolFilledWith(from l in type.Split(',')
				where !Skip.Contains(l)
				select l))
			{
				return collection.GetRandomElement();
			}
		}
		if (Skip.Contains(type))
		{
			return null;
		}
		return type;
	}

	public bool HasTypeOption(string findType)
	{
		if (type == "random")
		{
			return GetRandomTypeList().Contains(findType);
		}
		if (type == "randomHighTier")
		{
			return GetRandomHighTierTypeList().Contains(findType);
		}
		if (type.Contains(","))
		{
			return type.Split(',').Contains(findType);
		}
		return type == findType;
	}
}
