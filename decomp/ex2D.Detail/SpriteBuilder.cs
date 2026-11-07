using UnityEngine;

namespace ex2D.Detail;

internal static class SpriteBuilder
{
	public static void SimpleUpdateBuffers(exSpriteBase _sprite, exTextureInfo _textureInfo, bool _useTextureOffset, Space _space, exList<Vector3> _vertices, exList<Vector2> _uvs, exList<int> _indices, int _vbIndex, int _ibIndex)
	{
		if ((_sprite.updateFlags & exUpdateFlags.Vertex) != 0)
		{
			SimpleUpdateVertexBuffer(_sprite, _textureInfo, _useTextureOffset, _space, _vertices, _vbIndex);
		}
		if ((_sprite.updateFlags & exUpdateFlags.Index) != 0 && _indices != null)
		{
			_indices.buffer[_ibIndex] = _vbIndex;
			_indices.buffer[_ibIndex + 1] = _vbIndex + 1;
			_indices.buffer[_ibIndex + 2] = _vbIndex + 2;
			_indices.buffer[_ibIndex + 3] = _vbIndex + 2;
			_indices.buffer[_ibIndex + 4] = _vbIndex + 3;
			_indices.buffer[_ibIndex + 5] = _vbIndex;
		}
		if ((_sprite.updateFlags & exUpdateFlags.UV) != 0)
		{
			Vector2 vector = ((!(_textureInfo.texture != null)) ? new Vector2(1f / (float)_textureInfo.rawWidth, 1f / (float)_textureInfo.rawHeight) : _textureInfo.texture.texelSize);
			Vector2 vector2 = new Vector2((float)_textureInfo.x * vector.x, (float)_textureInfo.y * vector.y);
			Vector2 vector3 = new Vector2((float)(_textureInfo.x + _textureInfo.rotatedWidth) * vector.x, (float)(_textureInfo.y + _textureInfo.rotatedHeight) * vector.y);
			if (_textureInfo.rotated)
			{
				_uvs.buffer[_vbIndex] = new Vector2(vector3.x, vector2.y);
				_uvs.buffer[_vbIndex + 1] = vector2;
				_uvs.buffer[_vbIndex + 2] = new Vector2(vector2.x, vector3.y);
				_uvs.buffer[_vbIndex + 3] = vector3;
			}
			else
			{
				_uvs.buffer[_vbIndex] = vector2;
				_uvs.buffer[_vbIndex + 1] = new Vector2(vector2.x, vector3.y);
				_uvs.buffer[_vbIndex + 2] = vector3;
				_uvs.buffer[_vbIndex + 3] = new Vector2(vector3.x, vector2.y);
			}
		}
	}

