namespace XRL.UI;

internal static class Overlay
{
	public static string CurrentScreen = Options.StageViewID;

	public static Vector2i CurrentCell
	{
		get
		{
			if (The.Player != null)
			{
				return new Vector2i(The.Player.Physics.CurrentCell.X, The.Player.Physics.CurrentCell.Y);
			}
			return new Vector2i(39, 12);
		}
	}
}
