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

        private void Awake()
        {
            if (_heroRef == null)
                _heroRef = Resources.Load<GameObject>("Hero");
        }

        public void Start()
        {
            if (_heroRef != null)
            {
                var go =Instantiate(_heroRef, transform.position, transform.rotation);
                go.GetComponent<HeroController>().SetBase(this.transform);
            }
        }
    }
    
}
