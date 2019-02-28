using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Protocols.Map;

namespace CommonServer.MapPartitioning
{
    [Serializable]
    public class MapPartition
    {
        public int X { get; set; }

        public int Y { get; set; }

        public MapPartition()
        {
            X = 0;
            Y = 0;
        }

        public MapPartition(PositionMessage msg)
        {
            X = (int)(msg.X / GameDesign.MapAreaSize) * (int)GameDesign.MapAreaSize;
            Y = (int) (msg.Z / GameDesign.MapAreaSize) * (int) GameDesign.MapAreaSize;
        }

        public MapPartition(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            var otherPartition = obj as MapPartition;
            if (otherPartition == null)
                return false;

            return X == otherPartition.X && Y == otherPartition.Y;
        }

        public static bool operator ==(MapPartition a, MapPartition b)
        {
            if ((object)a == null)
            {
                return (object)b == null;
            }

            return a.Equals(b);
        }

        public static bool operator !=(MapPartition a, MapPartition b)
        {
            return !(a == b);
        }
    }
}
