using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Agents
{
    public class AgentObstacleAvoidancePlugin : MonoBehaviour
    {
        [SerializeField]
        private bool enableCollisionAvoidance = true;
        
        [SerializeField] private LayerMask _obstacleAvoidanceMask;
        [SerializeField] private Transform _obsAvoidRayOrigin;
        [SerializeField] private float _obsDetectionAnglePerSide = 90;
        [SerializeField] private int _rayResolution=50;
        [SerializeField] private float _maxObsDetectDist = 3f;
        [SerializeField] private float _correctionThreshold = 10f;

        private Vector3 _correction;
        public Vector3 Correction => _correction;

        private void FixedUpdate()   
        {
            _correction = Vector3.zero;
            if (!enableCollisionAvoidance)
                return;
            
            for (int i = 0; i < _rayResolution; i++)
            {
                var rot = this.transform.rotation;
                var rotMod = Quaternion.AngleAxis(i/((float)_rayResolution-1) * _obsDetectionAnglePerSide * 2 - _obsDetectionAnglePerSide,transform.up);
                var dir = rot * rotMod * Vector3.forward;
                RaycastHit hit;
                var ray = new Ray(_obsAvoidRayOrigin.position, dir);
                if (Physics.Raycast(ray, out hit, _maxObsDetectDist, _obstacleAvoidanceMask))
                {
                    Debug.DrawLine(ray.origin, hit.point, Color.red);
                    _correction -= dir * ((1.0f / _rayResolution) * Time.deltaTime);
                }
                else
                {
                    Debug.DrawRay(ray.origin, ray.direction * _maxObsDetectDist, Color.green);
                    _correction += dir * ((1.0f / _rayResolution) * Time.deltaTime);
                }
            }

            _correction *= _correctionThreshold;
            _correction = Vector3.ClampMagnitude(_correction, 1);
        }

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            Handles.color = Color.black;
            Handles.Label(transform.position + new Vector3(0,2f,0), $"Avoidance: {Correction}");            
#endif
        }
    }
}

