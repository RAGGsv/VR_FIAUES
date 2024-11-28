using System;
using UnityEngine;
using Viroo.Interactions;
using Virtualware.Networking.Client;

namespace VirooLab
{
    public class ChangeAvatarOnStart : MonoBehaviour
    {
        [SerializeField]
        private InternalChangeAvatarAction changeAvatarAction = default;

        private InstantiableAvatarElementRegistry instantiableAvatarElementRegistry;

        protected void Inject(InstantiableAvatarElementRegistry instantiableAvatarElementRegistry)
        {
            this.instantiableAvatarElementRegistry = instantiableAvatarElementRegistry;

            if (instantiableAvatarElementRegistry.IsAvatarRegistered(changeAvatarAction.AvatarId))
            {
                changeAvatarAction.LocalExecute(string.Empty);
            }
            else
            {
                instantiableAvatarElementRegistry.OnAvatarRegistered += OnAvatarRegistered;
            }
        }

        protected void Awake()
        {
            this.QueueForInject();
        }

        protected void OnDestroy()
        {
            if (instantiableAvatarElementRegistry != null)
            {
                instantiableAvatarElementRegistry.OnAvatarRegistered -= OnAvatarRegistered;
            }
        }

        private void OnAvatarRegistered(object sender, InstantiableAvatarElementRegistryEventArgs e)
        {
            if (changeAvatarAction.AvatarId.Equals(e.AvatarId, StringComparison.Ordinal))
            {
                changeAvatarAction.LocalExecute(string.Empty);
            }
        }
    }
}
