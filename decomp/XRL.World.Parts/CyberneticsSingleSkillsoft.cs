using System;
using System.Collections.Generic;
using XRL.World.Skills;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsSingleSkillsoft : IPart
{
	public int MinCost;

	public int MaxCost = 50;

	public string Skill;

	public bool Applied;

	public bool AddOn;

	public override bool SameAs(IPart p)
	{
		return false;
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
		if (!Skill.IsNullOrEmpty())
		{
			if (AddOn)
			{
				E.Add("You gain the skill " + SkillFactory.GetSkillOrPowerName(Skill) + ".");
			}
			else
			{
				E.Description = "You gain the skill " + SkillFactory.GetSkillOrPowerName(Skill) + ".";
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		if (!E.Implantee.HasPart(Skill))
		{
			E.Implantee.AddSkill(Skill);
			E.Implantee.SetIntProperty(DependencyKey(), 1);
			Applied = true;
		}
		E.Implantee.ModIntProperty(TrackingKey(), 1);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		E.Implantee.ModIntProperty(TrackingKey(), -1, RemoveIfZero: true);
		if (E.Implantee.GetIntProperty(DependencyKey()) > 0 && E.Implantee.GetIntProperty(TrackingKey()) <= 0)
		{
			E.Implantee?.RemoveSkill(Skill);
		}
		Applied = false;
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

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public string DependencyKey()
	{
		return CyberneticsTreeSkillsoft.DependencyKey(Skill);
	}

	public string TrackingKey()
	{
		return CyberneticsTreeSkillsoft.TrackingKey(Skill);
	}

	public void InitChip(bool ChangeName = true)
	{
		if (Skill != null)
		{
			return;
		}
		List<PowerEntry> powerPool = SkillFactory.GetPowerPool(null, null, MinCost, MaxCost);
		if (powerPool.Count > 0)
		{
			PowerEntry randomElement = powerPool.GetRandomElement();
			if (ChangeName && ParentObject != null)
			{
				ParentObject.Render.DisplayName = "{{Y|Skillsoft [{{W|" + randomElement.Name + "}}]}}";
			}
			Skill = randomElement.Class;
		}
	}
}
