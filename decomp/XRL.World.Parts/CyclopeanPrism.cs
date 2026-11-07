using System;
using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class CyclopeanPrism : IPart
{
	public int TurnCount;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<AfterGameLoadedEvent>.ID && ID != SingletonEvent<BeginTakeActionEvent>.ID && ID != EnteredCellEvent.ID && ID != PooledEvent<GenericDeepNotifyEvent>.ID && ID != PooledEvent<GetPrecognitionRestoreGameStateEvent>.ID && ID != EquippedEvent.ID && ID != UnequippedEvent.ID && ID != PooledEvent<RealityStabilizeEvent>.ID)
		{
			return ID == PooledEvent<ReplaceInContextEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(RealityStabilizeEvent E)
	{
		if (ParentObject.Equipped != null && E.Check())
		{
			ParentObject.Equipped.ApplyEffect(new Dazed(1));
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterGameLoadedEvent E)
	{
		string precognitionTransferKey = GetPrecognitionTransferKey();
		if (The.Game.StringGameState.TryGetValue(precognitionTransferKey, out var value))
		{
			The.Game.RemoveStringGameState(precognitionTransferKey);
			if (ParentObject.Equipped == null)
			{
				PtohAnnoyed(The.ZoneManager.FindObjectByID(value));
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		if (E.Actor.IsPlayer())
		{
			Achievement.WIELD_PRISM.Unlock();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		ResetPrism();
		if (!E.Actor.IsDying)
		{
			PtohAnnoyed(E.Actor);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (E.Object == ParentObject.Equipped)
		{
			TurnCount++;
			if (E.Object == null || !E.Object.HasStat("Ego") || !E.Object.HasStat("Willpower"))
			{
				ResetPrism();
			}
			else
			{
				int num = E.Object.Stat("Ego");
				if (num <= 15)
				{
					ParentObject.Render.DisplayName = "{{K|amaranthine}} prism";
					ParentObject.SetStringProperty("EquipmentFrameColors", "KKKK");
				}
				else if (num <= 19)
				{
					ParentObject.Render.DisplayName = "{{K|amara{{y|n}}thine}} prism";
					ParentObject.SetStringProperty("EquipmentFrameColors", "yKKy");
				}
				else if (num <= 23)
				{
					ParentObject.Render.DisplayName = "{{K|amar{{y|a{{Y|n}}t}}hine}} prism";
					ParentObject.SetStringProperty("EquipmentFrameColors", "yKyY");
				}
				else if (num <= 27)
				{
					ParentObject.Render.DisplayName = "{{K|am{{y|ar{{Y|a{{R|n}}t}}hi}}ne}} prism";
					ParentObject.SetStringProperty("EquipmentFrameColors", "ryYK");
				}
				else if (num <= 31)
				{
					ParentObject.Render.DisplayName = "{{y|am{{Y|a{{y|r{{r|a{{R|n}}t}}h}}i}}ne}} prism";
					ParentObject.SetStringProperty("EquipmentFrameColors", "yYrR");
				}
				else
				{
					ParentObject.Render.DisplayName = "{{r|a{{R|m{{Y|a{{y|r{{r|a{{R|n}}t}}h}}i}}n}}e}} prism";
					ParentObject.SetStringProperty("EquipmentFrameColors", "yRYr");
				}
				if (TurnCount >= 4800)
				{
					if (E.Object.Stat("Willpower") <= 1)
					{
						if (E.Object.IsPlayer())
						{
							Achievement.WANDER_DARKLING.Unlock();
						}
						E.Object.Die(ParentObject, null, "You had a dream, which was not all a dream. The bright sun was extinguish'd, and the stars did wander darkling in the eternal space.", ParentObject.It + " had a dream, which was not all a dream. The bright sun was extinguish'd, and the stars did wander darkling in the eternal space.");
					}
					Armor part = ParentObject.GetPart<Armor>();
					part.Ego++;
					part.Willpower--;
					part.UpdateStatShifts();
					TurnCount = 0;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		ResetPrism();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetPrecognitionRestoreGameStateEvent E)
	{
		if (ParentObject.Equipped == E.Object)
		{
			E.Set(GetPrecognitionTransferKey(), E.Object.ID);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ReplaceInContextEvent E)
	{
		PtohAnnoyed();
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public void PtohAnnoyed(GameObject who = null)
	{
		if (who == null)
		{
			who = ParentObject.Holder ?? The.Player;
			if (who == null)
			{
				return;
			}
		}
		if (who.IsPlayer())
		{
			Popup.Show("From across the psychic sea, you feel the glare of unseen eyes. Someone is disappointed in you.");
			The.Game.PlayerReputation.Modify("Entropic", -100, "PtohDispleasure");
			Achievement.DISAPPOINT_HEB.Unlock();
		}
		Cell cell = who.CurrentCell;
		if (cell == null || cell.ParentZone == null)
		{
			return;
		}
		for (int num = Stat.Random(2, 5); num >= 0; num--)
		{
			Cell cell2 = null;
			int num2 = 0;
			while (++num2 < 100)
			{
				cell2 = cell.ParentZone.GetRandomCell();
				int num3 = cell2.PathDistanceTo(cell);
				if (num3 >= 4 && num3 <= 12 && !cell2.HasObjectWithPart("Brain") && !cell2.HasObjectWithPart("SpaceTimeVortex") && !cell2.HasObjectWithPart("SpaceTimeRift") && IComponent<GameObject>.CheckRealityDistortionAccessibility(null, cell2, null, ParentObject))
				{
					break;
				}
			}
			if (num2 < 100)
			{
				cell2?.AddObject("Space-Time Vortex");
			}
		}
	}

	public void ResetPrism()
	{
		Armor part = ParentObject.GetPart<Armor>();
		part.Ego = 1;
		part.Willpower = -1;
		ParentObject.Render.DisplayName = "{{K|amaranthine}} prism";
	}

	private string GetPrecognitionTransferKey()
	{
		return "AmaranthinePrism" + ParentObject.ID + "EquippedAtPrecognitionRestore";
	}
}
