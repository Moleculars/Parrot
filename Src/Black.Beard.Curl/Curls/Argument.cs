namespace Bb.Curls
{

    public class Argument
    {
        public Argument(string value, string name = null)
        {
            Value = value;
            Name = name;
        }

        public string Name { get; }

        public string Value { get; }

        public override string ToString()
        {
            return Value;
        }

    }

}
