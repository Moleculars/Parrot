using Microsoft.AspNetCore.Mvc;

#pragma warning disable CS8618, CS1591
namespace Bb.Mock
{
    public class WatchdogResultException
    {
        public WatchdogResultException(Guid uuid, String message)
        {
            Uuid = uuid;
            Message = message;
        }

        public Guid Uuid { get; set; }
        public String Message { get; set; }
    }
}