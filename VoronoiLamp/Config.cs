using System;
using System.Numerics;
using System.Collections.Generic;
namespace VoronoiLamp
{
    public class Config
    {
        public uint Seeds { get; set; }
        public float StepSize { get; set; }
        public int Steps { get; set; }
        public float MaxDist { get; set; }
        public float SurfaceThickness { get; set; }
        public int OuterWalls { get; set; }
        public float WallThickness { get; set; }
        public float SeedVariation { get; set; }
        public float RoofHeight { get; set; }
        public bool CreateNew { get; set; }
        public bool Pillars { get; set; }
        public int GridSegments { get; set; }
        public bool RoofLines { get; set; }
        public bool RoofBorder { get; set; }
        public bool RoofLineSupport { get; set; }
        public List<Triangle> Boundaries { get; set; }
        public List<Vector3> SeedPositions { get; set; }
    }
}
