using System;
using XRL.UI;

namespace XRL.World.Conversations.Parts;

public class TakeLiquid : IConversationPart
{
	public string Liquids;

	public string Amount = "1";

	public string Message;

	public bool Destroy;

	public bool Require = true;

	public TakeLiquid()
	{
		Priority = -1000;
	}

	public override void LoadText(string Text)
	{
		Message = Text;
	}

	public TakeLiquid(string Liquids)
	{
		this.Liquids = Liquids;
	}

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && (!Require || ID != EnterElementEvent.ID))
		{
			return ID == EnteredElementEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnterElementEvent E)
	{
		if (!Execute())
		{
			if (!Message.IsNullOrEmpty())
			{
				Popup.Show(Message);
			}
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredElementEvent E)
	{
		Execute(Consume: true);
		return base.HandleEvent(E);
	}

	public bool Execute(bool Consume = false)
	{
		string[] array = Liquids?.Split(',') ?? new string[0];
		int[] array2 = new int[array.Length];
		bool flag = Amount == "*" || Amount.EqualsNoCase("all");
		int num = (flag ? int.MaxValue : Amount.RollCached());
		int i = 0;
		for (int num2 = array.Length; i < num2; i++)
		{
			if (num <= 0)
			{
				break;
			}
			num -= (array2[i] = Math.Min(The.Player.GetFreeDrams(array[i]), num));
		}
		if (flag || num <= 0)
		{
			if (Consume)
			{
				int j = 0;
				for (int num3 = array.Length; j < num3; j++)
				{
					The.Player.UseDrams(array2[j], array[j]);
					if (!Destroy)
					{
						The.Speaker.GiveDrams(array2[j], array[j]);
					}
				}
			}
			return true;
		}
		return false;
	}
}
