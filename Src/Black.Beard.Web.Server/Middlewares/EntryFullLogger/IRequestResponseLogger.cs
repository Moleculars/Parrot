namespace Bb.Middlewares.EntryFullLogger
{
    public interface IRequestResponseLogger
    {
        void Log(IRequestResponseLogModelCreator logCreator);
    }


}
