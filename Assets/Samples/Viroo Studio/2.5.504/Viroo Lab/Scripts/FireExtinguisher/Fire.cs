using System.Collections;
using UnityEngine;

namespace VirooLab
{
    public class Fire : MonoBehaviour
    {
        [SerializeField]
        private float extinctionTime = 5f;

        [SerializeField]
        private GameObject fire = default;

        private float currentExtinctionTime;
        private Coroutine extinguishCoroutine;
        private bool fireIsExtinguished;

        public void StartExtinguishing()
        {
            extinguishCoroutine = StartCoroutine(Extinguish());
        }

        public void StopExtinguishing()
        {
            if (extinguishCoroutine != null)
            {
                StopCoroutine(extinguishCoroutine);
            }
        }

        private IEnumerator Extinguish()
        {
            while (!fireIsExtinguished)
            {
                yield return new WaitForSeconds(1);

                currentExtinctionTime++;

                if (currentExtinctionTime >= extinctionTime)
                {
                    fireIsExtinguished = true;

                    DisableFire();
                }
            }
        }

        private void DisableFire()
        {
            fire.SetActive(value: false);
        }
    }
}
