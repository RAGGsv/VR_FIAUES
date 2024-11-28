using UnityEngine;
using Viroo.Interactions;
using Virtualware.Networking.Client.SceneManagement;

namespace VirooLab
{
    public class RestoreDefaultAvatarOnSceneUnload : MonoBehaviour
    {
        [SerializeField]
        private InternalChangeAvatarAction changeAvatarAction = default;

        protected void Inject(INetworkScenesService networkScenesService)
        {
            networkScenesService.OnLocalClientSceneUnloadStarted += OnLocalClientSceneUnloadStarted;
        }

        protected void Awake()
        {
            this.QueueForInject();
        }

        private void OnLocalClientSceneUnloadStarted(object sender, NetworkSceneEventArgs e)
        {
            (sender as INetworkScenesService)!.OnLocalClientSceneUnloadStarted -= OnLocalClientSceneUnloadStarted;

            changeAvatarAction.LocalExecute(string.Empty);
        }
    }
}
