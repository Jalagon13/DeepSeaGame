using System;
using UnityEngine;

namespace DeepSeaGame
{
    public class WorldGenerationContext
    {
        private readonly Action<WorldGenerationState, float> _progressChanged;
        private int _currentStepIndex;
        private int _totalSteps;
        private float _stepProgress;

        public WorldGenerationContext(WorldGenerationData config, WorldDataStore dataStore, int seedHash, string resolvedSeed, Action<WorldGenerationState, float> progressChanged)
        {
            Config = config;
            DataStore = dataStore;
            SeedHash = seedHash;
            ResolvedSeed = resolvedSeed;
            Random = new System.Random(seedHash);
            SurfaceHeights = new int[config.WorldWidth];
            SpawnTile = new Vector3Int(config.WorldWidth / 2, (config.WorldHeight / 2) + 1, 0);
            _progressChanged = progressChanged;
        }

        public WorldGenerationData Config { get; }
        public WorldDataStore DataStore { get; }
        public int SeedHash { get; }
        public string ResolvedSeed { get; }
        public System.Random Random { get; }
        public int[] SurfaceHeights { get; }
        public Vector3Int SpawnTile { get; set; }
        public WorldGenerationState State { get; private set; }
        public float OverallProgress { get; private set; }

        public void Begin(int totalSteps)
        {
            _totalSteps = Mathf.Max(1, totalSteps);
            _currentStepIndex = 0;
            _stepProgress = 0f;
            SetState(WorldGenerationState.Initializing);
        }

        public void BeginStep(WorldGenerationState state, int stepIndex)
        {
            _currentStepIndex = stepIndex;
            _stepProgress = 0f;
            SetState(state);
        }

        public void SetStepProgress(float progress)
        {
            _stepProgress = Mathf.Clamp01(progress);
            OverallProgress = Mathf.Clamp01((_currentStepIndex + _stepProgress) / _totalSteps);
            _progressChanged?.Invoke(State, OverallProgress);
        }

        public void Complete()
        {
            SetState(WorldGenerationState.Completed);
            OverallProgress = 1f;
            _progressChanged?.Invoke(State, OverallProgress);
        }

        private void SetState(WorldGenerationState state)
        {
            State = state;
            OverallProgress = Mathf.Clamp01((_currentStepIndex + _stepProgress) / Mathf.Max(1, _totalSteps));
            _progressChanged?.Invoke(State, OverallProgress);
        }
    }
}
