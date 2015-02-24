namespace SelfishHttp.Params.Matching
{
    public interface IParamMatch
    {
        bool IsOptional { get; }

        bool IsMatch(string[] values);

        IParamMatch Optional();
    }
}