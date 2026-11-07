using System;

namespace XRL.World.Parts;

[Serializable]
public class CallToArmsScore : IPart
{
	public bool person;

	public string category;

	public int impact;

	public CallToArmsScore()
	{
	}

	public CallToArmsScore(int impact, string category, bool person = false)
		: this()
	{
		this.impact = impact;
		this.category = category;
		this.person = person;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<GetDebugInternalsEvent>.ID)
		{
			return ID == OnDestroyObjectEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "Impact", impact);
		E.AddEntry(this, "Category", category);
		E.AddEntry(this, "Person", person);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OnDestroyObjectEvent E)
	{
		if (!ParentObject.IsTemporary && ParentObject.GetIntProperty("IsClone") <= 0 && !ParentObject.HasProperty("EvilTwin"))
		{
			bool flag = true;
			string text = "CallToArmsObjectsDestroyed";
			string iD = ParentObject.ID;
			if (The.Game.HasStringGameState(text))
			{
				if (The.Game.GetStringGameState(text).Split(',').Contains(iD))
				{
					flag = false;
				}
				else
				{
					The.Game.AppendStringGameState(text, iD, ",");
				}
			}
			else
			{
				The.Game.SetStringGameState(text, iD);
			}
			if (flag && person && ParentObject.HasProperName)
			{
				string text2 = "CallToArmsPersonsKilled";
				string text3 = ParentObject.BaseDisplayNameStripped.Replace(",", "");
				if (The.Game.HasStringGameState(text2))
				{
					if (The.Game.GetStringGameState(text2).Split(',').Contains(text3))
					{
						MetricsManager.LogError("destroying person " + ParentObject.DebugName + " already recorded as destroyed in " + text2 + " by name " + text3);
						flag = false;
					}
					else
					{
						The.Game.AppendStringGameState(text2, text3, ",");
					}
				}
				else
				{
					The.Game.SetStringGameState(text2, text3);
				}
			}
			if (flag)
			{
				The.Game.IncrementIntGameState("CallToArmsScore", impact);
				The.Game.IncrementIntGameState(category, 1);
			}
		}
		return base.HandleEvent(E);
	}

	public static bool GiveCallToArmsReward_Top()
	{
		The.Game.PlayerReputation.Modify("Barathrumites", 300, "Quest");
		return true;
	}

	public static bool GiveCallToArmsReward_Mid()
	{
		The.Game.PlayerReputation.Modify("Barathrumites", 200, "Quest");
		return true;
	}

	public static bool GiveCallToArmsReward_Bottom()
	{
		The.Game.PlayerReputation.Modify("Barathrumites", 100, "Quest");
		return true;
	}
}
