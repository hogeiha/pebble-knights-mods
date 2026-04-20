using UnityEngine;

namespace UniversalKingActions
{
    internal static class UpgradeCampHider
    {
        public static void Hide(object instance)
        {
            var component = instance as Component;
            if (component != null && component.gameObject.activeSelf)
                component.gameObject.SetActive(false);
        }
    }
}
