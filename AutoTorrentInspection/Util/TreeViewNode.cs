using System.Collections.Generic;
using System.Linq;

namespace AutoTorrentInspection.Util
{
    public class Node
    {
        private Dictionary<string, Node> _childs = new Dictionary<string, Node>();

        private Node _parentNode = null;

        public string NodeName { get; set; } = string.Empty;

        public override string ToString() => NodeName;

        public Node() { }

        public Node(string node)
        {
            NodeName = node;
        }

        public Node this[string node]
        {
            get { return _childs[node]; }
            set { _childs[node] = value; }
        }

        public bool IsFile => this._childs.Count == 0;

        public bool IsDirectory => this._childs.Count > 0;

        public IEnumerable<Node> GetFiles()
        {
            return _childs.Where(item => item.Value._childs.Count == 0).Select(item => item.Value);
        }

        public IEnumerable<Node> GetDirectories()
        {
            return _childs.Where(item => item.Value._childs.Count > 0).Select(item => item.Value);
        }

        public string FullPath
        {
            get
            {
                string path;
                if (IsFile)
                {
                    path = NodeName;
                }
                else
                {
                    path = NodeName + "/";
                }
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
            return _childs.GetEnumerator();
        }

        public void Insert(IEnumerable<string> nodes)
        {
            var currentNode = this;
            foreach (string node in nodes)
            {
                if (!currentNode._childs.ContainsKey(node))
                {
                    currentNode._childs.Add(node, new Node(node));
                }
                currentNode._childs[node]._parentNode = currentNode;
                currentNode = currentNode._childs[node];
            }
        }

        public void Insert(string node)
        {
            if (!_childs.ContainsKey(node))
            {
                _childs.Add(node, new Node(node));
            }
        }
    }

}