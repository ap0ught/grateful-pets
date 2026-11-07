using System;
using System.Collections.Generic;
using Genkit;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class ForcePylon : IPart
{
	public struct Link
	{
		public GameObject Pylon;

		public GameObject[] Line;

		public bool Host;
	}

	public string Blueprint = "Forcefield";

	public int Walls;

	public int Range;

	public bool IsRealityDistortionBased = true;

	[NonSerialized]
	public Link[] Links;

	[NonSerialized]
	public int Count;

	[NonSerialized]
	private static int DesuspendDepth;

	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		Writer.WriteOptimized(Blueprint);
		Writer.WriteOptimized(Walls);
		Writer.WriteOptimized(Range);
		Writer.Write(IsRealityDistortionBased);
		Writer.WriteOptimized(Count);
		if (Count <= 0)
		{
			return;
		}
		int i = 0;
		for (int num = Links.Length; i < num; i++)
		{
			if (Links[i].Pylon == null)
			{
				continue;
			}
			Writer.WriteGameObject(Links[i].Pylon);
			if (!Links[i].Host)
			{
				Writer.WriteOptimized(-1);
				continue;
			}
			GameObject[] line = Links[i].Line;
			int num2 = line.Length;
			Writer.WriteOptimized(num2);
			for (int j = 0; j < num2; j++)
			{
				Writer.WriteGameObject(line[j]);
			}
		}
	}

	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		Blueprint = Reader.ReadOptimizedString();
		Walls = Reader.ReadOptimizedInt32();
		Range = Reader.ReadOptimizedInt32();
		IsRealityDistortionBased = Reader.ReadBoolean();
		Count = Reader.ReadOptimizedInt32();
		if (Count <= 0)
		{
			return;
		}
		Links = new Link[Walls];
		for (int i = 0; i < Count; i++)
		{
			Links[i] = default;
			Links[i].Pylon = Reader.ReadGameObject();
			int num = Reader.ReadOptimizedInt32();
			if (num > 0)
			{
				Links[i].Host = true;
				GameObject[] array = (Links[i].Line = new GameObject[num]);
				for (int j = 0; j < num; j++)
				{
					array[j] = Reader.ReadGameObject();
				}
			}
		}
	}

	public bool IsValidTarget(GameObject Object)
	{
		if (GameObject.Validate(Object) && Object.HasPart(typeof(ForcePylon)) && Object != ParentObject && ParentObject.InSameZone(Object))
		{
			return ParentObject.DistanceTo(Object) <= Range;
		}
		return false;
	}

	public bool IsValidField(ref GameObject Object)
	{
		if (Object?.Physics == null || Object.IsInGraveyard())
		{
			Object = null;
			return false;
		}
		return true;
	}

	public bool IsConnected(GameObject Object)
	{
		if (Links == null)
		{
			return false;
		}
		int i = 0;
		for (int num = Links.Length; i < num; i++)
		{
			if (Object == Links[i].Pylon)
			{
				return true;
			}
		}
		return false;
	}

	public bool TryConnectTo(GameObject Object)
	{
		if (Count >= Walls)
		{
			return false;
		}
		if (!IsValidTarget(Object) || IsConnected(Object) || !ParentObject.HasLOSTo(Object))
		{
			return false;
		}
		ForcePylon part = Object.GetPart<ForcePylon>();
		if (part == null || part.Count >= part.Walls)
		{
			return false;
		}
		if (IsRealityDistortionBased && !CheckMyRealityDistortionAdvisability(Object, null, ParentObject, null, this))
		{
			return false;
		}
		if (IsRealityDistortionBased && ParentObject.HasRegisteredEvent("InitiateRealityDistortionLocal") && !ParentObject.FireEvent(Event.New("InitiateRealityDistortionLocal", "Object", ParentObject, "Mutation", this)))
		{
			return false;
		}
		AddLink(Object, Host: true);
		part.AddLink(ParentObject);
		DesuspendLink(Object);
		return true;
	}

	public bool TryDisconnectFrom(GameObject Object)
	{
		int i = 0;
		for (int num = Links.Length; i < num; i++)
		{
			if (Links[i].Pylon != Object)
			{
				continue;
			}
			GameObject[] line = Links[i].Line;
			if (line != null && Links[i].Host)
			{
				int j = 0;
				for (int num2 = line.Length; j < num2; j++)
				{
					if (IsValidField(ref line[j]))
					{
						line[j].Obliterate();
						line[j] = null;
					}
				}
			}
			Links[i].Pylon = null;
			Links[i].Host = false;
			Count--;
			Object.GetPart<ForcePylon>()?.TryDisconnectFrom(ParentObject);
			return true;
		}
		return false;
	}

	public void AddLink(GameObject Object, bool Host = false)
	{
		if (Links == null)
		{
			Links = new Link[Walls];
		}
		int num = Array.FindIndex(Links, 0, Walls, (Link x) => x.Pylon == null);
		Links[num].Pylon = Object;
		Links[num].Host = Host;
		Count++;
		if (!Host)
		{
			return;
		}
		int num2 = Range * 2 - 1;
		ref GameObject[] line = ref Links[num].Line;
		GameObject[] array = line ?? (line = new GameObject[num2]);
		string primaryFaction = ParentObject.GetPrimaryFaction();
		bool flag = false;
		for (int num3 = 0; num3 < num2; num3++)
		{
			array[num3] = GameObject.Create(Blueprint);
			Forcefield firstPartDescendedFrom = array[num3].GetFirstPartDescendedFrom<Forcefield>();
			if (firstPartDescendedFrom != null)
			{
				firstPartDescendedFrom.Creator = ParentObject;
				firstPartDescendedFrom.MovesWithOwner = true;
				firstPartDescendedFrom.RejectOwner = false;
				firstPartDescendedFrom.AddAllowPassage(primaryFaction);
			}
			SoundOnCreate part = array[num3].GetPart<SoundOnCreate>();
			if (part != null)
			{
				if (!flag)
				{
					ParentObject.PlayWorldSound(part.Sounds.GetRandomSubstring(','));
					flag = true;
				}
				array[num3].RemovePart(part);
			}
		}
	}

	public void SuspendLink(GameObject Object)
	{
		int i = 0;
		for (int num = Links.Length; i < num; i++)
		{
			if (Links[i].Pylon == Object)
			{
				SuspendLink(Object, Links[i].Line);
				break;
			}
		}
	}

	public void SuspendLink(GameObject Object, GameObject[] Line)
	{
		int i = 0;
		for (int num = Line.Length; i < num; i++)
		{
			if (IsValidField(ref Line[i]))
			{
				Line[i].RemoveFromContext();
			}
		}
	}

	public void DesuspendLink(GameObject Object, string Direction = null)
	{
		int i = 0;
		for (int num = Links.Length; i < num; i++)
		{
			if (Links[i].Pylon == Object)
			{
				DesuspendLink(Object, Links[i].Line, Direction);
				break;
			}
		}
	}

	public void DesuspendLink(GameObject Object, GameObject[] Line, string Direction = null)
	{
		DesuspendDepth++;
		Cell cell = ParentObject.CurrentCell;
		Cell cell2 = Object.CurrentCell;
		List<Location2D> cardinalLine = Location2D.GetCardinalLine(cell.X, cell.Y, cell2.X, cell2.Y);
		if (cardinalLine.Count <= 2)
		{
			return;
		}
		if (Direction == null)
		{
			Direction = Directions.GetRandomDirection();
		}
		Forcefield firstPartDescendedFrom = Line[0].GetFirstPartDescendedFrom<Forcefield>();
		int phase = Line[0].GetPhase();
		int num = Math.Min(Line.Length, cardinalLine.Count - 2);
		for (int i = 0; i < num; i++)
		{
			if (GameObject.Validate(ref Line[i]))
			{
				cell.ParentZone.GetCell(cardinalLine[i + 1]).AddObject(Line[i]);
			}
		}
		for (int j = 0; j < num; j++)
		{
			Cell cell3 = Line[j]?.CurrentCell;
			if (cell3 == null)
			{
				continue;
			}
			for (int num2 = cell3.Objects.Count - 1; num2 >= 0; num2--)
			{
				GameObject gameObject = cell3.Objects[num2];
				if (gameObject.Physics != null && gameObject != Line[j] && (gameObject.Physics.Solid || gameObject.IsCombatObject()) && (firstPartDescendedFrom == null || !firstPartDescendedFrom.CanPass(gameObject)) && !gameObject.HasPart(typeof(ForcePylon)) && !gameObject.HasPart(typeof(Forcefield)) && !gameObject.HasPart(typeof(HologramMaterial)) && gameObject.PhaseMatches(phase))
				{
					gameObject.Physics.Push(Direction, 5000, 4);
				}
			}
		}
	}

	public void FindPylons(Cell Origin)
	{
		foreach (Cell item in Origin.IterateAdjacent(Range, IncludeSelf: false, LocalOnly: true))
		{
			foreach (GameObject @object in item.Objects)
			{
				if (TryConnectTo(@object))
				{
					return;
				}
			}
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EnteredCellEvent.ID)
		{
			return ID == OnDestroyObjectEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (Count > 0)
		{
			int i = 0;
			for (int num = Links.Length; i < num; i++)
			{
				if (Links[i].Pylon != null)
				{
					if (!IsValidTarget(Links[i].Pylon))
					{
						TryDisconnectFrom(Links[i].Pylon);
					}
					else if (Links[i].Host)
					{
						DesuspendLink(Links[i].Pylon, Links[i].Line, E.Direction);
					}
					else
					{
						Links[i].Pylon.GetPart<ForcePylon>().DesuspendLink(ParentObject, E.Direction);
					}
				}
			}
		}
		if (Count < Walls)
		{
			FindPylons(E.Cell);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OnDestroyObjectEvent E)
	{
		int i = 0;
		for (int num = Links.Length; i < num; i++)
		{
			if (Links[i].Pylon != null)
			{
				TryDisconnectFrom(Links[i].Pylon);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeginMove");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginMove" && Count > 0)
		{
			int i = 0;
			for (int num = Links.Length; i < num; i++)
			{
				if (Links[i].Pylon != null)
				{
					if (!IsValidTarget(Links[i].Pylon))
					{
						TryDisconnectFrom(Links[i].Pylon);
					}
					else if (Links[i].Host)
					{
						SuspendLink(Links[i].Pylon, Links[i].Line);
					}
					else
					{
						Links[i].Pylon.GetPart<ForcePylon>().SuspendLink(ParentObject);
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}
