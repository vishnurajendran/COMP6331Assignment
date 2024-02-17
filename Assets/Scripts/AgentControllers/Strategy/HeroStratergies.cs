namespace AgentControllers.Strategies
{
    public static class HeroStratergies
    {

        public static IHeroStrategy Get(Strategy strategy)
        {
            switch (strategy)
            {
                case Strategy.Aggressive: return GetAggressive();
                
                case Strategy.Default:
                default: 
                    return GetDefault();
            }
        }
        
        private static IHeroStrategy GetDefault()
        {
            return new DefaultStrategy();
        }
        
        private static IHeroStrategy GetAggressive()
        {
            return new AggressiveStrategy();
        }
    }
}