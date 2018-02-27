using SelfishHttp.Params;

namespace SelfishHttp
{
    public interface IServerConfiguration
    {
        IBodyParser BodyParser { get; set; }

        IBodyWriter BodyWriter { get; set; }

        IParamsParser ParamsParser { get; set; }
    }
}