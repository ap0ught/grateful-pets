using System;
using System.Collections.Generic;
using XRL.Language;
using XRL.World.Anatomy;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class ArmsOnEquip : IPart
{
	public string BaseArm;

	public string BaseHand;

	public string BaseHands;

	public string DefaultHandBehavior;

	public string Category;

	public int Pairs = 1;

	public int AttackChanceAdjust;

	public bool BreakOnDismember;

	public bool Extrinsic = true;

	public bool Describe = true;

	[NonSerialized]
	protected string _ManagerID;

	public virtual string ManagerID => _ManagerID ?? (_ManagerID = ParentObject.ID + "::" + GetType().Name);

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (ID != PooledEvent<BeforeDismemberEvent>.ID || !BreakOnDismember) && ID != EquippedEvent.ID && ID != UnequippedEvent.ID && (ID != PooledEvent<GetMeleeAttackChanceEvent>.ID || AttackChanceAdjust == 0))
		{
			if (ID == GetShortDescriptionEvent.ID)
			{
				return Describe;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(BeforeDismemberEvent E)
	{
		if (BreakOnDismember && E.IsObject && E.Part?.Manager != null && E.Part.Manager == ManagerID)
		{
			ParentObject.ApplyEffect(new Broken());
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		if (ParentObject.IsWorn())
		{
			AddArms(E.Actor);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		RemoveArms(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMeleeAttackChanceEvent E)
	{
		if (AttackChanceAdjust != 0 && E.Intrinsic && !E.Primary && E.BodyPart?.Manager != null && E.BodyPart.Manager == ManagerID)
		{
			E.Chance += AttackChanceAdjust;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (Describe)
		{
			ResolveTypes(out var ArmType, out var _, out var _);
			E.Postfix.Append("\n{{rules|").Append("Grants ").Append(Pairs * 2)
				.Append(' ')
				.Append(Grammar.Pluralize(ArmType.Name));
			if (!DefaultHandBehavior.IsNullOrEmpty() && GameObjectFactory.Factory.Blueprints.TryGetValue(DefaultHandBehavior, out var value))
			{
				E.Postfix.Append(" with ").Append(Grammar.Pluralize(value.CachedDisplayNameStripped));
			}
			E.Postfix.Append("}}");
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public void ResolveTypes(out BodyPartType ArmType, out BodyPartType HandType, out BodyPartType HandsType)
	{
		Dictionary<string, BodyPartType> bodyPartTypeTable = Anatomies.BodyPartTypeTable;
		HandsType = bodyPartTypeTable[BaseHands.Coalesce("Hands")];
		HandType = bodyPartTypeTable[BaseHand.Coalesce(HandsType.ImpliedBy, "Hand")];
		ArmType = bodyPartTypeTable[BaseArm.Coalesce(HandType.UsuallyOnVariant, "Arm")];
	}

	public void AddArms(GameObject Subject = null)
	{
		if (Subject == null)
		{
			Subject = ParentObject.Equipped;
		}
		BodyPart bodyPart = (Subject?.Body)?.GetBody();
		if (bodyPart == null)
		{
			return;
		}
		ResolveTypes(out var ArmType, out var HandType, out var HandsType);
		int num = ((Pairs != 1) ? 4 : 0);
		int? category = (Category.IsNullOrEmpty() ? ((int?)null) : new int?(BodyPartCategory.GetCode(Category)));
		for (int i = 0; i < Pairs; i++)
		{
			string text = ((i == 0) ? (ManagerID + " Hands") : $"{ManagerID} Hands {i + 1}");
			BodyPartType bodyPartType = ArmType;
			int laterality = 2 | num;
			bool? extrinsic = Extrinsic;
			string managerID = ManagerID;
			string[] orInsertBefore = new string[4] { "Hands", "Feet", "Roots", "Thrown Weapon" };
			BodyPart bodyPart2 = bodyPart.AddPartAt(bodyPartType, laterality, null, null, null, null, managerID, category, null, null, null, null, null, null, extrinsic, null, null, null, null, null, "Arm", orInsertBefore);
			BodyPartType bodyPartType2 = HandType;
			int laterality2 = 2 | num;
			extrinsic = Extrinsic;
			string managerID2 = ManagerID;
			string supportsDependent = text;
			bodyPart2.AddPart(bodyPartType2, laterality2, DefaultHandBehavior, supportsDependent, null, null, managerID2, category, null, null, null, null, null, null, extrinsic);
			BodyPartType bodyPartType3 = ArmType;
			BodyPart bodyPart3 = bodyPart.AddPartAt(bodyPart2, bodyPartType3, 1 | num, null, null, null, null, Extrinsic: Extrinsic, Manager: ManagerID, Category: category);
			BodyPartType bodyPartType4 = HandType;
			int laterality3 = 1 | num;
			extrinsic = Extrinsic;
			supportsDependent = ManagerID;
			managerID2 = text;
			bodyPart3.AddPart(bodyPartType4, laterality3, DefaultHandBehavior, managerID2, null, null, supportsDependent, category, null, null, null, null, null, null, extrinsic);
			BodyPartType bodyPartType5 = HandsType;
			extrinsic = Extrinsic;
			string managerID3 = ManagerID;
			orInsertBefore = new string[3] { "Feet", "Roots", "Thrown Weapon" };
			bodyPart.AddPartAt(bodyPartType5, 0, null, null, text, null, managerID3, category, null, null, null, null, null, null, extrinsic, null, null, null, null, null, "Hands", orInsertBefore);
			if (num != 0)
			{
				num <<= 1;
			}
		}
		Subject.WantToReequip();
	}

	public void RemoveArms(GameObject Subject = null)
	{
		if (Subject == null)
		{
			Subject = ParentObject.Equipped;
		}
		Subject?.RemoveBodyPartsByManager(ManagerID, EvenIfDismembered: true);
		Subject?.WantToReequip();
	}
}
