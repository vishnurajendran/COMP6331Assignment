using System;
using System.Collections.Generic;
using Level;
using UnityEngine;

namespace AgentControllers.Strategies
{
    public class AggressiveStrategy : HeroStrategy
    {
        public override Strategy CurrentStrategy => Strategy.Aggressive;
        private Collider[] _collidersInVicinity = Array.Empty<Collider>();

        private float activeTime;
        
        protected override void OnInit()
        {
            base.OnInit();
            _controller.Angry = true;
            activeTime = 0;
        }
        
        public override Vector3 GetMove()
        {
            if (LevelManager.Instance.GuardsInLevel <= 0)
                return Vector3.zero;

            Vector3 move = Vector3.zero;

            //Seek the target
            move += _controller.SeekTarget(_controller.Target) * _controller.SeekWeight;

            var colliders = _controller.QueryColliders();
            var filtered = Filter(colliders);
            if (_controller.IsTargetted)
            {
                // slow my speed down a notch, to let the AI keep up a bit
                _controller.SetSpeedModifier(GetSpeedModification(filtered));
                move += StayAwayFrom(colliders) * (_controller.EvadeGuardWeight * _controller.AggressiveWeight);
            }
            else
            {
                // use my full speed
                _controller.SetSpeedModifier(0);
                move += MoveTowards(colliders) * _controller.AggressiveWeight;
                move += StayAwayWithMinDist(colliders, _controller.MinSafeDistFromGuard);
            }

            return move.normalized;
        }

        private Guard[] Filter(Collider[] collider)
        {
            var guards = new List<Guard>();
            foreach (var col in collider)
            {
                if (col.CompareTag("Guard"))
                {
                    guards.Add(col.GetComponent<Guard>());
                }
            }

            return guards.ToArray();
        }
        
        private float GetSpeedModification(Guard[] guards)
        {
            if (guards.Length <= 0)
                return 0;
            
            var avgDist = 0f;
            for (int i = 0; i < guards.Length; i++)
            {
                avgDist += Vector3.Distance(_controller.transform.position, guards[i].transform.position);
            }
            avgDist /= guards.Length;
            var t = avgDist / guards[0].VisionRange;
            return Mathf.Lerp(_controller.MaxLureSpeedReductionModifier, _controller.MaxLureSpeedReductionModifier, 1 - t);
        } 
        
        public override void Decide()
        {
            _collidersInVicinity = _controller.QueryColliders();
            CheckIsVisibleToNearbyPlayers(_collidersInVicinity);
            if (LevelManager.Instance.GuardsInLevel <= 0)
            {
                _controller.SwitchStrategy(Strategy.Default);
                return;
            }

            if (_controller.CanResetAggro)
            {
                activeTime += Time.deltaTime;
                if (activeTime > 15f)
                {
                    _controller.ResetThresholds();
                    _controller.SwitchStrategy(Strategy.Default);
                }
            }
        }
        
        private Vector3 MoveTowards(Collider[] colliders)
        {
            Vector3 move = Vector3.zero;
            foreach (var col in colliders)
            {
                if (col.CompareTag("Player"))
                    continue;
                
                var guardPos = _controller.transform.position;
                var safePos = guardPos + col.transform.forward;
                var away = safePos - _controller.transform.position;
                // move to its vision
                move += away.normalized;
            }
            return Vector3.ClampMagnitude(move,1);
        }
        
        private Vector3 StayAwayFrom(Collider[] colliders)
        {
            Vector3 move = Vector3.zero;
            foreach (var col in colliders)
            {
                if (col.CompareTag("Player"))
                    continue;
                
                var away = col.transform.position - _controller.transform.position;
                move -= away.normalized;
            }
            return Vector3.ClampMagnitude(move,1);
        }

        private Vector3 StayAwayWithMinDist(Collider[] colliders, float minDist)
        {
            var move = Vector3.zero;
            foreach (var col in colliders)
            {
                if (col.CompareTag("Player"))
                    continue;
                
                var dir = col.transform.position - _controller.transform.position;
                var dist = Vector3.Distance(_controller.transform.position, col.transform.position);
                if (dist < minDist)
                {
                    // stay away from this enemy
                    move -= dir.normalized;
                }
            }
            return Vector3.ClampMagnitude(move,1);
        }
    }
}