using System;
using System.Text;

namespace XRL.World.Parts;

[Serializable]
public class ProgrammableRecoiler : IProgrammableRecoiler
{
	public static readonly string COMMAND_NAME = "ImprintRecoiler";

	public string _NameElide;

	[NonSerialized]
	private int NameCacheTick;

	[NonSerialized]
	private string NameCache;

	[NonSerialized]
	private string NameElideWork;

	public string NameElide
	{
		get
		{
			return _NameElide;
		}
		set
		{
			_NameElide = value;
			NameElideWork = null;
		}
	}

	public override void ProgrammedForLocation(Zone Z, Cell C)
	{
		NameCache = null;
		NameCacheTick = 0;
	}

	public override bool SameAs(IPart p)
	{
		if ((p as ProgrammableRecoiler)._NameElide != _NameElide)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetDisplayNameEvent>.ID && (ID != GetInventoryActionsEvent.ID || (!Reprogrammable && TimesProgrammed >= 1)))
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if ((Reprogrammable || TimesProgrammed < 1) && IsObjectActivePartSubject(IComponent<GameObject>.ThePlayer))
		{
			E.AddAction("Imprint", "imprint", COMMAND_NAME, null, 'i', FireOnActor: false, (TimesProgrammed < 1) ? 200 : 0);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == COMMAND_NAME && ProgramRecoiler(E.Actor, E))
		{
			E.Actor.UseEnergy(1000, "Item Recoiler Imprint");
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			if (The.ZoneManager != null && (NameCache == null || The.ZoneManager.NameUpdateTick > NameCacheTick))
			{
				if (ParentObject.TryGetPart<Teleporter>(out var Part))
				{
					string destinationZone = Part.DestinationZone;
					if (!destinationZone.IsNullOrEmpty())
					{
						string text = FindZoneName(destinationZone);
						if (!text.IsNullOrEmpty())
						{
							text += " ";
						}
						NameCache = text;
					}
				}
				NameCacheTick = The.ZoneManager.NameUpdateTick;
			}
			if (!NameCache.IsNullOrEmpty())
			{
				string text2 = E.DB.PrimaryBase;
				int num = 10;
				if (text2 == null)
				{
					text2 = "recoiler";
				}
				else
				{
					num = E.DB[text2];
					E.DB.Remove(text2);
				}
				StringBuilder stringBuilder = Event.NewStringBuilder();
				stringBuilder.Append(NameCache);
				if (!NameElide.IsNullOrEmpty())
				{
					if (NameElideWork == null)
					{
						NameElideWork = NameElide + " ";
					}
					if (text2.Contains(NameElideWork))
					{
						text2 = text2.Replace(NameElideWork, "");
					}
					if (text2.Contains(NameElide))
					{
						text2 = text2.Replace(NameElide, "");
					}
				}
				stringBuilder.Append(text2);
				E.AddBase(stringBuilder.ToString(), num - 10);
			}
		}
		return base.HandleEvent(E);
	}

	private static string FindZoneName(string ZoneID)
	{
		string text = null;
		if (The.ZoneManager.GetZoneHasProperName(ZoneID))
		{
			text = The.ZoneManager.GetZoneBaseDisplayName(ZoneID);
		}
		if (text.IsNullOrEmpty())
		{
			text = The.ZoneManager.GetZoneNameContext(ZoneID) ?? The.ZoneManager.GetZoneReferenceDisplayName(ZoneID);
		}
		if (text.StartsWith("the ") || text.StartsWith("The "))
		{
			text = text.Substring(4);
		}
		else if (text.StartsWith("an ") || text.StartsWith("An "))
		{
			text = text.Substring(3);
		}
		else if (text.StartsWith("a ") || text.StartsWith("A "))
		{
			text = text.Substring(2);
		}
		if (text.EndsWith(", surface"))
		{
			text = text.Substring(0, text.Length - ", surface".Length);
		}
		return text;
	}
}
