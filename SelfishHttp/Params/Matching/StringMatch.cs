using System.Linq;

namespace SelfishHttp.Params.Matching
{
    public class StringMatch : BaseStringParamMatch
    {
        private readonly string _value;

        public StringMatch(string value)
        {
            _value = value;
        }

        public override bool IsMatch(string[] values)
        {
            return values.All(v => _value.Equals(v, Comparison));
        }
    }
}