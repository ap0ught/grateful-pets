using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ConsoleLib.Console;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class FlashbangGrenade : IGrenade
{
	public int Radius = 4;

	public string Duration = "1d2+4";

	[NonSerialized]
	public static Event eModifyFlashbang = new Event("ModifyFlashbang", "Amount", 0);

	public override bool SameAs(IPart p)
	{
		FlashbangGrenade flashbangGrenade = p as FlashbangGrenade;
		if (flashbangGrenade.Radius != Radius)
		{
			return false;
		}
		if (flashbangGrenade.Duration != Duration)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetComponentNavigationWeightEvent.ID)
		{
			return ID == GetComponentAdjacentNavigationWeightEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetComponentNavigationWeightEvent E)
	{
		E.MinWeight(2);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetComponentAdjacentNavigationWeightEvent E)
	{
		E.MinWeight(2);
		return base.HandleEvent(E);
	}

	protected override bool DoDetonate(Cell C, GameObject Actor = null, GameObject ApparentTarget = null, bool Indirect = false)
	{
		DidX("explode", "with a flash", "!");
		PlayWorldSound(GetPropertyOrTag("DetonatedSound", "Sounds/Grenade/sfx_grenade_flashbang_explode"), 1f, 0f, Combat: true);
		Flashbang(C, Radius, Duration, IncludeBaseCell: true, ParentObject.GetPhase());
		ParentObject.Destroy(null, Silent: true);
		return true;
	}

	public static void Flashbang(Cell C, int Radius, string Duration, bool IncludeBaseCell, int Phase)
	{
		TextConsole textConsole = Look._TextConsole;
		ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
		XRLCore.Core.RenderMapToBuffer(scrapBuffer);
		List<Cell> list = new List<Cell>();
		C.GetRealAdjacentCells(Radius, list, LocalOnly: false);
		foreach (Cell item in list)
		{
			if (!((item != C) | IncludeBaseCell))
			{
				continue;
			}
			foreach (GameObject item2 in item.GetObjectsWithPart("Brain"))
			{
				int num = Duration.RollCached();
				if (!item2.PhaseMatches(Phase))
				{
					num = num * 2 / 3;
				}
				item2.FireEvent("FlashbangHit");
				if (item2.HasRegisteredEvent("ModifyFlashbang"))
				{
					eModifyFlashbang.SetParameter("Amount", num);
					item2.FireEvent(eModifyFlashbang);
					int intParameter = eModifyFlashbang.GetIntParameter("Amount");
					if (intParameter < num)
					{
						if (intParameter <= 0)
						{
							continue;
						}
						num = intParameter;
					}
				}
				if (item2.FireEvent("ApplyAttackConfusion"))
				{
					item2.ApplyEffect(new Confused(num, Radius * 5, 7, "SensoryConfusion"));
				}
			}
		}
		if (!C.ParentZone.IsActive())
		{
			return;
		}
		bool flag = false;
		StringBuilder stringBuilder = Event.NewStringBuilder();
		for (int i = 0; i < Radius; i++)
		{
			XRLCore.Core.RenderMapToBuffer(scrapBuffer);
			foreach (Cell item3 in list)
			{
				if (item3.ParentZone == C.ParentZone && item3.IsVisible())
				{
					flag = true;
					scrapBuffer.Goto(item3.X, item3.Y);
					if (30.in100())
					{
						scrapBuffer.Write("&K ");
						continue;
					}
					int num2 = Stat.Random(1, 3);
					scrapBuffer.Write(stringBuilder.Clear().Append('&').Append(XRL.World.Capabilities.Phase.getRandomFlashColor(Phase))
						.Append("^Y")
						.Append(num2 switch
						{
							1 => '*', 
							2 => ' ', 
							_ => '.', 
						})
						.ToString());
				}
			}
			if (flag)
			{
				textConsole.DrawBuffer(scrapBuffer);
				Thread.Sleep(25);
			}
		}
	}
}
