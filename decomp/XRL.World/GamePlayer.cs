using System;
using XRL.Collections;
using XRL.Messages;
using XRL.UI;
using XRL.World.Conversations;

namespace XRL.World;

[Serializable]
public class GamePlayer
{
	[NonSerialized]
	public GameObject _Body;

	[NonSerialized]
	internal RingDeque<Cell> PlayerCells = new RingDeque<Cell>(8);

	public MessageQueue Messages = new MessageQueue();

	public GameObject Body
	{
		get
		{
			return _Body;
		}
		set
		{
			SetBody(value);
		}
	}

	internal void EnqueuePlayerCell()
	{
		Cell cell = _Body?.Physics?._CurrentCell;
		if (cell != null && (PlayerCells.Count == 0 || PlayerCells.Last != cell))
		{
			if (PlayerCells.Count >= 8)
			{
				PlayerCells.Dequeue();
			}
			PlayerCells.Enqueue(cell);
		}
	}

	internal void CheckPlayerLocations()
	{
		int i = 0;
		for (int count = PlayerCells.Count; i < count; i++)
		{
			Cell cell = PlayerCells[i];
			int j = 0;
			for (int count2 = cell.Objects.Count; j < count2; j++)
			{
				GameObject gameObject = cell.Objects[j];
				if (gameObject.Physics == null || gameObject.Physics._CurrentCell != cell)
				{
					cell.LogInvalidPhysics(gameObject);
				}
			}
		}
	}

	public void Save(SerializationWriter Writer)
	{
		Writer.WriteFields(this);
		Choice.Hashes.Write(Writer);
	}

	public static GamePlayer Load(SerializationReader Reader)
	{
		GamePlayer gamePlayer = Reader.ReadInstanceFields<GamePlayer>();
		gamePlayer._Body = Reader.GetPlayer();
		Choice.Hashes.Read(Reader);
		return gamePlayer;
	}

	/// <summary>Set the player's controlled game object.</summary>
	/// <param name="Body">A new <see cref="T:XRL.World.GameObject" /> for the player to control.</param>
	/// <param name="Transient">Whether this is a temporary assignment that should not be visible to the player.</param>
	public void SetBody(GameObject Body, bool Transient = false)
	{
		if (Body == _Body)
		{
			return;
		}
		GameObject body = _Body;
		if (Body != null && !Transient)
		{
			Cell currentCell = Body.CurrentCell;
			if (currentCell != null)
			{
				_Body = Body;
				The.ZoneManager.SetActiveZone(currentCell.ParentZone);
				The.ActionManager.AddActiveObject(_Body);
				The.ZoneManager.ProcessGoToPartyLeader();
			}
			Body.Render.RenderLayer = 100;
			Body.Brain.Goals.Clear();
			AbilityManager.UpdateFavorites();
		}
		else
		{
			_Body = Body;
		}
		AfterPlayerBodyChangeEvent.Send(Body, body);
	}
}
