using Flurl;
using System.Text;

namespace Bb.Flurl
{
    public static class UrlExtension
    {

        /// <summary>
        /// return a <see cref="StringBuilder"/> with Concatenated url separated by ';'.
        /// </summary>
        /// <param name="urls"><see cref="Url"/></param>
        /// <returns></returns>
        public static StringBuilder ConcatUrl(this IEnumerable<Url> urls)
        {
            return ConcatUrl(new StringBuilder(), urls);
        }

        /// <summary>
        /// return a <see cref="StringBuilder"/> with Concatenated url separated by ';'.
        /// </summary>
        /// <param name="sb"><see cref="StringBuilder"/></param>
        /// <param name="urls"><see cref="Url"/></param>
        /// <returns></returns>
        public static StringBuilder ConcatUrl(this StringBuilder sb, IEnumerable<Url> urls)
        {
            if (sb == null)
                sb = new StringBuilder(200);

            string comma = string.Empty;
            foreach (var url in urls)
            {
                sb.Append(comma);
                sb.Append(url.ToString());
                comma = ";";
            }

            return sb;

        }

    }

}
