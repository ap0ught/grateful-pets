using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Capabilities;

public static class Phase
{
	public const int UNKNOWN = 0;

	public const int IN_PHASE = 1;

	public const int OUT_OF_PHASE = 2;

	public const int OMNIPHASE = 3;

	public const int NULLPHASE = 4;

	public const int PHASE_INSENSITIVE = 5;

	public static string getName(int phase)
	{
		return phase switch
		{
			0 => "unknown", 
			1 => "in-phase", 
			2 => "out-of-phase", 
			3 => "omniphase", 
			4 => "nullphase", 
			5 => "phase insensitive", 
			_ => null, 
		};
	}

	public static int getPhase(GameObject obj)
	{
		if (obj == null)
		{
			return 0;
		}
		if (obj.HasTag("Nullphase") || obj.GetIntProperty("Nullphase") >= 1)
		{
			return 4;
		}
		if (obj.HasEffect(typeof(Omniphase)))
		{
			return 3;
		}
		if (obj.HasEffect(typeof(Phased)))
		{
			return 2;
		}
		return 1;
	}

	public static bool phaseMatches(int phase1, int phase2)
	{
		if (phase1 == 5 || phase2 == 5)
		{
			return true;
		}
		if (phase1 == 4 || phase2 == 4)
		{
			return false;
		}
		if (phase1 == 3 || phase2 == 3)
		{
			return true;
		}
		if (phase1 == 0)
		{
			phase1 = 1;
		}
		if (phase2 == 0)
		{
			phase2 = 1;
		}
		return phase1 == phase2;
	}

	public static bool phaseMatches(GameObject obj, int phase2)
	{
		switch (phase2)
		{
		case 5:
			return true;
		case 4:
			return false;
		default:
		{
			if (obj == null)
			{
				return false;
			}
			int num = getPhase(obj);
			switch (num)
			{
			case 4:
				return false;
			default:
				if (phase2 != 3)
				{
					if (num == 0)
					{
						num = 1;
					}
					if (phase2 == 0)
					{
						phase2 = 1;
					}
					return num == phase2;
				}
				goto case 3;
			case 3:
				return true;
			}
		}
		}
	}

	public static bool phaseMatches(int phase1, GameObject obj)
	{
		return phaseMatches(obj, phase1);
	}

	public static bool phaseMatches(GameObject obj1, GameObject obj2)
	{
		if (obj2 == null)
		{
			return false;
		}
		return phaseMatches(obj1, getPhase(obj2));
	}

	public static bool IsTemporarilyPhasedOut(GameObject Object)
	{
		if (Object.GetPhase() != 2)
		{
			return false;
		}
		Phased effect = Object.GetEffect<Phased>();
		if (effect == null)
		{
			return false;
		}
		if (effect.Duration == 9999)
		{
			return false;
		}
		return true;
	}

	public static int getWeaponPhase(int actorPhase, int weaponPhase)
	{
		if (actorPhase == 3 || weaponPhase == 3)
		{
			return 3;
		}
		return actorPhase;
	}

	public static int getWeaponPhase(GameObject actor, GameObject weapon)
	{
		return getWeaponPhase(actor.GetPhase(), weapon.GetPhase());
	}

	public static int getWeaponPhase(int actorPhase, GameObject weapon)
	{
		return getWeaponPhase(actorPhase, weapon.GetPhase());
	}

	public static int getWeaponPhase(GameObject actor, int weaponPhase)
	{
		return getWeaponPhase(actor.GetPhase(), weaponPhase);
	}

	public static void carryOver(int Phase, GameObject Object, int Duration = 9999)
	{
		if (Object != null)
		{
			switch (Phase)
			{
			case 2:
				Object.ForceApplyEffect(new Phased(Duration));
				break;
			case 3:
				Object.ForceApplyEffect(new Omniphase(Duration));
				break;
			}
		}
	}

