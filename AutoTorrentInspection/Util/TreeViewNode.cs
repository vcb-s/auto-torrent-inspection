using System;
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

        public enum NodeTypeEnum
        {
            File,
            Directory
        }

        public NodeTypeEnum NodeType => _childNodes.Count == 0 ? NodeTypeEnum.File : NodeTypeEnum.Directory;

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
                path = GetParentsNode().Aggregate(path, (current, node) => node + separator + current);
                if (NodeType == NodeTypeEnum.Directory) path += separator;
                return path;
            }
        }

        private IEnumerable<Node> GetParentsNode()
        {
            for (var currentNode = _parentNode; currentNode != null; currentNode = currentNode._parentNode)
            {
                yield return currentNode;
            }
        }

        public Dictionary<string, Node>.Enumerator GetEnumerator()
        {
            return _childNodes.GetEnumerator();
        }

        public Node Insert(IEnumerable<string> nodes, FileSize attribute = null)
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

        public long InsertTo(System.Windows.Forms.TreeNodeCollection tn)
        {
            return InsertToViewInner(this, tn);
        }

        private static long InsertToViewInner(Node currentNode, System.Windows.Forms.TreeNodeCollection tn)
        {
            long length = 0;
            foreach (var node in currentNode.GetDirectories())
            {
                var treeNode = tn.Insert(tn.Count, node.NodeName);//将文件夹插入当前TreeNode节点的末尾
                var folderLength = InsertToViewInner(node, treeNode.Nodes);//由于是文件夹，故获取其子项并继续插入
                if (folderLength != 0) treeNode.Text += $" [{FileSize.FileSizeToString(folderLength)}]";
                length += folderLength;
            }
            foreach (var node in currentNode.GetFiles())
            {
                //将文件插入当前TreeNode结点的末尾
                tn.Insert(tn.Count, node.NodeName + (node.Attribute != null ? $" [{node.Attribute}]" : ""));
                length += node.Attribute?.Length ?? 0;
            }
            return length;
        }
    }
}
