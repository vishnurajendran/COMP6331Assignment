using System;
using System.Collections;
using System.Collections.Generic;
using AgentControllers.Strategies;
using Level;

using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AgentControllers
{
    public class HeroController : AgentController
    {
        protected override AgentParams Params => _params;
        [SerializeField] private AgentParams _params;
        [SerializeField] private LayerMask _guardMask;
        [FormerlySerializedAs("seekWeight")] [SerializeField] private float _seekWeight=1;
        [FormerlySerializedAs("evadeGuardWeight")] [SerializeField] private float _evadeGuardWeight=10;
        [SerializeField] private float _aggressiveModeWeight = 3;
        
        [SerializeField, Tooltip("Threshold of failed attempts before strategy switches to aggressive")] 
        private int _strategySwitchThresh = 5;
        
        [SerializeField, Tooltip("Aggressive Mode, Lure speed modifier thresh")] 
        private float _minLureSpeedReductionModifier = 1;
        
        [SerializeField, Tooltip("Aggressive Mode, Lure speed modifier thresh")] 
        private float _maxLureSpeedReductionModifier = 1;

        [SerializeField, Tooltip("Min Safe Distance from a Guard")] 
        private int _minSafeDistFromGuard = 4;
        
        private float _modifier;
        private int _totalMarked=0;
        private int _totalEscaped = 0;
        private List<Guard> _guardsTargettingMe;
        private Prisoner _prisoner;
        private Transform _prisonerToGoTo;
        private bool _isTargetByGuard;
        private Coroutine _evadeDelayRoutine;
        private IHeroStrategy _strategy;

        private float MoveSpeed => _params.AgentSpeed + _modifier;
        
        public float SeekWeight => _seekWeight;
        public float EvadeGuardWeight => _evadeGuardWeight;
        public float AggressiveWeight => _aggressiveModeWeight;
        public float MinSafeDistFromGuard => _minSafeDistFromGuard;
        public bool IsTargetted => _isTargetByGuard;
        public Transform Target => _target;
        public Prisoner Prisoner => _prisoner;
        
        public float MaxLureSpeedReductionModifier => _maxLureSpeedReductionModifier;
        public float MinLureSpeedReductionModifier => _minLureSpeedReductionModifier;

        public Transform ClosestBase => LevelManager.Instance.GetClosestBase(transform.position);
        
        public void SetSpeedModifier(float modifierVal)
        {
            _modifier = modifierVal;
        }
        
        protected override void Start()
        {
            base.Start();
            LevelManager.Instance?.AddHero();
            SwitchStrategy(Strategy.Default);
            _guardsTargettingMe = new List<Guard>();
            _prisonerToGoTo = LevelManager.Instance.GetNextTarget(transform.position);
            SetTarget(_prisonerToGoTo);
        }

        public void SwitchStrategy(Strategy strategy)
        {
            if(_strategy != null && _strategy.CurrentStrategy == strategy)
                return;
            
            _strategy = HeroStratergies.Get(strategy);
            _strategy.Initialise(this);
            Debug.Log($"Hero Strategy: <color=cyan>{strategy}</color>");
        }
        
        public void SetPrisoner(Prisoner prisoner)
        {
            _prisoner = prisoner;
        }
        
        private void Update()
        {
            if (Target && !ReachedTarget())
            {
                var moveDir = _strategy.GetMove();
                _agent.Move(moveDir, MoveSpeed, _params.LookSpeed, Time.deltaTime);
            }

            // let strategy decide what needs to be done.
            _strategy.Decide();
        }
        
        public Guard[] QueryGuards()
        {
            var cols = Physics.OverlapSphere(transform.position, _params.AreaDetectorSize, _guardMask);
            Guard[] guards = new Guard[cols.Length];
            for (int i = 0; i < cols.Length; i++)
            {
                guards[i] = cols[i].GetComponent<Guard>();
            }

            return guards;
        }
        
        public void MarkChasedByGuard(bool marked, Guard guardRef)
        {
            if (marked)
            {
                if(_evadeDelayRoutine != null)
                    StopCoroutine(_evadeDelayRoutine);
                _guardsTargettingMe.Add(guardRef);
                _isTargetByGuard = true;
                SetTarget(ClosestBase);
                _totalMarked += 1;
                if(_totalMarked >= _strategySwitchThresh)
                    SwitchStrategy(Strategy.Aggressive);
            }
            else
            {
                _guardsTargettingMe.Remove(guardRef);
                _totalEscaped += 1;
                if (_guardsTargettingMe.Count <= 0)
                {
                    if(_evadeDelayRoutine != null)
                        StopCoroutine(_evadeDelayRoutine);
                    _evadeDelayRoutine = StartCoroutine(DelayEvadeStop(1));
                }
            }
        }

        IEnumerator DelayEvadeStop(float delay)
        {
            yield return new WaitForSeconds(delay);
            _isTargetByGuard = false;
            SetTarget(_prisonerToGoTo);
               
        }
        
        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            Color status = Color.green;
            if (_isTargetByGuard)
            {
                status = Color.red;
            }

            Gizmos.color = status;
            Gizmos.DrawCube(transform.position + new Vector3(0,2.5f,0),Vector3.one * 0.5f);
            if (_target != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, _target.position);
            }

        }

        public void ResetThresholds()
        {
            _totalMarked = 0;
            _totalEscaped = 0;
        }
        
        private void OnDestroy()
        {
            if(_evadeDelayRoutine != null)
                StopCoroutine(_evadeDelayRoutine);
            
            // we failed, give em back.
            if (Prisoner && Prisoner.Target == transform && !Prisoner.ReachedTarget())
            {
                LevelManager.Instance.GiveBackPrisoner(Prisoner.transform);
            }
            
            LevelManager.Instance?.HeroKilled();
        }
    }
}