using Bb;
using System.Text;

namespace Black.Beard.OpenApiServices
{
    public class ContextGenerator
    {

        public ContextGenerator(string path)
        {
            this.TargetPath = path;
            this.Datas = new Dictionary<object, object>();
            this._files = new HashSet<string>();
        }



        /// <summary>
        /// create a new document on filesystem with specified content
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="filename">The filename.</param>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        public string AppendDocument(string? path, string filename, string content)
        {
            var file = ComputeFullPath(path, filename);
            file.Save(content);
            return file;
        }

        /// <summary>
        /// create a new document on filesystem with specified content
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        public string AppendDocument(string filename, string content)
        {
            var file = ComputeFullPath(string.Empty, filename);
            file.Save(content);
            return file;
        }

        /// <summary>
        /// create a new document on filesystem with specified content
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="filename">The filename.</param>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        public string AppendDocument(string? path, string filename, StringBuilder content)
        {
            var file = ComputeFullPath(path, filename);
            file.Save(content.ToString());
            return file;
        }

        /// <summary>
        /// create a new document on filesystem with specified content
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="filename">The filename.</param>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        public string AppendDocument(string filename, StringBuilder content)
        {
            var file = ComputeFullPath(string.Empty, filename);
            file.Save(content.ToString());
            return file;
        }

        /// <summary>
        /// Computes the full path for the target file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="filename">The filename.</param>
        /// <returns></returns>
        private string ComputeFullPath(string? path, string filename)
        {

            var _path = ComputeFullPath(path);

            int count = 1;
            var file = Path.Combine(_path, filename);
            while (_files.Contains(file))
            {
                var f = Path.GetFileNameWithoutExtension(filename) + count++.ToString();
                var e = Path.GetExtension(filename);
                file = Path.Combine(_path, f) + e;
            }

            _files.Add(file);

            return file;

        }

        /// <summary>
        /// Computes the full path for the target file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        private string ComputeFullPath(string? path)
        {
            var targetDirectory = TargetPath;
            if (!string.IsNullOrEmpty(path))
                targetDirectory = Path.Combine(targetDirectory, path);
            var dir = new DirectoryInfo(targetDirectory);

            if (!dir.Exists)
                dir.Create();

            return dir.FullName;

        }

        public string TargetPath { get; }

        public Dictionary<object, object> Datas { get; }

        public IEnumerable<string> Files => _files; 

        private HashSet<string> _files;
    }

}