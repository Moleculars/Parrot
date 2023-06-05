using Bb.Codings;

namespace Bb.OpenApiServices
{



    public class CSharpGeneratorBase
    {

        public CSharpGeneratorBase(string @namespace, params string[] usings)
        {
            this._usings = usings;
            this._namespace = @namespace;
        }

        public void Generate(ContextGenerator ctx)
        {

            var type = GetType();
            var methods =type.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            foreach (var method in methods) 
                if (method.ReturnType == typeof(CSharpArtifact))
                {
                    var artifact = (CSharpArtifact)method.Invoke(this, null);
                    if (artifact != null)
                        ctx.AppendDocument(artifact.ProjectPath, artifact.Name + ".cs", artifact.Code().ToString());
                }

        }


        protected CSharpArtifact CreateArtifact(string artifactName, string projectPath = null)
        => new CSharpArtifact(artifactName, projectPath)
        .Usings(_usings);

        protected CSNamespace CreateNamespace(CSharpArtifact cs) => cs.Namespace(_namespace);


        private readonly string[] _usings;
        private readonly string _namespace;

    }


}