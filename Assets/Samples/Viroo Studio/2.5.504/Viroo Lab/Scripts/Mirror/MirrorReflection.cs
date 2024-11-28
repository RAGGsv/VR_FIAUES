using UnityEngine;

namespace VirooLab
{
    public class MirrorReflection : MonoBehaviour
    {
        [SerializeField]
        private MirrorCamera mirrorCamera = default;

        protected void OnWillRenderObject()
        {
            mirrorCamera.RenderMirror();
        }
    }
}
