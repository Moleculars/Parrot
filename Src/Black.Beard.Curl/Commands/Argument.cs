namespace Bb.Curl.Commands
{

    public class Argument
    {
        public Argument(string value, string name = null)
        {
            this.Value = value;
            this.Name = name;
        }

        public string Name { get; }

        public string Value { get; }

        public override string ToString()
        {
            return Value;
        }

    }

}
