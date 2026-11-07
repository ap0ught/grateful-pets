using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using XRL.World.Capabilities;
using XRL.World.Tinkering;

namespace XRL.World.Parts;

[Serializable]
public class GenericInventoryRestocker : IPart
{
	public long LastRestockTick;

	public long RestockFrequency = 6000L;

	public int Chance = 100;

	public List<string> Tables;

	public List<string> HeroTables;

	public string Table
	{
		get
		{
			if (!Tables.IsNullOrEmpty())
			{
				return Tables[0];
			}
			return null;
		}
		set
		{
			SetTables(ref Tables, value);
		}
	}

	public string HeroTable
	{
		get
		{
			if (!HeroTables.IsNullOrEmpty())
			{
				return HeroTables[0];
			}
			return null;
		}
		set
		{
			SetTables(ref HeroTables, value);
		}
	}

	[JsonIgnore]
	public double RestockFrequencyDays
	{
		set
		{
			RestockFrequency = (long)(1200.0 * value);
		}
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		Writer.WriteOptimized(LastRestockTick);
		Writer.WriteOptimized(RestockFrequency);
		Writer.WriteOptimized(Chance);
		Writer.Write(Tables);
		Writer.Write(HeroTables);
	}

	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		LastRestockTick = Reader.ReadOptimizedInt64();
		RestockFrequency = Reader.ReadOptimizedInt64();
		Chance = Reader.ReadOptimizedInt32();
		Tables = Reader.ReadStringList();
		HeroTables = Reader.ReadStringList();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<GetDebugInternalsEvent>.ID && ID != SingletonEvent<StartTradeEvent>.ID)
		{
			return ID == PooledEvent<AllowTradeWithNoInventoryEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(AllowTradeWithNoInventoryEvent E)
	{
		return false;
	}

	public override bool HandleEvent(StartTradeEvent E)
	{
		if (LastRestockTick == 0L)
		{
			PerformRestock(Silent: true);
			LastRestockTick = The.Game.TimeTicks;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "LastRestockTick", LastRestockTick);
		E.AddEntry(this, "RestockFrequency", RestockFrequency);
		E.AddEntry(this, "Chance", Chance);
		E.AddEntry(this, "Tables", Tables.IsNullOrEmpty() ? "None" : string.Join(", ", Tables));
		E.AddEntry(this, "HeroTables", HeroTables.IsNullOrEmpty() ? "None" : string.Join(", ", HeroTables));
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		if (LastRestockTick == 0L)
		{
			PerformRestock(Silent: true);
			LastRestockTick = TimeTick;
			return;
		}
		long num = TimeTick - LastRestockTick;
		if (num >= RestockFrequency && ParentObject.InSameZone(The.Player))
		{
			LastRestockTick = TimeTick;
			if (!ParentObject.IsPlayerControlled() && !ParentObject.WasPlayer() && (Chance * (num / RestockFrequency)).in100())
			{
				PerformRestock();
			}
		}
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("MadeHero");
		base.Register(Object, Registrar);
	}

	public static Action<GameObject> GetCraftmarkApplication(GameObject Actor)
	{
		HasMakersMark part = Actor.GetPart<HasMakersMark>();
		string mark = part?.Mark;
		if (!mark.IsNullOrEmpty())
		{
			string markColor = part?.Color ?? "R";
			int.TryParse(Actor.GetPropertyOrTag("HeroGenericInventoryBasicBestowalChances"), out var basicBestowalChances);
			int.TryParse(Actor.GetPropertyOrTag("HeroGenericInventoryBasicBestowalPercentage"), out var basicBestowalPercentage);
			return (GameObject obj) =>
			{
				if (TinkeringHelpers.EligibleForMakersMark(obj))
				{
					int num = 5;
					obj.RequirePart<MakersMark>().AddCrafter(Actor, mark, markColor);
					for (int i = 0; i < basicBestowalChances; i++)
					{
						if (!basicBestowalPercentage.in100())
						{
							break;
						}
						if (RelicGenerator.ApplyBasicBestowal(obj, null, 1, null, Standard: false, ShowInShortDescription: true))
						{
							num += 30;
						}
					}
					obj.RequirePart<Commerce>().Value += num;
				}
			};
		}
		return null;
	}

	public void PerformStock(bool Restock = false, bool Silent = false)
	{
		if (ParentObject.IsTemporary)
		{
			return;
		}
		Inventory inventory = ParentObject.Inventory;
		string context = (Restock ? "Restock" : "Stock");
		Action<GameObject> craftmarkApplication = GetCraftmarkApplication(ParentObject);
		if (!Restock)
		{
			ParentObject.SetIntProperty("Merchant", 1);
			foreach (GameObject @object in inventory.Objects)
			{
				if (!@object.HasPropertyOrTag("norestock") && !@object.HasProperty("_stock"))
				{
					@object.SetIntProperty("norestock", 1);
				}
			}
		}
		int intProperty = ParentObject.GetIntProperty("InventoryTier", ZoneManager.zoneGenerationContextTier);
		List<GameObject> list = new List<GameObject>(inventory.Objects);
		if (!Tables.IsNullOrEmpty())
		{
			foreach (string table in Tables)
			{
				ParentObject.EquipFromPopulationTable(table, intProperty, craftmarkApplication, context);
			}
		}
		if (ParentObject.GetIntProperty("Hero") > 0 && !HeroTables.IsNullOrEmpty())
		{
			foreach (string heroTable in HeroTables)
			{
				ParentObject.EquipFromPopulationTable(heroTable, intProperty, craftmarkApplication, context);
			}
		}
		bool flag = false;
		foreach (GameObject object2 in inventory.Objects)
		{
			if (!list.Contains(object2))
			{
				object2.SetIntProperty("_stock", 1);
				flag = true;
			}
		}
		if (!flag)
		{
			return;
		}
		StockedEvent.Send(ParentObject, context);
		if (Restock && !Silent)
		{
			GameObject player = The.Player;
			if (player != null && player.InSameZone(ParentObject))
			{
				Messaging.XDidY(ParentObject, "have", "restocked " + ParentObject.its + " inventory", "!", null, null, The.Player, null, UseFullNames: true, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: true);
			}
		}
	}

	public void PerformRestock(bool Silent = false)
	{
		Inventory inventory = ParentObject.Inventory;
		List<GameObject> list = Event.NewGameObjectList();
		list.AddRange(inventory.Objects);
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			GameObject gameObject = list[i];
			if (gameObject.HasProperty("_stock") && !gameObject.HasPropertyOrTag("norestock") && !gameObject.IsImportant())
			{
				inventory.RemoveObject(gameObject);
				gameObject.Obliterate();
			}
		}
		PerformStock(LastRestockTick != 0, Silent);
	}

	public void Clear()
	{
		Tables = null;
		HeroTables = null;
		LastRestockTick = 0L;
	}

	public void AddTable(string Table)
	{
		if (Tables == null)
		{
			Tables = new List<string>();
		}
		Tables.Add(Table);
	}

	public void AddHeroTable(string Table)
	{
		if (HeroTables == null)
		{
			HeroTables = new List<string>();
		}
		HeroTables.Add(Table);
	}

	private void SetTables(ref List<string> List, string Value)
	{
		if (Value.IsNullOrEmpty())
		{
			List = null;
			return;
		}
		if (List == null)
		{
			List = new List<string>();
		}
		else
		{
			List.Clear();
		}
		DelimitedEnumeratorChar enumerator = Value.DelimitedBy(',').GetEnumerator();
		while (enumerator.MoveNext())
		{
			ReadOnlySpan<char> current = enumerator.Current;
			if (current.Length == Value.Length)
			{
				List.Add(Value);
				break;
			}
			List.Add(new string(current));
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "MadeHero")
		{
			PerformRestock(Silent: true);
		}
		return base.FireEvent(E);
	}
}
