using System;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World;

internal static class ParticleFX
{
	public static void Smoke(int X, int Y, int StartAngle, int EndAngle)
	{
		if (!Options.DisableSmoke)
		{
			float num = 0f;
			float num2 = 0f;
			float num3 = (float)Stat.RandomCosmetic(StartAngle, EndAngle) / 58f;
			num = (float)Math.Sin(num3) / 6f;
			num2 = (float)Math.Cos(num3) / 6f;
			int num4 = Stat.RandomCosmetic(1, 3);
			string text = "";
			switch (num4)
			{
			case 1:
				text = "°";
				break;
			case 2:
				text = "±";
				break;
			case 3:
				text = "²";
				break;
			}
			XRLCore.ParticleManager.Add(text, X, Y, num, num2);
		}
	}
}
