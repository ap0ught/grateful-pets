using System.Runtime.InteropServices;

namespace XRL.World.Parts;

[StructLayout(LayoutKind.Auto)]
public struct MeleeAttackResult
{
	public int Damage;

	public int Attacks;

	public int Penetrations;

	public int Criticals;

	public int Hits;

	public static explicit operator bool(MeleeAttackResult Result)
	{
		return Result.Attacks > 0;
	}

	public static MeleeAttackResult operator +(MeleeAttackResult First, MeleeAttackResult Second)
	{
		First.Damage += Second.Damage;
		First.Attacks += Second.Attacks;
		First.Penetrations += Second.Penetrations;
		First.Criticals += Second.Criticals;
		First.Hits += Second.Hits;
		return First;
	}
}
