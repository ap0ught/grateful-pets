using System;
using System.Collections.Generic;
using Qud.API;
using XRL.Rules;
using XRL.UI;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class MutationInfection : Effect
{
	public MutationInfection()
	{
		DisplayName = "&minhabited";
	}

	public MutationInfection(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override int GetEffectType()
	{
		return 67108868;
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.FireEvent(Event.New("ApplyGirshInfection", "Duration", Duration)))
		{
			return false;
		}
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeginTakeAction");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			Duration--;
			if (Duration == 0)
			{
				List<BaseMutation> list = new List<BaseMutation>();
				List<BaseMutation> list2 = new List<BaseMutation>();
				GameObject gameObject = Object;
				Mutations part = gameObject.GetPart<Mutations>();
				foreach (BaseMutation mutation in part.MutationList)
				{
					list.Add(mutation);
				}
				int num = 0;
				for (int i = 0; i < 1 && num < 50; list2.Add(list[list.Count - 1]), list.RemoveAt(list.Count - 1), i++)
				{
					while (num <= 50)
					{
						num++;
						if (gameObject.IsEsper())
						{
							MutationsAPI.AddNewMentalMutation(list, 2);
						}
						else if (gameObject.IsChimera())
						{
							MutationsAPI.AddNewPhysicalMutation(list, 2);
						}
						else if (Stat.RandomLevelUpChoice(0, 1) == 0)
						{
							MutationsAPI.AddNewMentalMutation(list, 2);
						}
						else
						{
							MutationsAPI.AddNewPhysicalMutation(list, 2);
						}
						for (int j = 0; j < list.Count - 1; j++)
						{
							if (list[j].GetDisplayName() == list[list.Count - 1].GetDisplayName())
							{
								list.RemoveAt(list.Count - 1);
								goto IL_008c;
							}
							foreach (BaseMutation item in list2)
							{
								if (item.GetDisplayName() == list[list.Count - 1].GetDisplayName())
								{
									list.RemoveAt(list.Count - 1);
									goto IL_008c;
								}
							}
						}
						goto IL_01a5;
						IL_008c:;
					}
					break;
					IL_01a5:;
				}
				if (list2.Count > 0)
				{
					Popup.Show("You gain " + list2[0].GetDisplayName() + "!");
					JournalAPI.AddAccomplishment("Your larva gestated and you gained the " + list2[0].GetDisplayName() + " mutation.", null, null, null, "general", MuralCategory.Generic, MuralWeight.Medium, null, -1L);
					part.AddMutation(list2[0]);
				}
			}
		}
		return true;
	}
}
