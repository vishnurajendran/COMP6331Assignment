using UnityEngine;

namespace AgentControllers.Strategies
{
    public interface IHeroStrategy
    {
        public Strategy CurrentStrategy { get; }
        public void Initialise(HeroController controller);
        public Vector3 GetMove();
        public void Decide();
    }
}