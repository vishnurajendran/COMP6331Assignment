using System;
using System.Collections.Generic;
using AgentControllers;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Level
{
    public class LevelManager : MonoBehaviour
    {
        private static LevelManager _instance;

        [SerializeField] private string randomGuardToSpawn = "Guard";
        
        private List<Transform> _targets;
        private List<Transform> _gaurds;
        private List<Transform> _bases;

        private List<Transform> _prisoners;

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
                    _instance = FindObjectOfType<LevelManager>();

                return _instance;
            }
        }

        public int GuardsInLevel => _gaurds.Count;
        
        private void Awake()
        {
            _targets = new List<Transform>();
            _gaurds = new List<Transform>();
            _prisoners = new List<Transform>();
            foreach (var target in FindObjectsOfType<Target>())
            {
                _targets.Add(target.transform);
                _prisoners.Add(target.transform);
            }

            _totalPrisoners = _targets.Count;
        }

        public void GiveBackPrisoner(Transform trf)
        {
            Debug.Log("Prisoner given back!");
            _targets.Add(trf);
        }

        public void PrisonerSaved(Transform prisoner)
        {
            _prisoners.Remove(prisoner);
            UIManager.Instance?.PrisonerSaved();
            UIManager.Instance?.IncrementScore(25);
            _totalSaved += 1;
            if (_totalSaved >= _totalPrisoners)
            {
                OnGameOver(true);
            }
        }

        public void MarkPrisonerNotAvaialble(Transform prisoner)
        {
            _prisoners.Remove(prisoner);
        }
        
        public Transform GetNextTarget(Vector3 referencePosition)
        {
            if (_targets.Count <= 0)
                return null;
            
            Transform minTrf = null;
            float minDist = Mathf.Infinity;
            foreach (var target in _targets)
            {
                var dist = Vector3.Distance(referencePosition, target.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    minTrf = target;
                }
            }

            _targets.Remove(minTrf);
            Debug.Log("Targets in Queue: " + _targets.Count);
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
            UIManager.Instance?.HeroKilled();
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

        public void SpawnGuardRandomly()
        {
            if(_prisoners.Count <=0 )
                return;

            var randPrisoner = _prisoners[Random.Range(0, _prisoners.Count)];
            var randCirclePos = Random.insideUnitCircle * 5;
            var randPos = new Vector3(randPrisoner.position.x + randCirclePos.x, 0,
                randPrisoner.position.z + randCirclePos.y);
            var go = Instantiate(Resources.Load(randomGuardToSpawn), randPos, Quaternion.identity);
            var guard = go.GetComponent<Guard>();
            guard.SetPrisioner(randPrisoner.GetComponent<Prisoner>());
        }
        
        private void OnGameOver(bool win)
        {
            _gameOver = true;
            foreach (var guard in _gaurds)
            {
                Destroy(guard.gameObject);
            }
            UIManager.Instance?.GameOver(win);
        }
    }
}