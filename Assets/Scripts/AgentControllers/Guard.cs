using System;
using System.Collections;
using System.Collections.Generic;
using Level;
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
        private const string HeroTag = "Hero";

        [SerializeField] private LayerMask _guardMask;
        [SerializeField] private SphereCollider visionRangeCollider;
        [SerializeField] private GuardAgentParams _params;

        private HashSet<Collider> _collidersInVicinity;

        protected override AgentParams Params => _params;

        // we will use this to reference any hero calls.
        private HeroController _currHeroTarget = null;
        
        //faster to check on transform, than use a get-component.
        private Transform _currHeroTargetTransform = null;
        private Prisoner _prisoner;
        private Vector3 wanderPos;

        private Coroutine delayedStop;

        public float VisionRange => _params.VisionRange;
        
        protected override void Start()
        {
            base.Start();
            LevelManager.Instance.RegisterGuard(transform);
            var rb = this.gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            
            visionRangeCollider.radius = _params.VisionRange;
            visionRangeCollider.isTrigger = true;
            _collidersInVicinity = new HashSet<Collider>();
            StartCoroutine(CheckForVisibility());
        }

        private IEnumerator CheckForVisibility()
        {
            while (true)
            {
                yield return new WaitForFixedUpdate();
                if (_collidersInVicinity.Count <= 0)
                    continue;

                // copy our data to avoid any modifications mid check
                var copy = new Collider[_collidersInVicinity.Count];
                _collidersInVicinity.CopyTo(copy);
                
                foreach (var other in copy)
                {
                    if(other == null)
                        continue;
                    
                    if (!_currHeroTarget)
                    {
                        TryCheckIfCanTarget(other);
                    }
                    
                    if (_currHeroTargetTransform != other.transform)
                        continue;
                        
                    if (IsInCaptureRange(_currHeroTargetTransform))
                    {
                        _collidersInVicinity.Remove(other);
                        Destroy(_currHeroTargetTransform.gameObject);
                    }
                }
            }
        }
        
        public void SetPrisioner(Prisoner prisoner)
        {
            _prisoner = prisoner;
            wanderPos = GetNextWanderPosition();
        }

        private Vector3 GetNextWanderPosition()
        {
            var basePos = transform.position;
            if (_prisoner)
                basePos = _prisoner.transform.position;
            
            var randAngle = Random.Range(0, Mathf.PI * 2f);
            var dist = Random.Range(_params.WanderMinRadius, _params.WanderMaxRadius);
            var delta = new Vector2(dist * Mathf.Cos(randAngle), dist * Mathf.Sin(randAngle));
            
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
            var dir = (_currHeroTarget.transform.position - transform.position).normalized;
            dir += StayAwayFrom(Physics.OverlapSphere(transform.position, _params.AreaDetectorSize, _guardMask));
            _agent.Move(dir.normalized, _params.AgentSpeed,_params.LookSpeed, Time.deltaTime);
        }
        
        private void DoWander()
        {
            var dir = (wanderPos - transform.position).normalized;
            dir += StayAwayFrom(Physics.OverlapSphere(transform.position, _params.AreaDetectorSize, _guardMask));
            _agent.Move(dir.normalized, _params.AgentSpeed,_params.LookSpeed, Time.deltaTime);
            
            if (ReachedTargetPosition(wanderPos))
            {
                wanderPos = GetNextWanderPosition();
            }
        }
        
        private Vector3 StayAwayFrom(Collider[] gaurds)
        {
            Vector3 move = Vector3.zero;
            foreach (var col in gaurds)
            {
                var away = transform.position - col.transform.position;
                move += away.normalized;
            }

            return move.normalized;
        }
        
        private bool IsVisible(Transform target)
        {
            var dir = target.position - transform.position;
            var angle = Vector3.SignedAngle(dir, transform.forward, transform.forward);
            return (angle >= -_params.VisionAngle / 2) && (angle <= _params.VisionAngle / 2);
        }

        private bool IsInCaptureRange(Transform target)
        {
            var dist = Vector3.Distance(transform.position, target.position);
            return dist <= _params.CaptureRadius;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            TryCheckIfCanTarget(other);
        }

        private void TryCheckIfCanTarget(Collider collider)
        {
            
            if (_currHeroTarget != null)
                return;

            if(collider == null)
                return;
            
            if (!collider.CompareTag(HeroTag))
                return;
            
            var hero = collider.GetComponent<HeroController>();
            if (hero == null)
                return;

            _collidersInVicinity.Add(collider);
            if (!IsVisible(collider.transform))
            {
                return;
            }
            
            _currHeroTarget = hero;
            _currHeroTargetTransform = hero.transform;
            
            if(delayedStop != null)
                StopCoroutine(delayedStop);
            
            hero.MarkChasedByGuard(true, this);
            if(IsInCaptureRange(hero.transform))
            {
                Destroy(hero.gameObject);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if(!other.CompareTag(HeroTag))
                return;
            
            var hero = other.GetComponent<HeroController>();
            if (hero == null)
                return;

            _collidersInVicinity.Remove(other);
            if (_currHeroTarget == hero)
            { 
               if(delayedStop != null)
                   StopCoroutine(delayedStop);

               delayedStop = StartCoroutine(DelayedChaseStop(1f));
            }
        }

        IEnumerator DelayedChaseStop(float delay)
        {
            yield return new WaitForSeconds(delay);
            _currHeroTarget.MarkChasedByGuard(false, this);
            _currHeroTarget = null;
            _currHeroTargetTransform = null;
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

        private void OnDestroy()
        {
            if(_currHeroTarget)
                _currHeroTarget.MarkChasedByGuard(false, this);
            
            if(LevelManager.Instance != null)
                LevelManager.Instance.DeRegisterGuard(transform);
        }
    }
}