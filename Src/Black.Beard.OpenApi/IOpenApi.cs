namespace Bb.OpenApi
{


    public interface IOpenApi
    {

        /// <summary>
        /// Gets the path of the current location.
        /// </summary>
        /// <returns></returns>
        string GetPath();

        /// <summary>
        /// clear all segments from current path.
        /// </summary>
        /// <param name="segment">The segment.</param>
        void ClearPath();



        /// <summary>
        /// Gets the stacked items.
        /// </summary>
        /// <returns></returns>
        object[] GetItems();

        /// <summary>
        /// Clears all stacked children.
        /// </summary>
        void ClearChildren();




        /// <summary>
        /// Gets the current context.
        /// </summary>
        /// <returns></returns>
        string GetContext();

        /// <summary>
        /// Gets the path of the current context.
        /// </summary>
        /// <returns></returns>
        string[] GetContexts();

        /// <summary>
        /// clear all segments from current context.
        /// </summary>
        /// <param name="segment">The segment.</param>
        void ClearContexts();


        /// <summary>
        /// Pushes the context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        IDisposable PushContext(string context);

        /// <summary>
        /// Pushes a new segment path.
        /// </summary>
        /// <param name="segment">The segment.</param>
        /// <returns></returns>
        IDisposable PushPath(string segment);

        /// <summary>
        /// Pushes a new segment item in the hierachy.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        IDisposable PushChildren(object item);





    }


}
