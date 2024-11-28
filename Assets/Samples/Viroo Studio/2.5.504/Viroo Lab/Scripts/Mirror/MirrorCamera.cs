using System;
using System.Globalization;
using Microsoft.Extensions.Options;
using UnityEngine;
using Viroo.Configuration;

namespace VirooLab
{
    [RequireComponent(typeof(Camera))]
    public class MirrorCamera : MonoBehaviour
    {
        [SerializeField]
        private Mirror mirror = default;

        [SerializeField]
        private Renderer mirrorRenderer = default;

        private bool isVr;

        private Material mirrorMaterial;
        private Camera cameraObject;
        private RenderTexture reflectionTexture;
        private Matrix4x4 reflectionMatrix;
        private int oldReflectionTextureSize;
        private static bool s_renderingMirror;

        protected void Inject(IOptions<GeneralOptions> generalOptionsAccessor)
        {
            isVr = generalOptionsAccessor.Value.IsUsingTracker();
        }

        protected void Awake()
        {
            this.QueueForInject();
        }

        protected void Start()
        {
            cameraObject = GetComponent<Camera>();

            if (Application.isPlaying)
            {
                Material material = Array
                    .Find(mirrorRenderer.sharedMaterials, m => m.name.Equals("MirrorMaterial", StringComparison.Ordinal));

                if (material)
                {
                    mirrorRenderer.sharedMaterial = material;
                }
            }

            mirrorMaterial = mirrorRenderer.sharedMaterial;

            CreateRenderTexture();
        }

        // Cleanup all the objects we possibly have created
        protected void OnDisable()
        {
            if (reflectionTexture)
            {
                DestroyImmediate(reflectionTexture);

                reflectionTexture = null;
            }
        }

        protected void Update()
        {
            if (isVr && Camera.current == Camera.main)
            {
                return;
            }

            CreateRenderTexture();
        }

        private void CreateRenderTexture()
        {
            if (reflectionTexture == null || oldReflectionTextureSize != mirror.TextureSize)
            {
                if (reflectionTexture)
                {
                    DestroyImmediate(reflectionTexture);
                }

                reflectionTexture = new RenderTexture(mirror.TextureSize, mirror.TextureSize, 16)
                {
                    antiAliasing = 4,
                    name = "MirrorRenderTexture_" + GetInstanceID().ToString(CultureInfo.InvariantCulture),
                    hideFlags = HideFlags.HideAndDontSave,
                    autoGenerateMips = false,
                    wrapMode = TextureWrapMode.Clamp,
                };

                mirrorMaterial.SetTexture("_MainTex", reflectionTexture);

                oldReflectionTextureSize = mirror.TextureSize;
            }

            if (cameraObject.targetTexture != reflectionTexture)
            {
                cameraObject.targetTexture = reflectionTexture;
            }
        }

        private void UpdateCameraProperties(Camera src, Camera dest, CameraClearFlags cameraClearFlags)
        {
            dest.clearFlags = cameraClearFlags;
            dest.backgroundColor = Color.black;

            if (cameraClearFlags == CameraClearFlags.Skybox)
            {
                Skybox sky = src.GetComponent<Skybox>();
                Skybox mySky = dest.GetComponent<Skybox>();
                if (!sky || !sky.material)
                {
                    mySky.enabled = false;
                }
                else
                {
                    mySky.enabled = true;
                    mySky.material = sky.material;
                }
            }

            dest.orthographic = src.orthographic;
            dest.orthographicSize = src.orthographicSize;
            if (mirror.AspectRatio > 0.0f)
            {
                dest.aspect = mirror.AspectRatio;
            }
            else
            {
                dest.aspect = src.aspect;
            }

            dest.renderingPath = src.renderingPath;
        }

        internal void RenderMirror()
        {
            // bail if we don't have a camera or renderer
            if (s_renderingMirror || !enabled || Camera.current == null ||
                mirrorRenderer == null || mirrorMaterial == null || !mirrorRenderer.enabled)
            {
                return;
            }

#pragma warning disable S2696 // Instance members should not write to "static" fields
            s_renderingMirror = true;
#pragma warning restore S2696 // Instance members should not write to "static" fields

            try
            {
                Camera cameraLookingAtThisMirror = Camera.current;

                UpdateCameraProperties(cameraLookingAtThisMirror, cameraObject, CameraClearFlags.Color);

                if (mirror.MirrorRecursion)
                {
                    ProcessMirrorRecursion();
                }
                else
                {
                    ProcessNonMirrorRecursion(cameraLookingAtThisMirror);
                }
            }
            finally
            {
                s_renderingMirror = false;
            }
        }

        private void ProcessMirrorRecursion()
        {
            mirrorMaterial.EnableKeyword("MIRROR_RECURSION");
            cameraObject.ResetWorldToCameraMatrix();
            cameraObject.ResetProjectionMatrix();
            cameraObject.projectionMatrix *= Matrix4x4.Scale(new Vector3(-1, 1, 1));
            cameraObject.cullingMask = ~(1 << 4) & mirror.ReflectLayers.value;
            GL.invertCulling = true;
            cameraObject.Render();
            GL.invertCulling = false;
        }

