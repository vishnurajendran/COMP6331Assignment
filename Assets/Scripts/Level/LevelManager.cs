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
        private List<Transform> _bases;

        private bool _gameOver=false;
        private int _herosInLevel;
        private int _totalPrisoners = 0;
        private int _totalSaved = 0;
        
        public bool GameOver => _gameOver;
        
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

            _totalPrisoners = _targets.Count;
        }

        public void GiveBackPrisoner(Transform trf)
        {
            _targets.Enqueue(trf);
        }

        public void PrisonerSaved()
        {
            _totalSaved += 1;
            if (_totalSaved >= _totalPrisoners)
            {
                _gameOver = true;
                UIManager.Instance?.GameOver();
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

        public void AddBase(Transform baseTrf)
        {
            if (_bases == null)
                _bases = new List<Transform>();
            _bases.Add(baseTrf);
        }

        public Transform GetClosestBase(Vector3 referencePosition)
        {
            Transform minTrf = null;
            float minDist = Mathf.Infinity;
            foreach (var baseCandidate in _bases)
            {
                var dist = Vector3.Distance(referencePosition, baseCandidate.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    minTrf = baseCandidate;
                }
            }

            return minTrf;
        }
    }
}