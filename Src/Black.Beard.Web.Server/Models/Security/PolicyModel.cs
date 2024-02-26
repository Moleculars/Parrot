using Bb;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Bb.Models.Security
{


    public class PolicyModel
    {

        public PolicyModel()
        {
            Routes = new Dictionary<string, PolicyModelRoute>();
            //this.Roles = new HashSet<string>();
            //this.AuthenticationSchemes = new HashSet<string>();
        }

        public string Name { get; set; }

        public Dictionary<string, PolicyModelRoute> Routes { get; set; }

        public PolicyModelRoute Evaluate(string path)
        {

            var pathArray = path.Trim('/').Split('/');

            if (_tree == null)
                lock (_lock)
                    if (_tree == null)
                        Build();

            var c = pathArray[0];
            if (_tree.TryGetValue(c, out var i))
            {

                var result = i.Evaluate(pathArray, 1);
                if (result != null)
                    return result;
            }

            if (_list != null)
            {
                foreach (var item in _list)
                {
                    var result = i.Evaluate(pathArray, 1);
                    if (result != null)
                        return result;
                }
            }

            return null;

        }

        private void Build()
        {
            _tree = new Dictionary<string, PathTree>();
            _list = new List<PathTree>();
            foreach (var item in Routes)
            {
                var r = item.Value.Route.Split('/');
                if (!_tree.TryGetValue(r[0], out var i1))
                    _tree.Add(r[0], i1 = new PathTree() { Label = r[0], IsVariable = r[0].StartsWith("{") && r[0].EndsWith("}") });

                if (i1.IsVariable)
                    _list.Add(i1);

                i1.Build(r, 1, item.Value);

            }
        }

        public override string ToString()
        {
            return Name;
        }

        private class PathTree
        {

            public PathTree()
            {
                _tree = new Dictionary<string, PathTree>();
                _list = new List<PathTree>();
            }

            internal void Build(string[] array, int index, PolicyModelRoute item)
            {

                if (index < array.Length)
                {

                    var current = array[index];

                    if (!_tree.TryGetValue(current, out var p))
                        _tree.Add(current, p = new PathTree() { Label = current, IsVariable = current.StartsWith("{") && current.EndsWith("}") });

                    if (p.IsVariable)
                        _list.Add(p);


                    p.Build(array, index + 1, item);


                }
                else
                    Datas = item;

            }

            internal PolicyModelRoute Evaluate(string[] pathArray, int index)
            {

                if (index < pathArray.Length)
                {

                    var c = pathArray[index];

                    if (_tree.TryGetValue(c, out var i))
                    {
                        var result = i.Evaluate(pathArray, index + 1);
                        if (result != null)
                            return result;
                    }

                    foreach (var p in _list)
                    {

                        var result = p.Evaluate(pathArray, index + 1);
                        if (result != null)
                            return result;
                    }


                }

                return Datas;

            }

            private Dictionary<string, PathTree> _tree;
            private List<PathTree> _list;

            public PolicyModelRoute Datas { get; private set; }

            public string Label { get; internal set; }

            public bool IsVariable { get; internal set; }


        }

        public PolicyProfil GetModel()
        {

            var model = new PolicyProfil()
            {
                Name = this.Name
            };

            foreach (var route in this.Routes)
                model.Routes.Add( route.Value.GetModel());

            return model;

        }

        private Dictionary<string, PathTree> _tree;
        private List<PathTree> _list;
        private volatile object _lock = new object();

    }


}
