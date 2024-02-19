using System;
using System.Collections.Generic;
using AgentControllers;
using UnityEngine;

namespace Level
{
    public class LevelManager : MonoBehaviour
    {
        private static LevelManager _instance;
        private List<Transform> _targets;
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
            _targets = new List<Transform>();
            _gaurds = new List<Transform>();
            foreach (var target in GameObject.FindObjectsOfType<Target>())
            {
                _targets.Add(target.transform);
            }

            _totalPrisoners = _targets.Count;
        }

        public void GiveBackPrisoner(Transform trf)
        {
            _targets.Add(trf);
        }

        public void PrisonerSaved()
        {
            UIManager.Instance?.IncrementScore(25);
            _totalSaved += 1;
            if (_totalSaved >= _totalPrisoners)
            {
                OnGameOver(true);
            }
        }
        
        public Transform GetNextTarget(Vector3 referencePosition)
        {
            if (_targets.Count <= 0)
                return null;
            
            Transform minTrf = null;
            float minDist = Mathf.Infinity;
            foreach (var baseCandidate in _targets)
            {
                var dist = Vector3.Distance(referencePosition, baseCandidate.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    minTrf = baseCandidate;
                }
            }

            _targets.Remove(minTrf);
            return minTrf;
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
            if (_herosInLevel <= 0)
            {
                OnGameOver(false);
            }
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

        private void OnGameOver(bool win)
        {
            _gameOver = true;
            foreach (var guard in _gaurds)
            {
                Destroy(guard.gameObject);
            }
            UIManager.Instance?.GameOver();
        }
    }
}