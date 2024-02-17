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
        }

        public abstract Vector3 GetMove();
        
        public abstract void Decide();
    }
}