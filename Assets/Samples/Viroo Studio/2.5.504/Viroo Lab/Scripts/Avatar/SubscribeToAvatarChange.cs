using System;
using System.Collections;
using Microsoft.Extensions.Options;
using TMPro;
using UnityEngine;
using Viroo.Configuration;
using Viroo.Networking;
using Virtualware.Networking.Client;
using Virtualware.Networking.Client.SessionManagement;

namespace VirooLab
{
    public class SubscribeToAvatarChange : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI avatarIdText = default;

        protected void Inject(
            IPlayerProvider playerProvider,
            ISessionClientsProvider sessionClientsProvider,
            NetworkPlayerEventsProxy networkPlayerEventsProxy,
            IOptions<GeneralOptions> generalOptionsAccessor)
        {
            networkPlayerEventsProxy.OnLocalPlayerStarted
                += (sender, e) => StartCoroutine(GetLocalPlayer(playerProvider, sessionClientsProvider));

            avatarIdText.enabled = !generalOptionsAccessor.Value.Invisible;
        }

        protected void Awake()
        {
            this.QueueForInject();
        }

        private IEnumerator GetLocalPlayer(IPlayerProvider playerProvider, ISessionClientsProvider sessionClientsProvider)
        {
            IPlayer player = null;

            while (player == null)
            {
                player = playerProvider.Get(sessionClientsProvider.LocalClient.ClientId);

                yield return null;
            }

            player.OnLocalAvatarLoaded += OnLocalAvatarLoaded;
        }

        private void OnLocalAvatarLoaded(object sender, EventArgs e)
        {
            avatarIdText.text = (sender as IPlayer)!.AvatarId;
        }
    }
}
