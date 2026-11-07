using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class NewControls : IInputActionCollection2, IInputActionCollection, IEnumerable<InputAction>, IEnumerable, IDisposable
{
	public struct UIActions(NewControls wrapper)
	{
		private NewControls m_Wrapper = wrapper;

		public InputAction Navigation => m_Wrapper.m_UI_Navigation;

		public InputAction test => m_Wrapper.m_UI_test;

		public bool enabled => Get().enabled;

		public InputActionMap Get()
		{
			return m_Wrapper.m_UI;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(UIActions set)
		{
			return set.Get();
		}

		public void AddCallbacks(IUIActions instance)
		{
			if (instance != null && !m_Wrapper.m_UIActionsCallbackInterfaces.Contains(instance))
			{
				m_Wrapper.m_UIActionsCallbackInterfaces.Add(instance);
				Navigation.started += instance.OnNavigation;
				Navigation.performed += instance.OnNavigation;
				Navigation.canceled += instance.OnNavigation;
				test.started += instance.OnTest;
				test.performed += instance.OnTest;
				test.canceled += instance.OnTest;
			}
		}

		private void UnregisterCallbacks(IUIActions instance)
		{
			Navigation.started -= instance.OnNavigation;
			Navigation.performed -= instance.OnNavigation;
			Navigation.canceled -= instance.OnNavigation;
			test.started -= instance.OnTest;
			test.performed -= instance.OnTest;
			test.canceled -= instance.OnTest;
		}

		public void RemoveCallbacks(IUIActions instance)
		{
			if (m_Wrapper.m_UIActionsCallbackInterfaces.Remove(instance))
			{
				UnregisterCallbacks(instance);
			}
		}

		public void SetCallbacks(IUIActions instance)
		{
			foreach (IUIActions uIActionsCallbackInterface in m_Wrapper.m_UIActionsCallbackInterfaces)
			{
				UnregisterCallbacks(uIActionsCallbackInterface);
			}
			m_Wrapper.m_UIActionsCallbackInterfaces.Clear();
			AddCallbacks(instance);
		}
	}

	public interface IUIActions
	{
		void OnNavigation(InputAction.CallbackContext context);

		void OnTest(InputAction.CallbackContext context);
	}

	private readonly InputActionMap m_UI;

	private List<IUIActions> m_UIActionsCallbackInterfaces = new List<IUIActions>();

	private readonly InputAction m_UI_Navigation;

	private readonly InputAction m_UI_test;

	public InputActionAsset asset { get; }

	public InputBinding? bindingMask
	{
		get
		{
			return asset.bindingMask;
		}
		set
		{
			asset.bindingMask = value;
		}
	}

	public ReadOnlyArray<InputDevice>? devices
	{
		get
		{
			return asset.devices;
		}
		set
		{
			asset.devices = value;
		}
	}

	public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

	public IEnumerable<InputBinding> bindings => asset.bindings;

	public UIActions UI => new UIActions(this);

	public NewControls()
	{
		asset = InputActionAsset.FromJson("{\r\n    \"version\": 1,\r\n    \"name\": \"New Controls\",\r\n    \"maps\": [\r\n        {\r\n            \"name\": \"UI\",\r\n            \"id\": \"918eb1a2-da56-438b-b107-11deb8d41e91\",\r\n            \"actions\": [\r\n                {\r\n                    \"name\": \"Navigation\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"3e21a19d-7259-40f6-b85e-a8d4c587b9d1\",\r\n                    \"expectedControlType\": \"Vector2\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"test\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"b302569b-286d-475f-a103-8a50f4845361\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                }\r\n            ],\r\n            \"bindings\": [\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"aff6d164-39e0-481d-bc15-071f4d7471aa\",\r\n                    \"path\": \"<Gamepad>/dpad\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Navigation\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"2D Vector\",\r\n                    \"id\": \"c3a69459-33b6-48ed-81d4-3ee3c2a5ebc8\",\r\n                    \"path\": \"2DVector\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Navigation\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"up\",\r\n                    \"id\": \"b5650407-fa16-4599-a3d0-4fda7628d183\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Navigation\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"down\",\r\n                    \"id\": \"8f57b9f3-6217-402e-b480-0d08e88cbd29\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Navigation\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"left\",\r\n                    \"id\": \"cb272385-bd67-486b-85c0-5b16d0e3d9a9\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Navigation\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"right\",\r\n                    \"id\": \"259bac90-0d48-4c03-a376-ccab66fb64ec\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Navigation\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"20fd9dd6-2ed2-4f67-beb0-9e91adf1c082\",\r\n                    \"path\": \"<Keyboard>/numpad8\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"test\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"Button With One Modifier\",\r\n                    \"id\": \"179b372e-2abb-42e5-93ec-cf1ac10f7512\",\r\n                    \"path\": \"ButtonWithOneModifier\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"test\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"Modifier\",\r\n                    \"id\": \"419607b9-565d-40a8-988e-e9533cf9c0a2\",\r\n                    \"path\": \"<Keyboard>/ctrl\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"test\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"Button\",\r\n                    \"id\": \"b9c10b7d-d672-4cad-b638-c1b9c041a0c4\",\r\n                    \"path\": \"<Keyboard>/w\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"test\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"One Modifier\",\r\n                    \"id\": \"11ec37e0-6dc4-47b0-897e-c31ded5ad80d\",\r\n                    \"path\": \"OneModifier\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"test\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"modifier\",\r\n                    \"id\": \"63d3c4f9-151e-4cdb-9134-ff345368ed59\",\r\n                    \"path\": \"<Keyboard>/shift\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"test\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"binding\",\r\n                    \"id\": \"68c90fc4-218d-48b6-b3a0-de17b0121450\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"test\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"ec371edd-c5be-4ed8-9965-e5f97214408b\",\r\n                    \"path\": \"<Keyboard>/alt\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"test\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                }\r\n            ]\r\n        }\r\n    ],\r\n    \"controlSchemes\": []\r\n}");
		m_UI = asset.FindActionMap("UI", throwIfNotFound: true);
		m_UI_Navigation = m_UI.FindAction("Navigation", throwIfNotFound: true);
		m_UI_test = m_UI.FindAction("test", throwIfNotFound: true);
	}

	~NewControls()
	{
	}

	public void Dispose()
	{
		UnityEngine.Object.Destroy(asset);
	}

	public bool Contains(InputAction action)
	{
		return asset.Contains(action);
	}

	public IEnumerator<InputAction> GetEnumerator()
	{
		return asset.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Enable()
	{
		asset.Enable();
	}

	public void Disable()
	{
		asset.Disable();
	}

	public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
	{
		return asset.FindAction(actionNameOrId, throwIfNotFound);
	}

	public int FindBinding(InputBinding bindingMask, out InputAction action)
	{
		return asset.FindBinding(bindingMask, out action);
	}
}
