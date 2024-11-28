using UnityEngine;
using VirooLab.Actions;
using Virtualware.Networking.Client;

namespace VirooLab
{
    public class AutoAttach : MonoBehaviour
    {
        protected void Start()
        {
            NetworkObject networkObject = GetComponent<NetworkObject>();
            ParentObjectToPlayer parentObjectToPlayer = GetComponent<ParentObjectToPlayer>();

            if (networkObject.Authority)
            {
                parentObjectToPlayer.SetParent();
            }
        }
    }
}
