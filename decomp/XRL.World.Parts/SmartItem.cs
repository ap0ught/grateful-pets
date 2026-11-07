using System;
using System.Collections.Generic;
using System.Text;
using Qud.API;
using XRL.Names;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class SmartItem : IPart
{
	public new string Name;

	public int FeelingTowardsPlayer;

	public List<string> Likes = new List<string>();

	public List<string> Dislikes = new List<string>();

	public bool Initialized;

	public override bool SameAs(IPart p)
	{
		if ((p as SmartItem).Name != Name)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EquippedEvent.ID && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != GetShortDescriptionEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		E.Actor.RegisterPartEvent(this, "Killed");
		E.Actor.RegisterPartEvent(this, "DrinkingFrom");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.UnregisterPartEvent(this, "Killed");
		E.Actor.UnregisterPartEvent(this, "DrinkingFrom");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood())
		{
			E.AddAdjective("{{R|" + Name + "}} the", -60);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.Append("\n{{R|").Append(Name).Append(ParentObject.Is)
			.Append(" sentient and ")
			.Append(ParentObject.it)
			.Append(' ')
			.Append(GetFeelingDescription())
			.Append(".}}");
		AppendLikesAndDislikes(E.Postfix);
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public string GetFeelingDescription()
	{
		if (FeelingTowardsPlayer <= -200)
		{
			return "hates you";
		}
		if (FeelingTowardsPlayer <= -100)
		{
			return "dislikes you";
		}
		if (FeelingTowardsPlayer < 100)
		{
			return "feels neutral towards you";
		}
		if (FeelingTowardsPlayer >= 200)
		{
			return "loves you";
		}
		return "likes you";
	}

	public string GetRandomAttitude()
	{
		int num = Stat.Random(1, 100);
		if (num <= 95)
		{
			return "faction:" + PopulationManager.GenerateOne("RandomFaction").Blueprint;
		}
		if (num <= 99)
		{
			return "drink:" + PopulationManager.GenerateOne("RandomLiquid").Blueprint;
		}
		return "violence:null";
	}

	public string FormatAttitude(string A)
	{
		string[] array = A.Split(':');
		if (array[0] == "faction")
		{
			return Faction.GetFormattedName(array[1]);
		}
		if (array[0] == "drink")
		{
			return "when its wielder drinks " + array[1];
		}
		if (array[0] == "violence")
		{
			return "violence";
		}
		return A;
	}

	public string AppendLikesAndDislikes(StringBuilder SB)
	{
		SB.Append("\n&R").Append(Name);
		SB.Append(" likes ");
		for (int i = 0; i < Likes.Count; i++)
		{
			if (Likes.Count > 1 && i == Likes.Count - 1)
			{
				SB.Append(" and ");
			}
			else if (i > 0)
			{
				SB.Append(", ");
			}
			SB.Append(FormatAttitude(Likes[i]));
		}
		SB.Append("; and hates ");
		for (int j = 0; j < Dislikes.Count; j++)
		{
			if (Dislikes.Count > 1 && j == Dislikes.Count - 1)
			{
				SB.Append(" and ");
			}
			else if (j > 0)
			{
				SB.Append(", ");
			}
			SB.Append(FormatAttitude(Dislikes[j]));
		}
		SB.Append(".");
		return SB.ToString();
	}

	public override void Initialize()
	{
		base.Initialize();
		SetUpSmartItem();
	}

	public void SetUpSmartItem(int nLikes = -1, int nDislikes = -1)
	{
		if (Initialized)
		{
			return;
		}
		Initialized = true;
		Name = NameMaker.MakeName(EncountersAPI.GetACreature());
		if (nLikes == -1)
		{
			nLikes = Stat.Roll("1d5-2");
		}
		if (nLikes < 1)
		{
			nLikes = 1;
		}
		if (nDislikes == -1)
		{
			nDislikes = Stat.Roll("1d5-2");
		}
		if (nDislikes < 1)
		{
			nDislikes = 1;
		}
		for (int i = 0; i < nLikes; i++)
		{
			string randomAttitude = GetRandomAttitude();
			if (Likes.Contains(randomAttitude) || Dislikes.Contains(randomAttitude))
			{
				i--;
			}
			else
			{
				Likes.Add(randomAttitude);
			}
		}
		for (int j = 0; j < nDislikes; j++)
		{
			string randomAttitude2 = GetRandomAttitude();
			if (Likes.Contains(randomAttitude2) || Dislikes.Contains(randomAttitude2))
			{
				j--;
			}
			else
			{
				Dislikes.Add(randomAttitude2);
			}
		}
	}

	public bool EquippedToPlayer()
	{
		if (ParentObject.Equipped != null)
		{
			return ParentObject.Equipped.IsPlayer();
		}
		return false;
	}
}
