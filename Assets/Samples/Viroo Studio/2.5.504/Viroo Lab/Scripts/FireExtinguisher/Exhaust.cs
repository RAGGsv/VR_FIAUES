using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.XR.Interaction.Toolkit;
using Viroo.Interactions.Grab;

namespace VirooLab
{
    public class Exhaust : MonoBehaviour
    {
        [SerializeField]
        private XRGrabInteractable extinguisherGrabbable = default;

        [SerializeField]
        private XRGrabInteractable exhaustGrabbable = default;

        [SerializeField]
        private ParentConstraint parentConstraint = default;

        private IGrabberRegistry grabberRegistry;

        protected void Inject(IGrabberRegistry grabberRegistry)
        {
            this.grabberRegistry = grabberRegistry;
        }

        protected void Awake()
        {
            exhaustGrabbable.enabled = false;

            this.QueueForInject();
        }

        protected void OnEnable()
        {
            extinguisherGrabbable.selectEntered.AddListener(OnExtinguisherSelectEntered);
            extinguisherGrabbable.selectExited.AddListener(OnExtinguisherSelectExited);

            exhaustGrabbable.selectEntered.AddListener(OnExhaustSelectEntered);
            exhaustGrabbable.selectExited.AddListener(OnExhaustSelectExited);
        }

        protected void OnDisable()
        {
            extinguisherGrabbable.selectEntered.RemoveListener(OnExtinguisherSelectEntered);
            extinguisherGrabbable.selectExited.RemoveListener(OnExtinguisherSelectExited);

            exhaustGrabbable.selectEntered.RemoveListener(OnExhaustSelectEntered);
            exhaustGrabbable.selectExited.RemoveListener(OnExhaustSelectExited);
        }

        private void OnExhaustSelectExited(SelectExitEventArgs args) => parentConstraint.constraintActive = true;

        private void OnExhaustSelectEntered(SelectEnterEventArgs args) => parentConstraint.constraintActive = false;

        private void OnExtinguisherSelectEntered(SelectEnterEventArgs args)
        {
            exhaustGrabbable.enabled = true;
        }

        private void OnExtinguisherSelectExited(SelectExitEventArgs args)
        {
            if (exhaustGrabbable.firstInteractorSelecting != null)
            {
                XRGrabber grabber = grabberRegistry.GetXRIGrabber(exhaustGrabbable);

                grabber?.ForceUnGrab();
            }

            exhaustGrabbable.enabled = false;
        }
    }
}
