#define NLOG_ALL
using System;
using System.Collections.Generic;
using XRL.World.Skills;

namespace XRL.World.Parts;

[Serializable]
public class CyberneticsTreeSkillsoft : IPart
{
	public int MinCost = 1;

	public int MaxCost = int.MaxValue;

	public string Skill;

	public bool Applied;

	public string PowersAdded = "";

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
				E.Add("You gain access to the " + SkillFactory.GetSkillOrPowerName(Skill) + " skill tree.");
			}
			else
			{
				E.Description = "You gain access to the " + SkillFactory.GetSkillOrPowerName(Skill) + " skill tree.";
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		PowersAdded = "";
		foreach (PowerEntry power in SkillFactory.Factory.SkillByClass[Skill].PowerList)
		{
			if (power.Class.IsNullOrEmpty())
			{
				continue;
			}
			E.Implantee.ModIntProperty(TrackingKey(power.Class), 1);
			if (E.Implantee.HasPart(power.Class))
			{
				continue;
			}
			try
			{
				E.Implantee.AddSkill(power.Class);
			}
			catch (Exception ex)
			{
				if (power.Class != null)
				{
					Logger.gameLog.Error("Invalid Skillsoft skill class: " + power.Class);
				}
				Logger.Exception(ex);
			}
			if (PowersAdded != "")
			{
				PowersAdded += ",";
			}
			PowersAdded += power.Class;
			E.Implantee.SetIntProperty(DependencyKey(power.Class), 1);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		foreach (PowerEntry power in SkillFactory.Factory.SkillByClass[Skill].PowerList)
		{
			if (!power.Class.IsNullOrEmpty())
			{
				E.Implantee.ModIntProperty(TrackingKey(power.Class), -1, RemoveIfZero: true);
				if (E.Implantee.GetIntProperty(DependencyKey(power.Class)) > 0 && E.Implantee.GetIntProperty(TrackingKey(power.Class)) <= 0)
				{
					E.Implantee.RemoveSkill(power.Class);
				}
			}
		}
		PowersAdded = "";
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		InitChip();
		return base.HandleEvent(E);
	}

	public override void AddedAfterCreation()
	{
		InitChip(ChangeName: false, AddCost: true);
		base.AddedAfterCreation();
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public static string DependencyKey(string Skill)
	{
		return Skill + "DependsOnSkillsoft";
	}

	public static string TrackingKey(string Skill)
	{
		return Skill + "Skillsofts";
	}

	public void InitChip(bool ChangeName = true, bool AddCost = false, double CostFactor = 1.0)
	{
		if (Skill != null)
		{
			return;
		}
		List<SkillEntry> skillPool = SkillFactory.GetSkillPool(null, null, MinCost, MaxCost);
		if (skillPool.Count <= 0)
		{
			return;
		}
		SkillEntry randomElement = skillPool.GetRandomElement();
		Skill = randomElement.Class;
		if (ParentObject == null)
		{
			return;
		}
		if (ChangeName)
		{
			ParentObject.Render.DisplayName = "{{Y|Skillsoft Plus [{{W|" + randomElement.Name + "}}]}}";
		}
		int num = randomElement.Cost;
		foreach (PowerEntry power in randomElement.PowerList)
		{
			num += power.Cost;
		}
		int num2 = ((CostFactor == 1.0) ? (num / 100) : ((int)((double)num * CostFactor / 100.0)));
		if (num2 != 0)
		{
			CyberneticsBaseItem part = ParentObject.GetPart<CyberneticsBaseItem>();
			if (AddCost)
			{
				part.Cost += num2;
			}
			else
			{
				part.Cost = num2;
			}
		}
	}
}
