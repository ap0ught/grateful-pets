using UnityEngine;
using UnityEngine.UI;
using XRL.UI;

public class FontSizeUIScalar : MonoBehaviour
{
	public UnityEngine.UI.Text text;

	public int target;

	private string lastScale;

	private void Start()
	{
	}

	private void Update()
	{
		if (lastScale != Options.StageScaleRaw)
		{
			lastScale = Options.StageScaleRaw;
			if (text != null)
			{
				text.fontSize = (int)((double)target / Options.StageScale);
			}
		}
	}
}
