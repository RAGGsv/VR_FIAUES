using TMPro;
using UnityEngine;

namespace VirooLab.Examples
{
    public class MaxObjectLabel : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI label;

        [SerializeField]
        private int maxObject = 0;

        private int createdObjects = 0;

        private void Awake()
        {
            label.text = maxObject.ToString();
        }

        public void CreateObject()
        {
            createdObjects++;

            label.text = (maxObject - createdObjects).ToString();
        }
    }
}