	public static void SimpleUpdateVertexBuffer(exSpriteBase _sprite, exTextureInfo _textureInfo, bool _useTextureOffset, Space _space, exList<Vector3> _vertices, int _startIndex)
	{
		float num = (float)_textureInfo.height * 0.5f;
		float num2 = (float)_textureInfo.width * 0.5f;
		Vector2 vector = default;
		if (_useTextureOffset)
		{
			switch (_sprite.anchor)
			{
			case Anchor.TopLeft:
				vector.x = num2 + (float)_textureInfo.trim_x;
				vector.y = 0f - num + (float)_textureInfo.trim_y - (float)(_textureInfo.rawHeight - _textureInfo.height);
				break;
			case Anchor.TopCenter:
				vector.x = (float)_textureInfo.trim_x - (float)(_textureInfo.rawWidth - _textureInfo.width) * 0.5f;
				vector.y = 0f - num + (float)_textureInfo.trim_y - (float)(_textureInfo.rawHeight - _textureInfo.height);
				break;
			case Anchor.TopRight:
				vector.x = 0f - num2 + (float)_textureInfo.trim_x - (float)(_textureInfo.rawWidth - _textureInfo.width);
				vector.y = 0f - num + (float)_textureInfo.trim_y - (float)(_textureInfo.rawHeight - _textureInfo.height);
				break;
			case Anchor.MidLeft:
				vector.x = num2 + (float)_textureInfo.trim_x;
				vector.y = (float)_textureInfo.trim_y - (float)(_textureInfo.rawHeight - _textureInfo.height) * 0.5f;
				break;
			case Anchor.MidCenter:
				vector.x = (float)_textureInfo.trim_x - (float)(_textureInfo.rawWidth - _textureInfo.width) * 0.5f;
				vector.y = (float)_textureInfo.trim_y - (float)(_textureInfo.rawHeight - _textureInfo.height) * 0.5f;
				break;
			case Anchor.MidRight:
				vector.x = 0f - num2 + (float)_textureInfo.trim_x - (float)(_textureInfo.rawWidth - _textureInfo.width);
				vector.y = (float)_textureInfo.trim_y - (float)(_textureInfo.rawHeight - _textureInfo.height) * 0.5f;
				break;
			case Anchor.BotLeft:
				vector.x = num2 + (float)_textureInfo.trim_x;
				vector.y = num + (float)_textureInfo.trim_y;
				break;
			case Anchor.BotCenter:
				vector.x = (float)_textureInfo.trim_x - (float)(_textureInfo.rawWidth - _textureInfo.width) * 0.5f;
				vector.y = num + (float)_textureInfo.trim_y;
				break;
			case Anchor.BotRight:
				vector.x = 0f - num2 + (float)_textureInfo.trim_x - (float)(_textureInfo.rawWidth - _textureInfo.width);
				vector.y = num + (float)_textureInfo.trim_y;
				break;
			default:
				vector.x = (float)_textureInfo.trim_x - (float)(_textureInfo.rawWidth - _textureInfo.width) * 0.5f;
				vector.y = (float)_textureInfo.trim_y - (float)(_textureInfo.rawHeight - _textureInfo.height) * 0.5f;
				break;
			}
		}
		else
		{
			switch (_sprite.anchor)
			{
			case Anchor.TopLeft:
				vector.x = num2;
				vector.y = 0f - num;
				break;
			case Anchor.TopCenter:
				vector.x = 0f;
				vector.y = 0f - num;
				break;
			case Anchor.TopRight:
				vector.x = 0f - num2;
				vector.y = 0f - num;
				break;
			case Anchor.MidLeft:
				vector.x = num2;
				vector.y = 0f;
				break;
			case Anchor.MidCenter:
				vector.x = 0f;
				vector.y = 0f;
				break;
			case Anchor.MidRight:
				vector.x = 0f - num2;
				vector.y = 0f;
				break;
			case Anchor.BotLeft:
				vector.x = num2;
				vector.y = num;
				break;
			case Anchor.BotCenter:
				vector.x = 0f;
				vector.y = num;
				break;
			case Anchor.BotRight:
				vector.x = 0f - num2;
				vector.y = num;
				break;
			default:
				vector.x = 0f;
				vector.y = 0f;
				break;
			}
		}
		Vector3 vector2 = new Vector3(0f - num2 + vector.x, 0f - num + vector.y, 0f);
		Vector3 vector3 = new Vector3(0f - num2 + vector.x, num + vector.y, 0f);
		Vector3 vector4 = new Vector3(num2 + vector.x, num + vector.y, 0f);
		Vector3 vector5 = new Vector3(num2 + vector.x, 0f - num + vector.y, 0f);
		if (_sprite.customSize)
		{
			Vector2 vector6 = new Vector2(_sprite.width / (float)_textureInfo.width, _sprite.height / (float)_textureInfo.height);
			vector2.x *= vector6.x;
			vector2.y *= vector6.y;
			vector3.x *= vector6.x;
			vector3.y *= vector6.y;
			vector4.x *= vector6.x;
			vector4.y *= vector6.y;
			vector5.x *= vector6.x;
			vector5.y *= vector6.y;
		}
		Vector3 vector7 = _sprite.offset;
		vector2 += vector7;
		vector3 += vector7;
		vector4 += vector7;
		vector5 += vector7;
		Vector2 shear = _sprite.shear;
		if (shear.x != 0f)
		{
			float num3 = _sprite.GetScaleY(_space) * shear.x;
			float num4 = num3 * (num + vector.y);
			float num5 = num3 * (0f - num + vector.y);
			vector2.x += num5;
			vector3.x += num4;
			vector4.x += num4;
			vector5.x += num5;
		}
		if (shear.y != 0f)
		{
			float num6 = _sprite.GetScaleX(_space) * shear.y;
			float num7 = num6 * (0f - num2 + vector.x);
			float num8 = num6 * (num2 + vector.x);
			vector2.y += num7;
			vector3.y += num7;
			vector4.y += num8;
			vector5.y += num8;
		}
		if (_space == Space.World)
		{
			vector2 = _sprite.cachedWorldMatrix.MultiplyPoint3x4(vector2);
			vector3 = _sprite.cachedWorldMatrix.MultiplyPoint3x4(vector3);
			vector4 = _sprite.cachedWorldMatrix.MultiplyPoint3x4(vector4);
			vector5 = _sprite.cachedWorldMatrix.MultiplyPoint3x4(vector5);
			vector2.z = 0f;
			vector3.z = 0f;
			vector4.z = 0f;
			vector5.z = 0f;
		}
		_vertices.buffer[_startIndex] = vector2;
		_vertices.buffer[_startIndex + 1] = vector3;
		_vertices.buffer[_startIndex + 2] = vector4;
		_vertices.buffer[_startIndex + 3] = vector5;
	}

