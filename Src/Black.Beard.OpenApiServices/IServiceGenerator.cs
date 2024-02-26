namespace Bb.OpenApiServices
{
    public interface IServiceGenerator<T>
    {

        public void Parse(T self, ContextGenerator ctx);


    }




}