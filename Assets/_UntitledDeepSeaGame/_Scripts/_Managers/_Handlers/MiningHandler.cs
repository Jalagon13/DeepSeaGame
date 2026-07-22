using System.Collections;
using UnityEngine;

namespace DeepSeaGame
{
    public class MiningHandler : MonoBehaviour, IItemUseHandler
    {
        private enum MiningActionType
        {
            Primary,
            Secondary
        }

        [SerializeField]
        private float _miningRange = 3f;
        public float MiningRange => _miningRange;

        [SerializeField]
        private float _timeBetweenMiningSounds = 0.225f;

        private MiningState _miningState;
        public MiningState MiningState => _miningState;

        private Coroutine _primaryMiningCoroutine;
        private Coroutine _secondaryMiningCoroutine;
        private ToolItemSO _currentTool;
        private Vector2Int _primaryTargetTilePosition;
        private TileSO _primaryTargetTile;
        private Vector2Int _secondaryTargetTilePosition;
        private TileSO _secondaryTargetTile;

        public ToolItemSO CurrentTool => _currentTool;

        private void OnDestroy()
        {
            StopMiningRoutine(MiningActionType.Primary);
            StopMiningRoutine(MiningActionType.Secondary);
        }

        public bool CanHandle(ItemSO item)
        {
            return item is ToolItemSO toolItemSO && toolItemSO.HarvestType == ToolType.Drill;
        }

        public void OnSelectedStackChanged(InventoryStack stack)
        {
            _currentTool = !stack.IsEmpty && stack.Item is ToolItemSO toolItemSO && toolItemSO.HarvestType == ToolType.Drill
                ? toolItemSO
                : null;

            UpdateMiningActivity();
        }

        public void OnPrimaryStarted()
        {
            UpdateMiningActivity();
        }

        public void OnSecondaryStarted()
        {
            UpdateMiningActivity();
        }

        public void Tick()
        {
            UpdateMiningActivity();
        }

        private void UpdateMiningActivity()
        {
            bool primaryHeld = GameInput.Instance != null && GameInput.Instance.PrimaryActionHeldDown;
            bool secondaryHeld = GameInput.Instance != null && GameInput.Instance.SecondaryActionHeldDown;

            if (_currentTool == null || (!primaryHeld && !secondaryHeld))
            {
                _miningState = MiningState.Idle;
                StopMiningRoutine(MiningActionType.Primary);
                StopMiningRoutine(MiningActionType.Secondary);
                return;
            }

            _miningState = MiningState.Detecting;

            if (primaryHeld)
            {
                if (_primaryMiningCoroutine == null)
                {
                    TryStartMiningRoutine(MiningActionType.Primary);
                }
            }
            else
            {
                StopMiningRoutine(MiningActionType.Primary);
            }

            if (secondaryHeld)
            {
                if (_secondaryMiningCoroutine == null)
                {
                    TryStartMiningRoutine(MiningActionType.Secondary);
                }
            }
            else
            {
                StopMiningRoutine(MiningActionType.Secondary);
            }
        }

        private void TryStartMiningRoutine(MiningActionType actionType)
        {
            if (_currentTool == null || WorldManager.Instance?.WorldDataStore == null)
            {
                return;
            }

            if (!TryGetMineableTile(out Vector2Int tilePosition, out TileSO tileSO, actionType))
            {
                return;
            }

            switch (actionType)
            {
                case MiningActionType.Primary:
                    _primaryTargetTilePosition = tilePosition;
                    _primaryTargetTile = tileSO;
                    _primaryMiningCoroutine = StartCoroutine(MiningRoutine(actionType));
                    break;
                case MiningActionType.Secondary:
                    _secondaryTargetTilePosition = tilePosition;
                    _secondaryTargetTile = tileSO;
                    _secondaryMiningCoroutine = StartCoroutine(MiningRoutine(actionType));
                    break;
            }
        }

        private IEnumerator MiningRoutine(MiningActionType actionType)
        {
            TileSO targetTile = GetMiningTargetTile(actionType);
            Vector2Int targetTilePosition = GetMiningTargetTilePosition(actionType);

            if (targetTile == null)
            {
                StopMiningRoutine(actionType);
                yield break;
            }

            float totalTicks = targetTile.Hardness * 30f / Mathf.Max(_currentTool.MiningPower, 0.1f);
            float totalMiningTime = totalTicks * 0.05f;
            float elapsedTime = 0f;
            float nextSoundTime = _timeBetweenMiningSounds;

            PlayMiningSound();

            while (elapsedTime < totalMiningTime)
            {
                if (!CanContinueMiningCurrentTarget(actionType))
                {
                    StopMiningRoutine(actionType);
                    yield break;
                }

                elapsedTime += Time.deltaTime;

                if (elapsedTime >= nextSoundTime)
                {
                    PlayMiningSound();
                    nextSoundTime += _timeBetweenMiningSounds;
                }

                yield return null;
            }

            HandleDestruction(actionType);
        }

