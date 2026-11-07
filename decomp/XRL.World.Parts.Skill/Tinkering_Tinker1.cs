using System;
using XRL.Collections;
using XRL.UI;
using XRL.World.Tinkering;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Tinkering_Tinker1 : BaseSkill
{
	public Guid ActivatedAbilityID = Guid.Empty;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("scholarship", 2);
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CommandRechargeObject");
		base.Register(Object, Registrar);
	}

	public bool Recharge(GameObject Object, IEvent FromEvent = null)
	{
		if (!ParentObject.CanMoveExtremities("Recharge", ShowMessage: true))
		{
			return false;
		}
		bool AnyParts = false;
		bool AnyRechargeable = false;
		bool AnyRecharged = false;
		Object.ForeachPartDescendedFrom((IRechargeable rcg) =>
		{
			AnyParts = true;
			if (!rcg.CanBeRecharged())
			{
				return true;
			}
			AnyRechargeable = true;
			int rechargeAmount = rcg.GetRechargeAmount();
			if (rechargeAmount <= 0)
			{
				return true;
			}
			char rechargeBit = rcg.GetRechargeBit();
			int rechargeValue = rcg.GetRechargeValue();
			string bits = rechargeBit.GetString();
			int num = rechargeAmount / rechargeValue;
			if (num < 1)
			{
				num = 1;
			}
			int bitCount = BitLocker.GetBitCount(ParentObject, rechargeBit);
			if (bitCount == 0)
			{
				return ParentObject.Fail("You don't have any " + BitType.GetString(bits) + " bits, which are required for recharging.");
			}
			int num2 = Math.Min(bitCount, num);
			int valueOrDefault = Popup.AskNumber("It would take {{C|" + num + "}} " + BitType.GetString(bits) + " " + ((num == 1) ? "bit" : "bits") + " to fully recharge " + Object.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ". You have {{C|" + bitCount + "}}. How many do you want to use?", "Sounds/UI/ui_notification", "", num2, 0, num2).GetValueOrDefault();
			if (valueOrDefault > 0)
			{
				BitLocker.UseBits(ParentObject, rechargeBit, valueOrDefault);
				Object.SplitFromStack();
				rcg.AddCharge((valueOrDefault < num) ? (valueOrDefault * rechargeValue) : rechargeAmount);
				IComponent<GameObject>.PlayUISound("Sounds/Abilities/sfx_ability_energyCell_recharge");
				Popup.Show("You have " + ((valueOrDefault < num) ? "partially " : "") + "recharged " + Object.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ".");
				Object.CheckStack();
				AnyRecharged = true;
			}
			return true;
		});
		if (!AnyParts)
		{
			Popup.ShowFail("That isn't an energy cell and does not have a rechargeable capacitor.");
		}
		else if (!AnyRechargeable)
		{
			Popup.ShowFail(Object.T() + " can't be recharged that way.");
		}
		if (AnyRecharged)
		{
			ParentObject.UseEnergy(1000, "Skill Tinkering Recharge");
			FromEvent?.RequestInterfaceExit();
		}
		return AnyRecharged;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandRechargeObject")
		{
			if (!ParentObject.CanMoveExtremities("Recharge", ShowMessage: true))
			{
				return false;
			}
			using ScopeDisposedList<GameObject> scopeDisposedList = ScopeDisposedList<GameObject>.GetFromPool();
			using ScopeDisposedList<GameObject> scopeDisposedList2 = ScopeDisposedList<GameObject>.GetFromPool();
			ParentObject.GetContents(scopeDisposedList2);
			foreach (GameObject item in scopeDisposedList2)
			{
				if (item.Understood() && item.NeedsRecharge())
				{
					scopeDisposedList.Add(item);
				}
			}
			if (scopeDisposedList.Count == 0)
			{
				if (ParentObject.IsPlayer())
				{
					Popup.ShowFail("You have no items that require charging.");
				}
			}
			else
			{
				GameObject gameObject = Popup.PickGameObject("Select an item to charge.", scopeDisposedList, AllowEscape: true, ShowContext: true);
				if (gameObject != null)
				{
					Recharge(gameObject, E);
				}
			}
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		if (GO.GetIntProperty("ReceivedTinker1Recipe") <= 0 && (GO.CurrentCell != null || GO.IsOriginalPlayerBody()))
		{
			Tinkering.LearnNewRecipe(GO, 0, 3);
			GO.SetIntProperty("ReceivedTinker1Recipe", 1);
		}
		ActivatedAbilityID = AddMyActivatedAbility("Recharge", "CommandRechargeObject", "Skills", null, "\u009b");
		if (GO.IsPlayer())
		{
			TinkeringSifrah.AwardInsight();
		}
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.RemoveSkill(GO);
	}
}
