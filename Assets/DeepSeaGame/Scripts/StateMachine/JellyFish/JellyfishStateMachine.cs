using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

namespace DeepSeaGame
{
    public class JellyfishStateMachine : StateMachine
    {
        public JellyfishCharMovement JellyfishMovement { get; private set; }
        public ServerCharacter NearestPlayer { get; private set; }

        public JellyfishStateMachine(ServerCharacter character)
        {
            // This constructor gets played on all client machines
            _serverCharacter = character;
            JellyfishMovement = character.Movement as JellyfishCharMovement;

            // Sub States
            _states[AIState.Moving] = new JellyfishMovingState(AIState.Moving, this);
            _states[AIState.Pursuing] = new JellyfishPursuingState(AIState.Pursuing, this);
            _states[AIState.Knockbacked] = new JellyfishKnockbackState(AIState.Knockbacked, this);

            // Super States
            _states[AIState.Locomotion] = new JellyfishLocomotionState(AIState.Locomotion, this);
            _states[AIState.Dead] = new JellyfishDeadState(AIState.Dead, this);

            _currentState = _states[AIState.Locomotion];
        }

        public override void UpdateAI()
        {
            base.UpdateAI();
            FindNearestPlayer();
        }

        private void FindNearestPlayer()
        {
            if (JellyfishMovement == null || NetworkManager.Singleton == null) return;

            float minDistance = JellyfishMovement.SeekRadius;
            NearestPlayer = null;

            Vector2 myPos = _serverCharacter.transform.position;

            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                if (client.PlayerObject != null)
                {
                    if (client.PlayerObject.TryGetComponent<ServerCharacter>(out var playerChar))
                    {
                        if (playerChar.LifeState == LifeState.Dead) continue;

                        float dist = Vector2.Distance(myPos, playerChar.transform.position);
                        if (dist < minDistance)
                        {
                            minDistance = dist;
                            NearestPlayer = playerChar;
                        }
                    }
                }
            }
        }

        public bool HasLineOfSight(Vector2 start, Vector2 end)
        {
            WorldDataStore worldData = WorldManager.Instance.WorldDataStore;
            if (worldData == null) return false;

            int x0 = Mathf.FloorToInt(start.x);
            int y0 = Mathf.FloorToInt(start.y);
            int x1 = Mathf.FloorToInt(end.x);
            int y1 = Mathf.FloorToInt(end.y);

            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                if (IsTileSolid(x0, y0, worldData))
                {
                    return false;
                }

                if (x0 == x1 && y0 == y1) break;
                
                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
            return true;
        }

        private bool IsTileSolid(int x, int y, WorldDataStore worldData)
        {
            if (!worldData.IsInBounds(x, y)) return true;
            ushort tileId = worldData.GetTileId(x, y, WorldTm.ForegroundTilemap);
            if (tileId == GameDataRegistry.INVALID_ID) return false;

            TileSO tileSO = GameDataRegistry.Instance.GetTileSOFromTileId(tileId);
            return tileSO == null || tileSO.IsSolid;
        }

        public override void ReceiveHP(ServerCharacter inflicter, int amount)
        {
            if (inflicter != null)
            {
                if (amount < 0)
                {
                    
                }
                else
                {
                    // Healed
                }
            }
        }
    }
}
