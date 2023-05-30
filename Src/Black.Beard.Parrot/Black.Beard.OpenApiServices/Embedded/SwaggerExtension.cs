using System.Xml.Linq;
using System.Xml.XPath;

namespace Bb.ParrotServices
{

    internal static class SwaggerExtension
    {

        internal static XPathDocument? LoadXmlFiles(string patternGlobing = "*.xml")
        {

            XElement? xml = null;

            var path = Path.GetDirectoryName(typeof(SwaggerExtension).Assembly.Location);

            if (!string.IsNullOrEmpty(path))        // Build one large xml with all comments files
                foreach (var fileName in Directory.EnumerateFiles(path, patternGlobing))
                {

                    if (xml == null)
                        xml = XElement.Load(fileName);

                    else
                    {
                        var dependentXml = XElement.Load(fileName);
                        foreach (var ele in dependentXml.Descendants())
                            xml.Add(ele);

                    }

                }

            if (xml == null)
                throw new FileLoadException("no project found");

            var streamer = xml?.CreateReader();
            if (streamer != null)
                return new XPathDocument(streamer);

            return null;

        }

    }


}
