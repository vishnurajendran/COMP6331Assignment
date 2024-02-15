using Unity.VisualScripting;
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
        [SerializeField] private GuardAgentParams _params;

        protected override AgentParams Params => _params;

        // we will use this to reference any hero calls.
        private HeroController _currHeroTarget = null;
        
        //faster to check on transform, than use a get-component.
        private Transform _currHeroTargetTransform = null;
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
            Vector3 basePos = transform.position;
            if (_prisoner)
                basePos = _prisoner.transform.position;

            Vector3 delta = Random.insideUnitCircle * _params.WanderRadius;
            var newPos = new Vector3(basePos.x + delta.x, basePos.y, basePos.z + delta.y);
            return newPos;
        }

        private void Update()
        {
            if (_prisoner == null && wanderPos.Equals(Vector3.zero))
                wanderPos = GetNextWanderPosition();
            
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
            _agent.Move(dir, _params.AgentSpeed,_params.LookSpeed, Time.deltaTime);
        }
        
        private void DoWander()
        {
            var dir = (wanderPos - transform.position).normalized;
            _agent.Move(dir, _params.AgentSpeed,_params.LookSpeed, Time.deltaTime);
            
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

        private bool IsInCaptureRange(Transform target)
        {
            return Vector3.Distance(transform.position, target.position) <= _params.CaptureRadius;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            TryCheckIfCanTarget(other);
        }

        private void TryCheckIfCanTarget(Collider collider)
        {
            if (_currHeroTarget != null)
                return;

            var hero = collider.GetComponent<HeroController>();
            if (hero == null)
                return;
            
            if (!IsVisible(collider.transform))
                return;
            
            
            _currHeroTarget = hero;
            _currHeroTargetTransform = hero.transform;
            
            hero.MarkChasedByGuard(true);
            if(IsInCaptureRange(hero.transform))
            {
                Destroy(hero.gameObject);
            }
        }
        
        private void OnTriggerStay(Collider other)
        {
            if (_currHeroTarget == null)
                TryCheckIfCanTarget(other);
            else
            {
                if (_currHeroTargetTransform != other.transform)
                    return;
                
                if(IsInCaptureRange(_currHeroTargetTransform.transform))
                {
                    Destroy(_currHeroTargetTransform.gameObject);
                }
            }
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
                _currHeroTargetTransform = null;
            }
        }
        
        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            if (_params != null)
            {

#if UNITY_EDITOR
                Color vision = new Color(0.55f, 0.55f, 0, 1);
                vision.a = 0.5f;
                Handles.color = vision;
                var pos = transform.position;
                var forward = transform.forward;
                Handles.DrawSolidArc(pos, Vector3.up, forward, _params.VisionAngle / 2, _params.VisionRange);
                Handles.DrawSolidArc(pos, Vector3.up, forward, -_params.VisionAngle / 2, _params.VisionRange);

                Color capture = Color.magenta;
                capture.a = 0.35f;
                Handles.color = capture;
                Handles.DrawSolidDisc(transform.position, Vector3.up, _params.CaptureRadius);
#endif

                if (_currHeroTarget)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(transform.position, _currHeroTarget.transform.position);
                }

                if (wanderPos != Vector3.zero)
                {
                    var destColor = Color.magenta;
                    destColor.a = 0.25f;
                    Gizmos.color = destColor;
                    
                    Gizmos.DrawLine(transform.position, wanderPos);
                    Gizmos.DrawSphere(wanderPos, 0.35f);
                }
            }
        }
    }
}