        private void HandleDestruction(MiningActionType actionType)
        {
            TileSO targetTile = GetMiningTargetTile(actionType);
            Vector2Int targetTilePosition = GetMiningTargetTilePosition(actionType);

            if (WorldManager.Instance?.WorldDataStore == null || targetTile == null)
            {
                StopMiningRoutine(actionType);
                return;
            }

            if (actionType == MiningActionType.Primary)
            {
                if (targetTile.IsMultiTile)
                {
                    WorldManager.Instance.WorldDataStore.DestroyMultiTile(targetTilePosition.x, targetTilePosition.y);
                }
                else
                {
                    WorldManager.Instance.WorldDataStore.SetForegroundTileId(targetTilePosition.x, targetTilePosition.y, GameDataRegistry.INVALID_ID);
                }
            }
            else
            {
                WorldManager.Instance.WorldDataStore.SetBackgroundTileId(targetTilePosition.x, targetTilePosition.y, GameDataRegistry.INVALID_ID);
            }

            SpawnTileDrops(targetTile, targetTilePosition);
            StopMiningRoutine(actionType);
        }

        private void PlayMiningSound()
        {
        }

        private void StopMiningRoutine(MiningActionType actionType)
        {
            Coroutine miningCoroutine = actionType == MiningActionType.Primary ? _primaryMiningCoroutine : _secondaryMiningCoroutine;
            if (miningCoroutine != null)
            {
                StopCoroutine(miningCoroutine);
            }

            if (actionType == MiningActionType.Primary)
            {
                _primaryMiningCoroutine = null;
                _primaryTargetTile = null;
            }
            else
            {
                _secondaryMiningCoroutine = null;
                _secondaryTargetTile = null;
            }
        }

        private bool PlayerWithinMiningRangeOfMouse()
        {
            return Vector2.Distance(Player.Instance.PlayerCenter, GameManager.MouseWorldPosition) <= _miningRange;
        }

        private bool TryGetMineableTile(out Vector2Int tilePosition, out TileSO tileSO, MiningActionType actionType)
        {
            tilePosition = GameManager.MouseTilePosition;
            tileSO = null;

            if (_currentTool == null || WorldManager.Instance?.WorldDataStore == null || !PlayerWithinMiningRangeOfMouse())
            {
                return false;
            }

            WorldDataStore worldDataStore = WorldManager.Instance.WorldDataStore;
            if (!worldDataStore.IsInBounds(tilePosition.x, tilePosition.y))
            {
                return false;
            }

            WorldTm targetMap = actionType == MiningActionType.Primary ? WorldTm.ForegroundTilemap : WorldTm.BackgroundTilemap;
            ushort tileId = worldDataStore.GetTileId(tilePosition.x, tilePosition.y, targetMap);
            if (tileId == GameDataRegistry.INVALID_ID)
            {
                return false;
            }

            tileSO = GameDataRegistry.Instance.GetTileSOFromTileId(tileId);
            return tileSO != null && tileSO.RequiredToolType == _currentTool.HarvestType;
        }

        private bool CanContinueMiningCurrentTarget(MiningActionType actionType)
        {
            TileSO targetTile = GetMiningTargetTile(actionType);
            if (targetTile == null)
            {
                return false;
            }

            bool actionHeld = actionType == MiningActionType.Primary
                ? GameInput.Instance != null && GameInput.Instance.PrimaryActionHeldDown
                : GameInput.Instance != null && GameInput.Instance.SecondaryActionHeldDown;
                
            if (!actionHeld)
            {
                return false;
            }

            if (!TryGetMineableTile(out Vector2Int tilePosition, out TileSO tileSO, actionType))
            {
                return false;
            }

            return tilePosition == GetMiningTargetTilePosition(actionType) && tileSO == targetTile;
        }

        private TileSO GetMiningTargetTile(MiningActionType actionType)
        {
            return actionType == MiningActionType.Primary ? _primaryTargetTile : _secondaryTargetTile;
        }

        private Vector2Int GetMiningTargetTilePosition(MiningActionType actionType)
        {
            return actionType == MiningActionType.Primary ? _primaryTargetTilePosition : _secondaryTargetTilePosition;
        }

        private void SpawnTileDrops(TileSO tileSO, Vector2Int tilePosition)
        {
            Vector2 spawnPosition = tilePosition + new Vector2(0.5f, 0.5f);

            if (tileSO.ItemDropTable != null && tileSO.ItemDropTable.Count > 0)
            {
                LootTable.SpawnLoot(tileSO.ItemDropTable, spawnPosition);
                return;
            }

            GameManager.Instance.SpawnItem(new InventoryStack(tileSO.TileItemSO, 1), spawnPosition);
        }

    }
}
