namespace Bb.Models
{
    public class Listener
    {

        public Listener()
        {
            Http = new Swagger();    
            Https = new Swagger();
        }

        public Swagger Http { get; internal set; }
        public Swagger Https { get; internal set; }
    }

}
