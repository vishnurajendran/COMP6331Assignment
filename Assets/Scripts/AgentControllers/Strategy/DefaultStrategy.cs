using System;
using Level;
using Unity.VisualScripting;
using UnityEngine;

namespace AgentControllers.Strategies
{
    public class DefaultStrategy : HeroStrategy
    {
        
        public override Strategy CurrentStrategy => Strategy.Default;
        private Collider[] _collidersInVicinity = Array.Empty<Collider>();

        protected override void OnInit()
        {
            base.OnInit();
            _controller.Angry = false;
        }

        public override Vector3 GetMove()
        {
            if(!_controller.Target)
                return Vector3.zero;
            
            Vector3 move = Vector3.zero;
            
            //Seek the target
            move += _controller.SeekTarget(_controller.Target) * (_controller.SeekWeight * (_controller.IsTargetted?_controller.EvadeGuardWeight:1));
            move += StayAwayFrom(_collidersInVicinity) * _controller.EvadeGuardWeight;
            return move.normalized;
        }
        
        public override void Decide()
        {
            _collidersInVicinity = _controller.QueryColliders();
            CheckIsVisibleToNearbyPlayers(_collidersInVicinity);
            
            if (!_controller.Target && !LevelManager.Instance.GameOver)
            {
                TryGetNextPrisonerTarget();
                return;
            }
            
            if (!_controller.ReachedTarget())
                return;

            if (_controller.IsTargetted)
                return;

            if (_controller.Target.CompareTag("Prisoner"))
            {
                _controller.SetPrisoner(_controller.Target.GetComponent<Prisoner>());
                if (_controller.Prisoner)
                {
                    _controller.Prisoner.SetTarget(_controller.transform);
                    _controller.SetTarget(_controller.ClosestBase);
                }

                return;
            }

            if (!_controller.Target.CompareTag("Base"))
                return;
            
            if (_controller.Prisoner && _controller.Prisoner.ReachedTarget())
            {
                LevelManager.Instance.PrisonerSaved(_controller.Prisoner.transform);
                _controller.SetTarget(null);
                GameObject.Destroy(_controller.Prisoner.gameObject);
                
            }
        }
 
        private void TryGetNextPrisonerTarget()
        {
            var prisoner = LevelManager.Instance.GetNextTarget(_controller.transform.position);
            if(!prisoner)
                return;
                    
            Debug.Log( $"{_controller.name} --> Next Prisoner: {prisoner.name}");
            _controller.SetPrisonerToGoTo(prisoner);
        }
        
        private Vector3 StayAwayFrom(Collider[] gaurds)
        {
            Vector3 move = Vector3.zero;
            foreach (var col in gaurds)
            {
                if(col == null)
                    continue;
                
                if (col.CompareTag("Player"))
                    continue;
                
                var away = _controller.transform.position - col.transform.position;
                move += away.normalized;
            }

            return Vector3.ClampMagnitude(move,1);
        }
        
    }
}