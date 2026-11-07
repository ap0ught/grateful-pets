using System;
using System.Collections.Generic;

namespace Qud.API;

public static class MuralCategoryHelpers
{
	public static Dictionary<MuralCategory, string> muralCategoryToChronologyCategory = new Dictionary<MuralCategory, string>
	{
		{
			MuralCategory.Generic,
			"Notes"
		},
		{
			MuralCategory.IsBorn,
			"Milestones"
		},
		{
			MuralCategory.Dies,
			"Milestones"
		},
		{
			MuralCategory.CrownedSultan,
			"Milestones"
		},
		{
			MuralCategory.CreatesSomething,
			"Discoveries"
		},
		{
			MuralCategory.LearnsSecret,
			"Discoveries"
		},
		{
			MuralCategory.FindsObject,
			"Discoveries"
		},
		{
			MuralCategory.WieldsItemInBattle,
			"Discoveries"
		},
		{
			MuralCategory.VisitsLocation,
			"Travels"
		},
		{
			MuralCategory.HasInspiringExperience,
			"Travels"
		},
		{
			MuralCategory.DoesBureaucracy,
			"Travels"
		},
		{
			MuralCategory.WeirdThingHappens,
			"Happenings"
		},
		{
			MuralCategory.CommitsFolly,
			"Happenings"
		},
		{
			MuralCategory.EnduresHardship,
			"Happenings"
		},
		{
			MuralCategory.BodyExperienceBad,
			"Happenings"
		},
		{
			MuralCategory.BodyExperienceGood,
			"Happenings"
		},
		{
			MuralCategory.BodyExperienceNeutral,
			"Happenings"
		},
		{
			MuralCategory.DoesSomethingRad,
			"Happenings"
		},
		{
			MuralCategory.DoesSomethingHumble,
			"Happenings"
		},
		{
			MuralCategory.DoesSomethingDestructive,
			"Happenings"
		},
		{
			MuralCategory.BecomesLoved,
			"Meetings"
		},
		{
			MuralCategory.Slays,
			"Meetings"
		},
		{
			MuralCategory.Treats,
			"Meetings"
		},
		{
			MuralCategory.MeetsWithCounselors,
			"Meetings"
		},
		{
			MuralCategory.Resists,
			"Meetings"
		},
		{
			MuralCategory.AppeasesBaetyl,
			"Meetings"
		}
	};

	public static MuralCategory parseCategory(string value)
	{
		if (value.IsNullOrEmpty())
		{
			return MuralCategory.DoesSomethingRad;
		}
		try
		{
			return (MuralCategory)Enum.Parse(typeof(MuralCategory), value);
		}
		catch
		{
			MetricsManager.LogError("Unknown hagiographic category: " + value);
			return MuralCategory.DoesSomethingRad;
		}
	}

	public static MuralWeight parseWeight(string value)
	{
		if (value.IsNullOrEmpty())
		{
			return MuralWeight.Medium;
		}
		try
		{
			return (MuralWeight)Enum.Parse(typeof(MuralWeight), value);
		}
		catch
		{
			MetricsManager.LogError("Unknown hagiographic category: " + value);
			return MuralWeight.Medium;
		}
	}
}