	public static void SlicedUpdateBuffers(exSpriteBase _sprite, exTextureInfo _textureInfo, bool _useTextureOffset, Space _space, exList<Vector3> _vertices, exList<Vector2> _uvs, exList<int> _indices, int _vbIndex, int _ibIndex)
	{
		SimpleUpdateBuffers(_sprite, _textureInfo, _useTextureOffset, _space, _vertices, _uvs, _indices, _vbIndex, _ibIndex);
		if ((_sprite.updateFlags & exUpdateFlags.Vertex) != 0)
		{
			SimpleVertexBufferToSliced(_sprite, _textureInfo, _vertices, _vbIndex);
		}
		if ((_sprite.updateFlags & exUpdateFlags.Index) != 0 && _indices != null)
		{
			int num = ((_sprite as exISprite).borderOnly ? 5 : int.MinValue);
			for (int i = 0; i <= 10; i++)
			{
				if (i != 3 && i != 7 && i != num)
				{
					int num2 = _vbIndex + i;
					_indices.buffer[_ibIndex++] = num2;
					_indices.buffer[_ibIndex++] = num2 + 4;
					_indices.buffer[_ibIndex++] = num2 + 5;
					_indices.buffer[_ibIndex++] = num2 + 5;
					_indices.buffer[_ibIndex++] = num2 + 1;
					_indices.buffer[_ibIndex++] = num2;
				}
			}
		}
		if ((_sprite.updateFlags & exUpdateFlags.UV) != 0)
		{
			float num3;
			float num4;
			float num5;
			float num6;
			if (!_textureInfo.rotated)
			{
				num3 = (float)_textureInfo.borderBottom / (float)_textureInfo.height;
				num4 = (float)(_textureInfo.height - _textureInfo.borderTop) / (float)_textureInfo.height;
				num5 = (float)_textureInfo.borderLeft / (float)_textureInfo.width;
				num6 = (float)(_textureInfo.width - _textureInfo.borderRight) / (float)_textureInfo.width;
			}
			else
			{
				num5 = (float)_textureInfo.borderBottom / (float)_textureInfo.height;
				num6 = (float)(_textureInfo.height - _textureInfo.borderTop) / (float)_textureInfo.height;
				num3 = (float)_textureInfo.borderLeft / (float)_textureInfo.width;
				num4 = (float)(_textureInfo.width - _textureInfo.borderRight) / (float)_textureInfo.width;
			}
			Vector2 vector = _uvs.buffer[_vbIndex];
			Vector2 vector2 = _uvs.buffer[_vbIndex + 2];
			Vector2 vector3 = new Vector2(vector.x + (vector2.x - vector.x) * num5, vector.y + (vector2.y - vector.y) * num3);
			Vector2 vector4 = new Vector2(vector.x + (vector2.x - vector.x) * num6, vector.y + (vector2.y - vector.y) * num4);
			if (!_textureInfo.rotated)
			{
				_uvs.buffer[_vbIndex + 1] = new Vector2(vector3.x, vector.y);
				_uvs.buffer[_vbIndex + 2] = new Vector2(vector4.x, vector.y);
				_uvs.buffer[_vbIndex + 3] = new Vector2(vector2.x, vector.y);
				_uvs.buffer[_vbIndex + 4] = new Vector2(vector.x, vector3.y);
				_uvs.buffer[_vbIndex + 5] = vector3;
				_uvs.buffer[_vbIndex + 6] = new Vector2(vector4.x, vector3.y);
				_uvs.buffer[_vbIndex + 7] = new Vector2(vector2.x, vector3.y);
				_uvs.buffer[_vbIndex + 8] = new Vector2(vector.x, vector4.y);
				_uvs.buffer[_vbIndex + 9] = new Vector2(vector3.x, vector4.y);
				_uvs.buffer[_vbIndex + 10] = vector4;
				_uvs.buffer[_vbIndex + 11] = new Vector2(vector2.x, vector4.y);
				_uvs.buffer[_vbIndex + 12] = new Vector2(vector.x, vector2.y);
				_uvs.buffer[_vbIndex + 13] = new Vector2(vector3.x, vector2.y);
				_uvs.buffer[_vbIndex + 14] = new Vector2(vector4.x, vector2.y);
				_uvs.buffer[_vbIndex + 15] = vector2;
			}
			else
			{
				_uvs.buffer[_vbIndex + 1] = new Vector2(vector.x, vector3.y);
				_uvs.buffer[_vbIndex + 2] = new Vector2(vector.x, vector4.y);
				_uvs.buffer[_vbIndex + 3] = new Vector2(vector.x, vector2.y);
				_uvs.buffer[_vbIndex + 4] = new Vector2(vector3.x, vector.y);
				_uvs.buffer[_vbIndex + 5] = vector3;
				_uvs.buffer[_vbIndex + 6] = new Vector2(vector3.x, vector4.y);
				_uvs.buffer[_vbIndex + 7] = new Vector2(vector3.x, vector2.y);
				_uvs.buffer[_vbIndex + 8] = new Vector2(vector4.x, vector.y);
				_uvs.buffer[_vbIndex + 9] = new Vector2(vector4.x, vector3.y);
				_uvs.buffer[_vbIndex + 10] = vector4;
				_uvs.buffer[_vbIndex + 11] = new Vector2(vector4.x, vector2.y);
				_uvs.buffer[_vbIndex + 12] = new Vector2(vector2.x, vector.y);
				_uvs.buffer[_vbIndex + 13] = new Vector2(vector2.x, vector3.y);
				_uvs.buffer[_vbIndex + 14] = new Vector2(vector2.x, vector4.y);
				_uvs.buffer[_vbIndex + 15] = vector2;
			}
		}
	}

