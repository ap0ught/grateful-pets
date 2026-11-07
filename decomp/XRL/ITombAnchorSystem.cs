using XRL.Collections;
using XRL.Messages;
using XRL.Rules;
using XRL.World;
using XRL.World.Effects;
using XRL.World.Parts;

namespace XRL;

public abstract class ITombAnchorSystem : IGameSystem
{
	public string Duration = "300";

	public int Timer = 300;

	public int Announcement = 20;

	public bool Active;

	public abstract int Depth { get; }

	public bool PlayerInRecallArea()
	{
		if ((The.Player?.CurrentZone?.Z ?? (-1)) == Depth)
		{
			return IsZoneInTomb(The.Player?.CurrentZone);
		}
		return false;
	}

	public bool IsZoneInTomb(Zone Z)
	{
		if (Z == null)
		{
			return false;
		}
		if (Z is InteriorZone interiorZone)
		{
			Z = interiorZone.ResolveBasisZone() ?? Z;
		}
		if (Z.wX == 53 && Z.wY == 3 && Z.Z >= 7)
		{
			return Z.Z <= 11;
		}
		return false;
	}

	public bool IsZoneInRange(Zone Z)
	{
		if (Z != null && Z.wX == 53 && Z.wY == 3 && Z.Z == Depth)
		{
			if (Z.X == 1)
			{
				return Z.Y != 1;
			}
			return true;
		}
		return false;
	}

	public virtual string GetAnchorZoneFor(Zone Z)
	{
		int num;
		int num2;
		do
		{
			num = Stat.Random(0, 2);
			num2 = Stat.Random(0, 2);
		}
		while ((num == 1 && num2 == 1) || (num == Z.X && num2 == Z.Y));
		return ZoneID.Assemble(Z.ZoneWorld, Z.wX, Z.wY, num, num2, Z.Z);
	}

	public void OnEndTurn()
	{
		if (The.Game.HasIntGameState("BellOfRestDestroyed"))
		{
			Active = false;
			return;
		}
		Timer--;
		if (Timer <= 0)
		{
			if (Active)
			{
				AnchorCall();
			}
			Timer = Stat.Roll(Duration);
		}
		else if (Timer % Announcement == 0 && Active && PlayerInRecallArea())
		{
			The.PlayerCell.PlayWorldSound("sfx_bellOfRest_toll");
			MessageQueue.AddPlayerMessage("&MThe Bell of Rest tolls! The dead will be recalled in " + Timer + " rounds.");
		}
	}

	public void Recall(Zone TargetZone)
	{
		if (!PlayerInRecallArea())
		{
			return;
		}
		GameObject player = The.Player;
		Cell cell = TargetZone.GetCellsWithObject("AnchorRoomTile").GetRandomElement();
		if (cell == null)
		{
			cell = TargetZone.GetCellsWithObject("AnchorRoomTile").GetRandomElement().GetCellOrFirstConnectedSpawnLocation();
		}
		if (cell != null && cell.HasObjectWithPart("StairsDown"))
		{
			cell = cell.GetConnectedSpawnLocation();
		}
		ScopeDisposedList<GameObject> scopeDisposedList = null;
		if (player.HasEffect(typeof(LatchedOnto)))
		{
			for (int num = player._Effects.Count - 1; num >= 0; num--)
			{
				if (player._Effects[num] is LatchedOnto latchedOnto)
				{
					GameObject equipped = latchedOnto.LatchedOnWeapon.Equipped;
					if (GameObject.Validate(equipped))
					{
						if (scopeDisposedList == null)
						{
							scopeDisposedList = ScopeDisposedList<GameObject>.GetFromPool();
						}
						scopeDisposedList.Add(equipped);
						equipped.TeleportTo(cell, 0, ignoreCombat: true, ignoreGravity: false, forced: true);
					}
					player.RemoveEffect(latchedOnto);
				}
			}
		}
		SoundManager.PlaySound("sfx_bellOfRest_toll");
		player.TeleportTo(cell, 0, ignoreCombat: true, ignoreGravity: false, forced: true);
		GameObject firstObjectWithPart = cell.GetFirstObjectWithPart("Enclosing");
		if (firstObjectWithPart != null)
		{
			Enclosing part = firstObjectWithPart.GetPart<Enclosing>();
			part.EnterEnclosure(player);
			if (!scopeDisposedList.IsNullOrEmpty())
			{
				foreach (GameObject item in scopeDisposedList)
				{
					part.EnterEnclosure(item);
					CryptFerretBehavior part2 = item.GetPart<CryptFerretBehavior>();
					if (part2 != null)
					{
						part2.behaviorState = "looting";
						item.Brain.Goals.Clear();
					}
				}
			}
		}
		scopeDisposedList?.Dispose();
		MessageQueue.AddPlayerMessage("You've been recalled to a resting place.", 'M');
		player.TeleportSwirl();
	}

	public void AnchorCall()
	{
		if (Player.HasMarkOfDeath())
		{
			if (Player.HasEffect(typeof(CorpseTethered)))
			{
				MessageQueue.AddPlayerMessage("You were not recalled as you're already in a resting place.", 'M');
				return;
			}
			Zone currentZone = Player.CurrentZone;
			string anchorZoneFor = GetAnchorZoneFor(currentZone);
			Recall(The.ZoneManager.GetZone(anchorZoneFor));
		}
	}

	public override void Register(XRLGame Game, IEventRegistrar Registrar)
	{
		Registrar.Register(SingletonEvent<EndTurnEvent>.ID);
		Registrar.Register(ZoneActivatedEvent.ID);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		OnEndTurn();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		Active = IsZoneInRange(E.Zone) && !The.Game.HasIntGameState("BellOfRestDestroyed");
		if (!Active && !IsZoneInTomb(E.Zone))
		{
			The.Game.FlagSystemForRemoval(this);
		}
		return base.HandleEvent(E);
	}
}
