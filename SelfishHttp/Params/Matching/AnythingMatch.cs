using System.Linq;

namespace SelfishHttp.Params.Matching
{
    public class AnythingMatch : BaseParamMatch
    {
        public override bool IsMatch(string[] values)
        {
            return values.All(v => !string.IsNullOrWhiteSpace(v));
        }
    }
}