using System;
using UnityEngine;

public static class exTextureUtility
{
	private static readonly int[] bleedXOffsets = new int[8] { -1, 0, 1, -1, 1, -1, 0, 1 };

	private static readonly int[] bleedYOffsets = new int[8] { -1, -1, -1, 0, 0, 1, 1, 1 };

	public static Rect GetTrimTextureRect(Texture2D _tex, int _trimThreshold, Rect _rect)
	{
		new Rect(0f, 0f, 0f, 0f);
		Color32[] pixels = _tex.GetPixels32(0);
		int num = _tex.width;
		int num2 = 0;
		int num3 = _tex.height;
		int num4 = 0;
		int num5 = (int)_rect.x;
		int num6 = (int)(_rect.x + _rect.width);
		int num7 = (int)_rect.y;
		int num8 = (int)(_rect.y + _rect.height);
		for (int i = num5; i < num6; i++)
		{
			for (int j = num7; j < num8; j++)
			{
				if (pixels[i + j * _tex.width].a >= _trimThreshold)
				{
					num = i;
					i = _tex.width;
					break;
				}
			}
		}
		for (int num9 = num6 - 1; num9 >= num5; num9--)
		{
			for (int k = num7; k < num8; k++)
			{
				if (pixels[num9 + k * _tex.width].a >= _trimThreshold)
				{
					num2 = num9;
					num9 = -1;
					break;
				}
			}
		}
		for (int l = num7; l < num8; l++)
		{
			for (int m = num5; m < num6; m++)
			{
				if (pixels[m + l * _tex.width].a >= _trimThreshold)
				{
					num3 = l;
					l = _tex.height;
					break;
				}
			}
		}
		for (int num10 = num8 - 1; num10 >= num7; num10--)
		{
			for (int n = num5; n < num6; n++)
			{
				if (pixels[n + num10 * _tex.width].a >= _trimThreshold)
				{
					num4 = num10;
					num10 = -1;
					break;
				}
			}
		}
		int num11 = num2 - num + 1;
		int num12 = num4 - num3 + 1;
		return new Rect(num, num3, num11, num12);
	}

	public static void Fill(ref Color32[] _destPixels, int _destWidth, Texture2D _src, string _name, int _destX, int _destY, int _srcX, int _srcY, int _srcWidth, int _srcHeight, bool _rotated)
	{
		Color32[] pixels = _src.GetPixels32(0);
		if (!_rotated)
		{
			for (int i = 0; i < _srcHeight; i++)
			{
				for (int j = 0; j < _srcWidth; j++)
				{
					int num = _srcX + j;
					int num2 = _srcY + i;
					int num3 = _destX + j;
					int num4 = _destY + i;
					try
					{
						Color32 color = pixels[num + num2 * _src.width];
						_destPixels[num3 + num4 * _destWidth] = color;
					}
					catch (Exception ex)
					{
						Debug.Log(num + " " + num2 + " " + _src.width + " " + pixels.Length + " " + _srcWidth + " " + _srcHeight + " " + _name + " ex: " + ex.ToString());
						throw ex;
					}
				}
			}
			return;
		}
		int num5 = _srcHeight;
		int num6 = _srcWidth;
		for (int k = 0; k < num6; k++)
		{
			for (int l = 0; l < num5; l++)
			{
				int num7 = _srcX + k;
				int num8 = _srcY + _srcHeight - 1 - l;
				int num9 = _destX + l;
				int num10 = _destY + k;
				try
				{
					Color32 color2 = pixels[num7 + num8 * _src.width];
					_destPixels[num9 + num10 * _destWidth] = color2;
				}
				catch (Exception ex2)
				{
					Debug.Log(num7 + " " + num8 + " " + _src.width + " " + pixels.Length + " " + _srcWidth + " " + _srcHeight + " " + _name + " ex: " + ex2.ToString());
					throw ex2;
				}
			}
		}
	}

	public static void ApplyContourBleed(ref Color32[] _result, Color32[] _srcPixels, int _textureWidth, Rect _rect)
	{
		if (_rect.width == 0f || _rect.height == 0f)
		{
			return;
		}
		int num = (int)_rect.x;
		int num2 = (int)(_rect.x + _rect.width);
		int num3 = (int)_rect.y;
		int num4 = (int)(_rect.y + _rect.height);
		for (int i = num; i < num2; i++)
		{
			for (int j = num3; j < num4; j++)
			{
				if (_srcPixels[i + j * _textureWidth].a != 0)
				{
					continue;
				}
				for (int k = 0; k < bleedXOffsets.Length; k++)
				{
					int num5 = i + bleedXOffsets[k];
					int num6 = j + bleedYOffsets[k];
					if (num5 >= num && num5 < num2 && num6 >= num3 && num6 < num4)
					{
						Color32 color = _srcPixels[num5 + num6 * _textureWidth];
						if (color.a != 0)
						{
							_result[i + j * _textureWidth] = new Color32(color.r, color.g, color.b, 0);
							break;
						}
					}
				}
			}
		}
	}

	public static void ApplyPaddingBleed(ref Color32[] _result, Color32[] _srcPixels, int _textureWidth, int _textureHeight, Rect _rect)
	{
		if (_rect.width == 0f || _rect.height == 0f)
		{
			return;
		}
		int num = (int)_rect.x;
		int num2 = (int)(_rect.x + _rect.width);
		int num3 = (int)_rect.y;
		int num4 = (int)(_rect.y + _rect.height);
		int num5 = num3;
		int num6 = num4 - 1;
		int num7 = num;
		int num8 = num2 - 1;
		for (int i = num; i < num2; i++)
		{
			if (num5 - 1 >= 0)
			{
				Color32 color = _srcPixels[i + num5 * _textureWidth];
				_result[i + (num5 - 1) * _textureWidth] = color;
			}
			if (num6 + 1 < _textureHeight)
			{
				Color32 color2 = _srcPixels[i + num6 * _textureWidth];
				_result[i + (num6 + 1) * _textureWidth] = color2;
			}
		}
		for (int j = num3; j < num4; j++)
		{
			if (num7 - 1 >= 0)
			{
				Color32 color3 = _srcPixels[num7 + j * _textureWidth];
				_result[num7 - 1 + j * _textureWidth] = color3;
			}
			if (num8 + 1 < _textureWidth)
			{
				Color32 color4 = _srcPixels[num8 + j * _textureWidth];
				_result[num8 + 1 + j * _textureWidth] = color4;
			}
		}
		if (num7 - 1 >= 0 && num5 - 1 >= 0)
		{
			Color32 color5 = _srcPixels[num7 + num5 * _textureWidth];
			_result[num7 - 1 + (num5 - 1) * _textureWidth] = color5;
		}
		if (num7 - 1 >= 0 && num6 + 1 < _textureHeight)
		{
			Color32 color6 = _srcPixels[num7 + num6 * _textureWidth];
			_result[num7 - 1 + (num6 + 1) * _textureWidth] = color6;
		}
		if (num8 + 1 < _textureWidth && num6 + 1 < _textureHeight)
		{
			Color32 color7 = _srcPixels[num8 + num6 * _textureWidth];
			_result[num8 + 1 + (num6 + 1) * _textureWidth] = color7;
		}
		if (num8 + 1 < _textureWidth && num5 - 1 >= 0)
		{
			Color32 color8 = _srcPixels[num8 + num5 * _textureWidth];
			_result[num8 + 1 + (num5 - 1) * _textureWidth] = color8;
		}
	}
}