	public static void carryOver(GameObject src, GameObject dest)
	{
		if (src != null && dest != null)
		{
			if (src.TryGetEffect<Phased>(out var Effect))
			{
				dest.ForceApplyEffect(new Phased(Effect.Duration));
			}
			if (src.TryGetEffect<Omniphase>(out var Effect2))
			{
				dest.ForceApplyEffect(new Omniphase(Effect2.Duration));
			}
		}
	}

	public static void carryOver(GameObject src, GameObject dest, int maxDuration)
	{
		if (src.TryGetEffect<Phased>(out var Effect))
		{
			dest.ForceApplyEffect(new Phased(Math.Min(Effect.Duration, maxDuration)));
		}
		if (src.TryGetEffect<Omniphase>(out var Effect2))
		{
			dest.ForceApplyEffect(new Omniphase(Math.Min(Effect2.Duration, maxDuration)));
		}
	}

	public static void carryOverPrep(GameObject src, out Phased FX1, out Omniphase FX2)
	{
		FX1 = src.GetEffect<Phased>();
		FX2 = src.GetEffect<Omniphase>();
	}

	public static void carryOver(GameObject src, GameObject dest, Phased FX1, Omniphase FX2)
	{
		if (FX1 != null)
		{
			dest.ForceApplyEffect(new Phased(FX1.Duration));
		}
		if (FX2 != null)
		{
			dest.ForceApplyEffect(new Omniphase(FX2.Duration));
		}
	}

	public static void syncPrep(GameObject src, out Phased FX1, out Omniphase FX2)
	{
		FX1 = src.GetEffect<Phased>();
		FX2 = src.GetEffect<Omniphase>();
	}

	public static bool sync(GameObject src, GameObject dest, Phased FX1, Omniphase FX2)
	{
		bool result = false;
		Phased effect = dest.GetEffect<Phased>();
		if (FX1 != null && effect != null)
		{
			if (effect.Duration != FX1.Duration)
			{
				effect.Duration = FX1.Duration;
				result = true;
			}
		}
		else if (FX1 != null)
		{
			dest.ForceApplyEffect(new Phased(FX1.Duration));
			result = true;
		}
		else if (effect != null)
		{
			dest.RemoveEffect(effect);
			result = true;
		}
		Omniphase effect2 = dest.GetEffect<Omniphase>();
		if (FX2 != null && effect2 != null)
		{
			if (effect2.Duration != FX2.Duration)
			{
				effect2.Duration = FX2.Duration;
				result = true;
			}
		}
		else if (FX2 != null)
		{
			dest.ForceApplyEffect(new Omniphase(FX2.Duration));
			result = true;
		}
		else if (effect2 != null)
		{
			dest.RemoveEffect(effect2);
			result = true;
		}
		return result;
	}

	public static bool sync(GameObject src, GameObject dest)
	{
		return sync(src, dest, src.GetEffect<Phased>(), src.GetEffect<Omniphase>());
	}

	public static char getColor(int phase)
	{
		return phase switch
		{
			1 => 'Y', 
			2 => 'b', 
			3 => 'M', 
			4 => 'K', 
			_ => 'y', 
		};
	}

	public static char getRandomExplosionColor(int phase)
	{
		int num = Stat.RandomCosmetic(1, 3);
		return phase switch
		{
			1 => num switch
			{
				2 => 'R', 
				1 => 'Y', 
				_ => 'W', 
			}, 
			2 => num switch
			{
				2 => 'b', 
				1 => 'K', 
				_ => 'c', 
			}, 
			3 => num switch
			{
				2 => 'G', 
				1 => 'M', 
				_ => 'm', 
			}, 
			4 => num switch
			{
				2 => 'y', 
				1 => 'K', 
				_ => 'k', 
			}, 
			_ => 'y', 
		};
	}

	public static char getRandomElectricArcColor(int phase)
	{
		int num = Stat.RandomCosmetic(1, 3);
		return phase switch
		{
			1 => num switch
			{
				2 => 'W', 
				1 => 'Y', 
				_ => 'W', 
			}, 
			2 => num switch
			{
				2 => 'b', 
				1 => 'K', 
				_ => 'c', 
			}, 
			3 => num switch
			{
				2 => 'G', 
				1 => 'M', 
				_ => 'm', 
			}, 
			4 => num switch
			{
				2 => 'y', 
				1 => 'K', 
				_ => 'Y', 
			}, 
			_ => 'y', 
		};
	}

