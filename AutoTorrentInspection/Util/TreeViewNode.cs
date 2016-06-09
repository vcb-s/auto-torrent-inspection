using System.Linq;
using System.Collections.Generic;

namespace AutoTorrentInspection.Util
{
    public class Node
    {
        private readonly Dictionary<string, Node> _childNodes = new Dictionary<string, Node>();

        public Dictionary<string, object> Attribute { get; set; } = new Dictionary<string, object>();

        private Node _parentNode = null;

        public string NodeName { get; private set; } = string.Empty;

        public override string ToString() => NodeName;

        public Node() { }

        private Node(string node)
        {
            NodeName = node;
        }

        public Node this[string node]
        {
            get { return _childNodes[node]; }
            set { _childNodes[node] = value; }
        }

        public bool IsFile => this._childNodes.Count == 0;

        public bool IsDirectory => this._childNodes.Count > 0;

        public IEnumerable<Node> GetFiles()
        {
            return _childNodes.Where(item => item.Value._childNodes.Count == 0).Select(item => item.Value);
        }

        public IEnumerable<Node> GetDirectories()
        {
            return _childNodes.Where(item => item.Value._childNodes.Count > 0).Select(item => item.Value);
        }

        public string FullPath
        {
            get
            {
                var path = NodeName + (IsFile?"":"/");
                var currentNode = _parentNode;
                while (currentNode != null)
                {
                    path = currentNode.NodeName + "/" + path;
                    currentNode = currentNode._parentNode;
                }
                return path;
            }
        }

        public Dictionary<string, Node>.Enumerator GetEnumerator()
        {
            return _childNodes.GetEnumerator();
        }

        public Node Insert(IEnumerable<string> nodes)
        {
            var currentNode = this;
            foreach (string node in nodes)
            {
                if (!currentNode._childNodes.ContainsKey(node))
                {
                    currentNode._childNodes.Add(node, new Node(node));
                }
                currentNode[node]._parentNode = currentNode;
                currentNode = currentNode[node];
            }
            return currentNode;
        }
    }
}
