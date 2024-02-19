using System;
using AgentControllers;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Level
{
    public class GuardSpawner : MonoBehaviour
    {
        [SerializeField] private string _prefabName="Guard";
        [SerializeField] 
        private float prisonerDetectionRadius=2;

        [SerializeField] private LayerMask detectionLayer;

        private GameObject _guardPrefab;

        private void Awake()
        {
            if (_guardPrefab == null)
                _guardPrefab = Resources.Load<GameObject>(_prefabName);
        }

        private void Start()
        {
            var colliders = Physics.OverlapSphere(transform.position, prisonerDetectionRadius, detectionLayer);
            Prisoner prisoner=null;
            var guard = Instantiate(_guardPrefab, transform.position, Quaternion.identity).GetComponent<Guard>();
            
            foreach (var col in colliders)
            {
                prisoner = col.GetComponent<Prisoner>();
                if (prisoner)
                    break;
            }

            if (!prisoner)
            {
                Destroy(this.gameObject);
                return;
            }

            guard.SetPrisioner(prisoner);
            Destroy(this.gameObject);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(transform.position, 0.25f);
            #if UNITY_EDITOR
            var color = Color.black;
            color.a = 0.05f;
            Handles.color = color;
            Handles.DrawSolidDisc(transform.position, Vector3.up, prisonerDetectionRadius);
            #endif
        }
    }
}