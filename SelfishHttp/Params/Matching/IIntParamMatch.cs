namespace SelfishHttp.Params.Matching
{
    public interface IIntParamMatch : IParamMatch
    {
        new IIntParamMatch Optional();

        IIntParamMatch GreaterThanZero();

        IIntParamMatch LessThanZero();

        IIntParamMatch GreaterThanOrEqualToZero();

        IIntParamMatch LessThanOrEqualToZero();

        IIntParamMatch Between(int lowerBound, int upperBound);
    }
}