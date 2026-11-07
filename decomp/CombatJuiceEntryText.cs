using System.Collections.Generic;
using UnityEngine;
using XRL.World;

public class CombatJuiceEntryText : CombatJuiceEntry
{
	public float floatTime;

	public float scale;

	public string text;

	public Color color;

	public Vector3 startPosition;

	public Vector3 endPosition;

	public XRL.World.GameObject emittingObject;

	public Easing.Functions ease;

	private static Dictionary<int, float> lastTextEmission = new Dictionary<int, float>();

	public CombatJuiceEntryText(Vector3 start, Vector3 end, float floatTime, string text, Color color, XRL.World.GameObject emittingObject, float scale)
	{
		startPosition = start;
		endPosition = end;
		duration = 0f;
		this.text = text;
		this.color = color;
		this.floatTime = floatTime;
		this.emittingObject = emittingObject;
		this.scale = scale;
	}

	public bool outOfTheWayOnEmittingObject()
	{
		return false;
	}

	public override bool canStart()
	{
		if (emittingObject == null)
		{
			return true;
		}
		int hashCode = emittingObject.GetHashCode();
		if (lastTextEmission.ContainsKey(hashCode) && Time.time - lastTextEmission[hashCode] < 0.6f)
		{
			return false;
		}
		return true;
	}

	public override void start()
	{
		if (emittingObject != null)
		{
			if (lastTextEmission.ContainsKey(emittingObject.GetHashCode()))
			{
				lastTextEmission[emittingObject.GetHashCode()] = Time.time;
			}
			else
			{
				lastTextEmission.Add(emittingObject.GetHashCode(), Time.time);
			}
		}
		while (true)
		{
			IL_0055:
			foreach (KeyValuePair<int, float> item in lastTextEmission)
			{
				if (Time.time - item.Value > 5f)
				{
					lastTextEmission.Remove(item.Key);
					goto IL_0055;
				}
			}
			break;
		}
		CombatJuice._text(startPosition, endPosition, text, color, floatTime, scale);
		emittingObject = null;
	}
}
