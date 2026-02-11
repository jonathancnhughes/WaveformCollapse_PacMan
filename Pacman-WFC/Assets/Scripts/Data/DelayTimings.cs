using UnityEngine;

namespace JFlex.PacmanWFC.Data
{
    [CreateAssetMenu(fileName = "DelayTimings", menuName = "Pacman-WFC/DelayTimings")]
    public class DelayTimings : ScriptableObject
    {
        [SerializeField]
        private string description;

        [SerializeField]
        private float generatingDelay;
        public float GeneratingDelay => generatingDelay;

        [SerializeField]
        private float completingDelay;
        public float CompletingDelay => completingDelay;

        [SerializeField]
        private float resetDelay;
        public float ResetDelay => resetDelay;
    }
}