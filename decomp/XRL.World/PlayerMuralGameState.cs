using System;
using System.Collections.Generic;
using Qud.API;
using XRL.World.Parts;

namespace XRL.World;

[Serializable]
public class PlayerMuralGameState : IGameStateSingleton, IComposite
{
	public const string ID = "PlayerMuralGameState";

	public const string CONTROLLER_ID = "JoppaWorld.53.3.1.0.0";

	private static PlayerMuralGameState _Instance;

	private List<JournalAccomplishment> Accomplishments = new List<JournalAccomplishment>();

	public static PlayerMuralGameState Instance
	{
		get
		{
			if (_Instance == null)
			{
				_Instance = (PlayerMuralGameState)The.Game.GetObjectGameState("PlayerMuralGameState");
				if (_Instance == null)
				{
					_Instance = new PlayerMuralGameState();
					The.Game.SetObjectGameState("PlayerMuralGameState", _Instance);
				}
			}
			return _Instance;
		}
	}

	public List<JournalAccomplishment> GetAccomplishments()
	{
		if (Accomplishments.IsNullOrEmpty() && The.ZoneManager.GetZone("JoppaWorld.53.3.1.0.0").FindObject("PlayerMuralController").TryGetPart<PlayerMuralController>(out var Part))
		{
			Part.initializeMurals();
			Accomplishments.AddRange(Part.playerMuralEventList);
		}
		return Accomplishments;
	}

	public void SetAccomplishments(IReadOnlyList<JournalAccomplishment> Accomplishments)
	{
		this.Accomplishments.Clear();
		this.Accomplishments.AddRange(Accomplishments);
	}

	public void Write(SerializationWriter Writer)
	{
		Writer.WriteOptimized(Accomplishments.Count);
		foreach (JournalAccomplishment accomplishment in Accomplishments)
		{
			Writer.Write(accomplishment);
		}
	}

	public void Read(SerializationReader Reader)
	{
		int num = Reader.ReadOptimizedInt32();
		Accomplishments.EnsureCapacity(num);
		for (int i = 0; i < num; i++)
		{
			Accomplishments.Add((JournalAccomplishment)Reader.ReadComposite());
		}
	}
}
