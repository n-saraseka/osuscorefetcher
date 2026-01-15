using osu.Game.Rulesets.Scoring;

namespace osuscorefetcher.ApiClasses
{
    public class HitResultAttribute : Attribute
    {
        public HitResult HitResult { get; }
        
        public HitResultAttribute(HitResult hitResult)
        {
            HitResult = hitResult;
        }
    }
}
