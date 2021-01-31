using System;
using System.Collections.Generic;

namespace BlockMonitor
{
    static class Status
    {
        public static List<NodeBlockCount> BlockCountList = new List<NodeBlockCount>();

        public static int BlockCount;

        public static DateTime Time = DateTime.Now;
    }
}
