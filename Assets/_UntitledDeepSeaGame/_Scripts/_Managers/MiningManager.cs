using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UntitledDeepSeaGame
{
    public class MiningManager : MonoBehaviour
    {
        public static MiningManager Instance { get; private set; }

        [SerializeField]
        private float _miningRange = 3f;
        public float MiningRange => _miningRange;

        [SerializeField]
        private float _timeBetweenMiningSounds = 0.225f;

        private MiningState _miningState;
        public MiningState MiningState => _miningState;

        private Coroutine _currentMiningCoroutine;
        private ToolItemSO _currentTool;
        private Vector2Int _currentTargetTilePosition;
        private TileSO _currentTargetTile;

        public ToolItemSO CurrentTool => _currentTool;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            GameInput.Instance.OnPrimaryActionStarted += OnPrimaryActionStarted;
            InventoryManager.Instance.OnSelectedHotbarSlotChanged += OnSelectedHotbarSlotChanged;
        }

        private void OnDestroy()
        {
            if (GameInput.Instance != null)
            {
                GameInput.Instance.OnPrimaryActionStarted -= OnPrimaryActionStarted;
            }

            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnSelectedHotbarSlotChanged -= OnSelectedHotbarSlotChanged;
            }
        }

        private void Update()
        {
            if (_currentMiningCoroutine != null && !CanContinueMiningCurrentTarget())
            {
                StopMiningRoutine();
            }

            TryToMineTile();
        }

        private void OnSelectedHotbarSlotChanged(int arg1, InventoryStack stack)
        {
            _currentTool = !stack.IsEmpty && stack.Item is ToolItemSO toolItemSO ? toolItemSO : null;

            if (_currentTool == null)
            {
                _miningState = MiningState.Idle;
                StopMiningRoutine();
            }
        }

        private void OnPrimaryActionStarted(object sender, InputAction.CallbackContext e)
        {
            if (_currentTool == null)
            {
                _miningState = MiningState.Idle;
                StopMiningRoutine();
                return;
            }

            MiningState newState = (e.started || e.performed) ? MiningState.Detecting : MiningState.Idle;
            if (_miningState == newState)
            {
                return;
            }

            _miningState = newState;

            if (_miningState == MiningState.Idle)
            {
                StopMiningRoutine();
            }
        }

        private void TryToMineTile()
        {
            if (_miningState != MiningState.Detecting || _currentMiningCoroutine != null)
            {
                return;
            }

            if (!TryGetMineableTile(out Vector2Int tilePosition, out TileSO tileSO))
            {
                return;
            }

            _currentTargetTilePosition = tilePosition;
            _currentTargetTile = tileSO;
            _currentMiningCoroutine = StartCoroutine(MiningRoutine());
        }

        private IEnumerator MiningRoutine()
        {
            float totalTicks = _currentTargetTile.Hardness * 30f / Mathf.Max(_currentTool.MiningPower, 0.1f);
            float totalMiningTime = totalTicks * 0.05f;
            float elapsedTime = 0f;
            float nextSoundTime = _timeBetweenMiningSounds;

            PlayMiningSound();

            while (elapsedTime < totalMiningTime)
            {
                if (!CanContinueMiningCurrentTarget())
                {
                    StopMiningRoutine();
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

            HandleDestruction();
        }

        private void HandleDestruction()
        {
            if (WorldManager.Instance?.WorldDataStore == null || _currentTargetTile == null)
            {
                StopMiningRoutine();
                return;
            }

            if(_currentTargetTile.IsMultiTile)
            {
                WorldManager.Instance.WorldDataStore.DestroyMultiTile(_currentTargetTilePosition.x, _currentTargetTilePosition.y);
            }
            else
            {
                WorldManager.Instance.WorldDataStore.SetTileId(_currentTargetTilePosition.x, _currentTargetTilePosition.y, GameDataRegistry.INVALID_ID);
            }
            
            SpawnTileDrops(_currentTargetTile, _currentTargetTilePosition);
            StopMiningRoutine();
        }

        private void PlayMiningSound()
        {
        }

        private void StopMiningRoutine()
        {
            if (_currentMiningCoroutine != null)
            {
                StopCoroutine(_currentMiningCoroutine);
            }

            _currentMiningCoroutine = null;
            _currentTargetTile = null;
        }

        private bool PlayerWithinMiningRangeOfMouse()
        {
            return Vector2.Distance(Player.Instance.PlayerCenter, GameManager.MouseWorldPosition) <= _miningRange;
        }

        private bool TryGetMineableTile(out Vector2Int tilePosition, out TileSO tileSO)
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

            ushort tileId = worldDataStore.GetTileId(tilePosition.x, tilePosition.y);
            if (tileId == GameDataRegistry.INVALID_ID)
            {
                return false;
            }

            tileSO = GameDataRegistry.Instance.GetTileSOFromTileId(tileId);
            return tileSO != null && tileSO.RequiredToolType == _currentTool.HarvestType;
        }

        private bool CanContinueMiningCurrentTarget()
        {
            if (_currentTargetTile == null)
            {
                return false;
            }

            if (!GameInput.Instance.PrimaryActionHeldDown)
            {
                return false;
            }

            if (!TryGetMineableTile(out Vector2Int tilePosition, out TileSO tileSO))
            {
                return false;
            }

            return tilePosition == _currentTargetTilePosition && tileSO == _currentTargetTile;
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
