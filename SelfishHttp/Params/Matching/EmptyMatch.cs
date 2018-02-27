using System.Linq;

namespace SelfishHttp.Params.Matching
{
    public class EmptyMatch : BaseParamMatch
    {
        public override bool IsMatch(string[] values)
        {
            return values.All(string.IsNullOrWhiteSpace);
        }
    }
}