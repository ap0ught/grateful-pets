using System;
using Qud.API;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class EatMemoriesOnHit : IPart
{
	public int Chance = 100;

	public string MemoriesLost = "1d2";

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("WeaponHit");
		Registrar.Register("WeaponAfterAttack");
		base.Register(Object, Registrar);
	}

	public static bool CheckEatMemories(GameObject attacker, GameObject defender, GameObject weapon, int chance)
	{
		if (defender != null && defender.IsPlayer())
		{
			if (GetSpecialEffectChanceEvent.GetFor(attacker, weapon, "Part EatMemoriesOnHit Activation", chance, defender).in100() && Stat.rollMentalAttackPenetrations(attacker, defender) > 0)
			{
				return true;
			}
		}
		return false;
	}

	public static void EatMemories(GameObject attacker, GameObject defender, GameObject weapon, string memoriesLost)
	{
		bool flag = false;
		GameManager.Instance.Fuzzing = true;
		defender.CurrentZone.GetExploredCells().ForEach((Cell c) =>
		{
			c.SetExplored(State: false);
		});
		defender.ParticlePulse("&K?");
		int num = 0;
		for (int num2 = Stat.Roll(memoriesLost); num < num2; num++)
		{
			IBaseJournalEntry randomRevealedNote = JournalAPI.GetRandomRevealedNote((IBaseJournalEntry c) => c.Forgettable());
			if (randomRevealedNote != null)
			{
				randomRevealedNote.Forget();
				flag = true;
			}
		}
		if (flag)
		{
			IComponent<GameObject>.AddPlayerMessage("{{R|You forget something.}}");
			GenericDeepNotifyEvent.Send(defender, "MemoriesEaten", defender, weapon);
		}
		else
		{
			IComponent<GameObject>.AddPlayerMessage(attacker.Does("try") + " to eat your memories, but " + attacker.does("starve", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, WithIndefiniteArticle: false, null, IndicateHidden: false, Pronoun: true) + ".");
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponHit")
		{
			GameObject Object = E.GetGameObjectParameter("Defender");
			if (GameObject.Validate(ref Object) && CheckEatMemories(ParentObject.equippedOrSelf(), Object, ParentObject, Chance))
			{
				E.SetParameter("DidSpecialEffect", 1);
				string text = E.GetStringParameter("Properties", "") ?? "";
				if (!text.Contains("AteMemories"))
				{
					E.SetParameter("Properties", (text == "") ? "AteMemories" : (text + ",AteMemories"));
				}
			}
		}
		else if (E.ID == "WeaponAfterAttack")
		{
			GameObject Object2 = E.GetGameObjectParameter("Attacker");
			GameObject Object3 = E.GetGameObjectParameter("Defender");
			GameObject Object4 = E.GetGameObjectParameter("Weapon");
			if (GameObject.Validate(ref Object2) && GameObject.Validate(ref Object3) && GameObject.Validate(ref Object4) && (E.GetStringParameter("Properties", "") ?? "").Contains("AteMemories"))
			{
				EatMemories(Object2, Object3, Object4, MemoriesLost);
			}
		}
		return base.FireEvent(E);
	}
}
