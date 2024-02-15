using UnityEngine;

namespace AgentControllers
{
    [CreateAssetMenu(fileName = "New Guard Params", menuName = "COMP6331/Create Guard Agent Params")]
    public class GuardAgentParams : AgentParams
    {
        [SerializeField] 
        private float visionRange = 5;
        
        [SerializeField, Range(0, 360)] 
        private float visionAngle = 120;
        
        [SerializeField]
        private float wanderRadius = 5;
        
        [SerializeField]
        private float captureRadius = 2;
        
        public float VisionRange => visionRange;
        public float VisionAngle => visionAngle;
        public float WanderRadius => wanderRadius;
        public float CaptureRadius => captureRadius;

        private void OnValidate()
        {
            visionRange = Mathf.Max(visionRange, 2 * captureRadius);
        }
    }
    
}

