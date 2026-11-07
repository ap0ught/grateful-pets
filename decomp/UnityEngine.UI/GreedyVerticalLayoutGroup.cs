namespace UnityEngine.UI;

public class GreedyVerticalLayoutGroup : VerticalLayoutGroup
{
	public override float minHeight => preferredHeight;
}
