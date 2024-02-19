using UnityEngine;

namespace AgentControllers
{
    [CreateAssetMenu(fileName = "New Guard Params", menuName = "COMP6331/Create Guard Agent Params")]
    public class GuardAgentParams : StandardAIParams
    {
        [SerializeField] private float wanderMinRadius = 5;
        [SerializeField] private float wanderMaxRadius = 5;
        [SerializeField] private float captureRadius = 2;
        public float WanderMaxRadius => wanderMaxRadius;
        public float WanderMinRadius => wanderMinRadius;
        public float CaptureRadius => captureRadius;

    }
    
}

