using UnityEngine;

namespace VirooLab
{
    public class Mirror : MonoBehaviour
    {
        [Tooltip("Texture size for the mirror, depending on how close the player can get to the mirror, this will need to be larger")]
        [SerializeField]
        private int textureSize = 768;

        public int TextureSize => textureSize;

        [Tooltip("Subtracted from the near plane of the mirror")]
        [SerializeField]
        private float clipPlaneOffset = 0.07f;

        public float ClipPlaneOffset => clipPlaneOffset;

        [Tooltip("Far clip plane for mirror camera")]
        [SerializeField]
        private float farClipPlane = 1000.0f;

        public float FarClipPlane => farClipPlane;

        [Tooltip("What layers will be reflected?")]
        [SerializeField]
        private LayerMask reflectLayers = -1;

        public LayerMask ReflectLayers => reflectLayers;

        [Tooltip("For quads, the normal points forward (true). For planes, the normal points up (false)")]
        [SerializeField]
        private bool normalIsForward = true;

        public bool NormalIsForward => normalIsForward;

        [Tooltip("Aspect ratio (width / height). Set to 0 to use default.")]
        [SerializeField]
        private float aspectRatio = 0.0f;

        public float AspectRatio => aspectRatio;

        [Tooltip("Set to true if you have multiple mirrors facing each other to get an infinite effect, " +
            "otherwise leave as false for a more realistic mirror effect.")]
        [SerializeField]
        private bool mirrorRecursion = false;

        public bool MirrorRecursion => mirrorRecursion;
    }
}
