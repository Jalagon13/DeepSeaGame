using Unity.Mathematics;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    public class Stat
    {
        private readonly int _baseAmount;

        private bool _dirty = true;
        private int _finalValue;

        public Stat(int baseAmount)
        {
            _baseAmount = baseAmount;
        }
        
        public void AddModifier()
        {
            // WIP
            _dirty = true;
        }
        
        public void RemoveModifier()
        {
            // WIP
            _dirty = true;
        }

        public int GetValue()
        {
            if (_dirty) Recalculate();
            return _finalValue;
        }

        // WIP
        private void Recalculate()
        {
            int flat = 0;
            float percent = 1f;

            // foreach (var mod in _modifiers)
            // {
            //     if (mod.Type == StatModifierType.Flat)
            //         flat += mod.Value;
            //     else if (mod.Type == StatModifierType.Percent)
            //         percent += mod.Value;
            // }

            _finalValue = Mathf.RoundToInt((_baseAmount + flat) * percent);
            _dirty = false;
        }
    }
}