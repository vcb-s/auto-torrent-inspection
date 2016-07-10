using System;
using System.Linq;
using System.Collections.Generic;

namespace AutoTorrentInspection.Util
{
    public class Node : Dictionary<string, Node>
    {
        public FileSize Attribute { get; private set; }

        private Node ParentNode { get; set; }

        public string NodeName { get; private set; } = "."; //string.Empty;

        public override string ToString() => NodeName;

        public Node() { }

        public string GetJson()
        {
            return "[" + _GetJson().TrimEnd(',') + "]";
        }

        private string _GetJson(int depth = 0)
        {
            string json = string.Empty;
            string subJson = Values.Aggregate(string.Empty, (current, value) => current + value._GetJson(depth + 1));
            string tab = new string(' ', depth*2);
            switch (NodeType)
            {
                case NodeTypeEnum.File:
                    json += $"{tab}{{\"type\":\"{NodeType.ToString().ToLower()}\",\"name\":\"{NodeName}\",\"size\":{Attribute.Length}}},\n";
                    break;
                case NodeTypeEnum.Directory:
                    json += $"{tab}{{\"type\":\"{NodeType.ToString().ToLower()}\",\"name\":\"{NodeName}\",\"contents\":[\n{subJson}{tab}]}},\n";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return json;
        }

        public Node(IEnumerable<IEnumerable<string>> fileList)
        {
            foreach (var list in fileList)
            {
                Insert(list);
            }
        }

        public Node(IEnumerable<KeyValuePair<IEnumerable<string>, FileSize>> fileList)
        {
            foreach (var list in fileList)
            {
                Insert(list.Key, list.Value);
            }
        }

        private Node(string node)
        {
            NodeName = node;
        }


        public enum NodeTypeEnum
        {
            File,
            Directory
        }

        public NodeTypeEnum NodeType => Values.Count == 0 ? NodeTypeEnum.File : NodeTypeEnum.Directory;

        public IEnumerable<Node> GetFiles()
        {
            return Values.Where(item => item.Count == 0);
        }

        public IEnumerable<Node> GetDirectories()
        {
            return Values.Where(item => item.Count > 0);
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
            for (var currentNode = ParentNode; currentNode != null; currentNode = currentNode.ParentNode)
            {
                yield return currentNode;
            }
        }

        public Node Insert(IEnumerable<string> nodes, FileSize attribute = null)
        {
            var currentNode = this;
            foreach (string node in nodes)
            {
                if (!currentNode.ContainsKey(node))
                {
                    currentNode.Add(node, new Node(node));
                    currentNode[node].ParentNode = currentNode;
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
                var treeNode = tn.Insert(tn.Count, node.NodeName); //将文件夹插入当前TreeNode节点的末尾
                var folderLength = InsertToViewInner(node, treeNode.Nodes); //由于是文件夹，故获取其子项并继续插入
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
