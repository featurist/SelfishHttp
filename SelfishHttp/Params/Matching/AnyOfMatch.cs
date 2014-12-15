using System;
using System.Linq;

namespace SelfishHttp.Params.Matching
{
    public class AnyOfMatch : BaseParamsMatch
    {
        private readonly string[] _anyOf;
        private readonly StringComparison _comparison;

        public AnyOfMatch(string[] anyOf, StringComparison comparison)
        {
            if (anyOf == null)
            {
                throw new ArgumentNullException("anyOf");
            }
            _anyOf = anyOf;
            _comparison = comparison;
        }

        public override bool IsMatch(string[] values)
        {
            return values.All(v => _anyOf.Any(of => of.Equals(v, _comparison)));
        }
    }
}