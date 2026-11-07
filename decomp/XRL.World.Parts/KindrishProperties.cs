using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class KindrishProperties : LowStatBooster
{
	public static bool ReturnAward()
	{
		The.Game.PlayerReputation.Modify("Hindren", 400, "Quest");
		GameObject gameObject = GameObject.Create("Force Bracelet", 0, 1);
		gameObject.MakeUnderstood();
		Popup.Show("You receive " + gameObject.an() + ".");
		IComponent<GameObject>.ThePlayer.ReceiveObject(gameObject);
		return true;
	}

	public KindrishProperties()
	{
		AffectedStats = "Strength,Agility,Toughness,Intelligence,Willpower,Ego";
		Amount = 3;
		DescribeStatusForProperty = null;
	}
}
