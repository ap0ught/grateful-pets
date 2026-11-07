using XRL.UI;

namespace XRL.World.Conversations.Parts;

public class WaterRitualBegin : IWaterRitualPart
{
	public int Drams;

	public override bool Affordable => Drams > 0;

	public override bool Available => The.Speaker.InSameOrAdjacentCellTo(The.Player);

	public override void Awake()
	{
		WaterRitual.Reset();
		Visible = true;
	}

	public override bool WantEvent(int ID, int Propagation)
	{
		if (!base.WantEvent(ID, Propagation) && ID != GetChoiceTagEvent.ID && ID != PrepareTextEvent.ID)
		{
			return ID == EnterElementEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnterElementEvent E)
	{
		if (!The.Speaker.InSameOrAdjacentCellTo(The.Player))
		{
			The.Player.ShowFailure("You are too far from " + The.Speaker.t() + " to perform the water ritual.");
			return false;
		}
		bool flag = false;
		if (!The.Speaker.HasIntProperty("WaterRitualed"))
		{
			string sifrahWaterRitual = Options.SifrahWaterRitual;
			if (sifrahWaterRitual == "Always")
			{
				flag = true;
			}
			else if (sifrahWaterRitual != "Never")
			{
				switch (Popup.ShowYesNoCancel("Do you want to play a game of Sifrah to perform the formal water ritual with " + The.Speaker.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + "? The formal ritual can be much more impactful. If you do not play the game of Sifrah, the informal water ritual will consume 1 dram of " + WaterRitual.LiquidName + "."))
				{
				case DialogResult.Yes:
					flag = true;
					break;
				case DialogResult.Cancel:
					return false;
				}
			}
		}
		if (flag)
		{
			FormalWaterRitualSifrah formalWaterRitualSifrah = new FormalWaterRitualSifrah(The.Speaker);
			formalWaterRitualSifrah.Play(The.Speaker);
			if (formalWaterRitualSifrah.Abort)
			{
				return false;
			}
			The.Speaker.SetIntProperty("SifrahWaterRitual", 1);
			The.Speaker.SetIntProperty("WaterRitualPerformance", formalWaterRitualSifrah.Performance);
		}
		else
		{
			if (!Affordable)
			{
				Popup.ShowFail("You don't have enough " + WaterRitual.Liquid.GetName() + " to begin the ritual.");
				return false;
			}
			The.Player.UseDrams(1, WaterRitual.Liquid.ID);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(PrepareTextEvent E)
	{
		Drams = The.Player.GetFreeDrams(WaterRitual.Liquid.ID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetChoiceTagEvent E)
	{
		E.Tag = "{{" + Lowlight + "|[begin water ritual" + ((Options.SifrahWaterRitual == "Never") ? ("; {{" + Numeric + "|1}} dram of " + (Affordable ? WaterRitual.LiquidName : WaterRitual.LiquidNameStripped)) : "") + "]}}";
		return base.HandleEvent(E);
	}
}
