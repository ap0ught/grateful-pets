using System;
using XRL.UI;
using XRL.World.Parts.Skill;
using XRL.World.Skills;

namespace XRL.World.Parts;

[Serializable]
public class TrainingBook : IPart
{
	public const string SKL_CONTEXT = "TrainingBook";

	public string Attribute;

	public string Skill;

	public override bool SameAs(IPart p)
	{
		TrainingBook trainingBook = p as TrainingBook;
		if (trainingBook.Attribute != Attribute)
		{
			return false;
		}
		if (trainingBook.Skill != Skill)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override void Initialize()
	{
		base.Initialize();
		ParentObject.SetStringProperty("BookID", ParentObject.ID);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<AIBoredEvent>.ID && ID != GetInventoryActionsEvent.ID && ID != GetShortDescriptionEvent.ID && ID != PooledEvent<HasBeenReadEvent>.ID && ID != InventoryActionEvent.ID)
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(HasBeenReadEvent E)
	{
		if (HasRead(E.Actor))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIBoredEvent E)
	{
		if (!HasRead(E.Actor))
		{
			InventoryActionEvent.Check(ParentObject, E.Actor, ParentObject, "Read");
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (!Attribute.IsNullOrEmpty())
		{
			E.Postfix.AppendRules("Increases the " + Attribute + " of anyone who reads " + ParentObject.them + ".");
		}
		if (!Skill.IsNullOrEmpty())
		{
			PowerEntry value2;
			if (SkillFactory.Factory.SkillByClass.TryGetValue(Skill, out var value))
			{
				if (value.Initiatory)
				{
					E.Postfix.AppendRules("Allows anyone who reads " + ParentObject.them + " to initiate themselves in " + value.Name + ".");
				}
				else
				{
					E.Postfix.AppendRules("Teaches " + value.Name + " to anyone who reads " + ParentObject.them + ".");
				}
			}
			else if (SkillFactory.Factory.PowersByClass.TryGetValue(Skill, out value2))
			{
				E.Postfix.AppendRules("Teaches " + value2.Name + " to anyone who reads " + ParentObject.them + ".");
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Read", "read", "Read", null, 'r', FireOnActor: false, (!Skill.IsNullOrEmpty() && !E.Actor.HasSkill(Skill)) ? 100 : 15, 0, Override: true);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Read")
		{
			string readKey = GetReadKey();
			if (!E.Actor.IsPlayer())
			{
				IComponent<GameObject>.XDidYToZ(E.Actor, "read", ParentObject);
			}
			if (!Attribute.IsNullOrEmpty() && !HasRead(E.Actor, readKey) && E.Actor.HasStat(Attribute))
			{
				if (E.Actor.IsPlayer())
				{
					Popup.Show("Your " + Attribute + " is increased by {{G|1}}!");
				}
				E.Actor.GetStat(Attribute).BaseValue++;
			}
			if (!Skill.IsNullOrEmpty())
			{
				PowerEntry value2;
				if (SkillFactory.Factory.SkillByClass.TryGetValue(Skill, out var value))
				{
					if (value.Initiatory)
					{
						if (!HasRead(E.Actor, readKey))
						{
							IBaseSkillEntry skillFor = BaseInitiatorySkill.GetSkillFor(E.Actor, value);
							if (skillFor == null)
							{
								if (E.Actor.IsPlayer())
								{
									BaseInitiatorySkill.ShowCompletedPopup(E.Actor, ParentObject, value, "TrainingBook");
								}
							}
							else
							{
								E.Actor.AddSkill(skillFor.Class, ParentObject, "TrainingBook");
							}
						}
						else if (E.Actor.IsPlayer())
						{
							BaseInitiatorySkill.ShowExpendedPopup(E.Actor, ParentObject, value, "TrainingBook");
						}
					}
					else if (!E.Actor.HasSkill(value.Class) && !HasRead(E.Actor, readKey))
					{
						E.Actor.AddSkill(value.Class, ParentObject, "TrainingBook");
					}
				}
				else if (SkillFactory.Factory.PowersByClass.TryGetValue(Skill, out value2) && !E.Actor.HasSkill(value2.Class) && !HasRead(E.Actor, readKey))
				{
					E.Actor.AddSkill(value2.Class, ParentObject, "TrainingBook");
				}
			}
			E.Actor.SetIntProperty(readKey, 1);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (Skill.IsNullOrEmpty() && Attribute.IsNullOrEmpty())
		{
			AssignRandomTraining();
		}
		return base.HandleEvent(E);
	}

	public string GetReadKey()
	{
		return "HasReadBook_" + ParentObject.GetStringProperty("BookID");
	}

	public bool HasRead(GameObject who, string ReadKey)
	{
		return who.GetIntProperty(ReadKey) > 0;
	}

	public bool HasRead(GameObject who)
	{
		return HasRead(who, GetReadKey());
	}

	public bool AssignRandomTraining()
	{
		if (70.in100())
		{
			PowerEntry randomElement = SkillFactory.GetPowerPool().GetRandomElement();
			if (randomElement.ParentSkill != null && (randomElement.Cost <= 0 || randomElement.ParentSkill.Initiatory))
			{
				Skill = randomElement.ParentSkill.Class;
			}
			else
			{
				Skill = randomElement.Class;
			}
		}
		else
		{
			Attribute = Statistic.Attributes.GetRandomElement();
		}
		return true;
	}
}
