using System;
using System.Collections.Generic;
using AgentControllers;
using UnityEngine;

namespace Level
{
    public class LevelManager : MonoBehaviour
    {
        private static LevelManager _instance;
        private Queue<Transform> _targets;
        private List<Transform> _gaurds;

        private int _herosInLevel;
        
        public static LevelManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = GameObject.FindObjectOfType<LevelManager>();

                return _instance;
            }
        }

        public int GuardsInLevel => _gaurds.Count;
        
        private void Awake()
        {
            _targets = new Queue<Transform>();
            _gaurds = new List<Transform>();
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

        public void RegisterGuard(Transform prisoner)
        {
            _gaurds.Add(prisoner);
        }
        
        public void DeRegisterGuard(Transform prisoner)
        {
            _gaurds.Remove(prisoner);
        }

        public void AddHero()
        {
            _herosInLevel++;
        }

        public void HeroKilled()
        {
            _herosInLevel--;
            if(_herosInLevel <= 0)
                UIManager.Instance?.GameOver();
        }
    }
}