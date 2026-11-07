using System;
using System.Collections.Generic;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class RecoilAbility : IPart
{
	public static readonly string COMMAND_NAME = "CommandRecoil";

	public Guid ActivatedAbilityID = Guid.Empty;

	[NonSerialized]
	private static List<GameObject> Recoilers = new List<GameObject>(32);

	public override void Initialize()
	{
		ActivatedAbilityID = AddMyActivatedAbility("Recoil", COMMAND_NAME, "Items", null, "\u001b", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: false, IsWorldMapUsable: true);
		base.Initialize();
	}

	public override void Remove()
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		base.Remove();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<CommandEvent>.ID)
		{
			return ID == GetMovementCapabilitiesEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetMovementCapabilitiesEvent E)
	{
		E.Add("Recoil", COMMAND_NAME, 30000, MyActivatedAbility(ActivatedAbilityID));
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == COMMAND_NAME)
		{
			List<GameObject> list = GetRecoilersEvent.GetFor(E.Actor);
			if (list == null || list.Count <= 0)
			{
				return E.Actor.Fail("You do not have any recoilers.");
			}
			Recoilers.Clear();
			Recoilers.AddRange(list);
			if (Recoilers.Count > 1)
			{
				Recoilers.Sort(SortRecoilers);
			}
			int num = 0;
			bool flag;
			do
			{
				GameObject gameObject = Popup.PickGameObject("Use which recoiler?", Recoilers, AllowEscape: true, ShowContext: false, UseYourself: true, LabelRecoiler, num);
				if (gameObject == null)
				{
					return false;
				}
				num = Recoilers.IndexOf(gameObject);
				flag = gameObject.Twiddle();
				if (flag)
				{
					return false;
				}
				for (int num2 = Recoilers.Count - 1; num2 >= 0; num2--)
				{
					if (Recoilers[num2] == null || Recoilers[num2].IsNowhere())
					{
						Recoilers.RemoveAt(num2);
						if (num >= num2)
						{
							num--;
						}
					}
				}
			}
			while (!flag);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	private string LabelRecoiler(GameObject Object)
	{
		string text = Object.GetFirstPartDescendedFrom<ITeleporter>()?.GetStatusSummary();
		if (text == "EMP" || text == "warming up")
		{
			text = null;
		}
		return text;
	}

	private int SortRecoilers(GameObject A, GameObject B)
	{
		ITeleporter firstPartDescendedFrom = A.GetFirstPartDescendedFrom<ITeleporter>();
		ITeleporter firstPartDescendedFrom2 = B.GetFirstPartDescendedFrom<ITeleporter>();
		int num = (firstPartDescendedFrom != null).CompareTo(firstPartDescendedFrom2 != null);
		if (num != 0)
		{
			return num;
		}
		long num2 = The.CurrentTurn - firstPartDescendedFrom.LastTurnUsed;
		long value = The.CurrentTurn - firstPartDescendedFrom2.LastTurnUsed;
		int num3 = num2.CompareTo(value);
		if (num3 != 0)
		{
			return num3;
		}
		return A.GetCachedDisplayNameForSort().CompareTo(B.GetCachedDisplayNameForSort());
	}
}
