using UnityEngine;
using UnityEngine.UI;

namespace CatUIPanel.UI
{
    /// <summary>保证拖动滑条时数字框每帧跟随（部分原版 Slider 预制体 onValueChanged 不可靠）。</summary>
    internal sealed class SliderRowDriver : MonoBehaviour
    {
        public Slider Slider;
        public InputField Input;
        public float Min;
        public float Max;
        public int Decimals;
        public System.Action<float> OnValue;

        private float lastSliderValue = float.NaN;

        private void Start()
        {
            if (Slider != null)
            {
                lastSliderValue = Slider.value;
            }
        }

        private void Update()
        {
            if (Slider == null || Input == null)
            {
                return;
            }
            float v = Slider.value;
            if (float.IsNaN(lastSliderValue) || Mathf.Abs(v - lastSliderValue) > 1e-7f)
            {
                lastSliderValue = v;
                float clamped = Mathf.Clamp(v, Min, Max);
                Widgets.SetNumericInputDisplay(Input, clamped, Decimals);
                OnValue?.Invoke(clamped);
            }
        }
    }
}
