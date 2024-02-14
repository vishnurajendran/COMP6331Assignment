using System;
using UnityEngine;

namespace AgentControllers
{
    [CreateAssetMenu(fileName = "New Params", menuName = "COMP6331/Create Agent Params")]
    public class AgentParams : ScriptableObject
    {
        [SerializeField]
        private float speed;
        
        [SerializeField]
        private float lookSpeed=5;
        
        [SerializeField, Range(0.1f,10)]
        private float stoppingDist=1.25f;

        [SerializeField, Range(0, 10)] 
        private float areaDetectorSize = 2;
        
        [SerializeField, Range(0, 10)] 
        private float visionRange = 5;
        
        [SerializeField, Range(0, 360)] 
        private float visionAngle = 120;
        
        [SerializeField]
        private float wanderRadius = 5;
        
        public float AgentSpeed => speed;
        public float LookSpeed => lookSpeed;
        public float StoppingDistance => stoppingDist;
        public float AreaDetectorSize => areaDetectorSize;
        public float VisionRange => visionRange;
        public float VisionAngle => visionAngle;
        public float WanderRadius => wanderRadius;
        
    }
}