	public static char getRandomHeatColor(int phase)
	{
		int num = Stat.RandomCosmetic(1, 3);
		return phase switch
		{
			1 => num switch
			{
				2 => 'W', 
				1 => 'Y', 
				_ => 'R', 
			}, 
			2 => num switch
			{
				2 => 'B', 
				1 => 'K', 
				_ => 'y', 
			}, 
			3 => num switch
			{
				2 => 'G', 
				1 => 'M', 
				_ => 'W', 
			}, 
			4 => num switch
			{
				2 => 'y', 
				1 => 'K', 
				_ => 'Y', 
			}, 
			_ => 'R', 
		};
	}

	public static char getRandomColdColor(int phase)
	{
		int num = Stat.RandomCosmetic(1, 3);
		return phase switch
		{
			1 => num switch
			{
				2 => 'C', 
				1 => 'Y', 
				_ => 'c', 
			}, 
			2 => num switch
			{
				2 => 'B', 
				1 => 'y', 
				_ => 'b', 
			}, 
			3 => num switch
			{
				2 => 'G', 
				1 => 'M', 
				_ => 'B', 
			}, 
			4 => num switch
			{
				2 => 'y', 
				1 => 'K', 
				_ => 'Y', 
			}, 
			_ => 'C', 
		};
	}

	public static char getRandomElectromagneticPulseColor(int phase)
	{
		int num = Stat.RandomCosmetic(1, 3);
		return phase switch
		{
			1 => num switch
			{
				2 => 'Y', 
				1 => 'W', 
				_ => 'C', 
			}, 
			2 => num switch
			{
				2 => 'b', 
				1 => 'K', 
				_ => 'c', 
			}, 
			3 => num switch
			{
				2 => 'c', 
				1 => 'm', 
				_ => 'C', 
			}, 
			4 => num switch
			{
				2 => 'y', 
				1 => 'K', 
				_ => 'Y', 
			}, 
			_ => 'W', 
		};
	}

	public static char getRandomFlashColor(int phase)
	{
		int num = Stat.RandomCosmetic(1, 3);
		return phase switch
		{
			1 => num switch
			{
				2 => 'Y', 
				1 => 'B', 
				_ => 'B', 
			}, 
			2 => num switch
			{
				2 => 'B', 
				1 => 'K', 
				_ => 'c', 
			}, 
			3 => num switch
			{
				2 => 'c', 
				1 => 'm', 
				_ => 'B', 
			}, 
			4 => num switch
			{
				2 => 'B', 
				1 => 'K', 
				_ => 'Y', 
			}, 
			_ => 'Y', 
		};
	}

	public static char getRandomDisintegrationColor(int phase)
	{
		int num = Stat.RandomCosmetic(1, 3);
		return phase switch
		{
			1 => num switch
			{
				2 => 'R', 
				1 => 'r', 
				_ => 'w', 
			}, 
			2 => num switch
			{
				2 => 'b', 
				1 => 'K', 
				_ => 'c', 
			}, 
			3 => num switch
			{
				2 => 'G', 
				1 => 'M', 
				_ => 'm', 
			}, 
			4 => num switch
			{
				2 => 'y', 
				1 => 'K', 
				_ => 'Y', 
			}, 
			_ => 'K', 
		};
	}

	public static char getRandomStunningForceColor(int phase)
	{
		int num = Stat.RandomCosmetic(1, 2);
		switch (phase)
		{
		case 1:
			if (num != 1)
			{
				return 'y';
			}
			return 'Y';
		case 2:
			if (num != 1)
			{
				return 'b';
			}
			return 'K';
		case 3:
			if (num != 1)
			{
				return 'G';
			}
			return 'M';
		case 4:
			if (num != 1)
			{
				return 'y';
			}
			return 'K';
		default:
			return 'y';
		}
	}
}
