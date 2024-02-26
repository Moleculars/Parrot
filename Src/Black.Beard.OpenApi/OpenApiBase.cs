using System.Diagnostics;
using System.Text;

namespace Bb.OpenApi
{
    public class OpenApiBase : IOpenApi
    {

        public string GetPath()
        {

            StringBuilder sb = new StringBuilder();
            sb.Append("#");

            var l = _path.Reverse().ToList();

            foreach (var item in l)
            {
                sb.Append("/");
                sb.Append(item);
            }

            return sb.ToString();
        }

        /// <summary>
        /// clear all segments from current path.
        /// </summary>
        public void ClearPath()
        {
            _path.Clear();
        }



        /// <summary>
        /// Gets the stacked items.
        /// </summary>
        /// <returns></returns>
        public object[] GetItems()
        {
            var l = _pathObject.ToArray();
            return l;
        }

        /// <summary>
        /// Clears all stacked children.
        /// </summary>
        public void ClearChildren()
        {
            _pathObject.Clear();
        }



        /// <summary>
        /// Gets the current context.
        /// </summary>
        /// <returns></returns>
        public string GetContext()
        {
            return _context.Peek();
        }

        /// <summary>
        /// Gets the path of the current context.
        /// </summary>
        /// <returns></returns>
        public string[] GetContexts()
        {
            return _context.ToArray();
        }

        /// <summary>
        /// clear all segments from current path.
        /// </summary>
        public void ClearContexts()
        {
            _context.Clear();
        }



        /// <summary>
        /// Pushes a new context.
        /// </summary>
        /// <param name="segment">The segment.</param>
        /// <returns></returns>
        public IDisposable PushContext(string context)
        {
            return new _disposeContextClass(this, context);
        }

        /// <summary>
        /// Pushes a new segment path.
        /// </summary>
        /// <param name="segment">The segment.</param>
        /// <returns></returns>
        public IDisposable PushPath(string segment)
        {
            return new _disposePathClass(this, segment);
        }

        public IDisposable PushChildren(object segment)
        {
            return new _disposeItemClass(this, segment);
        }



        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        protected void Stop()
        {

            var st = new StackTrace();
            var f = st.GetFrame(1);
            Debug.WriteLine($"{f.ToString().Trim()} try to stop");

            if (Debugger.IsAttached)
                Debugger.Break();

        }



        private class _disposeContextClass : IDisposable
        {

            public _disposeContextClass(OpenApiBase document, string item)
            {
                _document = document;
                _document._context.Push(item);
            }

            public void Dispose()
            {
                _document._context.Pop();
            }

            private readonly OpenApiBase _document;

        }

        private class _disposeItemClass : IDisposable
        {

            public _disposeItemClass(OpenApiBase document, object item)
            {
                _document = document;
                _document._pathObject.Push(item);
            }

            public void Dispose()
            {
                _document._pathObject.Pop();
            }

            private readonly OpenApiBase _document;

        }

        private class _disposePathClass : IDisposable
        {

            public _disposePathClass(OpenApiBase document, string segments)
            {
                _document = document;
                _document._path.Push(segments);
            }

            public void Dispose()
            {
                _document._path.Pop();
            }

            private readonly OpenApiBase _document;

        }


        private Stack<string> _context = new Stack<string>();
        private Stack<string> _path = new Stack<string>();
        private Stack<object> _pathObject = new Stack<object>();

    }


}
