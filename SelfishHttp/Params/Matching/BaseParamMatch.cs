namespace SelfishHttp.Params.Matching
{
    public abstract class BaseParamMatch : IParamMatch
    {
        public abstract bool IsMatch(string[] values);

        public bool IsOptional { get; private set; }

        public IParamMatch Optional()
        {
            IsOptional = true;
            return this;
        }
    }
}