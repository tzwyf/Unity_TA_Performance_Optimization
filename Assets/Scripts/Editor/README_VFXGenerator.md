# VFX 粒子效果生成器

## 使用方式

1. 打开 Unity 2022.3.62f3。
2. 等待脚本编译完成。
3. 顶部菜单选择 `TA Tools > Generate Particle Effects`。
4. 点击对应按钮生成或安装效果。

常用菜单：

- `TA Tools > Install Showcase Particle Effects`：安装浮尘、电火花和咖啡热气示例。
- `TA Tools > Install Scifi Office Atmosphere`：安装适合直接使用的科幻办公室氛围粒子。
- `TA Tools > Install Particle Bombardment Stress Test`：安装极限粒子轰炸压力测试，不适合当正式氛围效果。

## 生成资源

- 贴图：`Assets/Art/Textures/Particles/T_SoftParticle.png`
- 贴图：`Assets/Art/Textures/Particles/T_ElectricSpark.png`
- 材质：`Assets/Art/Materials/Particles/M_DustAir.mat`
- 材质：`Assets/Art/Materials/Particles/M_ElectricSparks.mat`
- 材质：`Assets/Art/Materials/Particles/M_CoffeeSteam.mat`
- 材质：`Assets/Art/Materials/Particles/M_ScifiOffice_Haze.mat`
- 材质：`Assets/Art/Materials/Particles/M_ScifiOffice_DataMotes.mat`
- 材质：`Assets/Art/Materials/Particles/M_ScifiOffice_MicroSparks.mat`
- 材质：`Assets/Art/Materials/Particles/M_ParticleBombardment.mat`
- 预制体：`Assets/Prefabs/Particles/VFX/VFX_DustAir.prefab`
- 预制体：`Assets/Prefabs/Particles/VFX/VFX_ElectricSparks.prefab`
- 预制体：`Assets/Prefabs/Particles/VFX/VFX_CoffeeSteam.prefab`
- 预制体：`Assets/Prefabs/Particles/VFX/VFX_ScifiOfficeAmbientLoad.prefab`
- 预制体：`Assets/Prefabs/Particles/VFX/VFX_ParticleBombardment_Stress.prefab`
- Mesh：`Assets/Art/Models/Generated/M_ParticleBombardment_TessellatedQuad.asset`

## 可直接使用的科幻办公室氛围

### VFX_ScifiOfficeAmbientLoad

适合直接放进当前室内办公室场景的氛围型粒子组合。它不是极限轰炸效果，而是带有一定粒子负载、同时视觉上贴合科幻办公室的版本。

- `Room_Cool_Haze`：低透明冷色空气雾，让空间更有体积感。
- `Ceiling_Light_Motes_A/B`：灯下浮尘，增强局部光束和空气流动。
- `Vent_Coolant_Mist_A/B`：通风口薄雾，适合贴近墙边或设备散热口。
- `Screen_Data_Motes`：屏幕附近的青色数据微粒和短线。
- `Equipment_Micro_Sparks`：少量设备微火花，频率低，不会抢画面。

推荐使用菜单 `TA Tools > Install Scifi Office Atmosphere`，它会在 `Showcase_Scene` 中创建 `VFX_ScifiOfficeAtmosphereRoot`。

## 单项效果

### VFX_DustAir

室内空气浮尘效果，包含三个粒子子系统：

- `Ambient_Dust_Motes`：覆盖房间体积的中高密度微尘。
- `Light_Shaft_Motes`：更亮、更集中的光束尘埃层。
- `Soft_Room_Haze`：大颗粒、低透明度的空气雾感层，让画面更朦胧。

### VFX_ElectricSparks

电流火花效果，包含三个粒子子系统：

- `Arc_Streaks`：蓝白色高速电弧和拖尾。
- `Hot_Spark_Points`：短寿命白黄色火星。
- `Blue_Flash_Glow`：瞬时蓝色闪光。

### VFX_CoffeeSteam

热咖啡蒸汽效果，包含两个粒子子系统：

- `Steam_Wisps`：细长、缓慢上升的主蒸汽丝。
- `Steam_Soft_Puffs`：更大、更淡的柔雾体积，用来打散轮廓。

## 压力测试

### VFX_ParticleBombardment_Stress

粒子系统轰炸压力测试效果，专门用于制造高粒子数、高透明叠加、高碰撞和高顶点压力：

- 20 个子粒子系统，每个系统持续发射数百个大尺寸半透明粒子。
- 半数子系统使用 Stretch 粒子，增加拉伸与排序压力。
- 半数子系统使用 Mesh Particle 渲染模式，并绑定高细分 quad mesh。
- 所有子系统开启 World Collision，让 CPU/物理查询也参与压力。
- 多层透明 alpha blend 叠加，用于观察 GPU fill rate 和 overdraw。

这个效果会创建 `VFX_ParticleBombardment_StressRoot`，可能显著降低帧率。正式办公室氛围请使用 `VFX_ScifiOfficeAmbientLoad`。
