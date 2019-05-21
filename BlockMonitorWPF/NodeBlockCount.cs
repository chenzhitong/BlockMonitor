using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockMonitor
{
    class NodeBlockCount
    {
        public string Node;
        public int BlockCount;

        public NodeBlockCount(string node, int blockCount)
        {
            Node = node;
            BlockCount = blockCount;
        }

        public void Refresh()
        {
            BlockCount = Tools.GetBlockCount(Node);
        }

        public override string ToString()
        {
            return $"{Node}\t {BlockCount}";
        }
    }
}
