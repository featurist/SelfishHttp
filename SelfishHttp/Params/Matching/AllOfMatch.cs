using System;
using System.Linq;

namespace SelfishHttp.Params.Matching
{
    public class AllOfMatch : BaseParamsMatch
    {
        private readonly string[] _allOf;
        private readonly StringComparison _comparison;

        public AllOfMatch(string[] allOf, StringComparison comparison)
        {
            if (allOf == null)
            {
                throw new ArgumentNullException("allOf");
            }
            _allOf = allOf;
            _comparison = comparison;
        }

        public override bool IsMatch(string[] values)
        {
            if (values.Length != _allOf.Length)
            {
                return false;
            }

            return values.All(v => _allOf.Any(of => of.Equals(v, _comparison))) && _allOf.All(of => values.Any(v => v.Equals(of, _comparison)));
        }
    }
}