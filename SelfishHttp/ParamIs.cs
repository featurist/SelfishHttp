using System;
using System.Text.RegularExpressions;
using SelfishHttp.Params.Matching;

namespace SelfishHttp
{
    public static class ParamIs
    {
        public static IParamsMatch AnyOf(string[] values, StringComparison comparison = StringComparison.CurrentCulture)
        {
            if (values == null || values.Length == 0)
            {
                throw new ArgumentException("Cannot be null or an empty array", "values");
            }
            return new AnyOfMatch(values, comparison);
        }

        public static IParamsMatch AllOf(string[] values, StringComparison comparison = StringComparison.CurrentCulture)
        {
            if (values == null || values.Length == 0)
            {
                throw new ArgumentException("Cannot be null or an empty array", "values");
            }
            return new AllOfMatch(values, comparison);
        }

        public static IParamsMatch Equal(string value, StringComparison comparison = StringComparison.CurrentCulture)
        {
            return new EqualityMatch(value, comparison);
        }

        public static IParamsMatch Like(Regex expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }
            return new RegexMatch(expression);
        }

        public static IParamsMatch Custom(IParamsMatch match)
        {
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }
            return match;
        }
    }
}