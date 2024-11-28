using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using Viroo.Networking;
using Virtualware.Networking.Client.SessionManagement;

namespace VirooLab
{
    public class GetConnectedPlayersNames : MonoBehaviour
    {
        [SerializeField]
        private TextMeshPro textMeshProText = default;

        private IPlayerProvider playerProvider;
        private List<IPlayer> players;

        protected void Inject(IPlayerProvider playerProvider, ISessionClientsEventListener sessionClientsEventListener)
        {
            this.playerProvider = playerProvider;

            players = playerProvider.GetAll().ToList();

            UpdatePlayers();

            sessionClientsEventListener.ClientUnregistered += OnClientUnregistered;
            playerProvider.OnPlayerRegistered += OnPlayerRegistered;
        }

        protected void Awake()
        {
            this.QueueForInject();
        }

        private void OnPlayerRegistered(object sender, PlayerRegisteredEventArgs e)
        {
            if (!players.Contains(e.Player))
            {
                AddPlayer(e.Player);
            }

            UpdatePlayers();
        }

        private void OnClientUnregistered(object sender, NetworkSessionClientEventArgs e)
        {
            IPlayer disconnectedPlayer = playerProvider.Get(e.SessionClient.ClientId);

            if (players.Contains(disconnectedPlayer))
            {
                players.Remove(disconnectedPlayer);
            }

            UpdatePlayers();
        }

        private void AddPlayer(IPlayer player)
        {
            if (!players.Contains(player))
            {
                players.Add(player);
            }
        }

        private void UpdatePlayers()
        {
            UpdateText();
        }

        private void UpdateText()
        {
            textMeshProText.text = string.Empty;

            StringBuilder stringBuilder = new();

            foreach (IPlayer player in players)
            {
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}", player.GetPlayerName(), "\n");
            }

            textMeshProText.text = stringBuilder.ToString();
        }
    }
}