	public static void SimpleVertexBufferToSliced(exSpriteBase _sprite, exTextureInfo textureInfo_, exList<Vector3> _vertices, int _startIndex)
	{
		exISprite exISprite2 = _sprite as exISprite;
		float num;
		float num2;
		float num3;
		float num4;
		if (exISprite2.customBorderSize)
		{
			num = exISprite2.leftBorderSize;
			num2 = exISprite2.rightBorderSize;
			num3 = exISprite2.topBorderSize;
			num4 = exISprite2.bottomBorderSize;
		}
		else
		{
			num = textureInfo_.borderLeft;
			num2 = textureInfo_.borderRight;
			num3 = textureInfo_.borderTop;
			num4 = textureInfo_.borderBottom;
		}
		Vector3 vector = _vertices.buffer[_startIndex];
		Vector3 vector2 = _vertices.buffer[_startIndex + 1];
		Vector3 vector3 = _vertices.buffer[_startIndex + 2];
		Vector3 vector4 = _vertices.buffer[_startIndex + 3];
		_vertices.buffer[_startIndex + 12] = vector2;
		_vertices.buffer[_startIndex + 15] = vector3;
		float height = _sprite.height;
		float num5 = num4 / height;
		float num6 = (height - num3) / height;
		_vertices.buffer[_startIndex + 4] = vector + (vector2 - vector) * num5;
		_vertices.buffer[_startIndex + 7] = vector4 + (vector3 - vector4) * num5;
		_vertices.buffer[_startIndex + 8] = vector + (vector2 - vector) * num6;
		_vertices.buffer[_startIndex + 11] = vector4 + (vector3 - vector4) * num6;
		float width = _sprite.width;
		float num7 = num / width;
		float num8 = (width - num2) / width;
		for (int i = 0; i <= 12; i += 4)
		{
			Vector3 vector5 = _vertices.buffer[_startIndex + i];
			Vector3 vector6 = _vertices.buffer[_startIndex + i + 3];
			_vertices.buffer[_startIndex + i + 1] = vector5 + (vector6 - vector5) * num7;
			_vertices.buffer[_startIndex + i + 2] = vector5 + (vector6 - vector5) * num8;
		}
	}

