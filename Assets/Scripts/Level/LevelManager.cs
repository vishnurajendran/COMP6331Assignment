using System;
using System.Collections.Generic;
using UnityEngine;

namespace Level
{
    public class LevelManager : MonoBehaviour
    {
        private static LevelManager _instance;
        private Queue<Transform> _targets;
        public static LevelManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = GameObject.FindObjectOfType<LevelManager>();

                return _instance;
            }
        }

        private void Awake()
        {
            _targets = new Queue<Transform>();
            foreach (var target in GameObject.FindObjectsOfType<Target>())
            {
                _targets.Enqueue(target.transform);
            }
        }

        public Transform GetNextTarget()
        {
            if (_targets.Count <= 0)
                return null;
            return _targets.Dequeue();
        }
    }
}