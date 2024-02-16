using System.Collections;
using System.Collections.Generic;
using Level;
using UnityEngine;
using UnityEngine.Serialization;

namespace AgentControllers
{
    public class HeroController : AgentController
    {
        [SerializeField] 
        private AgentParams _params;
        
        [SerializeField] 
        private LayerMask _guardMask;

        [SerializeField] private float seekWeight=1;
        [SerializeField] private float evadeGuardWeight=1;
        [SerializeField] private float _luringWeight=10;

        private List<Guard> _guardsTargettingMe;

        private Transform _baseTrf;
        private Prisoner _prisoner;

        private Transform _prisonerToGoTo;
        
        private bool _isTargetByGuard;

        private Coroutine _evadeDelayRoutine;

        protected override AgentParams Params => _params;

        protected override void Start()
        {
            base.Start();
            _guardsTargettingMe = new List<Guard>();
            _prisonerToGoTo = LevelManager.Instance.GetNextTarget();
            SetTarget(_prisonerToGoTo);
        }
        
        private void Update()
        {
            if(!_target)
                return;
            
            Vector3 move = Vector3.zero;

            //Seek the target
            move += SeekTarget(_target) * seekWeight;
            move += StayAwayFromGuards() * evadeGuardWeight;
            move += LureGuardToBase();
            
            //Clamp the move direction
            move = Vector3.ClampMagnitude(move, 1);
            _agent.Move(move, _params.AgentSpeed,_params.LookSpeed, Time.deltaTime);
            // move this to a state machine later if needed.
            doHeroLogic();
        }
        
        private Vector3 StayAwayFromGuards()
        {
            Vector3 move = Vector3.zero;
            var colliders = Physics.OverlapSphere(transform.position, _params.AreaDetectorSize, _guardMask);
            foreach (var col in colliders)
            {
                var away = transform.position - col.transform.position;
                move += away.normalized;
            }

            return move;
        }
        
        private Vector3 LureGuardToBase()
        {
            if (_isTargetByGuard)
                return Vector3.zero;
            
            // make myself be within the guard's vision cone and
            //lure him to base.

            return Vector3.zero;
        }
        
        private void doHeroLogic()
        {
            if (!_isTargetByGuard && ReachedTarget() && _target != _baseTrf)
            {
                Debug.Log("Reached");
                _prisoner = _target.GetComponent<Prisoner>();
                if (_prisoner)
                {
                    _prisoner.SetTarget(this.transform);
                    _target = _baseTrf;
                }
            }
            else if (ReachedTarget() && _target == _baseTrf && _prisoner && _prisoner.ReachedTarget())
            {
                Destroy(_prisoner.gameObject);
                _target = null;
            }
        }

        public void MarkChasedByGuard(bool marked, Guard guardRef)
        {
            if (marked)
            {
                if(_evadeDelayRoutine != null)
                    StopCoroutine(_evadeDelayRoutine);
                _guardsTargettingMe.Add(guardRef);
                _isTargetByGuard = true;
                SetTarget(_baseTrf);
            }
            else
            {
                _guardsTargettingMe.Remove(guardRef);
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
        
        public void SetBase(Transform baseTrf)
        {
            this._baseTrf = baseTrf;
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
    }
}