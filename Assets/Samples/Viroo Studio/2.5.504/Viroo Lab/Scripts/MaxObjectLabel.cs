using System.Globalization;
using TMPro;
using UnityEngine;

namespace VirooLab
{
    public class MaxObjectLabel : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI label = default;

        [SerializeField]
        private int maxObject = 0;

        private int createdObjects = 0;

        protected void Awake()
        {
            label.text = maxObject.ToString(CultureInfo.InvariantCulture);
        }

        public void CreateObject()
        {
            createdObjects++;

            label.text = (maxObject - createdObjects).ToString(CultureInfo.InvariantCulture);
        }
    }
}
