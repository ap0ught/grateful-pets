using System;
using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using XRL.Language;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class SapChargeOnHit : IActivePart
{
	public int Chance = 100;

	public int ChanceEach = 100;

	public string SaveStat;

	public string SaveDifficultyStat;

	public int SaveTarget = 15;

	public string SaveVs = "ChargeDrain";

	public string Amount = "2d6";

	public string RequireDamageAttribute;

	public string LossAmount;

	public string LossFactor;

	public bool RequireAvailableStorage;

	public bool TransientStorageSatisfies = true;

	public bool Single;

	public bool LiveChargeOnly = true;

	public string ChargeStoredMessage = "A {{W|ribbon of electricity}} leaps from %o to %w.";

	public string ChargeNotStoredMessage = "A {{W|ribbon of electricity}} leaps from %o into the air.";

	public bool DrawZapOnMessage = true;

	public bool ForceCharge = true;

	public string BehaviorDescription;

	public SapChargeOnHit()
	{
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		SapChargeOnHit sapChargeOnHit = p as SapChargeOnHit;
		if (sapChargeOnHit.Chance != Chance)
		{
			return false;
		}
		if (sapChargeOnHit.ChanceEach != ChanceEach)
		{
			return false;
		}
		if (sapChargeOnHit.SaveStat != SaveStat)
		{
			return false;
		}
		if (sapChargeOnHit.SaveDifficultyStat != SaveDifficultyStat)
		{
			return false;
		}
		if (sapChargeOnHit.SaveTarget != SaveTarget)
		{
			return false;
		}
		if (sapChargeOnHit.SaveVs != SaveVs)
		{
			return false;
		}
		if (sapChargeOnHit.Amount != Amount)
		{
			return false;
		}
		if (sapChargeOnHit.RequireDamageAttribute != RequireDamageAttribute)
		{
			return false;
		}
		if (sapChargeOnHit.LossAmount != LossAmount)
		{
			return false;
		}
		if (sapChargeOnHit.LossFactor != LossFactor)
		{
			return false;
		}
		if (sapChargeOnHit.RequireAvailableStorage != RequireAvailableStorage)
		{
			return false;
		}
		if (sapChargeOnHit.TransientStorageSatisfies != TransientStorageSatisfies)
		{
			return false;
		}
		if (sapChargeOnHit.Single != Single)
		{
			return false;
		}
		if (sapChargeOnHit.LiveChargeOnly != LiveChargeOnly)
		{
			return false;
		}
		if (sapChargeOnHit.ChargeStoredMessage != ChargeStoredMessage)
		{
			return false;
		}
		if (sapChargeOnHit.ChargeNotStoredMessage != ChargeNotStoredMessage)
		{
			return false;
		}
		if (sapChargeOnHit.DrawZapOnMessage != DrawZapOnMessage)
		{
			return false;
		}
		if (sapChargeOnHit.ForceCharge != ForceCharge)
		{
			return false;
		}
		if (sapChargeOnHit.BehaviorDescription != BehaviorDescription)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			if (ID == GetShortDescriptionEvent.ID)
			{
				return !string.IsNullOrEmpty(BehaviorDescription);
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (!string.IsNullOrEmpty(BehaviorDescription))
		{
			E.Postfix.AppendRules(BehaviorDescription);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AttackerHit");
		Registrar.Register("WeaponHit");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AttackerHit" || E.ID == "WeaponHit")
		{
			CheckApply(E);
		}
		return base.FireEvent(E);
	}

	public GameObject SapCharge(GameObject obj, ref int ChargeStored, int useChanceEach)
	{
		if (!useChanceEach.in100())
		{
			return null;
		}
		int num = obj.QueryCharge(LiveChargeOnly, 0L);
		if (num <= 0)
		{
			return null;
		}
		int num2 = Amount.RollCached();
		if (RequireAvailableStorage)
		{
			int num3 = ParentObject.QueryChargeStorage(TransientStorageSatisfies);
			if (num3 < num2)
			{
				num2 = num3;
				if (num2 <= 0)
				{
					return null;
				}
			}
		}
		if (num2 > num)
		{
			num2 = num;
		}
		obj.SplitFromStack();
		if (!obj.UseCharge(num2, LiveChargeOnly, 0L))
		{
			return null;
		}
		if (!string.IsNullOrEmpty(LossFactor))
		{
			num2 /= LossFactor.RollCached();
		}
		if (!string.IsNullOrEmpty(LossAmount))
		{
			num2 -= LossAmount.RollCached();
		}
		if (num2 > 0)
		{
			ChargeStored += ParentObject.ChargeAvailable(num2, 0L, 1, ForceCharge);
		}
		return obj;
	}

	public void PerformSapCharge(GameObject obj, ref int ChargeStored, int useChanceEach, List<GameObject> Affected = null, bool Single = false)
	{
		int count = obj.Count;
		if (count > 1)
		{
			for (int i = 0; i < count; i++)
			{
				GameObject gameObject = SapCharge(obj, ref ChargeStored, useChanceEach);
				if (Affected != null && gameObject != null && gameObject.IsValid())
				{
					Affected.Add(gameObject);
				}
				if (obj.IsInvalid() || obj == gameObject || (Single && gameObject != null))
				{
					break;
				}
			}
		}
		else
		{
			GameObject gameObject2 = SapCharge(obj, ref ChargeStored, useChanceEach);
			if (Affected != null && gameObject2 != null)
			{
				Affected.Add(gameObject2);
			}
		}
	}

	public bool CheckApply(Event E)
	{
		if (!string.IsNullOrEmpty(RequireDamageAttribute) && (!(E.GetParameter("Damage") is Damage damage) || !damage.HasAttribute(RequireDamageAttribute)))
		{
			return false;
		}
		if (IsDisabled(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
		GameObject gameObjectParameter2 = E.GetGameObjectParameter("Defender");
		GameObject gameObjectParameter3 = E.GetGameObjectParameter("Projectile");
		GameObject parentObject = ParentObject;
		GameObject subject = gameObjectParameter2;
		GameObject projectile = gameObjectParameter3;
		if (!GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, parentObject, "Part SapChargeOnHit Activation Main", Chance, subject, projectile).in100())
		{
			return false;
		}
		if (!string.IsNullOrEmpty(SaveStat) && gameObjectParameter2.MakeSave(SaveStat, SaveTarget, ParentObject.Equipped ?? ParentObject, SaveDifficultyStat, SaveVs, IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, ParentObject))
		{
			return false;
		}
		GameObject parentObject2 = ParentObject;
		projectile = gameObjectParameter2;
		subject = gameObjectParameter3;
		int useChanceEach = GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, parentObject2, "SapChargeOnHitActivationEach", ChanceEach, projectile, subject);
		bool flag = (!string.IsNullOrEmpty(ChargeStoredMessage) || !string.IsNullOrEmpty(ChargeNotStoredMessage)) && !E.IsSilent() && (gameObjectParameter.IsPlayer() || gameObjectParameter2.IsPlayer() || IComponent<GameObject>.Visible(gameObjectParameter2));
		List<GameObject> list = Event.NewGameObjectList();
		list.Add(gameObjectParameter2);
		Inventory inventory = gameObjectParameter2.Inventory;
		if (inventory != null)
		{
			list.AddRange(inventory.Objects);
		}
		Body body = gameObjectParameter2.Body;
		if (body != null)
		{
			body.GetEquippedObjects(list);
			body.GetInstalledCybernetics(list);
		}
		if (Single)
		{
			list.ShuffleInPlace();
		}
		int ChargeStored = 0;
		List<GameObject> list2 = Event.NewGameObjectList();
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			GameObject gameObject = list[i];
			PerformSapCharge(gameObject, ref ChargeStored, useChanceEach, list2, Single);
			if (Single && list2.Count > 0)
			{
				break;
			}
			if (count != list.Count)
			{
				count = list.Count;
				if (i < count && list[i] != gameObject)
				{
					i--;
				}
			}
		}
		if (flag && list2.Count > 0)
		{
			bool flag2 = list2.Contains(gameObjectParameter2);
			if (flag2)
			{
				list2.Remove(gameObjectParameter2);
			}
			string text = ((ChargeStored > 0) ? ChargeStoredMessage : ChargeNotStoredMessage);
			if (text == "")
			{
				text = null;
			}
			if (text != null && (text.Contains("%o") || text.Contains("%O")))
			{
				Dictionary<string, int> dictionary = new Dictionary<string, int>(list2.Count);
				Dictionary<string, GameObject> dictionary2 = new Dictionary<string, GameObject>(list2.Count);
				foreach (GameObject item in list2)
				{
					if (item.IsValid())
					{
						string shortDisplayName = item.ShortDisplayName;
						if (dictionary.ContainsKey(shortDisplayName))
						{
							dictionary[shortDisplayName] += item.Count;
						}
						else
						{
							dictionary.Add(shortDisplayName, item.Count);
						}
						if (!dictionary2.ContainsKey(shortDisplayName))
						{
							dictionary2.Add(shortDisplayName, item);
						}
					}
				}
				List<string> list3 = new List<string>(dictionary.Count);
				StringBuilder stringBuilder = Event.NewStringBuilder();
				foreach (string key in dictionary.Keys)
				{
					int num = dictionary[key];
					stringBuilder.Length = 0;
					if (num == 1)
					{
						stringBuilder.Append(dictionary2[key].a).Append(key);
					}
					else
					{
						stringBuilder.Append(Grammar.Cardinal(num)).Append(' ').Append(dictionary2[key].GetPluralName());
					}
					list3.Add(stringBuilder.ToString());
				}
				if (flag2 || list3.Count > 0)
				{
					StringBuilder stringBuilder2 = Event.NewStringBuilder();
					if (flag2)
					{
						stringBuilder2.Append(gameObjectParameter2.IsPlayer() ? "you" : gameObjectParameter2.t());
						if (list3.Count > 0)
						{
							stringBuilder2.Append(" and ").Append(Grammar.MakeAndList(list3)).Append(" on ")
								.Append(gameObjectParameter2.them);
						}
					}
					else
					{
						stringBuilder2.Append(Grammar.MakeAndList(list3)).Append(" on ").Append(gameObjectParameter2.IsPlayer() ? "your person" : gameObjectParameter2.t());
					}
					if (text.Contains("%o"))
					{
						text = text.Replace("%o", stringBuilder2.ToString());
					}
					if (text.Contains("%O"))
					{
						text = text.Replace("%O", ColorUtility.CapitalizeExceptFormatting(stringBuilder2.ToString()));
					}
				}
				else
				{
					text = null;
				}
			}
			if (text != null)
			{
				if (text.Contains("%a"))
				{
					text = text.Replace("%a", gameObjectParameter.IsPlayer() ? "you" : gameObjectParameter.t());
				}
				if (text.Contains("%A"))
				{
					text = text.Replace("%A", gameObjectParameter.IsPlayer() ? "You" : gameObjectParameter.T());
				}
				if (text.Contains("%d"))
				{
					text = text.Replace("%d", gameObjectParameter2.IsPlayer() ? "you" : gameObjectParameter2.t());
				}
				if (text.Contains("%D"))
				{
					text = text.Replace("%D", gameObjectParameter2.IsPlayer() ? "You" : gameObjectParameter2.T());
				}
				if (text.Contains("%w") || text.Contains("%W"))
				{
					if (ParentObject.IsPlayer())
					{
						text = text.Replace("%w", "you").Replace("%W", "You");
					}
					else
					{
						GameObject equipped = ParentObject.Equipped;
						string text2 = ((equipped == null || ParentObject.HasProperName) ? ParentObject.t() : equipped.poss(ParentObject));
						if (text.Contains("%w"))
						{
							text = text.Replace("%w", text2);
						}
						if (text.Contains("%W"))
						{
							text = text.Replace("%W", ColorUtility.CapitalizeExceptFormatting(text2));
						}
					}
				}
				IComponent<GameObject>.AddPlayerMessage(text);
				if (DrawZapOnMessage)
				{
					DrawZap(gameObjectParameter2, ParentObject);
				}
			}
		}
		E.SetParameter("DidSpecialEffect", 1);
		return true;
	}

	private void DrawZap(Cell O, Cell D)
	{
		if (O == null || D == null)
		{
			return;
		}
		List<Point> list = Zone.Line(O.X, O.Y, D.X, D.Y, ReadOnly: true);
		for (int i = 0; i < list.Count; i++)
		{
			Cell cell = D.ParentZone.GetCell(list[i].X, list[i].Y);
			if (cell == null || !cell.IsVisible())
			{
				continue;
			}
			GameObject firstObject = cell.GetFirstObject();
			if (firstObject != null)
			{
				if (Stat.RandomCosmetic(0, 1) == 0)
				{
					firstObject.ParticleBlip("&W" + (char)Stat.RandomCosmetic(191, 198), 30, 0L);
				}
				else
				{
					firstObject.ParticleBlip("&Y" + (char)Stat.RandomCosmetic(191, 198), 30, 0L);
				}
			}
		}
	}

	private void DrawZap(GameObject O, GameObject D)
	{
		DrawZap(O.CurrentCell, D.CurrentCell);
	}
}
