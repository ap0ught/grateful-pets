using System.Collections.Generic;
using UnityEngine;

public class exUIEffect : MonoBehaviour
{
	public List<EffectInfo_Scale> scaleInfos;

	public List<EffectInfo_Offset> offsetInfos;

	public List<EffectInfo_Color> colorInfos;

	private List<EffectState_Base> states = new List<EffectState_Base>();

	private void Awake()
	{
		exUIControl component = GetComponent<exUIControl>();
		if (!component)
		{
			return;
		}
		if (scaleInfos != null)
		{
			for (int i = 0; i < scaleInfos.Count; i++)
			{
				EffectInfo_Scale effectInfo_Scale = scaleInfos[i];
				EffectState_Scale effectState_Scale = new EffectState_Scale();
				effectState_Scale.info = effectInfo_Scale;
				effectState_Scale.func = effectInfo_Scale.GetCurveFunction();
				AddState_Scale(component, effectState_Scale);
			}
		}
		if (offsetInfos != null)
		{
			for (int j = 0; j < offsetInfos.Count; j++)
			{
				EffectInfo_Offset effectInfo_Offset = offsetInfos[j];
				EffectState_Offset effectState_Offset = new EffectState_Offset();
				effectState_Offset.info = effectInfo_Offset;
				effectState_Offset.func = effectInfo_Offset.GetCurveFunction();
				AddState_Offset(component, effectState_Offset);
			}
		}
		if (colorInfos != null)
		{
			for (int k = 0; k < colorInfos.Count; k++)
			{
				EffectInfo_Color effectInfo_Color = colorInfos[k];
				EffectState_Color effectState_Color = new EffectState_Color();
				effectState_Color.info = effectInfo_Color;
				effectState_Color.func = effectInfo_Color.GetCurveFunction();
				AddState_Color(component, effectState_Color);
			}
		}
	}

	private void Update()
	{
		bool flag = true;
		for (int i = 0; i < states.Count; i++)
		{
			if (!states[i].Tick(Time.deltaTime))
			{
				flag = false;
			}
		}
		if (flag)
		{
			enabled = false;
		}
	}

	public void AddEffect_Scale(Transform _target, EffectEventType _type, exEase.Type _curveType, Vector3 _to, float _duration)
	{
		exUIControl component = GetComponent<exUIControl>();
		if ((bool)component)
		{
			EffectInfo_Scale effectInfo_Scale = new EffectInfo_Scale();
			effectInfo_Scale.duration = _duration;
			effectInfo_Scale.target = _target;
			effectInfo_Scale.normal = _target.localScale;
			effectInfo_Scale.curveType = _curveType;
			EffectInfo_Scale.PropInfo propInfo = new EffectInfo_Scale.PropInfo();
			propInfo.type = _type;
			propInfo.val = _to;
			effectInfo_Scale.propInfos.Add(propInfo);
			EffectState_Scale effectState_Scale = new EffectState_Scale();
			effectState_Scale.info = effectInfo_Scale;
			effectState_Scale.func = effectInfo_Scale.GetCurveFunction();
			AddState_Scale(component, effectState_Scale);
		}
	}

	public void AddEffect_Color(exSpriteBase _target, EffectEventType _type, exEase.Type _curveType, Color _to, float _duration)
	{
		exUIControl component = GetComponent<exUIControl>();
		if ((bool)component)
		{
			EffectInfo_Color effectInfo_Color = new EffectInfo_Color();
			effectInfo_Color.duration = _duration;
			effectInfo_Color.target = _target;
			effectInfo_Color.normal = _target.color;
			effectInfo_Color.curveType = _curveType;
			EffectInfo_Color.PropInfo propInfo = new EffectInfo_Color.PropInfo();
			propInfo.type = _type;
			propInfo.val = _to;
			effectInfo_Color.propInfos.Add(propInfo);
			EffectState_Color effectState_Color = new EffectState_Color();
			effectState_Color.info = effectInfo_Color;
			effectState_Color.func = effectInfo_Color.GetCurveFunction();
			AddState_Color(component, effectState_Color);
		}
	}

	public void AddEffect_Offset(exSpriteBase _target, EffectEventType _type, exEase.Type _curveType, Vector2 _to, float _duration)
	{
		exUIControl component = GetComponent<exUIControl>();
		if ((bool)component)
		{
			EffectInfo_Offset effectInfo_Offset = new EffectInfo_Offset();
			effectInfo_Offset.duration = _duration;
			effectInfo_Offset.target = _target;
			effectInfo_Offset.normal = _target.offset;
			effectInfo_Offset.curveType = _curveType;
			EffectInfo_Offset.PropInfo propInfo = new EffectInfo_Offset.PropInfo();
			propInfo.type = _type;
			propInfo.val = _to;
			effectInfo_Offset.propInfos.Add(propInfo);
			EffectState_Offset effectState_Offset = new EffectState_Offset();
			effectState_Offset.info = effectInfo_Offset;
			effectState_Offset.func = effectInfo_Offset.GetCurveFunction();
			AddState_Offset(component, effectState_Offset);
		}
	}

