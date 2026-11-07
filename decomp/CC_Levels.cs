using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/Levels")]
public class CC_Levels : CC_Base
{
	public bool isRGB;

	public float inputMinL;

	public float inputMaxL = 255f;

	public float inputGammaL = 1f;

	public float inputMinR;

	public float inputMaxR = 255f;

	public float inputGammaR = 1f;

	public float inputMinG;

	public float inputMaxG = 255f;

	public float inputGammaG = 1f;

	public float inputMinB;

	public float inputMaxB = 255f;

	public float inputGammaB = 1f;

	public float outputMinL;

	public float outputMaxL = 255f;

	public float outputMinR;

	public float outputMaxR = 255f;

	public float outputMinG;

	public float outputMaxG = 255f;

	public float outputMinB;

	public float outputMaxB = 255f;

	public int currentChannel;

	public bool logarithmic;

	public int mode
	{
		get
		{
			if (!isRGB)
			{
				return 0;
			}
			return 1;
		}
		set
		{
			isRGB = value > 0;
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (!isRGB)
		{
			material.SetVector("_InputMin", new Vector4(inputMinL / 255f, inputMinL / 255f, inputMinL / 255f, 1f));
			material.SetVector("_InputMax", new Vector4(inputMaxL / 255f, inputMaxL / 255f, inputMaxL / 255f, 1f));
			material.SetVector("_InputGamma", new Vector4(inputGammaL, inputGammaL, inputGammaL, 1f));
			material.SetVector("_OutputMin", new Vector4(outputMinL / 255f, outputMinL / 255f, outputMinL / 255f, 1f));
			material.SetVector("_OutputMax", new Vector4(outputMaxL / 255f, outputMaxL / 255f, outputMaxL / 255f, 1f));
		}
		else
		{
			material.SetVector("_InputMin", new Vector4(inputMinR / 255f, inputMinG / 255f, inputMinB / 255f, 1f));
			material.SetVector("_InputMax", new Vector4(inputMaxR / 255f, inputMaxG / 255f, inputMaxB / 255f, 1f));
			material.SetVector("_InputGamma", new Vector4(inputGammaR, inputGammaG, inputGammaB, 1f));
			material.SetVector("_OutputMin", new Vector4(outputMinR / 255f, outputMinG / 255f, outputMinB / 255f, 1f));
			material.SetVector("_OutputMax", new Vector4(outputMaxR / 255f, outputMaxG / 255f, outputMaxB / 255f, 1f));
		}
		Graphics.Blit(source, destination, material);
	}
}
