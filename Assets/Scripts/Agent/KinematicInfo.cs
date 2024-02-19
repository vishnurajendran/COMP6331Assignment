using UnityEngine;

namespace Agents
{
    public class KinematicInfo : MonoBehaviour
    {
        private Vector3 _prevPosition;
        private float _currDeltaSpeed;
        private Vector3 _movementDir;

        public float DeltaSpeed => _currDeltaSpeed;
        public Vector3 MoveDir => _movementDir;
   
        private void FixedUpdate()
        {
            var currPos = transform.position;
            var deltaPos = currPos - _prevPosition;
            _currDeltaSpeed = deltaPos.magnitude / Time.fixedDeltaTime;
            _movementDir = deltaPos.normalized;

            _prevPosition = currPos;
        }

        public Vector3 GetEstimatedPosition(float deltaTime)
        {
            return transform.position + (MoveDir * (DeltaSpeed));
        }
    }
}

