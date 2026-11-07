using System;
using XRL.World.Anatomy;
using XRL.World.Parts;
using XRL.World.Quests.GolemQuest;

namespace XRL.World.Units;

[Serializable]
public class GameObjectBodyPartUnit : GameObjectUnit
{
	public string Type;

	public string Manager;

	public string InsertAfter;

	public string OrInsertBefore;

	public int Category = -1;

	public int Laterality;

	public bool Metachromed;

	public override void Apply(GameObject Object)
	{
		BodyPart bodyPart = Object.Body?.GetBody();
		if (bodyPart == null)
		{
			return;
		}
		BodyPart bodyPart2 = ((!(Type == "Random")) ? bodyPart.AddPartAt(Type, Manager: Manager, InsertAfter: InsertAfter, OrInsertBefore: OrInsertBefore, Laterality: Laterality, DefaultBehavior: null, SupportsDependent: null, DependsOn: null, RequiresType: null, Category: (Category == -1) ? bodyPart.Category : Category) : Object.RequirePart<Mutations>().AddChimericBodyPart(Silent: true, Manager));
		if (Metachromed && bodyPart2 != null)
		{
			string text = GolemQuestSystem.Get()?.Armament?.GetFirstSkill() ?? GolemArmamentSelection.Skills.GetRandomElement();
			if (!text.IsNullOrEmpty())
			{
				bool AddFinal = true;
				AddMetachrome(bodyPart2, text, ref AddFinal);
			}
		}
	}

	public void AddMetachrome(BodyPart Limb, string Skill, ref bool AddFinal)
	{
		if (((AddFinal && Limb.Parts.IsNullOrEmpty()) || !Limb.DefaultBehaviorBlueprint.IsNullOrEmpty()) && GameObjectMetachromeUnit.TryGetBlueprintFor(Limb.Type, Skill, out var Blueprint))
		{
			GameObjectMetachromeUnit.SetDefaultBehavior(Limb, Blueprint);
			AddFinal = false;
		}
		foreach (BodyPart item in Limb.LoopSubparts())
		{
			AddMetachrome(item, Skill, ref AddFinal);
		}
	}

	public override void Remove(GameObject Object)
	{
		BodyPart bodyPart = Object.Body?.GetBody();
		if (bodyPart != null)
		{
			if (Manager.IsNullOrEmpty())
			{
				BodyPart firstPart = bodyPart.GetFirstPart(Type, (Laterality == 0) ? 65535 : Laterality);
				bodyPart.RemovePart(firstPart);
			}
			else
			{
				Object.Body?.RemovePartsByManager(Manager, EvenIfDismembered: true);
			}
		}
	}

	public override void Reset()
	{
		base.Reset();
		Type = null;
		Manager = null;
		InsertAfter = null;
		OrInsertBefore = null;
		Category = -1;
		Laterality = 0;
		Metachromed = false;
	}

	public override string GetDescription(bool Inscription = false)
	{
		if (Anatomies.BodyPartTypeTable.TryGetValue(Type, out var value))
		{
			return "Extra " + value.Description + " slot";
		}
		return "Extra " + Type.ToLowerInvariant() + " slot";
	}
}
