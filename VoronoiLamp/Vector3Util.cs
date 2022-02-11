using System;
using System.Numerics;
namespace VoronoiLamp
{
    public static class Vector3Util
    {
        public static double Angle(Vector3 v1, Vector3 v2)
        {
            return Math.Acos(Vector3.Dot(v1, v2) /
                (Math.Sqrt(Vector3.Dot(v1, v1)) *
                Math.Sqrt(Vector3.Dot(v2, v2))));
        }
        public static double Length(Vector3 v)
        {
            return Math.Sqrt(Vector3.Dot(v, v));
        }
        public static Vector3 UX { get; } = new Vector3(1, 0, 0);
        public static Vector3 UY { get; } = new Vector3(0, 1, 0);
        public static Vector3 UZ { get; } = new Vector3(0, 0, 1);
        public static Vector3 Origin { get; } = new Vector3(0, 0, 0);
    }
}
