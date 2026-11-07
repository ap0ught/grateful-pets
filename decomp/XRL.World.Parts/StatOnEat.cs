using System;
using Qud.API;

namespace XRL.World.Parts;

[Serializable]
public class StatOnEat : IPart
{
	public string Stats = "";

	public bool LogInChronology = true;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("OnEat");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "OnEat")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Eater");
			if (gameObjectParameter != null)
			{
				string[] array = Stats.Split(';');
				for (int i = 0; i < array.Length; i++)
				{
					string[] array2 = array[i].Split(':');
					int num = Convert.ToInt32(array2[1]);
					if (gameObjectParameter.HasStat(array2[0]))
					{
						gameObjectParameter.GetStat(array2[0]).BaseValue += num;
						if (gameObjectParameter.IsPlayer() && LogInChronology)
						{
							JournalAPI.AddAccomplishment("You ate " + ParentObject.an() + ".", "=name= ate " + ParentObject.an() + ". What can we infer from this?", "While traveling through " + The.Player.CurrentZone.GetTerrainDisplayName() + ", =name= stopped at a tavern near " + JournalAPI.GetLandmarkNearestPlayer().Text + ". There =name= ate " + ParentObject.an() + " and left the tavern with " + The.Player.GetPronounProvider().PossessiveAdjective + " perspective dramatically altered.", null, "general", MuralCategory.BodyExperienceNeutral, MuralWeight.Medium, null, -1L);
						}
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}
