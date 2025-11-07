using osu.Game.Rulesets.Scoring;

namespace osuscorefetcher.ApiClasses
{
    internal class HitResultAttribute : Attribute
    {
        public HitResult HitResult { get; }
        
        public HitResultAttribute(HitResult hitResult)
        {
            HitResult = hitResult;
        }
    }
}
