using System;
using XRL.Messages;
using XRL.UI;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class QuantumFugue : TemporalFugue
{
	public long EndTick = -1L;

	public override string CommandID => "CommandQuantumFugue";

	public override string AbilityClass => "Mutations";

	public override bool IsRealityDistortionBased => false;

	public override bool AffectedByWillpower => false;

	public override int AIMaxDistance => 9999999;

	public override int GetCooldown(int Level)
	{
		return 100;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeforeDieEvent.ID;
		}
		return true;
	}

	public override IPart DeepCopy(GameObject Parent)
	{
		QuantumFugue quantumFugue = (QuantumFugue)base.DeepCopy(Parent);
		quantumFugue.EndTick = -1L;
		return quantumFugue;
	}

	public override bool HandleEvent(BeforeDieEvent E)
	{
		if (EndTick >= 0 && !ParentObject.HasStringProperty("FugueCopy"))
		{
			GameObject bestCopyIn = GetBestCopyIn(ParentObject.CurrentZone);
			if (bestCopyIn != ParentObject)
			{
				Inhabit(bestCopyIn);
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool PerformTemporalFugue(IEvent TriggeringEvent = null)
	{
		if (EndTick >= 0)
		{
			return false;
		}
		int temporalFugueDuration = TemporalFugue.GetTemporalFugueDuration(Level);
		if (TemporalFugue.PerformTemporalFugue(ParentObject, ParentObject, null, this, TriggeringEvent, Involuntary: false, Duration: temporalFugueDuration + 1, IsRealityDistortionBased: IsRealityDistortionBased))
		{
			EndTick = The.Game.TimeTicks + temporalFugueDuration;
		}
		return false;
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		if (EndTick >= 0 && TimeTick >= EndTick)
		{
			EndTick = -1L;
			Cohere(GetAnyBasisZone());
		}
	}

	public void Inhabit(GameObject Copy)
	{
		try
		{
			MessageQueue.Suppress = true;
			ParentObject.SystemMoveTo(Copy.CurrentCell, null, forced: true);
			ParentObject.hitpoints = Copy.hitpoints;
			ParentObject.RemoveEffectsOfPartialType(67108864);
			if (Copy.AnyEffects())
			{
				foreach (Effect effect in Copy.Effects)
				{
					if (effect.IsOfType(67108864) && !effect.IsOfType(128))
					{
						ParentObject.Effects.Add(effect.DeepCopy(ParentObject));
					}
				}
			}
			if (Sidebar.CurrentTarget == Copy)
			{
				Sidebar.CurrentTarget = ParentObject;
			}
			Copy.GetPart<Temporary>().Expire(Silent: true);
		}
		finally
		{
			MessageQueue.Suppress = false;
		}
	}

	public GameObject GetBestCopyIn(Zone Zone, bool Expire = false)
	{
		string iD = ParentObject.ID;
		GameObject result = ParentObject;
		int num = ParentObject.hitpoints;
		Zone.ObjectEnumerator enumerator = Zone.IterateObjects().GetEnumerator();
		while (enumerator.MoveNext())
		{
			GameObject current = enumerator.Current;
			string stringProperty = current.GetStringProperty("FugueCopy");
			if (stringProperty == null || stringProperty != iD)
			{
				continue;
			}
			int hitpoints = current.hitpoints;
			if (hitpoints <= num)
			{
				if (Expire)
				{
					current.GetPart<Temporary>().Expire();
				}
			}
			else
			{
				result = current;
				num = hitpoints;
			}
		}
		return result;
	}

	public void Cohere(Zone Zone)
	{
		GameObject bestCopyIn = GetBestCopyIn(Zone, Expire: true);
		if (bestCopyIn != ParentObject)
		{
			Inhabit(bestCopyIn);
		}
		if (ParentObject.CurrentCell != null)
		{
			DidX("cohere", null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: true);
			ParentObject.FugueVFX();
		}
	}
}