	public static void TiledUpdateBuffers(exSpriteBase _sprite, exTextureInfo _textureInfo, bool _useTextureOffset, Vector2 _tiledSpacing, Space _space, exList<Vector3> _vertices, exList<Vector2> _uvs, exList<int> _indices, int _vbIndex, int _ibIndex)
	{
		if (_vertices.Count == 0)
		{
			return;
		}
		SimpleUpdateBuffers(_sprite, _textureInfo, _useTextureOffset, _space, _vertices, _uvs, _indices, _vbIndex, _ibIndex);
		if ((_sprite.updateFlags & exUpdateFlags.Vertex) != 0)
		{
			TiledUpdateVertexBuffer(_sprite, _textureInfo, _useTextureOffset, _tiledSpacing, _space, _vertices, _vbIndex);
		}
		exSpriteUtility.GetTilingCount((exISprite)_sprite, out var _colCount, out var _rowCount);
		if ((_sprite.updateFlags & exUpdateFlags.Index) != 0 && _indices != null)
		{
			int num = _vbIndex;
			int num2 = _ibIndex;
			int num3 = _colCount * _rowCount;
			for (int i = 0; i < num3; i++)
			{
				_indices.buffer[num2++] = num;
				_indices.buffer[num2++] = num + 1;
				_indices.buffer[num2++] = num + 2;
				_indices.buffer[num2++] = num + 2;
				_indices.buffer[num2++] = num + 3;
				_indices.buffer[num2++] = num;
				num += 4;
			}
		}
		if ((_sprite.updateFlags & exUpdateFlags.UV) == 0)
		{
			return;
		}
		Vector2 vector = _uvs.buffer[_vbIndex];
		Vector2 vector2 = _uvs.buffer[_vbIndex + 2];
		Vector2 vector3 = _uvs.buffer[_vbIndex + 3];
		Vector2 vector4 = new Vector2(_sprite.width % ((float)_textureInfo.width + _tiledSpacing.x), _sprite.height % ((float)_textureInfo.height + _tiledSpacing.y));
		Vector2 vector5 = vector2;
		if (0f < vector4.y && vector4.y < (float)_textureInfo.height)
		{
			float t = vector4.y / (float)_textureInfo.height;
			if (!_textureInfo.rotated)
			{
				vector5.y = Mathf.Lerp(vector.y, vector2.y, t);
			}
			else
			{
				vector5.x = Mathf.Lerp(vector.x, vector2.x, t);
			}
		}
		if (0f < vector4.x && vector4.x < (float)_textureInfo.width)
		{
			float t2 = vector4.x / (float)_textureInfo.width;
			if (!_textureInfo.rotated)
			{
				vector5.x = Mathf.Lerp(vector.x, vector2.x, t2);
			}
			else
			{
				vector5.y = Mathf.Lerp(vector.y, vector2.y, t2);
			}
		}
		int num4 = _vbIndex;
		if (!_textureInfo.rotated)
		{
			for (int j = 0; j < _rowCount; j++)
			{
				float y = ((j < _rowCount - 1) ? vector2.y : vector5.y);
				for (int k = 0; k < _colCount; k++)
				{
					_uvs.buffer[num4++] = vector;
					_uvs.buffer[num4++] = new Vector2(vector.x, y);
					_uvs.buffer[num4++] = new Vector2(vector2.x, y);
					_uvs.buffer[num4++] = vector3;
				}
				_uvs.buffer[num4 - 2].x = vector5.x;
				_uvs.buffer[num4 - 1].x = vector5.x;
			}
			return;
		}
		for (int l = 0; l < _rowCount; l++)
		{
			float x = ((l < _rowCount - 1) ? vector2.x : vector5.x);
			for (int m = 0; m < _colCount; m++)
			{
				_uvs.buffer[num4++] = vector;
				_uvs.buffer[num4++] = new Vector2(x, vector.y);
				_uvs.buffer[num4++] = new Vector2(x, vector2.y);
				_uvs.buffer[num4++] = vector3;
			}
			_uvs.buffer[num4 - 2].y = vector5.y;
			_uvs.buffer[num4 - 1].y = vector5.y;
		}
	}

