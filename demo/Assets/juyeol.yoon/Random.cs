﻿// by style0912@gmail.com.

namespace style0912 {
    public class Random : System.Random {
        //readonly System.Random _random;
        public Random() : base() {
            //_random = new System.Random();
        }
        public Random(int seed) : base(seed) {
            //_random = new System.Random(seed);
        }

        // 0.0 ~ 1.0f
        public float Value() {
            return base.Next(0, 101) * 0.01f;
        }

        override public int Next() {
            return base.Next();
        }

        override public int Next(int maxValue) {
            return base.Next(maxValue);
        }

        override public int Next(int minValue, int maxValue) {
            return base.Next(minValue, maxValue);
        }
        public int Range(int minValue, int maxValue) {
            return Next(minValue, maxValue + 1);
        }

        public float Range(float minValue, float maxValue) {
            return minValue + ((maxValue - minValue) * Value());
        }

        #region 컬러
        public UnityEngine.Color Color() {
            return new UnityEngine.Color32((byte)Next(0, 256), (byte)Next(0, 256), (byte)Next(0, 256), 255);
        }

        public UnityEngine.Color Color(float minOffset, float maxOffset) {
            return new UnityEngine.Color32((byte)Range(255 * minOffset, 255 * maxOffset), (byte)Range(255 * minOffset, 255 * maxOffset), (byte)Range(255 * minOffset, 255 * maxOffset), 255);
        }

        public UnityEngine.Vector3 Vector3(float minValue, float maxValue) {
            return new UnityEngine.Vector3(Range(minValue, maxValue), Range(minValue, maxValue), Range(minValue, maxValue));
        }
        #endregion
    }
}