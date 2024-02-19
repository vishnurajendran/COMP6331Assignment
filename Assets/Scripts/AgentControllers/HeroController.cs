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
        protected override BaseAgentParams Params => _params;
        public StandardAIParams MyParams => _params;
       
        [SerializeField] private Transform _angryBillboard;
        [SerializeField] private StandardAIParams _params;
        [FormerlySerializedAs("_guardMask")] [SerializeField] private LayerMask _detectionMask;
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

        [SerializeField, Tooltip("Min Safe Distance from a Guard")]
        private LineRenderer _line;
        
        private float _modifier;
        private int _totalMarked=0;
        private int _totalEscaped = 0;
        private List<Transform> _guardsTargettingMe;
        private Prisoner _prisoner;
        private Transform _prisonerToGoTo;
        private bool _isTargetByGuard;
        private Coroutine _evadeDelayRoutine;
        private IHeroStrategy _strategy;

        private Vector3 _startPosition;
        
        private float MoveSpeed => _params.AgentSpeed + _modifier;
        
        public float SeekWeight => _seekWeight;
        public float EvadeGuardWeight => _evadeGuardWeight;
        public float AggressiveWeight => _aggressiveModeWeight;
        public float MinSafeDistFromGuard => _minSafeDistFromGuard;
        public bool IsTargetted => _isTargetByGuard;
        public Transform Target => _target;
        public Prisoner Prisoner => _prisoner;
        
        public bool CanResetAggro { get;private set; }

        private bool _surenderPrisoner = true;
        
        public bool Angry
        {
            get => _angryBillboard.gameObject.activeSelf;
            set => _angryBillboard.gameObject.SetActive(value);
        }

        public float MaxLureSpeedReductionModifier => _maxLureSpeedReductionModifier;
        public float MinLureSpeedReductionModifier => _minLureSpeedReductionModifier;

        public Transform ClosestBase => LevelManager.Instance.GetClosestBase(transform.position);

        public void AllowAggroReset(bool allow)
        {
            CanResetAggro = allow;
        }
        
        public void SetSpeedModifier(float modifierVal)
        {
            _modifier = modifierVal;
        }
        
        protected override void Start()
        {
            base.Start();
            _startPosition = transform.position;
            LevelManager.Instance?.AddHero();
            SwitchStrategy(Strategy.Default);
            _guardsTargettingMe = new List<Transform>();
            SetPrisonerToGoTo(LevelManager.Instance.GetNextTarget(transform.position));
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
        
        public void SetPrisonerToGoTo(Transform prisoner)
        {
            _prisonerToGoTo = prisoner;
            SetTarget(_prisonerToGoTo);
        }
        
        private void Update()
        {
            if (Prisoner)
            {
                _line.positionCount = 2;
                _line.SetPosition(0, transform.position + new Vector3(0,1,0));
                _line.SetPosition(1, Prisoner.transform.position+ new Vector3(0,1,0));
            }
            else
            {
                _line.positionCount = 0;
            }
            
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
            var cols = Physics.OverlapSphere(transform.position, _params.AreaDetectorSize, _detectionMask);
            Guard[] guards = new Guard[cols.Length];
            for (int i = 0; i < cols.Length; i++)
            {
                guards[i] = cols[i].GetComponent<Guard>();
            }

            return guards;
        }
        
        public Collider[] QueryColliders()
        {
            return Physics.OverlapSphere(transform.position, _params.AreaDetectorSize, _detectionMask);;
        }
        
        public void MarkChasedByGuard(bool marked, Transform guardRef)
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
                    if(!this)
                        return;
                    _evadeDelayRoutine = StartCoroutine(DelayEvadeStop(1));
                }
            }
        }

        public void ResetThresholds()
        {
            _totalMarked = 0;
            _totalEscaped = 0;
        }
        
        public void CaughtByPlayer(bool killed=false)
        {
            if (killed)
            {
                Debug.Log("Hero Killed!!");
                _surenderPrisoner = false;
                Destroy(this.gameObject);
                return;
            }
            
            Debug.Log("Hero Caught!!");
            UIManager.Instance?.IncrementScore(-10);
            _agent.Teleport(_startPosition);
            ResetThresholds();
            SwitchStrategy(Strategy.Default);
            
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
                foreach (var _guard in _guardsTargettingMe)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(transform.position, _guard.transform.position);
                }
            }

            Gizmos.color = status;
            Gizmos.DrawCube(transform.position + new Vector3(0,2.5f,0),Vector3.one * 0.5f);
            if (_target != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, _target.position);
            }

#if UNITY_EDITOR
            Color vision = new Color(0.55f, 0.55f, 0, 1);
            vision.a = 0.5f;
            Handles.color = vision;
            var pos = transform.position;
            var forward = transform.forward;
            Handles.DrawSolidArc(pos, Vector3.up, forward, _params.VisionAngle / 2, _params.VisionRange);
            Handles.DrawSolidArc(pos, Vector3.up, forward, -_params.VisionAngle / 2, _params.VisionRange);
#endif
            
        }

        private void OnDestroy()
        {
            if(_evadeDelayRoutine != null)
                StopCoroutine(_evadeDelayRoutine);
            
            // we failed, give em back.
            if (Prisoner && _surenderPrisoner)
            {
                LevelManager.Instance.GiveBackPrisoner(Prisoner.transform);
            }
            else if (!Prisoner && _prisonerToGoTo)
            {
                LevelManager.Instance.GiveBackPrisoner(_prisonerToGoTo.transform);
            }
            
            LevelManager.Instance?.HeroKilled();
        }
        
    }
}