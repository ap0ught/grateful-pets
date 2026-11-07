using XRL.World.Parts.Skill;

namespace XRL.World;

[GameEvent(Base = true)]
public abstract class IAddSkillEvent : MinEvent
{
	public GameObject Actor;

	public GameObject Source;

	public BaseSkill Skill;

	public IBaseSkillEntry Entry;

	public string Context;

	private bool? WaterRitualContext;

	private bool? InclusionContext;

	private bool? BookContext;

	public bool IsWaterRitual
	{
		get
		{
			bool valueOrDefault = WaterRitualContext == true;
			if (!WaterRitualContext.HasValue)
			{
				valueOrDefault = Context.HasDelimitedSubstring(',', "WaterRitual");
				WaterRitualContext = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	public bool IsInclusion
	{
		get
		{
			bool valueOrDefault = InclusionContext == true;
			if (!InclusionContext.HasValue)
			{
				valueOrDefault = Context.HasDelimitedSubstring(',', "Inclusion");
				InclusionContext = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	public bool IsBook
	{
		get
		{
			bool valueOrDefault = BookContext == true;
			if (!BookContext.HasValue)
			{
				valueOrDefault = Context.HasDelimitedSubstring(',', "TrainingBook");
				BookContext = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Actor = null;
		Source = null;
		Skill = null;
		Entry = null;
		Context = null;
		WaterRitualContext = null;
		InclusionContext = null;
		BookContext = null;
	}
}
