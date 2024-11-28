using Microsoft.Extensions.Options;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.XR.Interaction.Toolkit;
using Viroo.Configuration;
using Viroo.Interactions;
using Viroo.Interactions.Grab;
using static Viroo.Hands.InternalBaseHand;

namespace VirooLab
{
    public class Extinguisher : MonoBehaviour
    {
        [SerializeField]
        private BroadcastObjectAction activateJetAction = default; // We use broadcast action to execute in all players

        [SerializeField]
        private BroadcastObjectAction deactivateJetAction = default; // We use broadcast action to execute in all players

        [SerializeField]
        private XRGrabInteractable mainGrabbable = default;

        [SerializeField]
        private XRGrabInteractable auxGrabbable = default;

        [SerializeField]
        private GameObject extinguisherJetCollision = default;

        [SerializeField]
        private bool isOneHanded = false;

        private IGrabberRegistry grabberRegistry;
        private bool actionIsExecuted; // This variable prevents for execute action in every Update frame

        protected void Inject(
            IOptions<GeneralOptions> optionsAccessor,
            IGrabberRegistry grabberRegistry)
        {
            this.grabberRegistry = grabberRegistry;

            bool isVrPlayer = optionsAccessor.Value.IsUsingTracker();

            if (!isOneHanded && !isVrPlayer)
            {
                mainGrabbable.selectEntered.AddListener(OnGrabbed);
            }
            else if (!isOneHanded)
            {
                NetworkGrabbable auxiliaryNetworkGrabbable = GetComponent<NetworkGrabbable>();
                ParentConstraint parentConstraint = mainGrabbable.GetComponent<ParentConstraint>();

                auxiliaryNetworkGrabbable.OnGrabStateChanged += (object sender, NetworkGrabEventArgs e) => parentConstraint
                    .constraintActive = !e.IsGrabbed;
            }
        }

        private void OnGrabbed(SelectEnterEventArgs arg)
        {
            if (auxGrabbable == null)
            {
                return;
            }

            IGrabbable grab = new XRGrabbable(auxGrabbable);
            XRDirectInteractor directInteractor = arg.interactorObject.transform.GetComponent<XRDirectInteractor>();

            HandTypes handType = grabberRegistry.GetXRIHandType(directInteractor);

            XRGrabber grabber = handType == HandTypes.Left
                ? grabberRegistry.GetXRIGrabber(HandTypes.Right)
                : grabberRegistry.GetXRIGrabber(HandTypes.Left);

            grabber.ForceGrab(grab);
        }

        protected void Awake()
        {
            this.QueueForInject();
        }

        protected void OnEnable()
        {
            mainGrabbable.selectExited.AddListener(OnSelectExited);
        }

        protected void OnDisable()
        {
            mainGrabbable.selectExited.RemoveListener(OnSelectExited);
        }

        protected void Start()
        {
            extinguisherJetCollision.SetActive(value: false);
        }

        private void OnSelectExited(SelectExitEventArgs arg)
        {
            DeactivateAction();
        }

        public void DeactivateAction()
        {
            if (actionIsExecuted)
            {
                deactivateJetAction.Execute();
                actionIsExecuted = false;
            }
        }

        public void ActivateAction()
        {
            if (!actionIsExecuted)
            {
                activateJetAction.Execute();
                actionIsExecuted = true;
            }
        }
    }
}
