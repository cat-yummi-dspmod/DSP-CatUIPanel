using UnityEngine;

namespace YummiUIHarness.UI
{
    /// <summary>运行时画出的白色扁平猫头，和原版按钮上的白色图标同一种画法。</summary>
    internal static class CatIcon
    {
        private const int Size = 64;
        private const int Supersample = 4;
        private static Sprite sprite;

        public static Sprite Get()
        {
            if (sprite != null)
            {
                return sprite;
            }
            var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
            {
                name = "yummi-cat-icon",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color32[Size * Size];
            const int samples = Supersample * Supersample;
            for (int py = 0; py < Size; py++)
            {
                for (int px = 0; px < Size; px++)
                {
                    int hit = 0;
                    for (int sy = 0; sy < Supersample; sy++)
                    {
                        for (int sx = 0; sx < Supersample; sx++)
                        {
                            float u = (px + (sx + 0.5f) / Supersample) / Size * 2f - 1f;
                            float v = (py + (sy + 0.5f) / Supersample) / Size * 2f - 1f;
                            if (Inside(u, v))
                            {
                                hit++;
                            }
                        }
                    }
                    pixels[py * Size + px] = new Color32(255, 255, 255, (byte)(255 * hit / samples));
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply(false, true);
            sprite = Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), 100f);
            sprite.name = "yummi-cat-icon";
            return sprite;
        }

        // 坐标范围 [-1, 1]，y 朝上
        private static bool Inside(float x, float y)
        {
            bool head = Ellipse(x, y, 0f, -0.14f, 0.80f, 0.64f)
                || Triangle(x, y, -0.74f, 0.00f, -0.62f, 0.92f, -0.12f, 0.45f)
                || Triangle(x, y, 0.74f, 0.00f, 0.62f, 0.92f, 0.12f, 0.45f);
            if (!head)
            {
                return false;
            }
            bool eye = Ellipse(x, y, -0.32f, -0.08f, 0.12f, 0.17f) || Ellipse(x, y, 0.32f, -0.08f, 0.12f, 0.17f);
            bool nose = Triangle(x, y, -0.09f, -0.31f, 0.09f, -0.31f, 0f, -0.42f);
            return !eye && !nose;
        }

        private static bool Ellipse(float x, float y, float cx, float cy, float rx, float ry)
        {
            float dx = (x - cx) / rx;
            float dy = (y - cy) / ry;
            return dx * dx + dy * dy <= 1f;
        }

        private static bool Triangle(float x, float y, float ax, float ay, float bx, float by, float cx, float cy)
        {
            float d1 = (x - bx) * (ay - by) - (ax - bx) * (y - by);
            float d2 = (x - cx) * (by - cy) - (bx - cx) * (y - cy);
            float d3 = (x - ax) * (cy - ay) - (cx - ax) * (y - ay);
            bool neg = d1 < 0 || d2 < 0 || d3 < 0;
            bool pos = d1 > 0 || d2 > 0 || d3 > 0;
            return !(neg && pos);
        }
    }
}
