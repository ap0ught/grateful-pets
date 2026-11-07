using System;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class ProceduralCookingEffectUnitMutation<T> : ProceduralCookingEffectUnit where T : BaseMutation, new()
{
	public bool wasApplied;

	public string AddedTier = "1-2";

	public string BonusTier = "2-3";

	public int Tier = 1;

	public int moddedAmount;

	public string _MutationDisplayName;

	public string ClassName;

	public string TrackingProperty;

	public bool mutationWasAdded;

	public Guid modTracker;

	public string MutationDisplayName
	{
		get
		{
			if (_MutationDisplayName == null)
			{
				T val = new T();
				_MutationDisplayName = val.GetDisplayName();
			}
			return _MutationDisplayName;
		}
		set
		{
			_MutationDisplayName = value;
		}
	}

	public override void Init(GameObject target)
	{
		T val = new T();
		wasApplied = false;
		_MutationDisplayName = val.GetDisplayName();
		ClassName = val.Name;
		TrackingProperty = "Equipped" + ClassName;
		if (!target.HasPart(typeof(T)))
		{
			mutationWasAdded = true;
			Tier = AddedTier.RollCached();
		}
		else
		{
			Tier = BonusTier.RollCached();
		}
	}

	public virtual string GetBonusZeroMessage()
	{
		return "No effect.";
	}

	public override string GetDescription()
	{
		if (mutationWasAdded)
		{
			return "Can use " + MutationDisplayName + " at level " + Tier + ".";
		}
		if (Tier == 1)
		{
			return Tier.Signed() + " level to " + MutationDisplayName + ".";
		}
		if (Tier == 0)
		{
			return GetBonusZeroMessage();
		}
		return Tier.Signed() + " levels to " + MutationDisplayName + ".";
	}

	public override string GetTemplatedDescription()
	{
		string text = "Can use " + MutationDisplayName + " at level " + AddedTier + ".";
		if (BonusTier != "0")
		{
			text = text + " If @they already have " + MutationDisplayName + ", it's enhanced by " + BonusTier + " levels.";
		}
		return text;
	}

	public override void Apply(GameObject go, Effect parent)
	{
		wasApplied = true;
		string name = typeof(T).Name;
		if (!go.HasPart(name))
		{
			go.SetIntProperty(TrackingProperty, 1);
		}
		modTracker = go.RequirePart<Mutations>().AddMutationMod(name, null, Tier, Mutations.MutationModifierTracker.SourceType.Cooking, parent.DisplayName);
		go.RegisterEffectEvent(parent, "BeforeMutationAdded");
		go.Body?.RegenerateDefaultEquipment();
		go.SyncMutationLevelAndGlimmer();
	}

	public override void Remove(GameObject go, Effect parent)
	{
		if (wasApplied)
		{
			go.RequirePart<Mutations>().RemoveMutationMod(modTracker);
			go.Body?.RegenerateDefaultEquipment();
			go.UnregisterEffectEvent(parent, "BeforeMutationAdded");
			go.SyncMutationLevelAndGlimmer();
		}
	}

	public override void FireEvent(Event E)
	{
		if (E.ID == "BeforeMutationAdded")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Object");
			if (E.GetStringParameter("Mutation") == ClassName && gameObjectParameter.HasIntProperty(TrackingProperty))
			{
				try
				{
					BaseMutation part = gameObjectParameter.GetPart<T>();
					if (part != null)
					{
						part.Unmutate(gameObjectParameter);
						gameObjectParameter.RemovePart(part);
					}
					gameObjectParameter.RemoveIntProperty(TrackingProperty);
				}
				catch (Exception x)
				{
					MetricsManager.LogError("Exception on ProceduralCookingEffectUnitMutation during BeforeMutationAdded", x);
				}
			}
		}
		base.FireEvent(E);
	}
}
