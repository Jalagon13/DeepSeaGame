using System.Collections.Generic;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    [CreateAssetMenu(fileName = "Shelter Core Behavior", menuName = "MultiTile/Lifecycle/ShelterCore")]
    public class ShelterCoreBehavior : MultiTileBehavior
    {
        [SerializeField] private int _maxTileDetection = 40;
        [SerializeField] private float _drainInterval = 5f;
        [SerializeField] private int _minYHeightToWork = 250;

        public override void Update(MultiTileInstance instance, WorldDataStore dataStore, float deltaTime)
        {
            
        }
    }
}
