using System;
using UnityEngine;

namespace AgentControllers
{
    [CreateAssetMenu(fileName = "New Params", menuName = "COMP6331/Create Agent Params")]
    public class BaseAgentParams : ScriptableObject
    {
        [SerializeField]
        private float speed;
        
        [SerializeField]
        private float lookSpeed=5;
        
        [SerializeField, Range(0.1f,10)]
        private float stoppingDist=1.25f;

        [SerializeField, Range(0, 10)] 
        private float areaDetectorSize = 2;
        
        public float AgentSpeed => speed;
        public float LookSpeed => lookSpeed;
        public float StoppingDistance => stoppingDist;
        public float AreaDetectorSize => areaDetectorSize;

    }
}