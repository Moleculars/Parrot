using System.Collections;

namespace Bb.Curls
{

    public class ArgumentList : IEnumerable<Argument>
    {

        public ArgumentList()
        {
            _arguments = new List<Argument>();
        }

        public ArgumentList(Argument[] arguments) : this()
        {
            foreach (Argument argument in arguments)
                Add(argument);
        }

        public ArgumentList Add(Argument argument)
        {
            _arguments.Add(argument);
            return this;
        }

        public IEnumerator<Argument> GetEnumerator()
        {
            return _arguments.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _arguments.GetEnumerator();
        }

        public static implicit operator ArgumentList(Argument[] arguments)
        {
            return new ArgumentList(arguments);
        }

        private List<Argument> _arguments { get; }

    }

}
