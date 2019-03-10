using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Protocols.Map;
using CommonServer.MapPartitioning;

namespace UdpMapService
{
    public class MapPartitionRegistration
    {
        public List<MapPartition> Partitions { get; set; }
        public string Map { get; set; }

        public MapPartitionRegistration(string map)
        {
            Partitions = new List<MapPartition>();
            Map = map;
        }

        public void Update(PositionMessage msg)
        {
            var partition = new MapPartition(msg);

            List<MapPartition> toRemove = new List<MapPartition>();
            foreach (var registeredPartition in Partitions)
            {
                int distance = Math.Max(registeredPartition.X - partition.X, registeredPartition.Y - partition.Y) /
                               GameDesign.MapAreaSize;

                if (distance > GameDesign.UnregistrationBorder)
                {
                    toRemove.Add(registeredPartition);
                }
            }

            foreach (var remove in toRemove)
            {
                Partitions.Remove(remove);
            }

            for (int x = -GameDesign.RegistrationBorder; x <= GameDesign.RegistrationBorder; x++)
            {
                for (int y = -GameDesign.RegistrationBorder; y <= GameDesign.RegistrationBorder; y++)
                {
                    Partitions.Add(new MapPartition(partition.X + (x * GameDesign.MapAreaSize), partition.Y + (y * GameDesign.MapAreaSize)));
                }
            }

            RemoveDuplicates();
        }

        private void RemoveDuplicates()
        {
            Partitions = Partitions.Distinct().ToList();
        }

        public void Merge(MapPartitionRegistration other)
        {
            Partitions.AddRange(other.Partitions);

            RemoveDuplicates();
        }
    }
}
