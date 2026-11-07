using UnityEngine;

namespace ex2D.Detail;

public static class exSpriteUtility
{
	public static exSprite NewSimpleSprite(GameObject _go, exTextureInfo _info, int _width, int _height, Color _color)
	{
		exSprite exSprite2 = _go.GetComponent<exSprite>();
		if (exSprite2 == null)
		{
			exSprite2 = _go.AddComponent<exSprite>();
		}
		if (exSprite2.shader == null)
		{
			exSprite2.shader = Shader.Find("ex2D/Alpha Blended");
		}
		exSprite2.spriteType = exSpriteType.Simple;
		exSprite2.textureInfo = _info;
		exSprite2.customSize = true;
		exSprite2.width = _width;
		exSprite2.height = _height;
		exSprite2.color = _color;
		return exSprite2;
	}

	public static exSprite NewSlicedSprite(GameObject _go, exTextureInfo _info, int _left, int _right, int _top, int _bottom, int _width, int _height, Color _color, bool _borderOnly)
	{
		exSprite exSprite2 = _go.GetComponent<exSprite>();
		if (exSprite2 == null)
		{
			exSprite2 = _go.AddComponent<exSprite>();
		}
		if (exSprite2.shader == null)
		{
			exSprite2.shader = Shader.Find("ex2D/Alpha Blended");
		}
		exSprite2.spriteType = exSpriteType.Sliced;
		exSprite2.textureInfo = _info;
		exSprite2.borderOnly = _borderOnly;
		exSprite2.customBorderSize = true;
		exSprite2.leftBorderSize = _left;
		exSprite2.rightBorderSize = _right;
		exSprite2.topBorderSize = _top;
		exSprite2.bottomBorderSize = _bottom;
		exSprite2.customSize = true;
		exSprite2.width = _width;
		exSprite2.height = _height;
		exSprite2.color = _color;
		return exSprite2;
	}

	public static void GetDicingCount(exTextureInfo _ti, out int _colCount, out int _rowCount)
	{
		_colCount = 1;
		_rowCount = 1;
		if (_ti != null)
		{
			if (_ti.diceUnitWidth > 0 && _ti.width > 0)
			{
				_colCount = Mathf.CeilToInt((float)_ti.width / (float)_ti.diceUnitWidth);
			}
			if (_ti.diceUnitHeight > 0 && _ti.height > 0)
			{
				_rowCount = Mathf.CeilToInt((float)_ti.height / (float)_ti.diceUnitHeight);
			}
		}
	}

	public static void GetTilingCount(exISprite _sprite, out int _colCount, out int _rowCount)
	{
		exTextureInfo textureInfo = _sprite.textureInfo;
		if (textureInfo != null && (float)textureInfo.width + _sprite.tiledSpacing.x != 0f && (float)textureInfo.height + _sprite.tiledSpacing.y != 0f)
		{
			_colCount = Mathf.Max(Mathf.CeilToInt(_sprite.width / ((float)textureInfo.width + _sprite.tiledSpacing.x)), 1);
			_rowCount = Mathf.Max(Mathf.CeilToInt(_sprite.height / ((float)textureInfo.height + _sprite.tiledSpacing.y)), 1);
		}
		else
		{
			_colCount = 1;
			_rowCount = 1;
		}
	}

	public static void SetTextureInfo(exSpriteBase _sprite, ref exTextureInfo _ti, exTextureInfo _newTi, bool _useTextureOffset, exSpriteType _spriteType)
	{
		exTextureInfo exTextureInfo2 = _ti;
		_ti = _newTi;
		if (!(_newTi != null))
		{
			return;
		}
		if (_newTi.texture == null)
		{
			Debug.LogWarning("invalid textureInfo");
		}
		switch (_spriteType)
		{
		case exSpriteType.Tiled:
			if (exTextureInfo2 == null || (object)exTextureInfo2 == _newTi || _newTi.width != exTextureInfo2.width || _newTi.height != exTextureInfo2.height)
			{
				(_sprite as exISprite).UpdateBufferSize();
				_sprite.updateFlags |= exUpdateFlags.Vertex;
			}
			break;
		case exSpriteType.Diced:
			(_sprite as exISprite).UpdateBufferSize();
			_sprite.updateFlags |= exUpdateFlags.Vertex;
			break;
		default:
			if (!_sprite.customSize && (exTextureInfo2 == null || _newTi.width != exTextureInfo2.width || _newTi.height != exTextureInfo2.height))
			{
				_sprite.updateFlags |= exUpdateFlags.Vertex;
			}
			break;
		}
		if (_useTextureOffset)
		{
			_sprite.updateFlags |= exUpdateFlags.Vertex;
		}
		_sprite.updateFlags |= exUpdateFlags.UV;
		if (exTextureInfo2 == null || (object)exTextureInfo2.texture != _newTi.texture)
		{
			_sprite.updateFlags |= exUpdateFlags.Vertex | exUpdateFlags.UV;
			(_sprite as exISprite).UpdateMaterial();
		}
	}
}
