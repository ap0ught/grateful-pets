using UnityEngine;

public class ShowSkin : MonoBehaviour
{
	public GUISkin skin;

	public float elemWidth = 100f;

	public float elemHeight = 30f;

	public Texture2D testIcon;

	private bool testBool;

	private int selection;

	private void OnGUI()
	{
		GUI.skin = skin;
		GUISkin gUISkin = GUI.skin;
		GUI.BeginGroup(new Rect(30f, 20f, Screen.width - 60, Screen.height - 40), gUISkin.name, "window");
		GUIStyle style = GUI.skin.GetStyle("window");
		int num = 0;
		int num2 = 0;
		foreach (GUIStyle item in gUISkin)
		{
			testBool = GUI.Toggle(new Rect((float)num * (elemWidth + 20f) + (float)style.padding.left, (float)num2 * (elemHeight + 15f) + (float)style.padding.top, elemWidth, elemHeight), testBool, new GUIContent(item.name.ToUpper(), testIcon), item);
			num++;
			if ((float)num * (elemWidth + 20f) > (float)Screen.width - elemWidth - 40f - (float)style.padding.right)
			{
				num = 0;
				num2++;
			}
		}
		GUI.EndGroup();
	}
}
