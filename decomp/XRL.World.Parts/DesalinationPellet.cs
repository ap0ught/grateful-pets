using System;
using System.Collections.Generic;
using XRL.Liquids;
using XRL.UI;
using XRL.World.Text;

namespace XRL.World.Parts;

[Serializable]
public class DesalinationPellet : IPart
{
	public string RemoveLiquid = "salt";

	public string RemoveLiquidAmount = "200";

	public string Message = "=subject.T= =verb:fizzle= for several seconds.";

	public string DestroyMessage = "=subject.T= =verb:fizzle= for several seconds, then =verb:evaporate=.";

	public string ConvertMessage = "=object.T= =object.verb:react= strangely with =subject.t= and =object.verb:convert= =pronouns.objective= to =newLiquid=.";

	public bool Convert = true;

	public override bool SameAs(IPart p)
	{
		DesalinationPellet desalinationPellet = p as DesalinationPellet;
		if (desalinationPellet.RemoveLiquid != RemoveLiquid)
		{
			return false;
		}
		if (desalinationPellet.RemoveLiquidAmount != RemoveLiquidAmount)
		{
			return false;
		}
		if (desalinationPellet.Message != Message)
		{
			return false;
		}
		if (desalinationPellet.DestroyMessage != DestroyMessage)
		{
			return false;
		}
		if (desalinationPellet.ConvertMessage != ConvertMessage)
		{
			return false;
		}
		if (desalinationPellet.Convert != Convert)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EnteredCellEvent.ID && ID != InventoryActionEvent.ID)
		{
			return ID == ObjectEnteringCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Apply")
		{
			if (!E.Actor.CheckFrozen(Telepathic: false, Telekinetic: true))
			{
				return false;
			}
			GameObject gameObject = null;
			Inventory inventory = E.Actor.Inventory;
			List<GameObject> list = Event.NewGameObjectList();
			inventory.GetObjects(list);
			gameObject = PickItem.ShowPicker(list, null, PickItem.PickItemDialogStyle.SelectItemDialog, E.Actor);
			if (gameObject == null)
			{
				return false;
			}
			if (gameObject.LiquidVolume == null)
			{
				E.Actor.ShowFailure("It doesn't seem to do anything.");
				return false;
			}
			IComponent<GameObject>.EmitMessage(E.Actor, "You drop " + ParentObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: true) + " into " + gameObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ".\n\n" + PurifyLiquid(gameObject, E.Actor, ShowMessage: false, Single: true, E.Actor.IsPlayer()), ' ', FromDialog: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		GameObject firstObjectWithPart = E.Cell.GetFirstObjectWithPart("LiquidVolume");
		if (firstObjectWithPart != null && firstObjectWithPart.IsOpenLiquidVolume())
		{
			PurifyLiquid(firstObjectWithPart, null, ShowMessage: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteringCellEvent E)
	{
		if (E.Object.IsOpenLiquidVolume())
		{
			PurifyLiquid(E.Object, null, ShowMessage: true);
		}
		return base.HandleEvent(E);
	}

	public string PurifyLiquid(GameObject Item, GameObject Actor = null, bool ShowMessage = false, bool Single = false, bool MakeUnderstood = false)
	{
		LiquidVolume LV = Item.LiquidVolume;
		if (LV == null)
		{
			return "";
		}
		string oldLiquidName = LV.GetLiquidName();
		GameObject purifier = (Single ? ParentObject.RemoveOne() : ParentObject);
		bool flag = false;
		bool flag2 = false;
		if (!RemoveLiquid.IsNullOrEmpty() && LiquidVolume.GetLiquid(RemoveLiquid) != null && LV.ContainsLiquid(RemoveLiquid))
		{
			flag = true;
			int num = 0;
			for (int num2 = purifier.Count; num2 > 0; num2--)
			{
				num += RemoveLiquidAmount.RollCached();
			}
			if (num > 0)
			{
				if (LV.IsPure())
				{
					if (LV.Volume > num)
					{
						LV.Volume -= num;
					}
					else
					{
						LV.Empty();
					}
				}
				else
				{
					int num3 = Math.Min(Math.Max(LV.UpperAmount(RemoveLiquid), 1), num);
					LV.MixWith(new LiquidVolume(RemoveLiquid, -num3, num3));
				}
			}
		}
		if (Convert && LV.Volume > 0)
		{
			foreach (BaseLiquid allLiquid in LiquidVolume.getAllLiquids())
			{
				if (allLiquid.ConversionProduct.IsNullOrEmpty() || !LV.ContainsLiquid(allLiquid.ID))
				{
					continue;
				}
				flag = true;
				int num4 = 0;
				int maxVolume = LV.MaxVolume;
				string liquid = ((LiquidVolume.GetLiquid(allLiquid.ConversionProduct) == null) ? allLiquid.ConversionProduct.CachedNumericDictionaryExpansion().GetRandomElement() : allLiquid.ConversionProduct);
				try
				{
					LV.MaxVolume = LV.Volume;
					if (LV.IsPure())
					{
						num4 = LV.Volume;
						LV.Empty();
					}
					else
					{
						num4 = Math.Max(LV.UpperAmount(allLiquid.ID), 1);
						LV.MixWith(new LiquidVolume(allLiquid.ID, -num4, num4));
					}
					if (num4 > 0)
					{
						LV.MixWith(new LiquidVolume(liquid, num4, num4));
						flag2 = true;
					}
				}
				finally
				{
					LV.MaxVolume = maxVolume;
				}
			}
		}
		string liquidName = LV.GetLiquidName();
		string text;
		if (LV.Volume <= 0)
		{
			text = SetupBuilder(DestroyMessage).ToString();
		}
		else if (flag2 && liquidName != oldLiquidName)
		{
			string liquidDescription = LV.GetLiquidDescription(IncludeAmount: true, IgnoreSeal: true, Stripped: true, Syntactic: true);
			text = SetupBuilder(ConvertMessage).AddReplacer("newLiquid", liquidDescription).ToString();
		}
		else
		{
			text = SetupBuilder(this.Message).ToString();
		}
		string Message = null;
		if (flag)
		{
			LV.Update();
			if (MakeUnderstood)
			{
				purifier.MakeUnderstood(out Message);
			}
		}
		if (ShowMessage)
		{
			if (!text.IsNullOrEmpty())
			{
				IComponent<GameObject>.EmitMessage(Actor ?? Item, text, ' ', FromDialog: true);
			}
			if (!Message.IsNullOrEmpty())
			{
				IComponent<GameObject>.EmitMessage(Actor ?? Item, Message, ' ', FromDialog: true);
			}
		}
		else if (!Message.IsNullOrEmpty())
		{
			text = ((!text.IsNullOrEmpty()) ? (text + "\n\n" + Message) : Message);
		}
		purifier.Obliterate();
		return text;
		ReplaceBuilder SetupBuilder(string message)
		{
			ReplaceBuilder replaceBuilder = message.StartReplace().StripColors();
			if (LV.IsOpenVolume())
			{
				replaceBuilder.AddObject(Item);
			}
			else
			{
				replaceBuilder.AddExplicit(oldLiquidName, Plural: false);
			}
			replaceBuilder.AddObject(purifier);
			return replaceBuilder;
		}
	}
}
