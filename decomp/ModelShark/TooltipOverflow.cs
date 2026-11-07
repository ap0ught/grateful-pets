namespace ModelShark;

public class TooltipOverflow
{
	public bool IsAny
	{
		get
		{
			if (!BottomLeftCorner && !TopLeftCorner && !TopRightCorner)
			{
				return BottomRightCorner;
			}
			return true;
		}
	}

	public bool TopEdge
	{
		get
		{
			if (TopLeftCorner)
			{
				return TopRightCorner;
			}
			return false;
		}
	}

	public bool RightEdge
	{
		get
		{
			if (TopRightCorner)
			{
				return BottomRightCorner;
			}
			return false;
		}
	}

	public bool LeftEdge
	{
		get
		{
			if (TopLeftCorner)
			{
				return BottomLeftCorner;
			}
			return false;
		}
	}

	public bool BottomEdge
	{
		get
		{
			if (BottomLeftCorner)
			{
				return BottomRightCorner;
			}
			return false;
		}
	}

	public bool TopRightCorner { get; set; }

	public bool TopLeftCorner { get; set; }

	public bool BottomRightCorner { get; set; }

	public bool BottomLeftCorner { get; set; }

	public TipPosition SuggestNewPosition(TipPosition fromPosition)
	{
		bool flag = fromPosition == TipPosition.MouseBottomLeftCorner || fromPosition == TipPosition.MouseTopLeftCorner || fromPosition == TipPosition.MouseBottomRightCorner || fromPosition == TipPosition.MouseTopRightCorner || fromPosition == TipPosition.MouseTopMiddle || fromPosition == TipPosition.MouseLeftMiddle || fromPosition == TipPosition.MouseRightMiddle || fromPosition == TipPosition.MouseBottomMiddle;
		switch (fromPosition)
		{
		case TipPosition.TopRightCorner:
		case TipPosition.MouseTopRightCorner:
			if (TopEdge && RightEdge)
			{
				if (!flag)
				{
					return TipPosition.BottomLeftCorner;
				}
				return TipPosition.MouseBottomLeftCorner;
			}
			if (TopEdge)
			{
				if (!flag)
				{
					return TipPosition.BottomRightCorner;
				}
				return TipPosition.MouseBottomRightCorner;
			}
			if (RightEdge)
			{
				if (!flag)
				{
					return TipPosition.TopLeftCorner;
				}
				return TipPosition.MouseTopLeftCorner;
			}
			break;
		case TipPosition.BottomRightCorner:
		case TipPosition.MouseBottomRightCorner:
			if (BottomEdge && RightEdge)
			{
				if (!flag)
				{
					return TipPosition.TopLeftCorner;
				}
				return TipPosition.MouseTopLeftCorner;
			}
			if (BottomEdge)
			{
				if (!flag)
				{
					return TipPosition.TopRightCorner;
				}
				return TipPosition.MouseTopRightCorner;
			}
			if (RightEdge)
			{
				if (!flag)
				{
					return TipPosition.BottomLeftCorner;
				}
				return TipPosition.MouseBottomLeftCorner;
			}
			break;
		case TipPosition.TopLeftCorner:
		case TipPosition.MouseTopLeftCorner:
			if (TopEdge && LeftEdge)
			{
				if (!flag)
				{
					return TipPosition.BottomRightCorner;
				}
				return TipPosition.MouseBottomRightCorner;
			}
			if (TopEdge)
			{
				if (!flag)
				{
					return TipPosition.BottomLeftCorner;
				}
				return TipPosition.MouseBottomLeftCorner;
			}
			if (LeftEdge)
			{
				if (!flag)
				{
					return TipPosition.TopRightCorner;
				}
				return TipPosition.MouseTopRightCorner;
			}
			break;
		case TipPosition.BottomLeftCorner:
		case TipPosition.MouseBottomLeftCorner:
			if (BottomEdge && LeftEdge)
			{
				if (!flag)
				{
					return TipPosition.TopRightCorner;
				}
				return TipPosition.MouseTopRightCorner;
			}
			if (BottomEdge)
			{
				if (!flag)
				{
					return TipPosition.TopLeftCorner;
				}
				return TipPosition.MouseTopLeftCorner;
			}
			if (LeftEdge)
			{
				if (!flag)
				{
					return TipPosition.BottomRightCorner;
				}
				return TipPosition.MouseBottomRightCorner;
			}
			break;
		case TipPosition.TopMiddle:
		case TipPosition.MouseTopMiddle:
			if (TopEdge && RightEdge)
			{
				if (!flag)
				{
					return TipPosition.BottomLeftCorner;
				}
				return TipPosition.MouseBottomLeftCorner;
			}
			if (TopEdge && LeftEdge)
			{
				if (!flag)
				{
					return TipPosition.BottomRightCorner;
				}
				return TipPosition.MouseBottomRightCorner;
			}
			if (TopEdge)
			{
				if (!flag)
				{
					return TipPosition.BottomMiddle;
				}
				return TipPosition.MouseBottomMiddle;
			}
			if (RightEdge)
			{
				if (!flag)
				{
					return TipPosition.LeftMiddle;
				}
				return TipPosition.MouseLeftMiddle;
			}
			if (LeftEdge)
			{
				if (!flag)
				{
					return TipPosition.RightMiddle;
				}
				return TipPosition.MouseRightMiddle;
			}
			break;
		case TipPosition.BottomMiddle:
		case TipPosition.MouseBottomMiddle:
			if (BottomEdge && RightEdge)
			{
				if (!flag)
				{
					return TipPosition.TopLeftCorner;
				}
				return TipPosition.MouseTopLeftCorner;
			}
			if (BottomEdge && LeftEdge)
			{
				if (!flag)
				{
					return TipPosition.TopRightCorner;
				}
				return TipPosition.MouseTopRightCorner;
			}
			if (BottomEdge)
			{
				if (!flag)
				{
					return TipPosition.TopMiddle;
				}
				return TipPosition.MouseTopMiddle;
			}
			if (RightEdge)
			{
				if (!flag)
				{
					return TipPosition.LeftMiddle;
				}
				return TipPosition.MouseLeftMiddle;
			}
			if (LeftEdge)
			{
				if (!flag)
				{
					return TipPosition.RightMiddle;
				}
				return TipPosition.MouseRightMiddle;
			}
			break;
		case TipPosition.LeftMiddle:
		case TipPosition.MouseLeftMiddle:
			if (TopEdge && LeftEdge)
			{
				if (!flag)
				{
					return TipPosition.BottomRightCorner;
				}
				return TipPosition.MouseBottomRightCorner;
			}
			if (BottomEdge && LeftEdge)
			{
				if (!flag)
				{
					return TipPosition.TopRightCorner;
				}
				return TipPosition.MouseTopRightCorner;
			}
			if (TopEdge)
			{
				if (!flag)
				{
					return TipPosition.BottomMiddle;
				}
				return TipPosition.MouseBottomMiddle;
			}
			if (BottomEdge)
			{
				if (!flag)
				{
					return TipPosition.TopMiddle;
				}
				return TipPosition.MouseTopMiddle;
			}
			if (LeftEdge)
			{
				if (!flag)
				{
					return TipPosition.RightMiddle;
				}
				return TipPosition.MouseRightMiddle;
			}
			break;
		case TipPosition.RightMiddle:
		case TipPosition.MouseRightMiddle:
			if (TopEdge && RightEdge)
			{
				if (!flag)
				{
					return TipPosition.BottomLeftCorner;
				}
				return TipPosition.MouseBottomLeftCorner;
			}
			if (BottomEdge && RightEdge)
			{
				if (!flag)
				{
					return TipPosition.TopLeftCorner;
				}
				return TipPosition.MouseTopLeftCorner;
			}
			if (TopEdge)
			{
				if (!flag)
				{
					return TipPosition.BottomMiddle;
				}
				return TipPosition.MouseBottomMiddle;
			}
			if (BottomEdge)
			{
				if (!flag)
				{
					return TipPosition.TopMiddle;
				}
				return TipPosition.MouseTopMiddle;
			}
			if (RightEdge)
			{
				if (!flag)
				{
					return TipPosition.LeftMiddle;
				}
				return TipPosition.MouseLeftMiddle;
			}
			break;
		}
		return fromPosition;
	}
}
