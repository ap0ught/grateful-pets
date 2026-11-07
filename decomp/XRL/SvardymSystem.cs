using System;
using Genkit;
using XRL.Messages;
using XRL.Rules;
using XRL.World;
using XRL.World.Parts;

namespace XRL;

[Serializable]
public class SvardymSystem : IGameSystem
{
	public string lastZone;

	public bool storming;

	public Location2D epicenter;

	public int radius;

	public int eggs;

	public int nextEgg;

	public int stormTurn;

	public int stormEndingTurn;

	public int lastRadius;

	private static readonly int CHANCE_PER_ROUND_SVARDYM_STORM_DENOM_10000 = 5;

	[NonSerialized]
	private Zone _StormZone;

	public Zone StormZone
	{
		get
		{
			if (lastZone != null)
			{
				if (_StormZone == null)
				{
					_StormZone = The.ZoneManager.GetZone(lastZone);
				}
				else if (_StormZone.Suspended)
				{
					StormZone = null;
				}
			}
			return _StormZone;
		}
		set
		{
			_StormZone = value;
			lastZone = value?.ZoneID;
		}
	}

	public override void Register(XRLGame Game, IEventRegistrar Registrar)
	{
		Registrar.Register(ZoneActivatedEvent.ID);
		if (storming || Registrar.IsUnregister)
		{
			Registrar.Register(SingletonEvent<EndTurnEvent>.ID);
			Registrar.Register(SingletonEvent<GetAmbientLightEvent>.ID);
		}
	}

	public override bool HandleEvent(GetAmbientLightEvent E)
	{
		if (storming && E.Source is Daylight && PlayerInStorm())
		{
			if (stormEndingTurn > 0)
			{
				E.Radius = Math.Min(radius, lastRadius);
			}
			else
			{
				lastRadius = Math.Min(radius, 40) - stormTurn;
				E.Radius = Math.Max(0, lastRadius);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		if (IsValidZone(E.Zone))
		{
			if (storming)
			{
				StormZone = E.Zone;
			}
			else if (CHANCE_PER_ROUND_SVARDYM_STORM_DENOM_10000.in10000())
			{
				StormZone = E.Zone;
				BeginStorm();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (storming)
		{
			Tick();
		}
		return base.HandleEvent(E);
	}

	public void BeginStorm()
	{
		storming = true;
		stormTurn = 0;
		stormEndingTurn = 0;
		eggs = Stat.Random(10, 15);
		nextEgg = Stat.Random(5, 7);
		if (PlayerInStorm())
		{
			MessageQueue.AddPlayerMessage("Your hear a swelling thpthp sound.");
			SoundManager.PlaySound("EggSackPlague");
			if (Calendar.IsDay())
			{
				MessageQueue.AddPlayerMessage("The sky begins to darken.");
			}
		}
		RegisterEvent(SingletonEvent<EndTurnEvent>.ID);
		RegisterEvent(SingletonEvent<GetAmbientLightEvent>.ID);
	}

	public void EndStorm()
	{
		storming = false;
		StormZone = null;
		UnregisterEvent(SingletonEvent<EndTurnEvent>.ID);
		UnregisterEvent(SingletonEvent<GetAmbientLightEvent>.ID);
	}

	public void SpawnEgg()
	{
		Zone stormZone = StormZone;
		Cell cell = ((stormZone != null) ? stormZone.GetEmptyCells().GetRandomElement() : null);
		if (cell == null)
		{
			return;
		}
		GameObject gameObject = cell.AddObject("Svardym Egg Sac");
		if (PlayerInStorm())
		{
			if (gameObject.IsVisible())
			{
				gameObject.Slimesplatter(SelfSplatter: false);
				gameObject.DustPuff();
			}
			gameObject.PlayWorldSound("svardym_plop", 1f);
		}
	}

	public bool PlayerInStorm()
	{
		return The.Player.CurrentZone == StormZone;
	}

	public void Tick()
	{
		stormTurn++;
		if (eggs > 0 && --nextEgg <= 0)
		{
			SpawnEgg();
			nextEgg = Stat.Random(5, 7);
			if (--eggs <= 3)
			{
				stormEndingTurn = stormTurn;
				if (PlayerInStorm())
				{
					MessageQueue.AddPlayerMessage("The thpthp sound wanes.");
					if (Calendar.IsDay())
					{
						MessageQueue.AddPlayerMessage("The sky begins to brighten.");
					}
				}
			}
		}
		if (stormEndingTurn > 0)
		{
			if (lastRadius < 80)
			{
				lastRadius++;
			}
			else
			{
				EndStorm();
			}
		}
	}

	public bool IsValidZone(Zone Z)
	{
		if (Z.Z == 10 && !Z.HasZoneProperty("NoSvardymStorm"))
		{
			return Z.GetTerrainObject().HasTag("SvardymStorm");
		}
		return false;
	}
}
