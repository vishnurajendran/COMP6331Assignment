using System;
using UnityEngine;

namespace AgentControllers
{
    public class Prisoner : AgentController
    {
        [SerializeField] private AgentParams _params;
        
        protected override AgentParams Params => _params;
        
        private void Update()
        {
            if(!_target)
                return;
            
            Vector3 move = Vector3.zero;
            move += SeekTarget(_target);
            
            _agent.Move(move.normalized, _params.AgentSpeed,_params.LookSpeed, Time.deltaTime);
        }

    }
}