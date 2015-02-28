using System;
using System.Linq;

namespace SelfishHttp.Params.Matching
{
    public class IntMatch : BaseParamMatch, IIntParamMatch
    {
        private Func<int, bool> _constraint; 

        public override bool IsMatch(string[] values)
        {
            return values.All(v =>
            {
                int t;
                if (int.TryParse(v, out t))
                {
                    return _constraint == null || _constraint(t);
                }

                return false;
            });
        }

        public new IIntParamMatch Optional()
        {
            base.Optional();
            return this;
        }

        public IIntParamMatch GreaterThanZero()
        {
            _constraint = i => i > 0;
            return this;
        }

        public IIntParamMatch LessThanZero()
        {
            _constraint = i => i < 0;
            return this;
        }

        public IIntParamMatch GreaterThanOrEqualToZero()
        {
            _constraint = i => i >= 0;
            return this;
        }

        public IIntParamMatch LessThanOrEqualToZero()
        {
            _constraint = i => i <= 0;
            return this;
        }

        public IIntParamMatch Between(int lowerBound, int upperBound)
        {
            _constraint = i => i > lowerBound && i < upperBound;
            return this;
        }
    }
}