using UnityEngine;

namespace DeepSeaGame
{

    // this script should be used for instantiating prefabs and configuring them.
    [RequireComponent(typeof(TooltipReferenceHolder))]
    public class TooltipsInstantiateHandler : MonoBehaviour
    {
        private TooltipReferenceHolder _referenceHolder;

        private void Awake()
        {
            _referenceHolder = GetComponent<TooltipReferenceHolder>();
            Tooltip.InstantiateHandler = this;
        }

        public JustTextHandler InstantiateJustText(Transform customLayout = null)
        {
            var gameObject = Instantiate(_referenceHolder.JustTextPrefab, customLayout == null ? _referenceHolder.Layout.transform : customLayout);
            _referenceHolder.oldPrefabs.Add(gameObject);

            return gameObject.GetComponent<JustTextHandler>();
        }
    }
}
