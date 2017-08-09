using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace SelfishHttp.Params.Matching
{
    public class RegexMatch : BaseParamMatch
    {
        private readonly Regex _expression;

        public RegexMatch(Regex expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            _expression = expression;
        }

        public override bool IsMatch(string[] values)
        {
            return values.All(v => _expression.IsMatch(v));
        }
    }
}