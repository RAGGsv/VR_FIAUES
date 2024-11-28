using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Viroo.Arena;
using Viroo.Context;
using Viroo.Interactions;
using Viroo.Networking;
using Virtualware.Networking.Client.SceneManagement;

namespace VirooLab
{
    public class Lift : MonoBehaviour
    {
        [SerializeField]
        private Collider buttonUp = default;

        [SerializeField]
        private Collider buttonDown = default;

        [SerializeField]
        private Material lightRed = default;

        [SerializeField]
        private Material lightGreen = default;

        [SerializeField]
        private Material buttonRed = default;

        [SerializeField]
        private Material buttonGreen = default;

        [SerializeField]
        private MeshRenderer buttonUpDisplayRenderer = default;

        [SerializeField]
        private MeshRenderer buttonDownDisplayRenderer = default;

        [SerializeField]
        private MeshRenderer lightRenderer = default;

        [SerializeField]
        private ObjectAction playBeepAction = default;

        private readonly Dictionary<string, List<string>> arenaPlayersInLift = new(StringComparer.Ordinal);

        private bool allPlayersIn;

        private bool liftIsDown = true;

        private IContextProvider contextProvider;

        private bool liftIsMoving;

        protected void Inject(IContextProvider contextProvider, INetworkScenesService networkScenesService)
        {
            this.contextProvider = contextProvider;

            networkScenesService.OnLocalClientSceneUnloadStarted += (sender, e) =>
            {
                // Reset all arenas to their original parent
                foreach (string arenaId in arenaPlayersInLift.Keys)
                {
                    Transform arena = GetArena(arenaId);

                    if (arena)
                    {
                        arena.SetParent(contextProvider.ArenaRoot.transform);
                    }
                }
            };
        }

        protected void Awake()
        {
            this.QueueForInject();
        }

        protected void OnTriggerEnter(Collider other)
        {
            if (liftIsMoving)
            {
                return;
            }

            if (other.TryGetComponent(out IPlayer player))
            {
                Transform arena = GetArena(player.ArenaId);

                if (arena != null)
                {
                    arena.SetParent(transform);

                    if (!arenaPlayersInLift.ContainsKey(player.ArenaId))
                    {
                        arenaPlayersInLift.Add(player.ArenaId, new List<string>());

                        Debug.Log($"Add Player To Lift: {player.ClientId}");
                    }

                    if (!arenaPlayersInLift[player.ArenaId].Contains(player.ClientId, StringComparer.Ordinal))
                    {
                        arenaPlayersInLift[player.ArenaId].Add(player.ClientId);
                    }

                    CheckAllPlayersIn();
                }
            }
        }

        protected void OnTriggerExit(Collider other)
        {
            StartCoroutine(CheckExit(other));
        }

        public void SetLiftMovementState(bool isMoving)
        {
            liftIsMoving = isMoving;
        }

        public void SetLiftState(bool isDown)
        {
            liftIsDown = isDown;
        }

        private void SetState()
        {
            if (liftIsDown)
            {
                SetButtonUp(allPlayersIn);
            }
            else
            {
                SetButtonDown(allPlayersIn);
            }

            lightRenderer.material = allPlayersIn ? lightGreen : lightRed;

            if (allPlayersIn)
            {
                playBeepAction.LocalExecute(string.Empty);
            }
        }

        public void SetButtonUp(bool enable)
        {
            buttonUp.enabled = enable;
            buttonUpDisplayRenderer.material = enable ? buttonGreen : buttonRed;
        }

        public void SetButtonDown(bool enable)
        {
            buttonDown.enabled = enable;
            buttonDownDisplayRenderer.material = enable ? buttonGreen : buttonRed;
        }

        private void CheckAllPlayersIn()
        {
            if (!arenaPlayersInLift.Any())
            {
                allPlayersIn = false;
            }
            else
            {
                Dictionary<string, Transform> arenaTransforms = arenaPlayersInLift.Keys
                    .ToDictionary(p => p, p => GetArena(p), StringComparer.Ordinal);
                Dictionary<string, int> sessionArenaPlayersCount = arenaTransforms
                    .ToDictionary(at => at.Key, at => at.Value.GetComponentsInChildren<IPlayer>().Length, StringComparer.Ordinal);

                allPlayersIn = arenaPlayersInLift.All(p => p.Value.Count == sessionArenaPlayersCount[p.Key]);
            }

            SetState();
        }

        private IEnumerator CheckExit(Collider other)
        {
            if (other.TryGetComponent(out IPlayer player))
            {
                while (liftIsMoving)
                {
                    yield return null;
                }

                Transform arena = GetArena(player.ArenaId);

                if (arena != null)
                {
                    arena.SetParent(contextProvider.ArenaRoot.transform);

                    if (arenaPlayersInLift.TryGetValue(player.ArenaId, out List<string> value))
                    {
                        value.Remove(player.ClientId);

                        if (!arenaPlayersInLift[player.ArenaId].Any())
                        {
                            arenaPlayersInLift.Remove(player.ArenaId);
                        }

                        Debug.Log($"Remove Player From Lift: {player.ClientId}");
                    }
                }

                CheckAllPlayersIn();
            }
        }

        private Transform GetArena(string arenaId)
        {
            GameObject arenaNodeRoot = contextProvider.ArenaRoot;
            IArenaNodePool arenaNodePool = arenaNodeRoot.GetComponent<IArenaNodePool>();
            IArenaNode arenaNode = arenaNodePool.GetArena(arenaId);

            return arenaNode.GameObject.transform;
        }
    }
}
