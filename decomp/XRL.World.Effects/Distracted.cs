using System;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class Distracted : Effect
{
	public GameObject Target;

	[NonSerialized]
	private Distraction _Distraction;

	public Distraction Distraction
	{
		get
		{
			return _Distraction ?? (_Distraction = Target.GetPart<Distraction>());
		}
		set
		{
			_Distraction = value;
			Target = value?.ParentObject;
		}
	}

	public Distracted()
	{
		DisplayName = "distracted by a decoy";
		Duration = 1;
	}

	public Distracted(Distraction Distraction)
		: this()
	{
		this.Distraction = Distraction;
	}

	public override string GetDetails()
	{
		return "Has a decoy object as " + Object.its + " target.";
	}

	public override int GetEffectType()
	{
		return 117440514;
	}

	public override bool WantEvent(int ID, int Cascade)
	{
		return ID == SingletonEvent<BeforeBeginTakeActionEvent>.ID;
	}

	public override bool HandleEvent(BeforeBeginTakeActionEvent E)
	{
		if (!GameObject.Validate(ref Target))
		{
			Duration = 0;
		}
		else
		{
			if (++Duration > Distraction.SaveTurns)
			{
				if (Distraction.MakeSave(Object))
				{
					Duration = 0;
				}
				else
				{
					Duration = 1;
				}
			}
			if (Duration > 0 && Object.Brain.Target != Target)
			{
				ChangeTarget();
			}
		}
		return base.HandleEvent(E);
	}

	public override void Applied(GameObject Object)
	{
		ChangeTarget();
	}

	public void ChangeTarget()
	{
		Object.Brain.Target = null;
		Object.Brain.WantToKill(Target, "out of distraction", Directed: true);
	}

	public override void Remove(GameObject Object)
	{
		if (GameObject.Validate(ref Target) && Object.Brain.Target == Target)
		{
			Object.Brain.Target = null;
			if (GameObject.Validate(Distraction.Original) && Object.IsHostileTowards(Distraction.Original))
			{
				Object.Brain.WantToKill(Distraction.Original);
			}
		}
	}
}
