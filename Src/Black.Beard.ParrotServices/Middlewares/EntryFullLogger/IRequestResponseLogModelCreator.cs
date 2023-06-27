namespace Bb.Middlewares.EntryFullLogger
{
    public interface IRequestResponseLogModelCreator
    {
        RequestResponseLogModel LogModel { get; }
        string LogString();
    }


}
