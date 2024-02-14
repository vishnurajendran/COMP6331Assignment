using System;
using UnityEngine;

namespace AgentControllers
{
    public class Prisoner : AgentController
    {
        private void Update()
        {
            if(!_target)
                return;
            
            Vector3 move = Vector3.zero;
            move += SeekTarget(_target);
            
            _agent.Move(move.normalized, _params.AgentSpeed, Time.deltaTime);
            var lookDir = move;
            lookDir.y = 0;
            _agent.LookAt(lookDir, _params.LookSpeed, Time.deltaTime);
        }
    }
}