namespace SelfishHttp.Params.Matching
{
    public abstract class BaseParamsMatch : IParamsMatch
    {
        public abstract bool IsMatch(string[] values);

        public bool IsOptional { get; private set; }

        public IParamsMatch Optional()
        {
            IsOptional = true;
            return this;
        }
    }
}