        private void ProcessNonMirrorRecursion(Camera cameraLookingAtThisMirror)
        {
            mirrorMaterial.DisableKeyword("MIRROR_RECURSION");
            Vector3 pos = transform.position;
            Vector3 normal = mirror.NormalIsForward ? transform.forward : transform.up;

            // Reflect camera around reflection plane
            float d = -Vector3.Dot(normal, pos) - mirror.ClipPlaneOffset;
            Vector4 reflectionPlane = new(normal.x, normal.y, normal.z, d);
            CalculateReflectionMatrix(ref reflectionPlane);
            Vector3 oldPosition = cameraObject.transform.position;
            float oldClip = cameraObject.farClipPlane;
            Vector3 newPosition = reflectionMatrix.MultiplyPoint(oldPosition);

            Matrix4x4 worldToCameraMatrix = cameraLookingAtThisMirror.worldToCameraMatrix;

            if (isVr)
            {
                if (cameraLookingAtThisMirror.stereoActiveEye == Camera.MonoOrStereoscopicEye.Left)
                {
                    worldToCameraMatrix[12] += 0.011f;
                }
                else if (cameraLookingAtThisMirror.stereoActiveEye == Camera.MonoOrStereoscopicEye.Right)
                {
                    worldToCameraMatrix[12] -= 0.011f;
                }
            }

            worldToCameraMatrix *= reflectionMatrix;
            cameraObject.worldToCameraMatrix = worldToCameraMatrix;

            // Clip out background
            Vector4 clipPlane = CameraSpacePlane(ref worldToCameraMatrix, ref pos, ref normal, 1.0f);
            cameraObject.projectionMatrix = cameraLookingAtThisMirror.CalculateObliqueMatrix(clipPlane);
            GL.invertCulling = true;
            cameraObject.transform.position = newPosition;
            cameraObject.farClipPlane = mirror.FarClipPlane;
            cameraObject.cullingMask = ~(1 << 4) & mirror.ReflectLayers.value;
            cameraObject.Render();
            cameraObject.transform.position = oldPosition;
            cameraObject.farClipPlane = oldClip;
            GL.invertCulling = false;
        }

        private Vector4 CameraSpacePlane(ref Matrix4x4 worldToCameraMatrix, ref Vector3 pos, ref Vector3 normal, float sideSign)
        {
            Vector3 offsetPos = pos + (normal * mirror.ClipPlaneOffset);
            Vector3 cPos = worldToCameraMatrix.MultiplyPoint(offsetPos);
            Vector3 cNormal = worldToCameraMatrix.MultiplyVector(normal).normalized * sideSign;
            return new Vector4(cNormal.x, cNormal.y, cNormal.z, -Vector3.Dot(cPos, cNormal));
        }

        private void CalculateReflectionMatrix(ref Vector4 plane)
        {
            // Calculates reflection matrix around the given plane

            reflectionMatrix.m00 = 1F - (2F * plane[0] * plane[0]);
            reflectionMatrix.m01 = -2F * plane[0] * plane[1];
            reflectionMatrix.m02 = -2F * plane[0] * plane[2];
            reflectionMatrix.m03 = -2F * plane[3] * plane[0];

            reflectionMatrix.m10 = -2F * plane[1] * plane[0];
            reflectionMatrix.m11 = 1F - (2F * plane[1] * plane[1]);
            reflectionMatrix.m12 = -2F * plane[1] * plane[2];
            reflectionMatrix.m13 = -2F * plane[3] * plane[1];

            reflectionMatrix.m20 = -2F * plane[2] * plane[0];
            reflectionMatrix.m21 = -2F * plane[2] * plane[1];
            reflectionMatrix.m22 = 1F - (2F * plane[2] * plane[2]);
            reflectionMatrix.m23 = -2F * plane[3] * plane[2];

            reflectionMatrix.m30 = 0F;
            reflectionMatrix.m31 = 0F;
            reflectionMatrix.m32 = 0F;
            reflectionMatrix.m33 = 1F;
        }

        protected static void CalculateObliqueMatrix(ref Matrix4x4 projection, ref Vector4 clipPlane)
        {
            Vector4 q = projection.inverse * new Vector4
            (
                Sign(clipPlane.x),
                Sign(clipPlane.y),
                1.0f,
                1.0f
            );
            Vector4 c = clipPlane * (2.0F / Vector4.Dot(clipPlane, q));
            // third row = clip plane - fourth row
            projection[2] = c.x - projection[3];
            projection[6] = c.y - projection[7];
            projection[10] = c.z - projection[11];
            projection[14] = c.w - projection[15];
        }

        private static float Sign(float a)
        {
            if (a > 0.0f)
            {
                return 1.0f;
            }

            if (a < 0.0f)
            {
                return -1.0f;
            }

            return 0.0f;
        }
    }
}
