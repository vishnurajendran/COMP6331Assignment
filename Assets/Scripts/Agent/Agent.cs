using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Agents
{
    [RequireComponent(typeof(CharacterController))]
    public class Agent : MonoBehaviour
    {
        [SerializeField] private float _gravity = -9.81f;
        
        private CharacterController _controller;
        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if(!_controller.isGrounded)
                Move(Vector3.up, _gravity, Time.deltaTime);
        }

        public void Move(Vector3 direction, float speed, float deltaTime)
        {
            _controller.Move(direction * (speed * deltaTime));
            if(!_controller.isGrounded)
                _controller.Move(Vector3.up * (_gravity * deltaTime));
        }
    
        public void LookAt(Vector3 direction)
        {
            Quaternion lookRot = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = lookRot;
        }
    }
}
