using System;
using System.Collections.Generic;
using Clipper2Lib;
using UnityEngine;

namespace LIBII.Light2D
{
    [System.Serializable]
    public class FreeFormPoints
    {
        [SerializeField] public List<Vector2> points = new List<Vector2>();
        [SerializeField] public List<float> pointDensities = new List<float>();

        [SerializeField] public Mesh mesh;

        public FreeFormPoints()
        {
            float size = 1;
            float i = 0;
            float pointsCount = 3;

            while (i < 360)
            {
                points.Add(new Vector2(Mathf.Cos(i * Mathf.Deg2Rad) * size, Mathf.Sin(i * Mathf.Deg2Rad) * size));

                i += 360f / (float) pointsCount;
            }
        }
    }

    public class LightFreeForm
    {
        public Polygon2 polygon = new Polygon2(1);

        public Rect worldRect = new Rect();

        private bool update = true;

        public bool UpdateNeeded = false;

        private float falloff;

        public void ForceUpdate()
        {
            update = true;
        }

        // only if something changed (UI / API)

        public void Update(Light2D source)
        {
            if (!update)
            {
                return;
            }

            update = false;

            bool changeUpdate = Mathf.Abs(falloff - source.freeFormFalloff) > Mathf.Epsilon;

            if (source.freeFormPoints.points.Count != polygon.points.Length)
            {
                System.Array.Resize(ref polygon.points, source.freeFormPoints.points.Count);

                changeUpdate = true;
            }

            float minSize = 0;

            for (int i = 0; i < polygon.points.Length; i++)
            {
                Vector2 point = polygon.points[i];

                Vector2 cPoint = source.freeFormPoints.points[i];

                minSize = Mathf.Max(minSize, cPoint.magnitude + source.freeFormFalloff);

                if (point != cPoint)
                {
                    changeUpdate = true;
                    polygon.points[i] = cPoint;
                }
            }

            if (minSize < source.size)
            {
                source.size = minSize;
            }

            if (minSize > source.size)
            {
                source.size = minSize;
            }

            if (changeUpdate)
            {
                UpdateNeeded = true;
                falloff = source.freeFormFalloff;

                worldRect = polygon.GetRect();
            }
        }

        private Mesh m_Mesh;
        public FreeFormPoints source;


        public Mesh GetMesh()
        {
#if UNITY_EDITOR
            if (UpdateNeeded || m_Mesh == null)
            {
                UpdateNeeded = false;
                polygon.strengthes = new float[polygon.points.Length];
                Array.Fill(polygon.strengthes, 1);
                if (falloff != 0)
                {
                    var outline = GetOutline(falloff);
                    var poly = new Polygon2(outline)
                    {
                        holes = new[] {polygon.Copy()},
                        strengthes = new float[outline.Length]
                    };

                    Array.Fill(poly.strengthes, 0);

                    m_Mesh = poly.CreateMesh(Vector2.one, Vector2.zero, PolygonTriangulator2.Triangulation.Advanced,
                        true);
                }
                else
                {
                    m_Mesh = polygon.CreateMesh(Vector2.one, Vector2.zero);
                }
            }

            source.mesh = m_Mesh;
#else
            m_Mesh = source.mesh;
#endif

            return m_Mesh;
        }

        private Vector2[] GetOutline(float size)
        {
            List<PointD> p = new List<PointD>();
            for (int i = 0; i < polygon.points.Length; i++)
            {
                var v = polygon.points[i];
                p.Add(v.ToPointD());
            }

            var result = Clipper.InflatePaths(new List<List<PointD>>() {p}, size, JoinType.Round, EndType.Polygon, .5f);
            return result[0].ToArray().ToVector2();
        }
    }
}