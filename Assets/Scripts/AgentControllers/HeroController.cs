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
        
        private Transform _baseTrf;
        private Prisoner _prisoner;
        private bool _isTargetByGuard;
        
        
        protected override void Start()
        {
            base.Start();
            SetTarget(LevelManager.Instance.GetNextTarget());
        }
        
        private void FixedUpdate()
        {
            if(!_target)
                return;

            Vector3 move = Vector3.zero;
            
            //Seek the target
            move += SeekTarget(_target) * seekWeight;
            //Stay Away from the Guard
            move += StayAwayFromGuards() * evadeGuardWeight;
            
            //Clamp the move direction
            move = Vector3.ClampMagnitude(move, 1);
            _agent.Move(move, _params.AgentSpeed);
            
            var lookDir = move * _params.AgentSpeed;
            lookDir.y = 0;
            _agent.LookAt(Vector3.Lerp(transform.forward, lookDir, Time.deltaTime * _params.AgentSpeed));
            
            // move this to a state machine later.
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
            if (ReachedTarget() && _target != _baseTrf)
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
            _isTargetByGuard = marked;
        }
        
        public void SetBase(Transform baseTrf)
        {
            this._baseTrf = baseTrf;
        }
    }
}