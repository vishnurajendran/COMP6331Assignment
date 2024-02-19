using UnityEngine;

namespace AgentControllers
{
    [CreateAssetMenu(fileName = "New Standard Params", menuName = "COMP6331/Create Standard Agent Params")]
    public class StandardAIParams : BaseAgentParams
    {
        [SerializeField] private float visionRange = 5;
        [SerializeField, Range(0, 360)] private float visionAngle = 120;
        public float VisionRange => visionRange;
        public float VisionAngle => visionAngle;
    }
}