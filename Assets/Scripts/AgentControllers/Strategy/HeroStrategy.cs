using UnityEngine;

namespace AgentControllers.Strategies
{
    public abstract class HeroStrategy : IHeroStrategy
    {
        protected HeroController _controller;
        
        public abstract Strategy CurrentStrategy { get; }

        public void Initialise(HeroController controller)
        {
            this._controller = controller;
            OnInit();
        }

        protected virtual void OnInit()
        {
        }

        public abstract Vector3 GetMove();
        
        public abstract void Decide();
        
        protected bool IsVisible(Transform target)
        {
            var dist = Vector3.Distance(_controller.transform.position, target.position);
            if (dist > _controller.MyParams.VisionRange)
                return false;
            
            var dir = target.position - _controller.transform.position;
            var angle = Vector3.SignedAngle(dir, _controller.transform.forward, _controller.transform.forward);
            return (angle >= -_controller.MyParams.VisionAngle / 2) && (angle <= _controller.MyParams.VisionAngle / 2);
        }
        
        protected void CheckIsVisibleToNearbyPlayers(Collider[] colliders)
        {
            foreach (var col in colliders)
            {
                if (col.CompareTag("Player"))
                {
                    if (!IsVisible(col.transform))
                        _controller.MarkChasedByGuard(false, col.transform);
                    else
                        _controller.MarkChasedByGuard(true, col.transform);
                }
            }
        }
    }
}