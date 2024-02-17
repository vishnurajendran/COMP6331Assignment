using UnityEngine;

namespace AgentControllers.Strategies
{
    public class DefaultStrategy : HeroStrategy
    {
        public override Strategy CurrentStrategy => Strategy.Default;

        public override Vector3 GetMove()
        {
            if(!_controller.Target)
                return Vector3.zero;
            
            Vector3 move = Vector3.zero;

            //Seek the target
            move += _controller.SeekTarget(_controller.Target) * _controller.SeekWeight;
            move += StayAwayFrom(_controller.QueryGuards()) * _controller.EvadeGuardWeight;
            return move.normalized;
        }
        
        public override void Decide()
        {
            if (!_controller.ReachedTarget())
            {
                return;
            }

            if (_controller.IsTargetted)
                return;
            
            if (_controller.Target.CompareTag("Prisoner"))
            {
                _controller.SetPrisoner(_controller.Target.GetComponent<Prisoner>());
                if (_controller.Prisoner)
                {
                    _controller.Prisoner.SetTarget(_controller.transform);
                    _controller.SetTarget(_controller.Base);
                }

                return;
            }

            if (_controller.Target != _controller.Base)
                return;

            if (_controller.Prisoner && _controller.Prisoner.ReachedTarget())
            {
                GameObject.Destroy(_controller.Prisoner.gameObject);
                _controller.SetTarget(null);
            }
        }
        
        private Vector3 StayAwayFrom(Guard[] gaurds)
        {
            Vector3 move = Vector3.zero;
            foreach (var col in gaurds)
            {
                var away = _controller.transform.position - col.transform.position;
                move += away.normalized;
            }

            return Vector3.ClampMagnitude(move,1);
        }
    }
}