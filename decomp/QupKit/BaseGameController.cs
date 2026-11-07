using UnityEngine;

namespace QupKit;

public class BaseGameController<T> : MonoBehaviour where T : class
{
	public Canvas MainCanvas;

	public virtual void OnStart()
	{
		RegisterViews();
	}

	public virtual void RegisterViews()
	{
	}

	public virtual void AfterOnGUI()
	{
	}

	private void Start()
	{
		OnStart();
	}

	public virtual void OnUpdate()
	{
	}

	private void Update()
	{
		OnUpdate();
	}
}
