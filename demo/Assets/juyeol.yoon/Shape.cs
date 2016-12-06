using UnityEngine;
using System.Collections;


namespace style0912 {
    public static class Shape {
        public static float ISLAND_FACTOR = 1.07f;  // 1.0 means no small islands; 2.0 leads to a lot
        public static System.Func<Vector2, bool> MakeRadial(Random seed) {
            var bumps = seed.Next(1, 7);
            var startAngle = seed.Value() * 2 * Mathf.PI;
            var dipAngle = seed.Value() * 2 * Mathf.PI;

            var random = seed.Value();
            var start = 0.2f;
            var end = 0.7f;

            var dipWidth = (end - start) * random + start;

            System.Func<Vector2, bool> inside = q => {
                var angle = Mathf.Atan2(q.y, q.x);
                var length = 0.5 * (Mathf.Max(Mathf.Abs(q.x), Mathf.Abs(q.y)) + q.magnitude);

                var r1 = 0.5 + 0.40 * Mathf.Sin(startAngle + bumps * angle + Mathf.Cos((bumps + 3) * angle));
                var r2 = 0.7 - 0.20 * Mathf.Sin(startAngle + bumps * angle - Mathf.Sin((bumps + 2) * angle));
                if (Mathf.Abs(angle - dipAngle) < dipWidth
                    || Mathf.Abs(angle - dipAngle + 2 * Mathf.PI) < dipWidth
                    || Mathf.Abs(angle - dipAngle - 2 * Mathf.PI) < dipWidth) {
                    r1 = r2 = 0.2;
                }
                var result = (length < r1 || (length > r1 * ISLAND_FACTOR && length < r2));
                return result;
            };

            return inside;
        }

        public static System.Func<Vector2, bool> MakePerlin(Random seed) {
            var offset = seed.Next(0, 100001);
            System.Func<Vector2, bool> inside = q => {
                var x = q.x + offset;
                var y = q.y + offset;
                var perlin = Mathf.PerlinNoise(x / 10f, y / 10f);
                var checkValue = (0.3f + 0.3f * q.magnitude * q.magnitude);
                var result = perlin > 0.3f;
                return result;
            };
            return inside;
        }

        public static System.Func<Vector2, bool> MakeSquare() {
            System.Func<Vector2, bool> inside = q => { return true; };
            return inside;
        }
    }
}