	public static void TiledUpdateVertexBuffer(exSpriteBase _sprite, exTextureInfo _textureInfo, bool _useTextureOffset, Vector2 _tiledSpacing, Space _space, exList<Vector3> _vertices, int _startIndex)
	{
		int width = _textureInfo.width;
		int height = _textureInfo.height;
		int rawWidth = _textureInfo.rawWidth;
		int rawHeight = _textureInfo.rawHeight;
		try
		{
			_textureInfo.width = Mathf.Max((int)Mathf.Abs(_sprite.width), 1);
			_textureInfo.height = Mathf.Max((int)Mathf.Abs(_sprite.height), 1);
			_textureInfo.rawWidth = Mathf.Max(_textureInfo.width + rawWidth - width, 1);
			_textureInfo.rawHeight = Mathf.Max(_textureInfo.height + rawHeight - height, 1);
			SimpleUpdateVertexBuffer(_sprite, _textureInfo, _useTextureOffset, _space, _vertices, _startIndex);
		}
		finally
		{
			_textureInfo.width = width;
			_textureInfo.height = height;
			_textureInfo.rawWidth = rawWidth;
			_textureInfo.rawHeight = rawHeight;
		}
		Vector3 vector = _vertices.buffer[_startIndex];
		Vector3 vector2 = _vertices.buffer[_startIndex + 1];
		Vector3 vector3 = _vertices.buffer[_startIndex + 2];
		exSpriteUtility.GetTilingCount((exISprite)_sprite, out var _colCount, out var _rowCount);
		Vector2 vector4 = new Vector2(_sprite.width % ((float)_textureInfo.width + _tiledSpacing.x), _sprite.height % ((float)_textureInfo.height + _tiledSpacing.y));
		Vector3 vector5;
		if (vector4.x > 0f)
		{
			float num = vector4.x / ((float)_textureInfo.width + _tiledSpacing.x);
			vector5 = (vector3 - vector2) / ((float)(_colCount - 1) + num);
		}
		else
		{
			vector5 = (vector3 - vector2) / _colCount;
		}
		Vector3 vector6;
		if (vector4.y > 0f)
		{
			float num2 = vector4.y / ((float)_textureInfo.height + _tiledSpacing.y);
			vector6 = (vector2 - vector) / ((float)(_rowCount - 1) + num2);
		}
		else
		{
			vector6 = (vector2 - vector) / _rowCount;
		}
		Vector2 vector7 = new Vector2(vector4.x / (float)_textureInfo.width, vector4.y / (float)_textureInfo.height);
		Vector3 vector8 = vector6 / ((float)_textureInfo.height + _tiledSpacing.y) * _textureInfo.height;
		Vector3 vector9 = vector5 / ((float)_textureInfo.width + _tiledSpacing.x) * _textureInfo.width;
		int num3 = _startIndex;
		Vector3 vector10 = vector;
		for (int i = 0; i < _rowCount; i++)
		{
			Vector3 vector11 = vector10;
			Vector3 vector12 = ((i >= _rowCount - 1 && !(vector7.y >= 1f) && vector7.y != 0f) ? vector2 : (vector11 + vector8));
			for (int j = 0; j < _colCount; j++)
			{
				_vertices.buffer[num3++] = vector11;
				_vertices.buffer[num3++] = vector12;
				_vertices.buffer[num3++] = vector12 + vector9;
				_vertices.buffer[num3++] = vector11 + vector9;
				vector11 += vector5;
				vector12 += vector5;
			}
			if (0f < vector7.x && vector7.x < 1f)
			{
				Vector3 vector13 = vector9 * (1f - vector7.x);
				_vertices.buffer[num3 - 2] -= vector13;
				_vertices.buffer[num3 - 1] -= vector13;
			}
			vector10 += vector6;
		}
	}

