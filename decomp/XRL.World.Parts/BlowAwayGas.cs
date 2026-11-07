using System;
using System.Collections.Generic;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class BlowAwayGas : IPart
{
	public string Message;

	public int Radius = 4;

	[NonSerialized]
	private List<GameObject> GasObjects = new List<GameObject>();

	[NonSerialized]
	private List<string> GasNames = new List<string>();

	public override bool WantEvent(int ID, int Cascade)
	{
		return ID == SingletonEvent<BeginTakeActionEvent>.ID;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		BlowAway();
		return base.HandleEvent(E);
	}

	public void BlowAway()
	{
		List<GameObject> gasObjects = GasObjects;
		List<string> gasNames = GasNames;
		bool flag = !Message.IsNullOrEmpty();
		try
		{
			foreach (Cell item in ParentObject.CurrentCell.IterateAdjacent(Radius, IncludeSelf: true, LocalOnly: true))
			{
				foreach (GameObject @object in item.Objects)
				{
					if (@object.TryGetPart<Gas>(out var Part) && Part.Creator != ParentObject)
					{
						gasObjects.Add(@object);
					}
				}
			}
			foreach (GameObject item2 in gasObjects)
			{
				string text = ParentObject.GetDirectionToward(item2);
				if (text == "." || text == "?")
				{
					text = Directions.GetRandomDirection();
				}
				Cell cellFromDirection = item2.CurrentCell.GetCellFromDirection(text);
				if (cellFromDirection != null)
				{
					cellFromDirection.AddObject(item2);
					if (flag && !gasNames.Contains(item2.Render.DisplayName))
					{
						gasNames.Add(item2.Render.DisplayName);
					}
				}
			}
			if (!flag)
			{
				return;
			}
			string color = IComponent<GameObject>.ConsequentialColor(ParentObject);
			foreach (string item3 in gasNames)
			{
				ParentObject.EmitMessage(Message.StartReplace().AddObject(ParentObject).AddReplacer("gas", item3)
					.ToString(), null, color);
			}
		}
		finally
		{
			gasObjects.Clear();
			gasNames.Clear();
		}
	}
}
