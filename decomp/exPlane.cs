using UnityEngine;

public abstract class exPlane : MonoBehaviour
{
	[SerializeField]
	protected float width_ = 1f;

	[SerializeField]
	protected float height_ = 1f;

	[SerializeField]
	protected Anchor anchor_ = Anchor.MidCenter;

	[SerializeField]
	protected Vector2 offset_ = Vector2.zero;

	public virtual float width
	{
		get
		{
			return width_;
		}
		set
		{
			width_ = value;
		}
	}

	public virtual float height
	{
		get
		{
			return height_;
		}
		set
		{
			height_ = value;
		}
	}

	public virtual Anchor anchor
	{
		get
		{
			return anchor_;
		}
		set
		{
			anchor_ = value;
		}
	}

	public virtual Vector2 offset
	{
		get
		{
			return offset_;
		}
		set
		{
			offset_ = value;
		}
	}

	public bool hasSprite
	{
		get
		{
			exSpriteBase component = GetComponent<exSpriteBase>();
			if (component != null && component != this)
			{
				return true;
			}
			return false;
		}
	}

	public Rect GetWorldAABoundingRect()
	{
		return exGeometryUtility.GetAABoundingRect(GetWorldVertices());
	}

	public Rect GetLocalAABoundingRect()
	{
		return exGeometryUtility.GetAABoundingRect(GetLocalVertices());
	}

	public Vector3[] GetRectVertices(Space _space)
	{
		float num = height_ * 0.5f;
		float num2 = width_ * 0.5f;
		Vector2 vector = default;
		switch (anchor_)
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
		vector.x += offset_.x;
		vector.y += offset_.y;
		Vector3 vector2 = new Vector3(0f - num2 + vector.x, 0f - num + vector.y, 0f);
		Vector3 vector3 = new Vector3(0f - num2 + vector.x, num + vector.y, 0f);
		Vector3 vector4 = new Vector3(num2 + vector.x, num + vector.y, 0f);
		Vector3 vector5 = new Vector3(num2 + vector.x, 0f - num + vector.y, 0f);
		if (_space == Space.World)
		{
			Matrix4x4 localToWorldMatrix = transform.localToWorldMatrix;
			vector2 = localToWorldMatrix.MultiplyPoint3x4(vector2);
			vector3 = localToWorldMatrix.MultiplyPoint3x4(vector3);
			vector4 = localToWorldMatrix.MultiplyPoint3x4(vector4);
			vector5 = localToWorldMatrix.MultiplyPoint3x4(vector5);
		}
		return new Vector3[4] { vector2, vector3, vector4, vector5 };
	}

	protected virtual Vector3[] GetVertices(Space _space)
	{
		return GetRectVertices(_space);
	}

	public virtual Vector3[] GetLocalVertices()
	{
		return GetVertices(Space.Self);
	}

	public virtual Vector3[] GetWorldVertices()
	{
		return GetVertices(Space.World);
	}
}
