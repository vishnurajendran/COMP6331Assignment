using System.Collections;
using Level;
using UnityEngine;

namespace AgentControllers
{
    public class HeroController : AgentController
    {
        [SerializeField] 
        private LayerMask _guardMask;

        [SerializeField] private float seekWeight=1;
        [SerializeField] private float evadeGuardWeight=1;
        [SerializeField] private float _targettedMultipler=10;
        
        private Transform _baseTrf;
        private Prisoner _prisoner;
        private bool _isTargetByGuard;

        private Coroutine _evadeDelayRoutine;
        
        protected override void Start()
        {
            base.Start();
            SetTarget(LevelManager.Instance.GetNextTarget());
        }
        
        private void Update()
        {
            if(!_target)
                return;
            
            Vector3 move = Vector3.zero;

            //Seek the target
            move += SeekTarget(_target) * seekWeight;
            
            move += StayAwayFromGuards() * (evadeGuardWeight * (_isTargetByGuard?_targettedMultipler:1));
            
            //Clamp the move direction
            move = move.normalized;
            _agent.Move(move, _params.AgentSpeed, Time.deltaTime);
            
            var lookDir = move;
            lookDir.y = 0;
            _agent.LookAt(lookDir, _params.LookSpeed, Time.deltaTime);
            
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

        public void MarkChasedByGuard(bool marked)
        {
            if (marked)
            {
                if(_evadeDelayRoutine != null)
                    StopCoroutine(_evadeDelayRoutine);
                _isTargetByGuard = true;
            }
            else
            {
                if(_evadeDelayRoutine != null)
                    StopCoroutine(_evadeDelayRoutine);
                
                _evadeDelayRoutine = StartCoroutine(DelayEvadeStop(1));
            }
        }

        IEnumerator DelayEvadeStop(float delay)
        {
            yield return new WaitForSeconds(delay);
            _isTargetByGuard = false;
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