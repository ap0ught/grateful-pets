using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class LongBladesProficiency : LongBladesSkillBase
{
	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
