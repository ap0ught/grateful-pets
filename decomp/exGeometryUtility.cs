using UnityEngine;

public static class exGeometryUtility
{
	public static int RectRect_Contains(Rect _a, Rect _b)
	{
		if (_a.xMin <= _b.xMin && _a.xMax >= _b.xMax && _a.yMin <= _b.yMin && _a.yMax >= _b.yMax)
		{
			return 1;
		}
		if (_b.xMin <= _a.xMin && _b.xMax >= _a.xMax && _b.yMin <= _a.yMin && _b.yMax >= _a.yMax)
		{
			return -1;
		}
		return 0;
	}

	public static bool RectRect_Intersect(Rect _a, Rect _b)
	{
		if (((_a.xMin <= _b.xMin && _a.xMax >= _b.xMin) || (_b.xMin <= _a.xMin && _b.xMax >= _a.xMin)) && ((_a.yMin <= _b.yMin && _a.yMax >= _b.yMin) || (_b.yMin <= _a.yMin && _b.yMax >= _a.yMin)))
		{
			return true;
		}
		return false;
	}

	public static Rect Rect_FloorToInt(Rect _rect)
	{
		return new Rect(Mathf.FloorToInt(_rect.x), Mathf.FloorToInt(_rect.y), Mathf.FloorToInt(_rect.width), Mathf.FloorToInt(_rect.height));
	}

	public static Rect GetAABoundingRect(Vector3[] _vertices)
	{
		Rect result = default;
		if (_vertices.Length != 0)
		{
			result.x = _vertices[0].x;
			result.y = _vertices[0].y;
			for (int i = 1; i < _vertices.Length; i++)
			{
				Vector3 vector = _vertices[i];
				if (vector.x < result.xMin)
				{
					result.xMin = vector.x;
				}
				else if (vector.x > result.xMax)
				{
					result.xMax = vector.x;
				}
				if (vector.y < result.yMin)
				{
					result.yMin = vector.y;
				}
				else if (vector.y > result.yMax)
				{
					result.yMax = vector.y;
				}
			}
		}
		return result;
	}

	public static Vector2 GetConstrainOffset(Rect _rect, Rect _bound)
	{
		Vector2 zero = Vector2.zero;
		if (_bound.width > _rect.width)
		{
			float num = _bound.width - _rect.width;
			_rect.xMin -= num;
			_rect.xMax += num;
		}
		if (_bound.height > _rect.height)
		{
			float num2 = _bound.height - _rect.height;
			_rect.yMin -= num2;
			_rect.yMax += num2;
		}
		if (_bound.xMin < _rect.xMin)
		{
			zero.x += _rect.xMin - _bound.xMin;
		}
		if (_bound.xMax > _rect.xMax)
		{
			zero.x -= _bound.xMax - _rect.xMax;
		}
		if (_bound.yMin < _rect.yMin)
		{
			zero.y += _rect.yMin - _bound.yMin;
		}
		if (_bound.yMax > _rect.yMax)
		{
			zero.y -= _bound.yMax - _rect.yMax;
		}
		return zero;
	}
}
