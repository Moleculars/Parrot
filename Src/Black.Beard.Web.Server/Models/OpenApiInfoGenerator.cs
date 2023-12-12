namespace Bb.Models
{

    /// <summary>
    /// OpenApi information generator
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class OpenApiInfoGenerator<T>
        where T : new()
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenApiInfoGenerator{T}"/> class.
        /// </summary>
        public OpenApiInfoGenerator()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenApiInfoGenerator{T}"/> class.
        /// </summary>
        /// <param name="builders">The builders.</param>
        public OpenApiInfoGenerator(params Action<OpenApiInfoGenerator<T>>[] builders)
        {
            _builders = new List<Action<OpenApiInfoGenerator<T>>>();
            _builders.AddRange(builders);
        }


        internal OpenApiInfoGenerator<T> Add(Action<T> action)
        {
            _properties.Add(action);
            return this;
        }

        /// <summary>
        /// Generates this instance.
        /// </summary>
        /// <returns></returns>
        public T Generate()
        {

            T result = new T();

            foreach (var builder in _builders)
                builder(this);

            foreach (var property in _properties)
                property(result);

            return result;

        }

        private List<Action<T>> _properties = new List<Action<T>>();
        private List<Action<OpenApiInfoGenerator<T>>> _builders;

    }


}
