namespace Spolty.Framework.Parameters.Loads
{
    internal class LoadTree : IParameterMarker
    {
        private LoadNode _root;

        public LoadTree(LoadNode root)
        {
            _root = root;
            _root.Including = false;
        }

        public LoadTree()
        {
        }

        public LoadNode Root
        {
            get { return _root; }
            set { _root = value; }
        }
    }
}