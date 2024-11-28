using System;
using UnityEngine;
using Virtualware.Networking.Client;

namespace VirooLab
{
    public class CreatedObject : MonoBehaviour
    {
        [SerializeField]
        private string id = default;

        protected void Start()
        {
            NetworkObject networkObject = GetComponent<NetworkObject>();

            if (networkObject.Authority)
            {
                CreatedObjectVault createdObjectVault = Array
                    .Find(FindObjectsOfType<CreatedObjectVault>(), v => v.Id.Equals(id, StringComparison.Ordinal));

                if (createdObjectVault)
                {
                    createdObjectVault.Add(networkObject);
                }
            }
        }
    }
}
