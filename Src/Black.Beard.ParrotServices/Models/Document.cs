namespace Bb.Models
{

    /// <summary>
    /// Describes a document of the project
    /// </summary>
    public class Document
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="Document"/> class.
        /// </summary>
        public Document()
        {
                
        }

        /// <summary>
        /// Gets or sets the kind of document.
        /// </summary>
        /// <value>
        /// The kind.
        /// </value>
        public string Kind { get; set; }

        /// <summary>
        /// Gets or sets the file name.
        /// </summary>
        /// <value>
        /// The file.
        /// </value>
        public string File { get; set; }

    }

}
