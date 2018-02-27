using System;
using System.Linq;

namespace SelfishHttp.Params.Matching
{
    public class AnyOfMatch : BaseStringParamMatch
    {
        private readonly string[] _anyOf;

        public AnyOfMatch(string[] anyOf)
        {
            if (anyOf == null)
                throw new ArgumentNullException("anyOf");

            _anyOf = anyOf;
        }

        public override bool IsMatch(string[] values)
        {
            return values.All(v => _anyOf.Any(of => of.Equals(v, Comparison)));
        }
    }
}