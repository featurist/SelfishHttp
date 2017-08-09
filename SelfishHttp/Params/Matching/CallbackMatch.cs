using System;
using System.Linq;

namespace SelfishHttp.Params.Matching
{
    public class CallbackMatch : BaseParamMatch
    {
        private readonly Func<string, bool> _callback;

        public CallbackMatch(Func<string, bool> callback)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            _callback = callback;
        }

        public override bool IsMatch(string[] values)
        {
            return values.All(v => _callback(v));
        }
    }
}