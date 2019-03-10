using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Navigation;
using GeometRi;
using UnityEngine;

namespace Assets.Scripts
{
    public static class GizmoHelper
    {
        public static void DrawSegement(NavMeshSegment s)
        {
            Gizmos.DrawLine(ToUnityVector(s.A), ToUnityVector(s.B));
        }

        private static Vector3 ToUnityVector(NavMeshVector p)
        {
            return new Vector3(p.X, p.Y, p.Z);
        }
    }
}
