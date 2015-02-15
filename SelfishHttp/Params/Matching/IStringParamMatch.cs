namespace SelfishHttp.Params.Matching
{
    public interface IStringParamMatch : IParamMatch
    {
        IStringParamMatch IgnoreCase();

        new IStringParamMatch Optional();
    }
}