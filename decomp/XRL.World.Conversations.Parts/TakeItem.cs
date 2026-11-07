using System;
using XRL.Collections;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World.Conversations.Parts;

public class TakeItem : IConversationPart
{
	public string Blueprints;

	public string IDs;

	public string Amount = "1";

	public string Message;

	public bool Unsellable = true;

	public bool ClearQuest;

	public bool Remove;

	public bool AllowTemporary;

	public bool Require = true;

	public bool FromSpeaker;

	public bool Destroy
	{
		get
		{
			return Remove;
		}
		set
		{
			Remove = value;
		}
	}

	public TakeItem()
	{
		Priority = -1000;
	}

	public TakeItem(string Blueprints)
	{
		this.Blueprints = Blueprints;
	}

	public override void LoadText(string Text)
	{
		Message = Text;
	}

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && (!Require || ID != EnterElementEvent.ID))
		{
			if (!Require)
			{
				return ID == EnteredElementEvent.ID;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(EnterElementEvent E)
	{
		if (!Execute())
		{
			try
			{
				GameObject subject = (FromSpeaker ? The.Speaker : The.Player);
				GameObject gameObject = null;
				if (!Blueprints.IsNullOrEmpty())
				{
					gameObject = GameObject.CreateSample(Blueprints.GetRandomSubstring(','));
				}
				string text = Message.Coalesce("=subject.T= =verb:do= not have =object.an=.");
				if (gameObject != null || !text.Contains("=object"))
				{
					Popup.ShowFail(GameText.VariableReplace(text, subject, gameObject));
				}
			}
			catch (Exception x)
			{
				MetricsManager.LogException("Require conversation item", x);
			}
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredElementEvent E)
	{
		Execute();
		return base.HandleEvent(E);
	}

	public bool Execute()
	{
		GameObject gameObject = (FromSpeaker ? The.Speaker : The.Player);
		GameObject gameObject2 = (FromSpeaker ? The.Player : The.Speaker);
		using ScopeDisposedList<GameObject> scopeDisposedList = ScopeDisposedList<GameObject>.GetFromPool();
		gameObject.GetContents(scopeDisposedList);
		string[] self = Blueprints?.Split(',') ?? Array.Empty<string>();
		string[] self2 = IDs?.Split(',') ?? Array.Empty<string>();
		bool flag = Amount == "*" || Amount.EqualsNoCase("all");
		int num = (flag ? int.MaxValue : Amount.RollCached());
		int i = 0;
		for (int count = scopeDisposedList.Count; i < count && num > 0; i++)
		{
			GameObject gameObject3 = scopeDisposedList[i];
			if ((!self.Contains(gameObject3.Blueprint) && !self2.Contains(gameObject3.GetStringProperty("id"))) || (!AllowTemporary && gameObject3.IsTemporary))
			{
				continue;
			}
			int num2 = 1;
			Stacker stacker = gameObject3.Stacker;
			if (stacker != null && stacker != null)
			{
				num2 = stacker.Number;
				if (num2 > num)
				{
					stacker.SplitStack(num, The.Player);
					num2 = num;
				}
			}
			if (Remove)
			{
				if (!gameObject3.TryRemoveFromContext())
				{
					Popup.ShowFail("You cannot give " + gameObject3.t() + "!");
					continue;
				}
			}
			else
			{
				if (!gameObject2.ReceiveObject(gameObject3))
				{
					Popup.ShowFail("You cannot give " + gameObject3.t() + "!");
					gameObject.ReceiveObject(gameObject3);
					continue;
				}
				Popup.Show(gameObject2.Does("take", int.MaxValue, null, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " " + gameObject3.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: true, Short: true, BaseOnly: false, null, IndicateHidden: false, SecondPerson: true, Reflexive: false, null, AsPossessed: false) + ".");
				if (Unsellable)
				{
					gameObject3.SetIntProperty("WontSell", 1);
				}
				if (ClearQuest)
				{
					gameObject3.Physics.Category = gameObject3.GetStringProperty("OriginalCategory") ?? gameObject3.GetBlueprint().GetPartParameter("Physics", "Category", "Miscellaneous");
					gameObject3.RemoveProperty("QuestItem");
					gameObject3.RemoveProperty("NoAIEquip");
				}
			}
			num -= num2;
		}
		return flag || num <= 0;
	}
}
