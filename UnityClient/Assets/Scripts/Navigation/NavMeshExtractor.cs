using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Navigation;
using GeometRi;
using UnityEngine;
using NavMesh = UnityEngine.AI.NavMesh;

namespace Assets.Scripts.Navigation
{
    public class NavMeshExtractor
    {
        private Dictionary<UnitySegment, int> navMeshSegmentCounter = new Dictionary<UnitySegment, int>();

        public List<NavMeshSegment> GetOutline()
        {
            navMeshSegmentCounter.Clear();
            var triangulation = NavMesh.CalculateTriangulation();

            for (int i = 0; i < triangulation.indices.Length; i += 3)
            {
                int triIndex = i / 3;
                int area = triangulation.areas[triIndex];

                if (area != 0)
                    continue;

                var a = triangulation.vertices[triangulation.indices[i]];
                var b = triangulation.vertices[triangulation.indices[i + 1]];
                var c = triangulation.vertices[triangulation.indices[i + 2]];
                var s1 = new UnitySegment(a, b);
                var s2 = new UnitySegment(b, c);
                var s3 = new UnitySegment(c, a);

                AddSegmentToCounter(s1);
                AddSegmentToCounter(s2);
                AddSegmentToCounter(s3);
            }

            return navMeshSegmentCounter.Where(x => x.Value == 1)
                .Select(x => new NavMeshSegment()
                {
                    A = ToNavMeshVector(x.Key.A),
                    B = ToNavMeshVector(x.Key.B)
                }).ToList();
        }

        public List<NavMeshSegment> GetOutline(Point3d point, float radius)
        {
            var outline = GetOutline();
            var segments = outline.ConvertAll(x => new Segment3d(new Point3d(x.A.X, 0, x.A.Z), new Point3d(x.B.X, 0, x.B.Z)));

            List<NavMeshSegment> result = new List<NavMeshSegment>();
            for (var i = 0; i < segments.Count; i++)
            {
                var s = segments[i];
                if (s.DistanceTo(point) <= radius)
                {
                    result.Add(outline[i]);
                }
            }

            return result;
        }

        private static NavMeshVector ToNavMeshVector(Vector3 vec)
        {
            return new NavMeshVector()
            {
                X = vec.x,
                Y = vec.y,
                Z = vec.z
            };
        }

        private void AddSegmentToCounter(UnitySegment s)
        {
            if (!navMeshSegmentCounter.ContainsKey(s))
            {
                navMeshSegmentCounter.Add(s, 1);
            }
            else
            {
                navMeshSegmentCounter[s]++;
            }
        }
    }
}
