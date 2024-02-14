using UnityEngine;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AgentControllers
{
    public class Guard : AgentController
    {
        [SerializeField] private SphereCollider visionRangeCollider;
        private HeroController _currHeroTarget = null;
        private Prisoner _prisoner;
        private Vector3 wanderPos;

        protected override void Start()
        {
            base.Start();
            var rb = this.gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            
            visionRangeCollider.radius = _params.VisionRange;
            visionRangeCollider.isTrigger = true;
        }

        public void SetPrisioner(Prisoner prisoner)
        {
            _prisoner = prisoner;
            wanderPos = GetNextWanderPosition();
        }

        private Vector3 GetNextWanderPosition()
        {
            if (_prisoner == null && wanderPos.Equals(Vector3.zero))
                wanderPos = GetNextWanderPosition();
            
            Vector3 basePos = transform.position;
            if (_prisoner)
                basePos = _prisoner.transform.position;

            Vector3 delta = Random.insideUnitCircle * _params.WanderRadius;
            var newPos = new Vector3(basePos.x + delta.x, basePos.y, basePos.z + delta.y);
            return newPos;
        }

        private void Update()
        {
            if (_currHeroTarget)
            {
                ChaseHero();
            }
            else
            {
                DoWander();
            }
        }

        private void ChaseHero()
        {
            Debug.Log("Chasing Hero");
            var dir = (_currHeroTarget.transform.position - transform.position).normalized;
            _agent.Move(dir, _params.AgentSpeed, Time.deltaTime);
            
            var lookDir = dir;
            lookDir.y = 0;
            _agent.LookAt(lookDir, _params.LookSpeed, Time.deltaTime);
        }
        
        private void DoWander()
        {
            var dir = (wanderPos - transform.position).normalized;
            _agent.Move(dir, _params.AgentSpeed, Time.deltaTime);
            
            var lookDir = dir;
            lookDir.y = 0;
            _agent.LookAt(lookDir, _params.LookSpeed, Time.deltaTime);

            if (ReachedTargetPosition(wanderPos))
            {
                wanderPos = GetNextWanderPosition();
            }
        }
        
        private bool IsVisible(Transform target)
        {
            var dir = target.position - transform.position;
            var angle = Vector3.SignedAngle(dir, transform.forward, transform.forward);
            return (angle >= -_params.VisionAngle / 2) && (angle <= _params.VisionAngle / 2);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_currHeroTarget != null)
                return;

            var hero = other.GetComponent<HeroController>();
            if (hero == null)
                return;
            
            if (!IsVisible(other.transform))
                return;
            
            
            _currHeroTarget = hero;
            hero.MarkChasedByGuard(true);
        }

        private void OnTriggerStay(Collider other)
        {
            if (_currHeroTarget != null)
                return;

            var hero = other.GetComponent<HeroController>();
            if (hero == null)
                return;
            
            if (!IsVisible(other.transform))
                return;
            
            _currHeroTarget = hero;
            hero.MarkChasedByGuard(true);
        }

        private void OnTriggerExit(Collider other)
        {
            var hero = other.GetComponent<HeroController>();
            if (hero == null)
                return;

            if (_currHeroTarget == hero)
            { 
                _currHeroTarget.MarkChasedByGuard(false);
                _currHeroTarget = null;
            }
        }
        
        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            if (_params != null)
            {

#if UNITY_EDITOR
                Color vision = Color.cyan;
                vision.a = 0.5f;
                Handles.color = vision;
                var pos = transform.position;
                var forward = transform.forward;
                Handles.DrawSolidArc(pos, Vector3.up, forward, _params.VisionAngle / 2, _params.VisionRange);
                Handles.DrawSolidArc(pos, Vector3.up, forward, -_params.VisionAngle / 2, _params.VisionRange);
#endif

                if (_currHeroTarget)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(transform.position, _currHeroTarget.transform.position);
                }

                if (wanderPos != Vector3.zero)
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawLine(transform.position, wanderPos);
                    Gizmos.DrawSphere(wanderPos, 0.35f);
                }
            }
        }
    }
}