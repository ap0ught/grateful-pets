using System;
using UnityEngine;

namespace Battlehub.UIControls;

public class ItemDataBindingArgs : EventArgs
{
	private bool m_canEdit = true;

	private bool m_canDrag = true;

	private bool m_canDrop = true;

	public object Item { get; set; }

	public GameObject ItemPresenter { get; set; }

	public GameObject EditorPresenter { get; set; }

	public bool CanEdit
	{
		get
		{
			return m_canEdit;
		}
		set
		{
			m_canEdit = value;
		}
	}

	public bool CanDrag
	{
		get
		{
			return m_canDrag;
		}
		set
		{
			m_canDrag = value;
		}
	}

	public bool CanDrop
	{
		get
		{
			return m_canDrop;
		}
		set
		{
			m_canDrop = value;
		}
	}
}
