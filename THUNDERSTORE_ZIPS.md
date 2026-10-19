# Thunderstore 本地上传包

完整列表（含 **CatAPI**、**StationAssistant**、**BigTower**、**SafeBigTower** 等）见仓库根目录：[THUNDERSTORE_UPLOAD.md](../THUNDERSTORE_UPLOAD.md)

## 猫猫面板相关（CatUIPanel 线）

上传顺序：**CatUIPanel → MetadataBoost → MetadataNoCost**（均需先上传 **CatUIPanel 0.0.1**；元数据线不依赖 CatAPI）。

| 顺序 | Thunderstore 包名 | 本地 zip |
|------|-------------------|----------|
| 1 | `cat_yummi-CatUIPanel` | [CatUIPanel.zip](file:///E:/developing/dspmod/CatUIPanel/bin/Release/CatUIPanel.zip) · [相对路径](bin/Release/CatUIPanel.zip) |
| 2 | `cat_yummi-MetadataBoost` | [MetadataBoost.zip](file:///E:/developing/dspmod/DSP-MetadataBoost/bin/Release/MetadataBoost.zip) · [相对路径](../DSP-MetadataBoost/bin/Release/MetadataBoost.zip) |
| 3 | `cat_yummi-MetadataNoCost` | [MetadataNoCost.zip](file:///E:/developing/dspmod/DSP-MetadataNoCost/bin/Release/MetadataNoCost.zip) · [相对路径](../DSP-MetadataNoCost/bin/Release/MetadataNoCost.zip) |

```powershell
powershell -File E:\developing\dspmod\CatUIPanel\build_thunderstore.ps1
Set-Location E:\developing\dspmod\DSP-MetadataBoost; .\build_thunderstore.ps1
Set-Location E:\developing\dspmod\DSP-MetadataNoCost; .\build_thunderstore.ps1
```
