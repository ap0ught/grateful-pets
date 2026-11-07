using System;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Discipline_MindOverBody : BaseSkill
{
	public override bool AddSkill(GameObject GO)
	{
		GO.RemoveEffect<Famished>();
		return true;
	}
}
