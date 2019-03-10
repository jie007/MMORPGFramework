using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Navigation;
using GeometRi;

namespace Common.Mobs
{
    [Serializable]
    public class MobWanderInformation
    {
        public NavMeshVector WanderingCenter;
        public NavMeshVector StartPosition;
        public NavMeshVector GoalPosition;

        public int CurrentPoint;
        public float Radius;

        public string Name;
        public DateTime StartPositionTime;
        public int ToNextPoint;
        public int Waiting;

        public List<NavMeshSegment> WanderingSegments = new List<NavMeshSegment>();
        private List<Segment3d> navMesh = new List<Segment3d>();

        public void Initialize(DateTime startTime, NavMeshVector startPosition)
        {
            this.navMesh = WanderingSegments.ConvertAll(x =>
                new Segment3d(new Point3d(x.A.X, 0, x.A.Z), new Point3d(x.B.X, 0, x.B.Z)));
            StartPositionTime = startTime;
            WanderingCenter = startPosition;
            StartPosition = startPosition;
            GoalPosition = GetGoalPoint(CurrentPoint);
        }

        public void CalculateNextPoint(DateTime currentTime)
        {
            DateTime nextPoint = StartPositionTime + TimeSpan.FromMilliseconds(ToNextPoint + Waiting);
            while (currentTime > nextPoint)
            {
                StartPositionTime = nextPoint;
                CurrentPoint++;

                StartPosition = GoalPosition;
                GoalPosition = GetGoalPoint(CurrentPoint);

                nextPoint = StartPositionTime + TimeSpan.FromMilliseconds(ToNextPoint + Waiting);
            }
        }

        private NavMeshVector GetGoalPoint(int point)
        {
            string xRnd = Name + "X" + point;
            string zRnd = Name + "Z" + point;

            var newGoal = new NavMeshVector()
            {
                X = DeterministicRandom.Get(xRnd, WanderingCenter.X - Radius, WanderingCenter.X + Radius),
                Y = WanderingCenter.Y,
                Z = DeterministicRandom.Get(zRnd, WanderingCenter.Z - Radius, WanderingCenter.Z + Radius)
            };

            Segment3d path = new Segment3d(new Point3d(StartPosition.X, 0, StartPosition.Z), new Point3d(newGoal.X, 0, newGoal.Z));
            List<Point3d> possibleHits = new List<Point3d>();
            foreach (var s in navMesh)
            {
                var result = path.IntersectionWith(s);
                if (result == null)
                    continue;
                Point3d pResult = result as Point3d;
                Segment3d sResult = result as Segment3d;

                if (pResult != null)
                {
                    var hit = pResult + ((path.P1 - pResult).ToVector.Normalized * 0.001f).ToPoint;
                    possibleHits.Add(hit);
                }
                else if (sResult != null)
                {
                    var hit1 = sResult.P1 + ((path.P1 - sResult.P1).ToVector.Normalized * 0.001f).ToPoint;
                    possibleHits.Add(hit1);
                    var hit2 = sResult.P2 + ((path.P1 - sResult.P2).ToVector.Normalized * 0.001f).ToPoint;
                    possibleHits.Add(hit2);
                }
            }

            if (possibleHits.Count == 0)
                return newGoal;

            float lowestDistance = float.MaxValue;
            Point3d newGoalHit = new Point3d(newGoal.X, WanderingCenter.Y, newGoal.Z);
            foreach (var hit in possibleHits)
            {
                var distance = hit.DistanceTo(path.P1);

                if (distance < lowestDistance)
                {
                    newGoalHit = hit;
                    lowestDistance = (float)distance;
                }
            }

            return new NavMeshVector()
            {
                X = (float)newGoalHit.X,
                Y = WanderingCenter.Y,
                Z = (float)newGoalHit.Z
            };
        }

        public Vector3d GetPosition(DateTime currentTime)
        {
            float walkingTime = (float)(currentTime - StartPositionTime).TotalMilliseconds;
            float walkingPercentage = walkingTime / (float)ToNextPoint;
            if (walkingPercentage > 1)
            {
                walkingPercentage = 1;
            }

            if (walkingPercentage < 0)
            {
                walkingPercentage = 0;
            }

            Vector3d goal = ToVector3d(GoalPosition);
            Vector3d start = ToVector3d(StartPosition);
            var walkingDir = goal - start;
            var result = start + walkingDir * walkingPercentage;
            return result;
        }

        private Vector3d ToVector3d(NavMeshVector vec)
        {
            return new Vector3d(vec.X, vec.Y, vec.Z);
        }

    }
}
