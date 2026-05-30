using System;
using UnityEngine;

namespace UntitledDeepSeaGame
{
    [Serializable]
    public class AnimationConfigSO
    {
        public AnimationClip SideMoveClip;
        public AnimationClip SideIdleClip;
    }
    
    public enum OxygenState
    {
        
        Depleted
    }

    public enum Direction
    {
        None,
        Up,
        Down,
        Left,
        Right
    }
    
    public enum ZoneType
    {
        Water,
        Air
    }

    public enum CharacterStateMachine
    {
        Player,
        BasicNpc
    }

    public enum MovementState
    {
        Idle,
        Moving,
        Knockback,
        Pursuing,
        Fleeing
    }

    public enum LifeState
    {
        Alive,
        IFrame,
        Dead
    }

    public enum ToolType
    {
        Drill,
    }
    
    public enum MiningState
    {
        Idle,
        Detecting
    }
    
    public enum PlacingState
    {
        Idle,
        Placing
    }

    public struct TileVisibility
    {
        public int Visibility; // 0 = transparent, 1 = opaque

        public TileVisibility(int visibility)
        {
            Visibility = visibility;
        }
    }
}