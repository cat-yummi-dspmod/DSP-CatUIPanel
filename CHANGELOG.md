# Changelog

## 0.1.6

- Refit window size from measured widget bounds after `Canvas.ForceUpdateCanvases()` (fixes content clipped past frame).
- Refresh wrapped label/checkbox heights before measuring; refit again on each open.

## 0.1.5
- Left tabs use UXA-style dyson orbit buttons (fixes wide gray/white tab strip).
- Checkboxes: never stretch root Image to row width (fixes white bars); label wraps beside the box.
- 左侧分页改为 UXA 轨道按钮样式；复选框不再拉宽方框根节点。

## 0.1.4
- Window sizing like UXA: only root `sizeDelta`; `panel-bg`/`shadow` stretch to fill (fixes visible white rim).
- 窗口尺寸与 UXA 一致：只改根节点，panel-bg 拉伸铺满（去掉白边）。

## 0.1.3
- Fix panel frame: parent UI inside `panel-bg`, hoist `panel-bg` to window root, set explicit `panel-bg`/`shadow` size from content (fixes overflow outside the border).
- 修复面板边框：控件放进 panel-bg，理顺层级并按内容撑开 panel-bg 尺寸。

## 0.1.2
- Fix window sizing: stretch `panel-bg`/`shadow`, measure wrapped checkbox/label height, wider content column.
- API v2: `RegisterPage(string key)` and single-string labels on controls; `CurrentPageKey` replaces `CurrentPageId`.
- 修复窗口尺寸：背景随窗口拉伸，复选框/说明文字换行计入高度，内容区更宽。
- API v2：单字符串注册分页与控件文案。

## 0.1.1
- Larger panel: wider content area, taller buttons/tabs, 18px body text.
- Cat launcher sits 0.25 button-heights lower (closer to screenshot).
- New Thunderstore icon art.
- 面板加大：内容区更宽、按钮/分页更高、正文 18 号字。
- 小猫按钮下移 0.25 个按钮高度（更靠近截图键）。
- 更换发布用封面图。

## 0.1.0
- First version: cat button above the screenshot button, shared panel with one tab per mod.
- Controls: checkbox (bound to `ConfigEntry<bool>`), button, label.
- `UIPanel.OpenPage` / `TogglePage` / `Close`; open requests made outside a game wait until a game is entered.
- 首个版本：截图按钮上方的小猫按钮，每个 mod 一个分页的共用面板。
- 控件：复选框（绑定 `ConfigEntry<bool>`）、按钮、文字。
- `UIPanel.OpenPage` / `TogglePage` / `Close`；不在游戏中发起的打开请求会等到进入游戏后执行。
