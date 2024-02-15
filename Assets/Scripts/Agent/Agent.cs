using UnityEngine;

namespace Agents
{
    [RequireComponent(typeof(CharacterController))]
    public class Agent : MonoBehaviour
    {
        [SerializeField] private float _gravity = -9.81f;
        [SerializeField, Tooltip("Requires the obstacle avoidance plugin")] 
        private float avoidanceCorrectionThresh=1;
        
        private AgentObstacleAvoidancePlugin _obsAvoidancePlugin;
        private CharacterController _controller;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _obsAvoidancePlugin = GetComponent<AgentObstacleAvoidancePlugin>();
        }

        private void Update()
        {
            if(!_controller.isGrounded)
                Move(Vector3.up, _gravity,0, Time.deltaTime);
        }

        public void Move(Vector3 direction, float moveSpeed,float lookSpeed, float deltaTime)
        {
            var move = Vector3.zero;
            var obstacleAvoidance = AvoidObstacles() * avoidanceCorrectionThresh;
            move += obstacleAvoidance;
            move += direction;
            
            move = move.normalized;
            _controller.Move(move * (moveSpeed * deltaTime));
            if(!_controller.isGrounded)
                _controller.Move(Vector3.up * (_gravity * deltaTime));

            var lookDir = move;
            lookDir.y = 0;
            LookAt(lookDir, lookSpeed, deltaTime);
        }

        private Vector3 AvoidObstacles()
        {
            if (!_obsAvoidancePlugin)
                return Vector3.zero;
            
            return _obsAvoidancePlugin.Correction;
        }
        
        private void LookAt(Vector3 direction, float speed, float deltaTime)
        {
            if(direction.Equals(Vector3.zero))
                return;
            Quaternion lookRot = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRot, speed* deltaTime);
        }

    }
}
