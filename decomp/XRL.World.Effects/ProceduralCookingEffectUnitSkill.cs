using System;
using XRL.Rules;
using XRL.World.Parts.Skill;
using XRL.World.Skills;

namespace XRL.World.Effects;

[Serializable]
public class ProceduralCookingEffectUnitSkill<T> : ProceduralCookingEffectUnit where T : BaseSkill, new()
{
	public bool wasApplied;

	public string AddedTier = "1-2";

	public string BonusTier = "1";

	public int Tier = 1;

	public int moddedAmount;

	public string _SkillDisplayName;

	public string TrackingProperty;

	public bool skillWasAdded;

	public string SkillDisplayName
	{
		get
		{
			if (_SkillDisplayName == null)
			{
				T val = new T();
				_SkillDisplayName = SkillFactory.Factory.PowersByClass[val.Name].Name;
			}
			return _SkillDisplayName;
		}
		set
		{
			_SkillDisplayName = value;
		}
	}

	public override void Init(GameObject target)
	{
		T val = new T();
		wasApplied = false;
		_SkillDisplayName = SkillFactory.Factory.PowersByClass[val.Name].Name;
		TrackingProperty = "Equipped" + typeof(T).Name;
		if (!target.HasPart(typeof(T)))
		{
			skillWasAdded = true;
			Tier = Stat.Roll(AddedTier);
		}
		else
		{
			Tier = Stat.Roll(BonusTier);
		}
	}

	public override string GetDescription()
	{
		throw new NotImplementedException();
	}

	public override string GetTemplatedDescription()
	{
		throw new NotImplementedException();
	}

	public override void Apply(GameObject go, Effect parent)
	{
		if (!go.HasPart(typeof(T)))
		{
			T val = new T();
			wasApplied = true;
			skillWasAdded = true;
			moddedAmount = Tier;
			val.ParentObject = go;
			val.AddSkill(go);
			go.AddPart(val);
			go.ModIntProperty(typeof(T).Name, Tier);
			go.ModIntProperty(TrackingProperty, 1);
		}
		else
		{
			skillWasAdded = false;
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
		go.RegisterEffectEvent(parent, "AfterAddSkill");
		go.RegisterEffectEvent(parent, "BeforeAddSkill");
	}

	public override void Remove(GameObject go, Effect parent)
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
				go.GetPart<T>().RemoveSkill(go);
				go.RemovePart(typeof(T));
			}
		}
		go.UnregisterEffectEvent(parent, "AfterAddSkill");
		go.UnregisterEffectEvent(parent, "BeforeAddSkill");
	}

	public override void FireEvent(Event E)
	{
		if (E.ID == "BeforeAddSkill")
		{
			if (E.GetParameter("Skill") is T)
			{
				Remove(E.GetGameObjectParameter("Actor"), parent);
			}
		}
		else if (E.ID == "AfterAddSkill" && E.GetParameter("Skill") is T)
		{
			Apply(E.GetGameObjectParameter("Actor"), parent);
		}
	}
}
