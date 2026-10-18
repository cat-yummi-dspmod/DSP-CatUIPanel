using System;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace YummiUIPanel.UI
{
    /// <summary>右下角截图按钮正上方的小猫按钮。克隆截图按钮本身，大小和外观自然与之一致。</summary>
    internal static class LauncherButton
    {
        private static GameObject root;
        private static RectTransform rect;
        private static RectTransform photoRect;
        private static bool failed;

        public static void Update(Action onClick)
        {
            if (failed)
            {
                return;
            }
            if (root == null)
            {
                try
                {
                    Create(onClick);
                }
                catch (Exception e)
                {
                    failed = true;
                    Plugin.Log.LogError("Failed to create the cat button: " + e);
                    return;
                }
            }
            if (photoRect == null)
            {
                return;
            }
            if (root.activeSelf != photoRect.gameObject.activeSelf)
            {
                root.SetActive(photoRect.gameObject.activeSelf);
            }
            float height = photoRect.rect.height * photoRect.localScale.y;
            // 默认比「正上方」再向下 0.25 个按钮高度，更贴近截图按钮
            float yAbovePhoto = height * 0.75f + Plugin.LauncherOffsetY.Value;
            rect.anchoredPosition = photoRect.anchoredPosition
                + new Vector2(Plugin.LauncherOffsetX.Value, yAbovePhoto);
        }

        private static void Create(Action onClick)
        {
            Button photo = UIRoot.instance.uiGame.gameMenu.buttonS;
            photoRect = (RectTransform)photo.transform;
            root = Object.Instantiate(photo.gameObject, photoRect.parent, false);
            root.name = "yummi-ui-harness-launcher";
            rect = (RectTransform)root.transform;
            Widgets.StripLocalizers(root);

            var layout = root.GetComponent<LayoutElement>() ?? root.AddComponent<LayoutElement>();
            layout.ignoreLayout = true;

            Button button = root.GetComponent<Button>();
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(() => onClick());

            UIButton uiButton = root.GetComponent<UIButton>();
            if (uiButton != null)
            {
                uiButton.highlighted = false;
                ApplyTips(uiButton);
            }

            Image icon = FindIcon(root.transform);
            if (icon != null)
            {
                icon.sprite = CatIcon.Get();
                icon.overrideSprite = null;
                icon.preserveAspect = true;
                icon.enabled = true;
            }
            else
            {
                Plugin.Log.LogWarning("Screenshot button has no icon image; the cat button shows without an icon.");
            }
            root.SetActive(photo.gameObject.activeSelf);
        }

        public static void ApplyLanguage()
        {
            if (root == null)
            {
                return;
            }
            UIButton uiButton = root.GetComponent<UIButton>();
            if (uiButton != null)
            {
                ApplyTips(uiButton);
            }
        }

        private static void ApplyTips(UIButton uiButton)
        {
            uiButton.tips.tipTitle = L10n.Pick("悠米面板(F9)", "Yummi Panel (F9)");
            uiButton.tips.tipText = L10n.Pick("一些mod的面板入口", "Entry to some mod panels");
            if (uiButton.tips.corner == 0)
            {
                uiButton.tips.corner = 7;
            }
        }

        private static Image FindIcon(Transform t)
        {
            Image fallback = null;
            foreach (Image img in t.GetComponentsInChildren<Image>(true))
            {
                if (img.transform == t)
                {
                    continue;
                }
                if (img.gameObject.name.IndexOf("icon", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return img;
                }
                fallback ??= img;
            }
            return fallback ?? t.GetComponent<Image>();
        }
    }
}
