using System.Text;

namespace Bb.Curls
{
    /// <summary>
    /// Lexer that tokenize curl command line
    /// </summary>
    public class CurlLexer
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="CurlLexer"/> class.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public CurlLexer(string args)
        {
            _args = args.Trim();
            _index = 0;
            _max = _args.Length;
            _sb = new StringBuilder(_max);
            _current = '\0';
        }

        /// <summary>
        /// return true if the line can read next token
        /// </summary>
        /// <returns></returns>
        public bool Next()
        {

            _sb.Clear();
            if (_index >= _max) return false;

            while (_index < _max && !char.IsWhiteSpace(_current = _args[_index++]))
            {

                if (_current == '\'')
                    ParseTextChain('\'');

                else if (_current == '"')
                    ParseTextChain('"');

                else
                    _sb.Append(_current);

                _old = _current;

            }

            if (_sb.ToString() == "\\")
            {

                if (_index <= _max)
                {

                    _current = _args[_index++];
                    if (_current == '\n')
                        _sb.Clear();

                }

            }

            return true;

        }

        private void ParseTextChain(char charset)
        {
            while (_index <= _max)
            {

                _current = _args[_index++];

                if (_current == charset)
                {
                    if (_old == '\\')
                    {
                        _sb.Append(_current);
                        _old = _current;
                    }
                    else
                        return;

                }
                else
                    _sb.Append(_current);

                _old = _current;

            }
        }

        /// <summary>
        /// Gets the current token
        /// </summary>
        /// <value>
        /// The current.
        /// </value>
        public string Current => _sb.ToString();

        private readonly string _args;
        private int _index;
        private readonly int _max;
        private readonly StringBuilder _sb;

        private char _current;
        private char _old;

    }


}
