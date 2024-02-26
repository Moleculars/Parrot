namespace Bb.Curls
{


    internal partial class CurlInterpreterAction
    {

        public CurlInterpreterAction(Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> configureAction, params Argument[] arguments)
        {
            _parameters = new List<KeyValuePair<string, string>>();
            Arguments = arguments;
            _configureAction = configureAction;
        }

        public async Task<HttpResponseMessage> CallAsync(Context context)
        {
            return await _configureAction(this, context);
        }

        internal Context CollectParameters(Context context)
        {

            if (_parameters.Count > 0)
                foreach (var item in _parameters)
                    context.Add(item);

            if (_next != null)
                _next.CollectParameters(context);

            return context;

        }


        public async Task<HttpResponseMessage> CallNext(Context context)
        {

            if (_next != null)
                return await _next._configureAction(_next, context);

            return await context.CallAsync();

        }

        internal void Append(List<CurlInterpreterAction> list, int index)
        {
            _next = list[index++];

            if (index < list.Count)
                _next.Append(list, index);

        }

        public int Priority { get; internal set; }

        public ArgumentList Arguments { get; }

        public Argument First => Arguments.First();

        public string FirstValue => Arguments.First().Value;

        public Argument Get(string name) => Arguments.First(c => c.Name.Trim() == name);

        public Argument Get(Func<Argument, bool> test) => Arguments.First(test);

        public bool Exists(string name) => Arguments.Any(c => c.Name.Trim() == name);

        internal CurlInterpreterAction Add(string key, string value)
        {
            _parameters.Add(new KeyValuePair<string, string>(key, value));
            return this;
        }


        internal CurlInterpreterAction _next;

        private Func<CurlInterpreterAction, Context, Task<HttpResponseMessage>> _configureAction;
        private List<KeyValuePair<string, string>> _parameters = new List<KeyValuePair<string, string>>();

    }

}
