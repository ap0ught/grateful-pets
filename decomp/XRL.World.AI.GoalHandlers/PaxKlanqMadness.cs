using System;
using System.Collections.Generic;
using XRL.Messages;
using XRL.Rules;
using XRL.World.Anatomy;
using XRL.World.Effects;
using XRL.World.Parts;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class PaxKlanqMadness : GoalHandler
{
	private bool bCanFight;

	private int Style = -1;

	private int Stage;

	public override bool Finished()
	{
		return false;
	}

	public override bool CanFight()
	{
		return bCanFight;
	}

	public override void TakeAction()
	{
		if (ParentObject.IsPlayer())
		{
			Pop();
			return;
		}
		if (Style == -1)
		{
			Style = Stat.Random(0, 1);
		}
		if (Style == 0)
		{
			if (Stage == 0)
			{
				if (ParentObject.HasPart<Body>())
				{
					Body body = ParentObject.Body;
					List<BodyPart> equippedParts = body.GetEquippedParts();
					for (int i = 0; i < equippedParts.Count; i++)
					{
						GameObject equipped = equippedParts[i].Equipped;
						if (ParentObject.FireEvent(Event.New("CommandUnequipObject", "BodyPart", equippedParts[i])) && equipped != null && equipped.IsValid() && ParentObject.CurrentCell != null && equipped.Equipped == null)
						{
							Cell randomLocalAdjacentCell = ParentObject.CurrentCell.GetRandomLocalAdjacentCell();
							if (randomLocalAdjacentCell != null)
							{
								equipped.Physics.CurrentCell = randomLocalAdjacentCell;
								randomLocalAdjacentCell.AddObject(equipped);
								body.DidXToY("toss", equipped, "aside", null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: true);
							}
						}
					}
				}
				Stage++;
				ParentObject.UseEnergy(1000);
				return;
			}
			if (Stage == 1)
			{
				PushGoal(new WanderRandomly(40));
				Stage++;
				ParentObject.UseEnergy(1000);
				return;
			}
			if (Stage == 2)
			{
				ParentObject.ApplyEffect(new Asleep(100));
				Stage++;
				ParentObject.UseEnergy(1000);
				Pop();
				return;
			}
		}
		if (Style == 1)
		{
			if (Stage == 0)
			{
				if (ParentObject.CurrentCell != null)
				{
					GameObject gameObject = ParentObject.CurrentCell.ParentZone.FindClosestObjectWithTag(ParentObject, "Wall");
					if (gameObject != null)
					{
						bCanFight = true;
						PushGoal(new Kill(gameObject));
					}
					Stage++;
					ParentObject.UseEnergy(1000);
				}
				return;
			}
			if (Stage == 1)
			{
				ParentObject.ApplyEffect(new Asleep(100));
				Stage++;
				ParentObject.UseEnergy(1000);
				Pop();
				return;
			}
		}
		if (Style != 2)
		{
			return;
		}
		if (Stage == 0)
		{
			if (ParentObject.IsVisible())
			{
				ParentObject.ParticleText("KLANQ!", 'O');
				MessageQueue.AddPlayerMessage(ParentObject.Does("shout", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " shouts {{O|KLANQ}}!");
			}
			ParentObject.UseEnergy(1000);
			if (5.in100())
			{
				Stage++;
			}
		}
		else if (Stage == 1)
		{
			ParentObject.ApplyEffect(new Asleep(100));
			Stage++;
			ParentObject.UseEnergy(1000);
			Pop();
		}
	}
}
