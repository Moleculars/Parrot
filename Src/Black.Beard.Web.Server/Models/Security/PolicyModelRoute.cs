namespace Bb.Models.Security
{
    public class PolicyModelRoute
    {

        public PolicyModelRoute(Type type) : this()
        {
            ControllerName = type.Name.Substring(0, type.Name.Length - "Controller".Length);
        }

        public PolicyModelRoute()
        {

        }

        public override string ToString()
        {
            return $"{ControllerName} - {Route}";
        }

        public PolicyProfilRoute GetModel()
        {

            var model = new PolicyProfilRoute()
            {
                ControllerName = ControllerName,
                Route = Route,
                Rule = Rule,
            };

            return model;

        }


        public string ControllerName { get; set; }

        public string Route { get; set; }

        public string Rule { get; set; } = "Role:Administror";

    }

}
