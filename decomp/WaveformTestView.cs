using System;
using Genkit;
using Qud.UI;
using UnityEngine;
using UnityEngine.UI;
using XRL.Rules;
using XRL.UI;

[UIView("WaveformTest", false, false, false, null, null, false, 0, false, UICanvas = "WaveformTest", UICanvasHost = 1)]
public class WaveformTestView : SingletonWindowBase<WaveformTestView>
{
	public int seed;

	public override void Show()
	{
		base.Show();
		FindChild("Controls/Script").GetComponent<InputField>().text = "pass:temple,n=3,width=78,height=23";
	}

	public void OnCommand(string Command)
	{
		if (Command == "Reseed To Static")
		{
			seed = 0;
			Stat.ReseedFrom(0);
		}
		if (Command == "Back")
		{
			UIManager.getWindow("WaveformTest").Hide();
			UIManager.showWindow("MainMenu");
		}
		if (Command == "TTest")
		{
			WaveCollapseTools.LoadTemplates(force: true);
			foreach (WaveTemplateEntry value in WaveCollapseTools.waveTemplates.Values)
			{
				WaveCollapseFastModel waveCollapseFastModel = new WaveCollapseFastModel(value, 3, 80, 25, periodicInput: true, periodicOutput: false, 8, 0);
				waveCollapseFastModel.Run(seed, 0);
				seed++;
				if (waveCollapseFastModel.T > 350)
				{
					Debug.LogWarning("template " + value.name + " T=" + waveCollapseFastModel.T + " exceeds 350");
				}
			}
		}
		if (Command == "Generate Fast")
		{
			WaveCollapseTools.LoadTemplates(force: true);
			Image component = FindChild("Controls/Image").GetComponent<Image>();
			WaveCollapseFastModel waveCollapseFastModel2 = null;
			string[] array = FindChild("Controls/Script").GetComponent<InputField>().text.Split('\n');
			int width = 80;
			int height = 25;
			int n = 3;
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split(':');
				if (array2[0] == "pass")
				{
					string[] array3 = array2[1].Split(',');
					for (int j = 0; j < array3.Length; j++)
					{
						if (array3[j].StartsWith("n="))
						{
							n = Convert.ToInt32(array3[j].Split('=')[1]);
						}
						if (array3[j].StartsWith("width="))
						{
							width = Convert.ToInt32(array3[j].Split('=')[1]);
						}
						if (array3[j].StartsWith("height="))
						{
							height = Convert.ToInt32(array3[j].Split('=')[1]);
						}
					}
					if (waveCollapseFastModel2 == null)
					{
						waveCollapseFastModel2 = new WaveCollapseFastModel(array3[0], n, width, height, periodicInput: true, periodicOutput: false, 8, 0);
					}
					else
					{
						waveCollapseFastModel2.UpdateSample(WaveCollapseTools.waveTemplates[array3[0]], n, periodicInput: true, periodicOutput: false, 8, 0);
					}
					waveCollapseFastModel2.Run(seed, 0);
					seed++;
				}
				else if (array2[0] == "clear")
				{
					waveCollapseFastModel2.ClearColors(array2[1], "all");
				}
				else if (array2[0].StartsWith("clear-"))
				{
					waveCollapseFastModel2.ClearColors(array2[1], array2[0].Split('-')[1]);
				}
			}
			Debug.Log("model T=" + waveCollapseFastModel2.T);
			Texture2D texture2D = new Texture2D(waveCollapseFastModel2.FMX, waveCollapseFastModel2.FMY, TextureFormat.ARGB32, mipChain: false);
			texture2D.filterMode = FilterMode.Point;
			texture2D.SetPixels32(waveCollapseFastModel2.GetResult());
			texture2D.Apply();
			Sprite sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0f, 0f));
			component.sprite = sprite;
		}
		if (!(Command == "Generate"))
		{
			return;
		}
		WaveCollapseTools.LoadTemplates();
		Image component2 = FindChild("Controls/Image").GetComponent<Image>();
		MultipassModel multipassModel = null;
		string[] array4 = FindChild("Controls/Script").GetComponent<InputField>().text.Split('\n');
		int width2 = 80;
		int height2 = 25;
		int n2 = 3;
		for (int k = 0; k < array4.Length; k++)
		{
			string[] array5 = array4[k].Split(':');
			if (array5[0] == "pass")
			{
				string[] array6 = array5[1].Split(',');
				for (int l = 0; l < array6.Length; l++)
				{
					if (array6[l].StartsWith("n="))
					{
						n2 = Convert.ToInt32(array6[l].Split('=')[1]);
					}
					if (array6[l].StartsWith("width="))
					{
						width2 = Convert.ToInt32(array6[l].Split('=')[1]);
					}
					if (array6[l].StartsWith("height="))
					{
						height2 = Convert.ToInt32(array6[l].Split('=')[1]);
					}
				}
				if (multipassModel == null)
				{
					multipassModel = new MultipassModel(WaveCollapseTools.waveTemplates[array6[0]], n2, width2, height2, periodicInput: true, periodicOutput: false, 8, 0);
				}
				else
				{
					multipassModel.UpdateSample(WaveCollapseTools.waveTemplates[array6[0]], n2, periodicInput: true, periodicOutput: false, 8, 0);
				}
				multipassModel.Run(seed, 0);
				seed++;
			}
			else if (array5[0] == "clear")
			{
				multipassModel.ClearColors(array5[1], "all");
			}
			else if (array5[0].StartsWith("clear-"))
			{
				multipassModel.ClearColors(array5[1], array5[0].Split('-')[1]);
			}
		}
		Texture2D texture2D2 = new Texture2D(multipassModel.FMX, multipassModel.FMY, TextureFormat.ARGB32, mipChain: false);
		texture2D2.filterMode = FilterMode.Point;
		texture2D2.SetPixels32(multipassModel.GetResult());
		texture2D2.Apply();
		Sprite sprite2 = Sprite.Create(texture2D2, new Rect(0f, 0f, texture2D2.width, texture2D2.height), new Vector2(0f, 0f));
		component2.sprite = sprite2;
	}
}
