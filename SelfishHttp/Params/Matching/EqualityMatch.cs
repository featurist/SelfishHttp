using System;
using System.Linq;

namespace SelfishHttp.Params.Matching
{
    public class EqualityMatch : BaseParamsMatch
    {
        private readonly string _value;

        private readonly StringComparison _comparer;

        public EqualityMatch(string value, StringComparison comparer)
        {
            _value = value;
            _comparer = comparer;
        }

        public override bool IsMatch(string[] values)
        {
            return values.All(v => _value.Equals(v, _comparer));
        }
    }
}