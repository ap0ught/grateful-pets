using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class ProceduralCookingEffect : Effect
{
	public long StartTick;

	public List<ProceduralCookingEffectUnit> units = new List<ProceduralCookingEffectUnit>();

	public bool bApplied;

	public bool init;

	public ProceduralCookingEffect()
	{
		DisplayName = "{{W|metabolizing}}";
		Duration = 1;
	}

	public override int GetEffectType()
	{
		return 67108868;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDescription()
	{
		return "{{W|metabolizing}}";
	}

	public override string GetDetails()
	{
		return GetProceduralEffectDescription();
	}

	public virtual string GetTemplatedProceduralEffectDescription()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (units.Count > 0)
		{
			for (int i = 0; i < units.Count; i++)
			{
				stringBuilder.Append(units[i].GetTemplatedDescription()).Append("\n");
			}
		}
		return stringBuilder.ToString();
	}

	public virtual string GetProceduralEffectDescription()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (units.Count > 0)
		{
			for (int i = 0; i < units.Count; i++)
			{
				stringBuilder.Append(units[i].GetDescription());
				stringBuilder.Append("\n");
			}
		}
		return stringBuilder.ToString();
	}

	public void AddUnit(ProceduralCookingEffectUnit newUnit)
	{
		newUnit.parent = this;
		units.Add(newUnit);
	}

	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		base.Read(Basis, Reader);
		foreach (ProceduralCookingEffectUnit unit in units)
		{
			unit.parent = this;
		}
	}

	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		base.Write(Basis, Writer);
	}

	public virtual bool SameAs(ProceduralCookingEffect effect)
	{
		if (units.Count != effect.units.Count)
		{
			return false;
		}
		foreach (ProceduralCookingEffectUnit unit in units)
		{
			if (!effect.units.Any((ProceduralCookingEffectUnit e) => e.GetType() == unit.GetType()))
			{
				return false;
			}
		}
		return true;
	}

	public override bool Apply(GameObject Object)
	{
		if (!init)
		{
			throw new Exception("You must call Init() before applying this to an object");
		}
		if (!bApplied)
		{
			StartTick = Calendar.TotalTimeTicks;
			bApplied = true;
			foreach (ProceduralCookingEffectUnit unit in units)
			{
				unit.Apply(Object, this);
			}
		}
		return true;
	}

	public override void Remove(GameObject Object)
	{
		if (!bApplied)
		{
			return;
		}
		bApplied = false;
		foreach (ProceduralCookingEffectUnit unit in units)
		{
			unit.Remove(Object, this);
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<AIBoredEvent>.ID && ID != SingletonEvent<AfterGameLoadedEvent>.ID)
		{
			return ID == ZoneThawedEvent.ID;
		}
		return true;
	}

	public override Effect DeepCopy(GameObject Parent)
	{
		ProceduralCookingEffect proceduralCookingEffect = base.DeepCopy(Parent) as ProceduralCookingEffect;
		proceduralCookingEffect.bApplied = false;
		if (units != null)
		{
			proceduralCookingEffect.units = new List<ProceduralCookingEffectUnit>(units.Count);
			foreach (ProceduralCookingEffectUnit unit in units)
			{
				ProceduralCookingEffectUnit item = unit.DeepCopy(proceduralCookingEffect);
				proceduralCookingEffect.units.Add(item);
			}
		}
		return proceduralCookingEffect;
	}

	public override bool HandleEvent(AIBoredEvent E)
	{
		CheckNonPlayerExpiry();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterGameLoadedEvent E)
	{
		CheckNonPlayerExpiry();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneThawedEvent E)
	{
		CheckNonPlayerExpiry();
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ApplyWellFed");
		Registrar.Register("BecameFamished");
		Registrar.Register("BecameHungry");
		Registrar.Register("ClearFoodEffects");
		Registrar.Register("JoinedPartyLeader");
		Registrar.Register("RemoveProceduralCookingEffects");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BecameHungry" || E.ID == "BecameFamished" || E.ID == "ApplyWellFed" || E.ID == "RemoveProceduralCookingEffects" || E.ID == "ClearFoodEffects")
		{
			Duration = 0;
		}
		else if (E.ID == "JoinedPartyLeader")
		{
			CheckNonPlayerExpiry();
		}
		foreach (ProceduralCookingEffectUnit unit in units)
		{
			unit.FireEvent(E);
		}
		return base.FireEvent(E);
	}

	public void CheckNonPlayerExpiry()
	{
		if (Duration > 0 && Object != null && !Object.IsPlayer() && !Object.WasPlayer() && Calendar.TotalTimeTicks >= StartTick + GetNonPlayerDuration())
		{
			Object.RemoveEffect(this);
		}
	}

	public int GetNonPlayerDuration()
	{
		return (Object?.GetPart<Stomach>())?.CalculateCookingIncrement() ?? 1200;
	}

	public virtual void Init(GameObject target)
	{
		bApplied = false;
		init = true;
		foreach (ProceduralCookingEffectUnit unit in units)
		{
			unit.Init(target);
		}
	}

	private static void ApplyUnitFromTypeToEffect(ProceduralCookingEffect effect, string type)
	{
		GameObjectBlueprint gameObjectBlueprint = GameObjectFactory.Factory.Blueprints["ProceduralCookingIngredient_" + type];
		if (string.IsNullOrEmpty(gameObjectBlueprint.GetTag("Units")))
		{
			Debug.LogError("Tried to get units from a type without units: ProceduralCookingIngredient_" + type);
			return;
		}
		string text = gameObjectBlueprint.GetTag("Units").Split(',').GetRandomElement();
		try
		{
			if (text.StartsWith("@"))
			{
				text = PopulationManager.RollOneFrom(text.Substring(1)).Blueprint;
			}
			effect.AddUnit(Activator.CreateInstance(ModManager.ResolveType("XRL.World.Effects." + text)) as ProceduralCookingEffectUnit);
		}
		catch (Exception ex)
		{
			Debug.LogError("bad action: " + text + " ex:" + ex.ToString());
		}
	}

	private static void ApplyActionsFromTypeToEffect(ProceduralCookingEffectWithTrigger effect, string type)
	{
		GameObjectBlueprint gameObjectBlueprint = GameObjectFactory.Factory.Blueprints["ProceduralCookingIngredient_" + type];
		if (string.IsNullOrEmpty(gameObjectBlueprint.GetTag("Actions")))
		{
			Debug.LogError("Tried to get actions from a type without actions: ProceduralCookingIngredient_" + type);
			return;
		}
		string text = gameObjectBlueprint.GetTag("Actions").Split(',').GetRandomElement();
		try
		{
			if (text.StartsWith("@"))
			{
				text = PopulationManager.RollOneFrom(text.Substring(1)).Blueprint;
			}
			effect.triggeredActions.Add(Activator.CreateInstance(ModManager.ResolveType("XRL.World.Effects." + text)) as ProceduralCookingTriggeredAction);
		}
		catch (Exception ex)
		{
			Debug.LogError("bad triggered action: " + text + " ex:" + ex.ToString());
		}
	}

	private static ProceduralCookingEffectWithTrigger GetTriggeredActionEffectFromType(string type)
	{
		GameObjectBlueprint gameObjectBlueprint = GameObjectFactory.Factory.Blueprints["ProceduralCookingIngredient_" + type];
		if (string.IsNullOrEmpty(gameObjectBlueprint.GetTag("Triggers")))
		{
			Debug.LogError("Tried to get triggers from a type without triggers: ProceduralCookingIngredient_" + type);
			return null;
		}
		string text = gameObjectBlueprint.GetTag("Triggers").Split(',').GetRandomElement();
		try
		{
			if (text.StartsWith("@"))
			{
				text = PopulationManager.RollOneFrom(text.Substring(1)).Blueprint;
			}
			return Activator.CreateInstance(ModManager.ResolveType("XRL.World.Effects." + text)) as ProceduralCookingEffectWithTrigger;
		}
		catch (Exception ex)
		{
			Debug.LogError("bad action with trigger: " + text + " ex:" + ex.ToString());
			return null;
		}
	}

	/// <summary>
	/// Creates a procedural effect from specific units, triggers and actions.
	/// </summary>
	/// <param name="units">All these units are applied.</param>
	/// <param name="triggers">One trigger is selected at random, if supplied and actions are supplied.</param>
	/// <param name="actions">One action is selected at random, if supplied and triggers are supplied.</param>
	/// <returns />
	public static ProceduralCookingEffect CreateSpecific(List<string> units = null, List<string> triggers = null, List<string> actions = null)
	{
		if (triggers != null && actions == null)
		{
			throw new ArgumentException("If either triggers or actions are supplied, both need to be supplied.");
		}
		if (triggers == null && actions != null)
		{
			throw new ArgumentException("If either triggers or actions are supplied, both need to be supplied.");
		}
		ProceduralCookingEffect proceduralCookingEffect = null;
		if (triggers == null)
		{
			proceduralCookingEffect = new ProceduralCookingEffect();
		}
		else
		{
			string text = triggers.GetRandomElement();
			if (text.StartsWith("@"))
			{
				text = PopulationManager.RollOneFrom(text.Substring(1)).Blueprint;
			}
			proceduralCookingEffect = Activator.CreateInstance(ModManager.ResolveType("XRL.World.Effects." + text)) as ProceduralCookingEffectWithTrigger;
		}
		if (units != null)
		{
			foreach (string unit in units)
			{
				try
				{
					string text2 = unit;
					if (text2.StartsWith("@"))
					{
						text2 = PopulationManager.RollOneFrom(text2.Substring(1)).Blueprint;
					}
					proceduralCookingEffect.AddUnit(Activator.CreateInstance(ModManager.ResolveType("XRL.World.Effects." + text2)) as ProceduralCookingEffectUnit);
				}
				catch (Exception ex)
				{
					Debug.LogError("bad action: " + unit + " ex:" + ex.ToString());
				}
			}
		}
		if (actions != null && triggers != null)
		{
			string text3 = actions.GetRandomElement();
			if (text3.StartsWith("@"))
			{
				text3 = PopulationManager.RollOneFrom(text3.Substring(1)).Blueprint;
			}
			(proceduralCookingEffect as ProceduralCookingEffectWithTrigger).triggeredActions.Add(Activator.CreateInstance(ModManager.ResolveType("XRL.World.Effects." + text3)) as ProceduralCookingTriggeredAction);
		}
		return proceduralCookingEffect;
	}

	public static ProceduralCookingEffect CreateJustUnits(List<string> types)
	{
		ProceduralCookingEffect proceduralCookingEffect = new ProceduralCookingEffect();
		foreach (string type in types)
		{
			ApplyUnitFromTypeToEffect(proceduralCookingEffect, type);
		}
		return proceduralCookingEffect;
	}

	public static ProceduralCookingEffectWithTrigger CreateTriggeredAction(string triggerType, string actionType)
	{
		ProceduralCookingEffectWithTrigger triggeredActionEffectFromType = GetTriggeredActionEffectFromType(triggerType);
		ApplyActionsFromTypeToEffect(triggeredActionEffectFromType, actionType);
		return triggeredActionEffectFromType;
	}

	public static ProceduralCookingEffectWithTrigger CreateBaseAndTriggeredAction(string baseType, string triggerType, string actionType)
	{
		ProceduralCookingEffectWithTrigger triggeredActionEffectFromType = GetTriggeredActionEffectFromType(triggerType);
		ApplyActionsFromTypeToEffect(triggeredActionEffectFromType, actionType);
		ApplyUnitFromTypeToEffect(triggeredActionEffectFromType, baseType);
		return triggeredActionEffectFromType;
	}
}
