using Clipper2Lib;
using UnityEngine;

namespace LIBII.Light2D
{
    public static class Light2DExtensions
    {
        public static Color AlphaMultiplied(this Color c, float multiplier) =>
            new Color(c.r, c.g, c.b, c.a * multiplier);

        public static Vector2 ToVector2(this PointD p)
        {
            return new Vector2((float) p.x, (float) p.y);
        }

        public static PointD ToPointD(this Vector2 p)
        {
            return new PointD(p.x, p.y);
        }

        public static Vector2[] ToVector2(this PointD[] ps)
        {
            var vs = new Vector2[ps.Length];
            for (int i = 0; i < vs.Length; i++)
            {
                vs[i] = ps[i].ToVector2();
            }

            return vs;
        }
    }
}