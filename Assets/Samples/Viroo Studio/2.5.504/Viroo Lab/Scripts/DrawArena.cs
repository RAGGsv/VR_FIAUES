using System.Linq;
using TMPro;
using UnityEngine;
using Viroo;
using Viroo.Arena;
using Viroo.Arena.Native.Data;
using Viroo.Arena.NMerso.Data;

namespace VirooLab
{
    public class DrawArena : MonoBehaviour
    {
        private const int ArenaHeight = 2;
        private const float LabelsHeight = 3f;
        private const float LineWidth = 0.01f;

        [SerializeField]
        private Transform parent = default;

        [SerializeField]
        private Material boundariesFloorMaterial = default;

        [SerializeField]
        private Material boundariesWallMaterial = default;

        [SerializeField]
        private GameObject uiLabelPrefab = default;

        [SerializeField]
        private GameObject noArenaFoundGameObject = default;

        protected void Inject(IArenaAccessor arenaAccessor, IArenaDrawerRegistry arenaDrawerRegistry)
        {
            arenaAccessor.OnArenaLoaded += (sender, args) =>
            {
                GameObject arenaContainer = new("ArenaContainer");
                arenaContainer.transform.SetParent(parent, worldPositionStays: false);

                GameObject offset = new("Offset");
                offset.transform.SetParent(arenaContainer.transform);

                GameObject arena = new("Arena");
                arena.transform.SetParent(offset.transform);

                GameObject labels = new("Labels");
                labels.transform.SetParent(offset.transform);

                IArenaDrawer arenaDrawer = arenaDrawerRegistry.ResolveForArenaData(arenaAccessor.Arena);

                arenaDrawer.DrawChaperone(arena.transform, boundariesWallMaterial, ArenaHeight);
                arenaDrawer.DrawFloorBoundaries(arena.transform, boundariesFloorMaterial, LineWidth);

                foreach (Transform child in arenaContainer.transform)
                {
                    child.localPosition = UnityEngine.Vector3.zero;
                    child.gameObject.layer = 0;
                }

                arenaContainer.transform.localScale = UnityEngine.Vector3.one * 0.1f;

                LineRenderer[] lineRenderers = arenaContainer
                    .GetComponentsInChildren<LineRenderer>();

                foreach (LineRenderer lineRenderer in lineRenderers)
                {
                    lineRenderer.sortingOrder = 1;
                }

                Bounds bounds = arenaDrawer.GetBounds();

                if (arenaAccessor.Arena is NativeArenaData nativeArenaData)
                {
                    offset.transform.localPosition = new UnityEngine.Vector3(-bounds.center.x, 0, -bounds.center.z);

                    DrawNativeArenaLabels(labels.transform, nativeArenaData);
                }
                else if (arenaAccessor.Arena is NMersoArenaData)
                {
                    offset.transform.localPosition = new UnityEngine.Vector3(bounds.min.x, 0, bounds.min.z);
                }

                noArenaFoundGameObject.SetActive(value: false);

                ChangeChildrenLayer(arenaContainer.transform, Constants.Layer.Default);
            };
        }

        private void ChangeChildrenLayer(Transform parent, string layer)
        {
            foreach (Transform child in parent)
            {
                child.gameObject.layer = LayerMask.NameToLayer(layer);

                ChangeChildrenLayer(child, layer);
            }
        }

        protected void Awake()
        {
            noArenaFoundGameObject.SetActive(value: true);

            this.QueueForInject();
        }

        private void DrawNativeArenaLabels(Transform parent, NativeArenaData nativeArenaData)
        {
            foreach (BoundaryItem boundaryItem in nativeArenaData.BoundaryItems.Where(i => !string.IsNullOrEmpty(i.Name)))
            {
                Viroo.Arena.Native.Data.Vector3 point = boundaryItem.Points[0];

                GameObject label = Instantiate(uiLabelPrefab);
                label.name = boundaryItem.Name;
                label.transform.position = new UnityEngine.Vector3(point.X, point.Y + LabelsHeight, point.Z);
                label.transform.SetParent(parent, worldPositionStays: false);
                label.transform.localScale = UnityEngine.Vector3.one * 15f;
                label.SetActive(value: true);

                TextMeshProUGUI labelText = label.GetComponentInChildren<TextMeshProUGUI>();
                labelText.text = boundaryItem.Name;

                LineRenderer lineRenderer = label.GetComponentInChildren<LineRenderer>();
                lineRenderer.sortingOrder = 2;
                lineRenderer.useWorldSpace = false;
                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, new UnityEngine.Vector3(0, -0.02f, 0));
                lineRenderer.SetPosition(1, new UnityEngine.Vector3(0, -0.2f, 0));
            }
        }
    }
}
