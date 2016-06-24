using System.Linq;
using System.Collections.Generic;

namespace AutoTorrentInspection.Util
{
    public class Node
    {
        private readonly Dictionary<string, Node> _childNodes = new Dictionary<string, Node>();

        public FileSize Attribute { get; private set; }

        private Node _parentNode = null;

        public string NodeName { get; private set; } = string.Empty;

        public override string ToString() => NodeName;

        public Node() { }

        public Node(IEnumerable<IEnumerable<string>> fileList)
        {
            foreach (var list in fileList)
            {
                this.Insert(list);
            }
        }

        public Node(IEnumerable<KeyValuePair<IEnumerable<string>, FileSize>> fileList)
        {
            foreach (var list in fileList)
            {
                this.Insert(list.Key, list.Value);
            }
        }

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
                var path = NodeName;
                const string separator = "/";
                if (!IsFile)
                {
                    path += separator;//文件夹后增加'/'以作区分
                }
                var currentNode = _parentNode;//不断回溯以获取完整路径
                while (currentNode != null)
                {
                    path = currentNode.NodeName + separator + path;
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
                    currentNode[node]._parentNode = currentNode;
                }
                currentNode = currentNode[node];
            }
            return currentNode;
        }

        public Node Insert(IEnumerable<string> nodes, FileSize attribute)
        {
            var currentNode = this;
            foreach (string node in nodes)
            {
                if (!currentNode._childNodes.ContainsKey(node))
                {
                    currentNode._childNodes.Add(node, new Node(node));
                    currentNode[node]._parentNode = currentNode;
                }
                currentNode = currentNode[node];
            }
            currentNode.Attribute = attribute;
            return currentNode;
        }

        public void InsertTo(System.Windows.Forms.TreeNodeCollection tn)
        {
            InsertToViewInner(this, tn);
        }

        private static void InsertToViewInner(Node currentNode, System.Windows.Forms.TreeNodeCollection tn)
        {
            foreach (var node in currentNode.GetDirectories())
            {
                var treeNode = tn.Insert(tn.Count, node.NodeName);//将文件夹插入当前TreeNode节点的末尾
                InsertToViewInner(node, treeNode.Nodes);//由于是文件夹，故获取其子项并继续插入
            }
            foreach (var node in currentNode.GetFiles())
            {
                //将文件插入当前TreeNode结点的末尾
                tn.Insert(tn.Count, node.NodeName + (node.Attribute != null ? $" [{node.Attribute}]" : ""));
            }
        }
    }
}
