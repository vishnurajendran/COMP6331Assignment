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

            move = Vector3.ClampMagnitude(move, 1);
            _agent.Move(move, _params.AgentSpeed, Time.deltaTime);
            
            var lookDir = move * _params.AgentSpeed;
            lookDir.y = 0;
            _agent.LookAt(Vector3.Lerp(transform.forward, lookDir, Time.deltaTime * _params.AgentSpeed));
        }
    }
}