using System.Collections.Specialized;

namespace SelfishHttp.Params.Matching
{
    public interface IParamsMatch
    {
        bool IsMatch(string[] values);

        bool IsOptional { get; }

        IParamsMatch Optional();
    }
}