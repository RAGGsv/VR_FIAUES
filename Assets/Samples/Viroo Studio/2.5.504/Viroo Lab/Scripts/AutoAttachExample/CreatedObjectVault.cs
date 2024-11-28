using System.Collections.Generic;
using UnityEngine;
using Virtualware.Networking.Client;

namespace VirooLab
{
    public class CreatedObjectVault : MonoBehaviour
    {
        [SerializeField]
        private string id = default;

        public string Id => id;

        private NetworkObjectsService networkObjectsService;

        private readonly ICollection<NetworkObject> netObjects = new List<NetworkObject>();

        protected void Inject(NetworkObjectsService networkObjectsService)
        {
            this.networkObjectsService = networkObjectsService;
        }

        protected void Awake()
        {
            this.QueueForInject();
        }

        public void Add(NetworkObject gameObject)
        {
            netObjects.Add(gameObject);
        }

#pragma warning disable S3168 // "async" methods should not return "void"
        public async void Clear()
#pragma warning restore S3168 // "async" methods should not return "void"
        {
            foreach (NetworkObject netObject in netObjects)
            {
                if (netObject.Authority)
                {
                    await networkObjectsService.DestroyObject(netObject);
                }
            }

            netObjects.Clear();
        }
    }
}
