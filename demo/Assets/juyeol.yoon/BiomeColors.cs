using UnityEngine;
using System.Collections.Generic;


namespace style0912 {
    public class BiomeColors : ScriptableObject {

        [System.Serializable]
        public class BiomeColor {
            public Biome biome;
            public Color color;

            public BiomeColor(Biome biome, Color color) {
                this.biome = biome;
                this.color = color;
            }
        }

        [SerializeField]
        public List<BiomeColor> theme;

        Dictionary<Biome, BiomeColor> colorMap = null;

        public Dictionary<Biome, BiomeColor> Colors {
            get {
                if(null == colorMap) {
                    colorMap = new Dictionary<Biome, BiomeColor>();

                    if(null == theme || 0 == theme.Count) {
                        foreach(var element in BiomeProperties.Colors) {
                            theme.Add(new BiomeColor(element.Key, element.Value));
                        }
                    }

                    foreach(var element in theme) {
                        if (colorMap.ContainsKey(element.biome)) {
                            Debug.Log("Alreay contains BiomeColor : " + element.biome);
                        }
                        else {
                            colorMap.Add(element.biome, element);
                        }
                    }
                }
                return colorMap;
            }
        }
    }
}