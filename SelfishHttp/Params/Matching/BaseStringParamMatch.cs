using System;

namespace SelfishHttp.Params.Matching
{
    public abstract class BaseStringParamMatch : BaseParamMatch, IStringParamMatch
    {
        protected StringComparison Comparison { get; private set; }

        protected BaseStringParamMatch()
        {
            Comparison = StringComparison.Ordinal;
        }

        public IStringParamMatch IgnoreCase()
        {
            Comparison = StringComparison.OrdinalIgnoreCase;
            return this;
        }

        public new IStringParamMatch Optional()
        {
            base.Optional();
            return this;
        }
    }
}