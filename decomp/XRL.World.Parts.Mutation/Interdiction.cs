using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Interdiction : BaseMutation
{
	public GameObject interdictTarget;

	public int SpeedPenalty = 10;

	[NonSerialized]
	private long lastms;

	[NonSerialized]
	private long glowstep;

	public Interdiction()
	{
		Type = "Mental";
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeforeTakeAction");
		base.Register(Object, Registrar);
	}

	public void StopInterdiction()
	{
		if (interdictTarget != null)
		{
			Interdicted effect = interdictTarget.GetEffect((Interdicted fx) => ParentObject.IDMatch(fx.interdictorId));
			interdictTarget.RemoveEffect(effect);
			interdictTarget = null;
		}
	}

	public void BeginInterdiction(GameObject target)
	{
		StopInterdiction();
		if (!ParentObject.IsEMPed() && target.PhaseMatches(3) && ParentObject.HasLOSTo(target))
		{
			target.ApplyEffect(new Interdicted(ParentObject.ID, SpeedPenalty));
			interdictTarget = target;
			DidXToY("lock", "onto", target, null, null, null, null, null, target);
		}
	}

	public void CheckInterdiction()
	{
		if (interdictTarget != null)
		{
			if (ParentObject.IsEMPed())
			{
				StopInterdiction();
			}
			if (!interdictTarget.PhaseMatches(3))
			{
				StopInterdiction();
			}
			if (!ParentObject.HasLOSTo(interdictTarget))
			{
				StopInterdiction();
			}
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeTakeAction")
		{
			CheckInterdiction();
			if (ParentObject.Target != null && (interdictTarget != ParentObject.Target || !interdictTarget.HasEffect((Interdicted FX) => FX.interdictorId == ParentObject.ID)))
			{
				BeginInterdiction(ParentObject.Target);
			}
		}
		return true;
	}

	public override bool Render(RenderEvent E)
	{
		if (interdictTarget != null)
		{
			E.WantsToPaint = true;
		}
		return true;
	}

	public override void OnPaint(ScreenBuffer buffer)
	{
		if (interdictTarget == null || !GameObject.Validate(ParentObject))
		{
			return;
		}
		List<Tuple<Cell, char>> lineTo = ParentObject.GetLineTo(interdictTarget);
		if (lineTo == null || lineTo.Count <= 0)
		{
			return;
		}
		if (lastms == 0L)
		{
			lastms = IComponent<GameObject>.frameTimerMS;
		}
		else if (IComponent<GameObject>.frameTimerMS - lastms > 200)
		{
			glowstep++;
		}
		if (glowstep >= lineTo.Count)
		{
			glowstep = 0L;
		}
		for (int i = 0; i < lineTo.Count; i++)
		{
			char c = 'c';
			if (i == glowstep)
			{
				c = 'C';
			}
			Tuple<Cell, char> tuple = lineTo[i];
			buffer.Goto(tuple.Item1.X, tuple.Item1.Y);
			buffer.Buffer[tuple.Item1.X, tuple.Item1.Y].SetBackground(c);
			buffer.Buffer[tuple.Item1.X, tuple.Item1.Y].Detail = ColorUtility.ColorMap[c];
		}
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		return base.Unmutate(GO);
	}
}
