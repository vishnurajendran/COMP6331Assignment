using System;
using System.Collections;
using System.Collections.Generic;
using AgentControllers;
using UnityEngine;

namespace Level
{
    public class Base : MonoBehaviour
    {
        private static GameObject _heroRef;
        [SerializeField, Range(1, 10)] private float guardKillRadius = 0.1f;
        private void Awake()
        {
            if (_heroRef == null)
                _heroRef = Resources.Load<GameObject>("Hero");
        }

        public void Start()
        {
            LevelManager.Instance.AddBase(transform);
            var collider = gameObject.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = guardKillRadius;
            var rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            if (_heroRef != null)
            {
                Instantiate(_heroRef, transform.position, transform.rotation);
            }
        }

        private void OnDrawGizmos()
        {
            var col = Color.red;
            col.a = 0.25f;
            Gizmos.color = col;
            Gizmos.DrawSphere(transform.position, guardKillRadius);
        }

        private void OnTriggerEnter(Collider other)
        {
            var guard = other.GetComponent<Guard>();
            if (guard)
            {
                UIManager.Instance?.IncrementScore();
                Destroy(guard.gameObject);
            }
        }
    }
    
}
