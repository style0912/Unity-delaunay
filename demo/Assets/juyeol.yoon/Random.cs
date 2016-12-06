using UnityEngine;
using System.Collections;


namespace style0912 {
    public class Random {
        readonly System.Random _random;
        public Random() {
            _random = new System.Random();
        }
        public Random(int seed) {
            _random = new System.Random(seed);
        }

        // 0.0 ~ 1.0f
        public float Value() {
            return _random.Next(0, 101) * 0.01f;
        }

        public int Next() {
            return _random.Next();
        }

        public int Next(int maxValue) {
            return _random.Next(maxValue);
        }

        public int Next(int minValue, int maxValue) {
            return _random.Next(minValue, maxValue);
        }

        public float Next(float minValue, float maxValue) {
            return minValue + ((maxValue - minValue) * Value());
        }

        #region 컬러
        public Color Color() {
            return new Color32((byte)Next(0, 256), (byte)Next(0, 256), (byte)Next(0, 256), 255);
        }
        public Vector3 Vector3(float minValue, float maxValue) {
            return new Vector3(
                Next(minValue, maxValue),
                Next(minValue, maxValue),
                Next(minValue, maxValue)
                );
        }
        #endregion
    }
}