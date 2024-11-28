using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Virtualware.Networking.Client;

namespace VirooLab
{
    public class Bullet : MonoBehaviour
    {
        private const float DestroyTime = 10;

        private NetworkObjectsService networkObjectsService;

        private bool injectionDone;

        protected void Inject(NetworkObjectsService networkObjectsService)
        {
            this.networkObjectsService = networkObjectsService;

            Rigidbody rigid = GetComponent<Rigidbody>();
            rigid.AddForce(transform.forward * 1000, ForceMode.Acceleration);

            injectionDone = true;
        }

#pragma warning disable S3168 // "async" methods should not return "void"
        protected async void Awake()
#pragma warning restore S3168 // "async" methods should not return "void"
        {
            this.QueueForInject();

            try
            {
                CancellationToken cancellationToken = this.GetCancellationTokenOnDestroy();

                await UniTask.WaitUntil(() => injectionDone, cancellationToken: cancellationToken);

                await UniTask.Delay((int)(DestroyTime * 1000), cancellationToken: cancellationToken);

                if (TryGetComponent(out NetworkObject networkObject))
                {
                    await networkObjectsService.DestroyObject(networkObject);
                }
            }
            catch (OperationCanceledException)
            {
                // Scene changed, ignore exception
            }
        }
    }
}
