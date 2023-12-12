using Microsoft.AspNetCore.Mvc;

#pragma warning disable CS8618, CS1591
namespace Bb.Mock
{

    [System.Diagnostics.DebuggerDisplay("{Name} : {Value} {Description}")]
    public class WatchdogResultItem
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="WatchdogResultItem"/> class.
        /// </summary>
        public WatchdogResultItem()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WatchdogResultItem"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <param name="description">The description.</param>
        public WatchdogResultItem(string name, string value, string description = null)
        {
            this.Name = name;   
            this.Value = value;
            this.Description = description ?? string.Empty;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public String Name { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public String Value { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public String Description { get; set; }

    }

}