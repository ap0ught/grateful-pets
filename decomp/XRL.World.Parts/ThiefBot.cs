using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using UnityEngine;
using XRL.Language;
using XRL.Rules;
using XRL.World.AI.GoalHandlers;
using XRL.World.Anatomy;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class ThiefBot : IPart
{
	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AIBeginKill");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIBeginKill" && ParentObject.Inventory != null)
		{
			if (ParentObject.Inventory.Objects.Count > 0)
			{
				Debug.Log("I should drop off my inventory");
				ParentObject.Brain.PushGoal(new DropOffStolenGoods());
			}
			else
			{
				GameObject target = ParentObject.Target;
				if (target != null && ParentObject.DistanceTo(target) <= 1 && (ParentObject.IsFlying || !target.IsFlying))
				{
					if (ParentObject.Brain != null)
					{
						ParentObject.Brain.Think("I'm going to steal something!");
					}
					ParentObject.UseEnergy(1000);
					if (!ParentObject.PhaseMatches(target))
					{
						if (target.IsPlayer())
						{
							IComponent<GameObject>.AddPlayerMessage(ConsoleLib.Console.ColorUtility.CapitalizeExceptFormatting(Grammar.MakePossessive(ParentObject.The + ParentObject.ShortDisplayName)) + " pincers pass through you harmlessly.", 'G');
						}
					}
					else if (target.MakeSave("Agility", 10, ParentObject, "Agility", "Disarm"))
					{
						int num = Stat.Random(1, 100);
						GameObject gameObject = null;
						if (num <= 25 && target.Body != null)
						{
							List<BodyPart> parts = target.Body.GetParts();
							parts.RemoveAll((BodyPart x) => x.Equipped == null || !x.Equipped.CanBeUnequipped());
							gameObject = parts.GetRandomElement()?.Equipped;
						}
						if (num > 25 || gameObject == null)
						{
							Inventory inventory = target.Inventory;
							gameObject = ((inventory != null) ? inventory.Objects.GetRandomElement() : null);
						}
						if (gameObject != null && ParentObject.TakeObject(gameObject, NoStack: false, Silent: true, 0))
						{
							if (ParentObject.IsPlayerControlled() || target.IsPlayerControlled())
							{
								string msg = Event.NewStringBuilder().Append(ParentObject.Does("snag")).Compound(target.poss(gameObject))
									.Append('!')
									.ToString();
								EmitMessage(msg, ColorCoding.ConsequentialColorChar(null, target));
							}
							target.DustPuff();
						}
					}
					else if (target.IsPlayer())
					{
						IComponent<GameObject>.AddPlayerMessage("You avoid " + Grammar.MakePossessive(ParentObject.the + ParentObject.ShortDisplayName) + " pincers.", 'G');
					}
					return false;
				}
			}
		}
		return base.FireEvent(E);
	}
}
