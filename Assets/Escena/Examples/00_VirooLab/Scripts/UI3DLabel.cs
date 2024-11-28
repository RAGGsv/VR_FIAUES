using UnityEngine;
using Viroo.Context;

namespace VirooLab.Examples
{
    public class UI3DLabel : MonoBehaviour
    {
        private Transform cam;
        private Canvas canvas;
        private bool injectionDone = false;

        private void Awake()
        {
            this.QueueForInject();
        }

        void Inject(IContextProvider contextProvider)
        {
            cam = contextProvider.Camera.transform;
            canvas = GetComponentInChildren<Canvas>();

            injectionDone = true;
        }

        private void Update()
        {
            if (!injectionDone) return;

            canvas.transform.LookAt(cam.position);
        }
    }
}
