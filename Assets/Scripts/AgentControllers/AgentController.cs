using Agents;
using Level;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AgentControllers
{
    [RequireComponent(typeof(Agent))]
    public abstract class AgentController : MonoBehaviour
    {
        [SerializeField] 
        protected AgentParams _params;
        [SerializeField] 
        protected Transform _target;
        
        protected Agent _agent;

        protected virtual void Start()
        {
            _agent = GetComponent<Agent>();
        }

        public void SetTarget(Transform target)
        {
            _target = target;
        }

        public bool ReachedTarget()
        {
            if (!_target) return false;
            return Vector3.Distance(_target.position, transform.position) <= _params.StoppingDistance;
        }
        
        public bool ReachedCustomTarget(Transform target)
        {
            if (!target) return false;
            return Vector3.Distance(target.position, transform.position) <= _params.StoppingDistance;
        }
        
        public bool ReachedTargetPosition(Vector3 position)
        {
            return Vector3.Distance(position, transform.position) <= _params.StoppingDistance;
        }
        
        public Vector3 SeekTarget(Transform target)
        {
            if (!_target)
            {
                return Vector3.zero;
            }
            return (_target.position - transform.position).normalized;
        }
        
        protected virtual void OnDrawGizmos()
        {
            if (_params != null)
            {
                Color detectorColor = Color.yellow;
                detectorColor.a = 0.25f;
                Gizmos.color = detectorColor;
                Gizmos.DrawSphere(transform.position, _params.AreaDetectorSize);
                
                #if UNITY_EDITOR
                Color stopColor = Color.red;
                stopColor.a = 0.25f;
                Handles.color = stopColor;
                Handles.DrawSolidDisc(transform.position, Vector3.up, _params.StoppingDistance );
                #endif
            }
        }
    }
}