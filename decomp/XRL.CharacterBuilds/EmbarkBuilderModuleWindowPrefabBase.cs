using UnityEngine;
using XRL.UI.Framework;

namespace XRL.CharacterBuilds;

public class EmbarkBuilderModuleWindowPrefabBase<T, PrefabComponent> : EmbarkBuilderModuleWindowBase<T> where T : AbstractEmbarkBuilderModule where PrefabComponent : MonoBehaviour, IFrameworkContext
{
	private PrefabComponent _prefabComponent;

	public PrefabComponent prefabComponent => _prefabComponent ?? (_prefabComponent = GetComponentInChildren<PrefabComponent>(includeInactive: true));

	public override NavigationContext GetNavigationContext()
	{
		return prefabComponent.GetNavigationContext();
	}

	public override void EnsureContentsActive()
	{
		prefabComponent.gameObject.SetActive(value: true);
	}
}
