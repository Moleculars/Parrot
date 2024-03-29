﻿using Bb.Services.ProcessHosting;

namespace Bb.Services.Managers
{

    /// <summary>
    /// manage the contract in the referential
    /// </summary>
    public class ProjectBuilderContract
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectBuilderContract"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="contract">The contract.</param>
        public ProjectBuilderContract(ProjectBuilderProvider parent, string contract)
        {
            Parent = parent;
            _logger = parent._logger;
            ContractName = contract;
            Root = parent.Root.Combine(contract);
            _templates = new Dictionary<string, ProjectBuilderTemplate>();
        }

        /// <summary>
        /// Templates wants to know if exist.
        /// </summary>
        /// <param name="templateName">Name of the template.</param>
        /// <returns></returns>
        public bool TemplateExistsOnDisk(string templateName)
        {
            return Directory.Exists(Root.Combine(templateName));
        }

        /// <summary>
        /// return the template the specified template name.
        /// </summary>
        /// <param name="templateName">Name of the template.</param>
        /// <param name="createIfNotExists">if set to <c>true</c> [create if not exists].</param>
        /// <returns></returns>
        public ProjectBuilderTemplate? Template(string templateName, bool createIfNotExists = false)
        {

            var r = Root.Combine(templateName).AsDirectory();
            if (r.Exists)
                createIfNotExists = true;

            if (!_templates.TryGetValue(templateName, out var template1))
                lock (_lock)
                    if (!_templates.TryGetValue(templateName, out template1))
                        if (createIfNotExists)
                            _templates.Add(templateName, template1 = new ProjectBuilderTemplate(this, templateName));

            return template1;

        }

        /// <summary>
        /// List of <see cref="ProjectBuilderTemplate"/> that exists in the referential
        /// </summary>
        /// <returns></returns>
        public List<ProjectBuilderTemplate> List()
        {

            List<ProjectBuilderTemplate> items = new List<ProjectBuilderTemplate>();
            var dirRoot = new DirectoryInfo(Root);
            var dirs = dirRoot.GetDirectories();
            foreach (var dir in dirs)
            {
                var template = Template(dir.Name);
                items.Add(template);
            }

            return items;

        }

        /// <summary>
        /// The contract name
        /// </summary>
        public readonly string ContractName;

        /// <summary>
        /// The path root
        /// </summary>
        public readonly string Root;

        /// <summary>
        /// Gets the parent that create the current class.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        public ProjectBuilderProvider Parent { get; }

        internal readonly ILogger<ProjectBuilderProvider> _logger;
        private Dictionary<string, ProjectBuilderTemplate> _templates;
        private volatile object _lock = new object();

    }

}
