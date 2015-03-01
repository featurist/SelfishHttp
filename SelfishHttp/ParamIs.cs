using System;
using System.Text.RegularExpressions;
using System.Web.UI;
using SelfishHttp.Params.Matching;

namespace SelfishHttp
{
    public static class ParamIs
    {
        public static IStringParamMatch AnyOf(params string[] values)
        {
            if (values == null || values.Length == 0)
            {
                throw new ArgumentException("Cannot be null or an empty array", "values");
            }

            return new AnyOfMatch(values);
        }

        public static IStringParamMatch AllOf(params string[] values)
        {
            if (values == null || values.Length == 0)
            {
                throw new ArgumentException("Cannot be null or an empty array", "values");
            }

            return new AllOfMatch(values);
        }

        public static IStringParamMatch Equal(string value)
        {
            return new StringMatch(value);
        }

        public static IIntParamMatch Int()
        {
            return new IntMatch();
        }

        public static IParamMatch Empty()
        {
            return new EmptyMatch();
        }

        public static IParamMatch Anything()
        {
            return new AnythingMatch();
        }

        public static IParamMatch Like(string pattern)
        {
            return Like(new Regex(pattern));
        }

        public static IParamMatch Like(string pattern, RegexOptions options)
        {
            return Like(new Regex(pattern, options));
        }

        public static IParamMatch Like(Regex expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            return new RegexMatch(expression);
        }

        public static IParamMatch Equal(Func<string, bool> callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            return new CallbackMatch(callback);
        }

        public static IParamMatch Custom(IParamMatch match)
        {
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }

            return match;
        }
    }
}