	private void AddState_Scale(exUIControl _ctrl, EffectState_Scale _state)
	{
		for (int i = 0; i < _state.info.propInfos.Count; i++)
		{
			EffectInfo_Scale.PropInfo propInfo = _state.info.propInfos[i];
			switch (propInfo.type)
			{
			case EffectEventType.Deactive:
				_ctrl.AddEventListener("onDeactive", (exUIEvent _event) =>
				{
					enabled = true;
					_state.Begin(propInfo.val);
				});
				_ctrl.AddEventListener("onActive", (exUIEvent _event) =>
				{
					enabled = true;
					_state.Begin(_state.info.normal);
				});
				break;
			case EffectEventType.Press:
				_ctrl.AddEventListener("onPressDown", (exUIEvent _event) =>
				{
					enabled = true;
					_state.Begin(propInfo.val);
				});
				_ctrl.AddEventListener("onPressUp", (exUIEvent _event) =>
				{
					enabled = true;
					_state.Begin(_state.info.GetValue(EffectEventType.Hover));
				});
				_ctrl.AddEventListener("onHoverOut", (exUIEvent _event) =>
				{
					if (!_ctrl.grabMouseOrTouch)
					{
						enabled = true;
						_state.Begin(_state.info.normal);
					}
				});
				break;
			case EffectEventType.Hover:
				_ctrl.AddEventListener("onHoverIn", (exUIEvent _event) =>
				{
					enabled = true;
					_state.Begin(propInfo.val);
				});
				_ctrl.AddEventListener("onHoverOut", (exUIEvent _event) =>
				{
					enabled = true;
					_state.Begin(_state.info.normal);
				});
				break;
			case EffectEventType.Unchecked:
				if (_ctrl as exUIToggle != null)
				{
					_ctrl.AddEventListener("onUnchecked", (exUIEvent _event) =>
					{
						enabled = true;
						_state.Begin(propInfo.val);
					});
					_ctrl.AddEventListener("onChecked", (exUIEvent _event) =>
					{
						enabled = true;
						_state.Begin(_state.info.GetValue(EffectEventType.Hover));
					});
				}
				break;
			}
		}
		states.Add(_state);
	}

	private void AddState_Offset(exUIControl _ctrl, EffectState_Offset _state)
	{
		for (int i = 0; i < _state.info.propInfos.Count; i++)
		{
			EffectInfo_Offset.PropInfo propInfo = _state.info.propInfos[i];
			switch (propInfo.type)
			{
			case EffectEventType.Deactive:
				_ctrl.AddEventListener("onDeactive", (exUIEvent _event) =>
				{
					enabled = true;
					_state.Begin(propInfo.val);
				});
				_ctrl.AddEventListener("onActive", (exUIEvent _event) =>
				{
					enabled = true;
					_state.Begin(_state.info.normal);
				});
				break;
			case EffectEventType.Press:
				_ctrl.AddEventListener("onPressDown", (exUIEvent _event) =>
				{
					enabled = true;
					_state.Begin(propInfo.val);
				});
				_ctrl.AddEventListener("onPressUp", (exUIEvent _event) =>
				{
					enabled = true;
					_state.Begin(_state.info.GetValue(EffectEventType.Hover));
				});
				break;
			case EffectEventType.Hover:
				_ctrl.AddEventListener("onHoverIn", (exUIEvent _event) =>
				{
					enabled = true;
					_state.Begin(propInfo.val);
				});
				_ctrl.AddEventListener("onHoverOut", (exUIEvent _event) =>
				{
					enabled = true;
					_state.Begin(_state.info.normal);
				});
				break;
			case EffectEventType.Unchecked:
				if (_ctrl as exUIToggle != null)
				{
					_ctrl.AddEventListener("onUnchecked", (exUIEvent _event) =>
					{
						enabled = true;
						_state.Begin(propInfo.val);
					});
					_ctrl.AddEventListener("onChecked", (exUIEvent _event) =>
					{
						enabled = true;
						_state.Begin(_state.info.GetValue(EffectEventType.Hover));
					});
				}
				break;
			}
		}
		states.Add(_state);
	}

	private void AddState_Color(exUIControl _ctrl, EffectState_Color _state)
	{
		for (int i = 0; i < _state.info.propInfos.Count; i++)
		{
			EffectInfo_Color.PropInfo propInfo = _state.info.propInfos[i];
			switch (propInfo.type)
			{
			case EffectEventType.Deactive:
				_ctrl.AddEventListener("onDeactive", (exUIEvent _event) =>
				{
					enabled = true;
					_state.Begin(propInfo.val);
				});
				_ctrl.AddEventListener("onActive", (exUIEvent _event) =>
				{
					enabled = true;
					_state.Begin(_state.info.normal);
				});
				break;
			case EffectEventType.Press:
				_ctrl.AddEventListener("onPressDown", (exUIEvent _event) =>
				{
					enabled = true;
					_state.Begin(propInfo.val);
				});
				_ctrl.AddEventListener("onPressUp", (exUIEvent _event) =>
				{
					enabled = true;
					_state.Begin(_state.info.GetValue(EffectEventType.Hover));
				});
				break;
			case EffectEventType.Hover:
				_ctrl.AddEventListener("onHoverIn", (exUIEvent _event) =>
				{
					enabled = true;
					_state.Begin(propInfo.val);
				});
				_ctrl.AddEventListener("onHoverOut", (exUIEvent _event) =>
				{
					enabled = true;
					_state.Begin(_state.info.normal);
				});
				break;
			case EffectEventType.Unchecked:
				if (_ctrl as exUIToggle != null)
				{
					_ctrl.AddEventListener("onUnchecked", (exUIEvent _event) =>
					{
						enabled = true;
						_state.Begin(propInfo.val);
					});
					_ctrl.AddEventListener("onChecked", (exUIEvent _event) =>
					{
						enabled = true;
						_state.Begin(_state.info.GetValue(EffectEventType.Hover));
					});
				}
				break;
			}
		}
		states.Add(_state);
	}
}
