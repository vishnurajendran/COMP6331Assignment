using Level;
using UnityEngine;

namespace AgentControllers.Strategies
{
    public class AggressiveStrategy : HeroStrategy
    {
        public override Strategy CurrentStrategy => Strategy.Aggressive;

        public override Vector3 GetMove()
        {
            if (LevelManager.Instance.GuardsInLevel <= 0)
                return Vector3.zero;

            Vector3 move = Vector3.zero;

            //Seek the target
            move += _controller.SeekTarget(_controller.Target) * _controller.SeekWeight;

            var guards = _controller.QueryGuards();
            if (_controller.IsTargetted)
            {
                // slow my speed down a notch, to let the AI keep up a bit
                _controller.SetSpeedModifier(GetSpeedModification(guards));
                move += StayAwayFrom(guards) * _controller.EvadeGuardWeight;
            }
            else
            {
                // use my full speed
                _controller.SetSpeedModifier(0);
                move += MoveTowards(guards) * _controller.AggressiveWeight;
                move += StayAwayWithMinDist(guards, _controller.MinSafeDistFromGuard);
            }

            return move.normalized;
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
            return Mathf.Lerp(0, _controller.MaxLureSpeedReductionModifier, 1 - t);
        } 
        
        public override void Decide()
        {
            if (LevelManager.Instance.GuardsInLevel <= 0)
            {
                _controller.SwitchStrategy(Strategy.Default);
            }
        }
        
        private Vector3 MoveTowards(Guard[] gaurds)
        {
            Vector3 move = Vector3.zero;
            foreach (var guard in gaurds)
            {
                var guardPos = _controller.transform.position;
                var safePos = guardPos + guard.transform.forward;
                var away = safePos - _controller.transform.position;
                // move to its vision
                move += away.normalized;
            }
            return Vector3.ClampMagnitude(move,1);
        }
        
        private Vector3 StayAwayFrom(Guard[] gaurds)
        {
            Vector3 move = Vector3.zero;
            foreach (var col in gaurds)
            {
                var away = col.transform.position - _controller.transform.position;
                move -= away.normalized;
            }
            return Vector3.ClampMagnitude(move,1);
        }

        private Vector3 StayAwayWithMinDist(Guard[] guards, float minDist)
        {
            var move = Vector3.zero;
            foreach (var guard in guards)
            {
                var dir = guard.transform.position - _controller.transform.position;
                var dist = Vector3.Distance(_controller.transform.position, guard.transform.position);
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