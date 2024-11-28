using UnityEngine;
using Viroo.Context;

namespace VirooLab
{
    public class UI3DLabel : MonoBehaviour
    {
        private Transform cam;
        private Canvas canvas;
        private bool injectionDone = false;

        protected void Inject(IContextProvider contextProvider)
        {
            cam = contextProvider.VirooCamera.transform;
            canvas = GetComponentInChildren<Canvas>();

            injectionDone = true;
        }

        protected void Awake()
        {
            this.QueueForInject();
        }

        protected void Update()
        {
            if (!injectionDone)
            {
                return;
            }

            canvas.transform.LookAt(cam.position);
        }
    }
}
