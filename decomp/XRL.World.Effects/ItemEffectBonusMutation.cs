using System;
using UnityEngine;
using XRL.Rules;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class ItemEffectBonusMutation<T> : Effect where T : BaseMutation, new()
{
	public bool wasApplied;

	public string AddedTier = "1";

	public string BonusTier = "0";

	public int Tier = 1;

	public int moddedAmount;

	public string _MutationDisplayName;

	public string MutationClassName;

	public string TrackingProperty;

	public bool mutationWasAdded;

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

	public void Init(GameObject target)
	{
		T val = new T();
		_MutationDisplayName = val.GetDisplayName();
		MutationClassName = val.Name;
		TrackingProperty = "Equipped" + MutationClassName;
		if (!target.HasPart(typeof(T)))
		{
			mutationWasAdded = true;
			Tier = Stat.Roll(AddedTier);
		}
		else
		{
			Tier = Stat.Roll(BonusTier);
		}
	}

	public override int GetEffectType()
	{
		return 2;
	}

	public override string GetDetails()
	{
		if (mutationWasAdded)
		{
			return "Can use " + MutationDisplayName + " at level " + Tier + ".";
		}
		if (Tier == 1)
		{
			return Tier.Signed() + " level to " + MutationDisplayName + ".";
		}
		return Tier.Signed() + " levels to " + MutationDisplayName + ".";
	}

	public override bool Apply(GameObject go)
	{
		Init(go);
		Duration = 3;
		if (!go.HasPart(typeof(T)))
		{
			T val = new T();
			if (!val.CompatibleWith(go))
			{
				return false;
			}
			wasApplied = true;
			mutationWasAdded = true;
			moddedAmount = Tier;
			val.ParentObject = go;
			val.Mutate(go, 0);
			go.AddPart(val);
			go.GetPart<T>().Level = 0;
			go.ModIntProperty(MutationClassName, Tier);
			go.ModIntProperty(TrackingProperty, 1);
		}
		else
		{
			mutationWasAdded = false;
			wasApplied = true;
			if (go.HasIntProperty(TrackingProperty))
			{
				moddedAmount = Tier;
				go.ModIntProperty(TrackingProperty, 1);
				go.ModIntProperty(typeof(T).Name, Tier);
			}
			else
			{
				moddedAmount = Tier;
				go.ModIntProperty(typeof(T).Name, Tier);
			}
		}
		go.RegisterEffectEvent(this, "BeforeMutationAdded");
		go.SyncMutationLevelAndGlimmer();
		return true;
	}

	public override void Remove(GameObject go)
	{
		if (!wasApplied)
		{
			return;
		}
		go.ModIntProperty(typeof(T).Name, -moddedAmount);
		moddedAmount = 0;
		if (go.HasIntProperty(TrackingProperty))
		{
			go.ModIntProperty(TrackingProperty, -1, RemoveIfZero: true);
			if (go.GetIntProperty(TrackingProperty) <= 0 && go.HasPart(typeof(T)))
			{
				go.GetPart<T>().Unmutate(go);
				go.RemovePart(typeof(T));
			}
		}
		go.UnregisterEffectEvent(this, "BeforeMutationAdded");
		go.SyncMutationLevelAndGlimmer();
		wasApplied = false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeMutationAdded")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Object");
			if (E.GetStringParameter("Mutation") == MutationClassName && gameObjectParameter.HasIntProperty(TrackingProperty))
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
				catch (Exception ex)
				{
					Debug.LogError("Exception on ProceduralCookingEffectUnitMutation during BeforeMutationAdded: " + ex.ToString());
				}
			}
		}
		return true;
	}
}
