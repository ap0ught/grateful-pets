using System;
using ConsoleLib.Console;
using UnityEngine;
using XRL.World.Parts;

namespace QupKit;

[ExecuteAlways]
public class ParticleRandomizer : MonoBehaviour
{
	public bool RandomizeColor;

	public bool RandomizeFungalColor;

	public void Awake()
	{
		if (RandomizeColor)
		{
			ParticleSystem component = GetComponent<ParticleSystem>();
			component.time = new System.Random().Next(1, 90);
			ParticleSystem.MainModule main = component.main;
			new System.Random().Next(1, 9);
			main.startColor = ConsoleLib.Console.ColorUtility.colorFromChar(Crayons.GetRandomColor(new System.Random())[0]);
		}
		if (RandomizeFungalColor)
		{
			ParticleSystem component2 = GetComponent<ParticleSystem>();
			component2.time = new System.Random().Next(1, 90);
			ParticleSystem.MainModule main2 = component2.main;
			int num = new System.Random().Next(1, 5);
			if (num == 1)
			{
				main2.startColor = ConsoleLib.Console.ColorUtility.colorFromChar('Y');
			}
			if (num == 2)
			{
				main2.startColor = ConsoleLib.Console.ColorUtility.colorFromChar('C');
			}
			if (num == 3)
			{
				main2.startColor = ConsoleLib.Console.ColorUtility.colorFromChar('W');
			}
			if (num == 4)
			{
				main2.startColor = ConsoleLib.Console.ColorUtility.colorFromChar('R');
			}
		}
	}
}
