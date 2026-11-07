using UnityEngine;

namespace ex2D.Detail;

public static class exISpriteExtends
{
	public static Vector2 GetTextureOffset(this exISprite _sprite)
	{
		Vector2 zero = Vector2.zero;
		if (_sprite.useTextureOffset)
		{
			exTextureInfo textureInfo = _sprite.textureInfo;
			switch (_sprite.anchor)
			{
			case Anchor.TopLeft:
				zero.x = textureInfo.trim_x;
				zero.y = textureInfo.trim_y - (textureInfo.rawHeight - textureInfo.height);
				break;
			case Anchor.TopCenter:
				zero.x = (float)textureInfo.trim_x - (float)(textureInfo.rawWidth - textureInfo.width) * 0.5f;
				zero.y = textureInfo.trim_y - (textureInfo.rawHeight - textureInfo.height);
				break;
			case Anchor.TopRight:
				zero.x = textureInfo.trim_x - (textureInfo.rawWidth - textureInfo.width);
				zero.y = textureInfo.trim_y - (textureInfo.rawHeight - textureInfo.height);
				break;
			case Anchor.MidLeft:
				zero.x = textureInfo.trim_x;
				zero.y = (float)textureInfo.trim_y - (float)(textureInfo.rawHeight - textureInfo.height) * 0.5f;
				break;
			case Anchor.MidCenter:
				zero.x = (float)textureInfo.trim_x - (float)(textureInfo.rawWidth - textureInfo.width) * 0.5f;
				zero.y = (float)textureInfo.trim_y - (float)(textureInfo.rawHeight - textureInfo.height) * 0.5f;
				break;
			case Anchor.MidRight:
				zero.x = textureInfo.trim_x - (textureInfo.rawWidth - textureInfo.width);
				zero.y = (float)textureInfo.trim_y - (float)(textureInfo.rawHeight - textureInfo.height) * 0.5f;
				break;
			case Anchor.BotLeft:
				zero.x = textureInfo.trim_x;
				zero.y = textureInfo.trim_y;
				break;
			case Anchor.BotCenter:
				zero.x = (float)textureInfo.trim_x - (float)(textureInfo.rawWidth - textureInfo.width) * 0.5f;
				zero.y = textureInfo.trim_y;
				break;
			case Anchor.BotRight:
				zero.x = textureInfo.trim_x - (textureInfo.rawWidth - textureInfo.width);
				zero.y = textureInfo.trim_y;
				break;
			default:
				zero.x = (float)textureInfo.trim_x - (float)(textureInfo.rawWidth - textureInfo.width) * 0.5f;
				zero.y = (float)textureInfo.trim_y - (float)(textureInfo.rawHeight - textureInfo.height) * 0.5f;
				break;
			}
			Vector2 vector = new Vector2(_sprite.width / (float)_sprite.textureInfo.width, _sprite.height / (float)_sprite.textureInfo.height);
			zero.x *= vector.x;
			zero.y *= vector.y;
		}
		return zero;
	}

	public static void GetVertexAndIndexCount(this exISprite _sprite, out int _vertexCount, out int _indexCount)
	{
		switch (_sprite.spriteType)
		{
		case exSpriteType.Simple:
			_vertexCount = 4;
			_indexCount = 6;
			break;
		case exSpriteType.Sliced:
			_vertexCount = 16;
			_indexCount = 54;
			if (_sprite.borderOnly)
			{
				_indexCount -= 6;
			}
			break;
		case exSpriteType.Tiled:
		{
			exSpriteUtility.GetTilingCount(_sprite, out var _colCount, out var _rowCount);
			int num2 = _colCount * _rowCount;
			_vertexCount = num2 * 4;
			if (_vertexCount > 65000)
			{
				Debug.LogWarning(_sprite.gameObject.name + " is too big. Consider using a bigger texture.", _sprite.gameObject);
				int num3 = (int)Mathf.Sqrt(16250f);
				if (_colCount > num3)
				{
					_sprite.width = ((float)_sprite.textureInfo.width + _sprite.tiledSpacing.x) * (float)num3;
				}
				if (_rowCount > num3)
				{
					_sprite.height = ((float)_sprite.textureInfo.height + _sprite.tiledSpacing.y) * (float)num3;
				}
				exSpriteUtility.GetTilingCount(_sprite, out _colCount, out _rowCount);
				num2 = _colCount * _rowCount;
				_vertexCount = num2 * 4;
			}
			_indexCount = num2 * 6;
			break;
		}
		case exSpriteType.Diced:
		{
			exTextureInfo textureInfo = _sprite.textureInfo;
			if (textureInfo == null || !textureInfo.isDiced)
			{
				_vertexCount = 4;
				_indexCount = 6;
				break;
			}
			int num = 0;
			DiceEnumerator dices = _sprite.textureInfo.dices;
			while (dices.MoveNext())
			{
				if (dices.Current.sizeType != exTextureInfo.DiceType.Empty)
				{
					num++;
				}
			}
			if (num == 0)
			{
				num = 1;
			}
			_vertexCount = num * 4;
			_indexCount = num * 6;
			if (_vertexCount > 65000)
			{
				Debug.LogError("The texture info [" + _sprite.textureInfo.name + "] has too many dices! Please using a bigger dice value.", _sprite.textureInfo);
				_vertexCount = 4;
				_indexCount = 6;
			}
			break;
		}
		default:
			_vertexCount = 4;
			_indexCount = 6;
			break;
		}
	}
}
