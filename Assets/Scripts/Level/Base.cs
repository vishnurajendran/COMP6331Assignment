using System;
using System.Collections;
using System.Collections.Generic;
using AgentControllers;
using AgentControllers.AgentControllers;
using UnityEngine;


namespace Level
{
    public class Base : MonoBehaviour
    {
        private static GameObject _heroRef;
        [SerializeField, Range(1, 10)] private float guardKillRadius = 0.1f;
        [SerializeField] private bool _allowAgroResetOnHeros = false;
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
               var hero = Instantiate(_heroRef, transform.position, transform.rotation).GetComponent<HeroController>();
               hero.AllowAggroReset(_allowAgroResetOnHeros);
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
           
            if (other.CompareTag("Guard"))
            {
                UIManager.Instance?.IncrementScore();
                Destroy(other.gameObject);
            }
            else if (other.CompareTag("Player"))
            {
                other.GetComponent<AgentUserController>().ResetPlayer();
            }
        }
    }
    
}
