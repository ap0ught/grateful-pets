using UnityEngine;

namespace XRL.World.AI;

/// <remarks>
/// As of yet only for hostility debugging purposes, may be leveraged somewhere player-facing in the future.
/// Current feeling values and attitude thresholds are subject to change.
/// </remarks>
public abstract class IOpinion : IComposite
{
	/// <summary>The current magnitude of the opinion, acting as a multiplier for its feeling value.</summary>
	public float Magnitude;

	/// <summary>When this type of opinion was last added.</summary>
	public long Time;

	public int Value => Mathf.RoundToInt((float)BaseValue * Magnitude);

	public virtual bool WantFieldReflection => true;

	/// <summary>The base feeling change from this opinion.</summary>
	public abstract int BaseValue { get; }

	/// <summary>The turns it takes for this opinion to abate.</summary>
	/// <remarks>Counts from this opinion's <see cref="F:XRL.World.AI.IOpinion.Time" />.</remarks>
	public virtual int Duration
	{
		get
		{
			if (BaseValue >= 0)
			{
				return 0;
			}
			return 16800;
		}
	}

	/// <summary>The turns that must elapse before the opinion can be re-applied.</summary>
	/// <remarks>Counts from this opinion's <see cref="F:XRL.World.AI.IOpinion.Time" />.</remarks>
	public virtual int Cooldown => 1200;

	/// <summary>The maximum magnitude for this opinion.</summary>
	public virtual float Limit => 1f;

	public virtual void Write(SerializationWriter Writer)
	{
	}

	public virtual void Read(SerializationReader Reader)
	{
	}

	public virtual string GetText(GameObject Actor)
	{
		return null;
	}

	public virtual bool HandleEvent(AfterAddOpinionEvent E)
	{
		return true;
	}
}