	public static void DicedUpdateBuffers(exSpriteBase _sprite, exTextureInfo _textureInfo, bool _useTextureOffset, Space _space, exList<Vector3> _vertices, exList<Vector2> _uvs, exList<int> _indices, int _vbIndex, int _ibIndex)
	{
		if (!_textureInfo.isDiced)
		{
			SimpleUpdateBuffers(_sprite, _textureInfo, _useTextureOffset, _space, _vertices, _uvs, _indices, _vbIndex, _ibIndex);
			return;
		}
		if ((_sprite.updateFlags & exUpdateFlags.Vertex) != 0)
		{
			SimpleUpdateVertexBuffer(_sprite, _textureInfo, _useTextureOffset, _space, _vertices, _vbIndex);
			SimpleVertexBufferToDiced(_sprite, _textureInfo, _vertices, _vbIndex);
		}
		if ((_sprite.updateFlags & exUpdateFlags.Index) != 0 && _indices != null)
		{
			int num = _ibIndex;
			for (int i = _vbIndex; i < _vertices.Count; i += 4)
			{
				_indices.buffer[num++] = i;
				_indices.buffer[num++] = i + 1;
				_indices.buffer[num++] = i + 2;
				_indices.buffer[num++] = i + 2;
				_indices.buffer[num++] = i + 3;
				_indices.buffer[num++] = i;
			}
		}
		if ((_sprite.updateFlags & exUpdateFlags.UV) == 0)
		{
			return;
		}
		Vector2 vector = ((!(_textureInfo.texture != null)) ? new Vector2(1f / (float)_textureInfo.rawWidth, 1f / (float)_textureInfo.rawHeight) : _textureInfo.texture.texelSize);
		foreach (exTextureInfo.Dice dix in _textureInfo.dices)
		{
			if (dix.sizeType != exTextureInfo.DiceType.Empty)
			{
				Vector2 vector2 = new Vector2((float)dix.x * vector.x, (float)dix.y * vector.y);
				Vector2 vector3 = new Vector2((float)(dix.x + dix.rotatedWidth) * vector.x, (float)(dix.y + dix.rotatedHeight) * vector.y);
				if (dix.rotated)
				{
					_uvs.buffer[_vbIndex++] = new Vector2(vector3.x, vector2.y);
					_uvs.buffer[_vbIndex++] = vector2;
					_uvs.buffer[_vbIndex++] = new Vector2(vector2.x, vector3.y);
					_uvs.buffer[_vbIndex++] = vector3;
				}
				else
				{
					_uvs.buffer[_vbIndex++] = vector2;
					_uvs.buffer[_vbIndex++] = new Vector2(vector2.x, vector3.y);
					_uvs.buffer[_vbIndex++] = vector3;
					_uvs.buffer[_vbIndex++] = new Vector2(vector3.x, vector2.y);
				}
			}
		}
	}

