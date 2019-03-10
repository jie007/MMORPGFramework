using UnityEngine;

namespace Assets.Scripts.Navigation
{
    public class UnitySegment
    {
        public Vector3 A;
        public Vector3 B;

        public UnitySegment(Vector3 a, Vector3 b)
        {
            A = a;
            B = b;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            return this == obj as UnitySegment;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public static bool operator ==(UnitySegment a, UnitySegment b)
        {
            if ((object) a == null)
            {
                return (object) b == null;
            }

            if ((object) b == null)
                return false;

            return AreClose(a.A, b.A) && AreClose(a.B, b.B) || AreClose(a.B, b.A) && AreClose(a.A, b.B);
        }

        public static bool operator !=(UnitySegment a, UnitySegment b)
        {
            return !(a == b);
        }

        private static bool AreClose(Vector3 a, Vector3 b)
        {
            var distance = (a - b).magnitude;

            return distance < 0.000001f;
        }

    }
}