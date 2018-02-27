using System;
using System.Linq;

namespace SelfishHttp.Params.Matching
{
    public class AllOfMatch : BaseStringParamMatch
    {
        private readonly string[] _allOf;

        public AllOfMatch(string[] allOf)
        {
            if (allOf == null)
                throw new ArgumentNullException("allOf");

            _allOf = allOf;
        }

        public override bool IsMatch(string[] values)
        {
            if (values.Length != _allOf.Length)
                return false;

            return values.All(v => _allOf.Any(of => of.Equals(v, Comparison))) && _allOf.All(of => values.Any(v => v.Equals(of, Comparison)));
        }
    }
}