	public static void SimpleVertexBufferToDiced(exSpriteBase _sprite, exTextureInfo _textureInfo, exList<Vector3> _vertices, int _startIndex)
	{
		if (!_textureInfo.isDiced)
		{
			return;
		}
		Vector3 vector = _vertices.buffer[_startIndex];
		Vector3 vector2 = _vertices.buffer[_startIndex + 1];
		Vector3 vector3 = _vertices.buffer[_startIndex + 2];
		exSpriteUtility.GetDicingCount(_textureInfo, out var _colCount, out var _rowCount);
		Vector2 vector4 = default;
		if (_textureInfo.diceUnitWidth > 0)
		{
			vector4.x = _textureInfo.width % _textureInfo.diceUnitWidth;
		}
		if (_textureInfo.diceUnitHeight > 0)
		{
			vector4.y = _textureInfo.height % _textureInfo.diceUnitHeight;
		}
		Vector3 vector5;
		if (vector4.x > 0f)
		{
			float num = vector4.x / (float)_textureInfo.diceUnitWidth;
			vector5 = (vector3 - vector2) / ((float)(_colCount - 1) + num);
		}
		else
		{
			vector5 = (vector3 - vector2) / _colCount;
		}
		Vector3 vector6;
		if (vector4.y > 0f)
		{
			float num2 = vector4.y / (float)_textureInfo.diceUnitHeight;
			vector6 = (vector2 - vector) / ((float)(_rowCount - 1) + num2);
		}
		else
		{
			vector6 = (vector2 - vector) / _rowCount;
		}
		Vector3 vector7 = vector5 / _textureInfo.diceUnitWidth;
		Vector3 vector8 = vector6 / _textureInfo.diceUnitHeight;
		int num3 = _startIndex;
		Vector3 vector9 = vector;
		DiceEnumerator dices = _textureInfo.dices;
		for (int i = 0; i < _rowCount; i++)
		{
			Vector3 vector10 = vector9;
			Vector3 vector11 = vector10 + vector6;
			for (int j = 0; j < _colCount; j++)
			{
				if (!dices.MoveNext())
				{
					return;
				}
				exTextureInfo.Dice current = dices.Current;
				if (current.sizeType == exTextureInfo.DiceType.Max)
				{
					_vertices.buffer[num3++] = vector10;
					_vertices.buffer[num3++] = vector11;
					_vertices.buffer[num3++] = vector11 + vector5;
					_vertices.buffer[num3++] = vector10 + vector5;
				}
				else if (current.sizeType == exTextureInfo.DiceType.Trimmed)
				{
					Vector3 vector12 = vector7 * current.offset_x;
					Vector3 vector13 = vector8 * current.offset_y;
					Vector3 vector14 = vector7 * (current.offset_x + current.width);
					Vector3 vector15 = vector8 * (current.offset_y + current.height);
					_vertices.buffer[num3++] = vector10 + vector12 + vector13;
					_vertices.buffer[num3++] = vector10 + vector12 + vector15;
					_vertices.buffer[num3++] = vector10 + vector14 + vector15;
					_vertices.buffer[num3++] = vector10 + vector14 + vector13;
				}
				vector10 += vector5;
				vector11 += vector5;
			}
			vector9 += vector6;
		}
	}
}
