using System;
using System.Collections.Generic;
using UnityEngine;

public class exAtlas : ScriptableObject
{
	public enum Algorithm
	{
		Basic,
		Tree,
		MaxRect
	}

	public enum SortBy
	{
		UseBest,
		Width,
		Height,
		Area,
		Name
	}

	public enum SortOrder
	{
		UseBest,
		Ascending,
		Descending
	}

	public enum PaddingMode
	{
		None,
		Auto,
		Custom
	}

	public int width = 512;

	public int height = 512;

	public List<exTextureInfo> textureInfos = new List<exTextureInfo>();

	public List<exBitmapFont> bitmapFonts = new List<exBitmapFont>();

	public Texture2D texture;

	public bool customBuildColor;

	public Color buildColor = new Color(1f, 1f, 1f, 0f);

	public bool useContourBleed = true;

	public bool usePaddingBleed = true;

	public bool trimElements = true;

	[SerializeField]
	public int trimThreshold_ = 1;

	public bool readable;

	public Color bgColor = Color.white;

	public bool showCheckerboard = true;

	public Algorithm algorithm = Algorithm.MaxRect;

	public SortBy sortBy;

	public SortOrder sortOrder;

	public PaddingMode paddingMode = PaddingMode.Auto;

	public int customPadding = 1;

	public bool allowRotate = true;

	public Color elementBgColor = new Color(1f, 1f, 1f, 0f);

	public Color elementSelectColor = new Color(0f, 0f, 1f, 1f);

	[SerializeField]
	private float scale_ = 1f;

	public bool needRebuild;

	public bool needLayout;

	public int trimThreshold
	{
		get
		{
			return trimThreshold_;
		}
		set
		{
			trimThreshold_ = Math.Min(Math.Max(value, 1), 255);
		}
	}

	public float scale
	{
		get
		{
			return scale_;
		}
		set
		{
			if (scale_ != value)
			{
				scale_ = value;
				scale_ = Mathf.Clamp(scale_, 0.1f, 2f);
				scale_ = Mathf.Round(scale_ * 100f) / 100f;
			}
		}
	}

	public int actualPadding
	{
		get
		{
			if (paddingMode == PaddingMode.None)
			{
				return 0;
			}
			if (paddingMode == PaddingMode.Custom)
			{
				return customPadding;
			}
			if (!usePaddingBleed)
			{
				return 1;
			}
			return 2;
		}
	}

	public exTextureInfo GetTextureInfoByName(string _name)
	{
		for (int i = 0; i < textureInfos.Count; i++)
		{
			exTextureInfo exTextureInfo2 = textureInfos[i];
			if (exTextureInfo2.name == _name)
			{
				return exTextureInfo2;
			}
		}
		return null;
	}
}
