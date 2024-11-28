using System.Globalization;
using TMPro;
using UnityEngine;
using Virtualware.Networking.Client.Variables;

namespace VirooLab
{
    public class Score : MonoBehaviour
    {
        private const string ScoreText = "SCORE: {0:D2}";

        [SerializeField]
        private TextMeshProUGUI scoreLabel = default;

        private NetworkVariable<int> scoreVariable;

        protected void Inject(NetworkVariableSynchronizer variableSynchronizer)
        {
            scoreVariable = new NetworkVariable<int>(variableSynchronizer, "ScoreVariable", 0);
            scoreVariable.OnValueChanged += OnVariableChanged;
        }

        protected void Awake()
        {
            this.QueueForInject();
        }

        // This event is also called when the variable is initialized
        private void OnVariableChanged(object sender, int value)
        {
            scoreLabel.text = string.Format(CultureInfo.InvariantCulture, ScoreText, value);
        }

        public void Increment()
        {
            scoreVariable.Value++;
        }
    }
}
