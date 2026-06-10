# 优化前：
<img width="2880" height="1704" alt="Profiler_Render" src="https://github.com/user-attachments/assets/6301fcf7-acd1-4652-8885-1e0b33580bb6" />

# 优化后：
<img width="2880" height="1704" alt="Profiler_Render_Opt" src="https://github.com/user-attachments/assets/67847140-a4e0-4e97-811d-2fb6594bc1c4" />

# 最终数据整理
## 1. 完整数据表
### 1.1 优化阶段总览
| 优化阶段 | 核心指标 | 优化前 | 优化后 | 变化率 | 主要瓶颈 |
|---------|---------|--------|--------|--------|---------|
| **初始状态** | Draw Calls | 4,000+ | - | - | 材质过多、阴影占3000+ |
| | GPU | 高 | - | - | Overdraw严重 |
| | CPU | 高 | - | - | 实时阴影、UGUI刷新 |
| **Static Batching** | Batches | 4,397 | 1,918 | **-56.4%** | 几何数据提交 |
| | Draw Calls | 4.6k | 3.6k | -21.7% | |
| | Triangles | 4.2M | 4.2M | 无变化 | |
| | Vertices | 3.4M | 3.4M | 无变化 | |
| | Used Buffers | - | +34MB | 内存代价 | |
| **GPU Instancing** | Batches | 1,848 | 1,679 | -9.1% | 材质/Shader变体 |
| | Static Batches | 602 | 388 | -35.5% | |
| | Draw Calls | 3.6k | 3.5k | -2.8% | |
| | SetPass Calls | 385 | 387 | 无变化 | |
| | Used Buffers | 287MB | 267.2MB | -20MB | |
| **SRP Batcher** | CPU Frame Time | 11.03ms | 5.39ms | **-51.1%** | CPU→GPU瓶颈转移 |
| | SetPass Calls | 386 | 351 | -9.1% | |
| | Draw Calls | 3.5k | 3.5k | 无变化 | |
| | Batches | 1,680 | 1,695 | 无变化 | |
| | Triangles | 4.2M | 4.2M | 无变化 | |
| | Shadow Casters | 4,022 | 4,003 | 无变化 | |
| **UI优化** | CPU主线程波动 | 高 | 低 | 明显改善 | 动态UI刷新 |
| | Canvas Rebuild | 频繁 | 减少 | 优化 | |
| | GC Alloc | 多 | 明显减少 | 优化 | |
| **透明/粒子优化** | GPU Overdraw | 严重(白色高热区) | 蓝绿区域 | 显著改善 | FillRate/Overdraw |
| **Occlusion Culling** | Draw Calls | 3.5k | 3.5k | 无变化 | 未生效 |
| | Triangles | 4.2M | 4.2M | 无变化 | |
| | SetPass Calls | 283 | 283 | 无变化 | |
| **LOD优化** | Draw Calls | 3.5k | 3.0k | **-14.3%** | 模型复杂度 |
| | Triangles | 4.2M | 3.4M | -19.0% | |
| | Vertices | 3.4M | 2.9M | -14.7% | |
| | SetPass Calls | 283 | 273 | -3.5% | |
| | Shadow Casters | 4,002 | 2,971 | **-25.8%** | |
| **阴影光照优化(Mixed)** | Draw Calls | - | - | **-40%** | 实时阴影计算 |
| | Batches | - | - | -30.6% | |
| | Triangles | - | - | -32% | |
| | Vertices | - | - | -32% | |
| | Shadow Casters | - | - | -40% | |
| **URP Asset参数调整** | Draw Calls | 1,800 | 1,200 | **-33.3%** | 多维度综合 |
| | Batches | 1,447 | 968 | -33.1% | |
| | SetPass Calls | 206 | 164 | -20.4% | |
| | Triangles | 2.3M | 1.6M | -30.4% | |
| | Vertices | 2.0M | 1.3M | -35.0% | |
| | Shadow Casters | 1,796 | 802 | **-55.3%** | |
| | Used Buffers | 100.5MB | 240.7MB | +139.5% | 显存增长⚠️ |
| **CPU波峰优化** | CPU波峰 | 频繁 | 明显减少 | 优化 | 动态文本刷新 |
---
## 2. FPS变化统计
```
PC 2880x1800分辨率下打包运行帧率 60-70 → 100-110
```
## 3. DrawCall变化统计
### 3.1 DrawCall优化路径（全过程追踪）
```
初始状态: 4,000+ (阴影占3,000+)
    ↓ Static Batching
    4,600 → 3,600 (-21.7%)
    ↓ GPU Instancing
    3,600 → 3,500 (-2.8%)
    ↓ SRP Batcher
    3,500 → 3,500 (无变化，瓶颈在材质/Shader)
    ↓ LOD优化
    3,500 → 3,000 (-14.3%)
    ↓ 阴影光照优化(Mixed)
    大幅下降 (-40%)
    ↓ URP Asset参数调整
    1,800 → 1,200 (-33.3%)
   
最终状态: ~1,200 (较初始下降约70%)
```
### 3.2 DrawCall分阶段详细数据
| 优化阶段 | DrawCalls | 变化量 | 变化率 | 优化手段 |
|---------|-----------|--------|--------|---------|
| 初始 | ~4,600 | - | - | 原始状态 |
| Static Batching | 3,600 | -1,000 | -21.7% | 静态物体合批 |
| GPU Instancing | 3,500 | -100 | -2.8% | 重复模型实例化 |
| SRP Batcher | 3,500 | 0 | 0% | 材质状态优化（CPU侧） |
| LOD | 3,000 | -500 | -14.3% | 模型复杂度降级 |
| Mixed LightMode | 大幅下降 | - | -40% | 实时阴影转烘焙 |
| URP Asset调整 | 1,200 | -600 | -33.3% | 渲染尺度+阴影分辨率+剔除 |
---
## 4. GPU优化成果整理
### 4.1 GPU优化效果汇总
| 优化项 | GPU收益 | 量化指标 | 效果评级 |
|--------|---------|---------|---------|
| **Static Batching** | 顶点数据提交优化 | Batches -56.4% | ⭐⭐⭐ |
| **GPU Instancing** | 重复模型渲染效率 | Batches -9.1%, Buffers -20MB | ⭐⭐ |
| **SRP Batcher** | 材质数据常驻GPU | CPU Frame Time -51%（间接收益） | ⭐⭐⭐ |
| **透明/粒子优化** | Overdraw大幅下降 | 白色高热区→蓝绿区域 | ⭐⭐⭐⭐⭐ |
| **LOD** | 几何负载降低 | Triangles -19%, Vertices -15% | ⭐⭐⭐⭐ |
| **阴影光照(Mixed)** | 实时阴影计算移除 | Shadow Casters -40% | ⭐⭐⭐⭐⭐ |
| **URP Asset调整** | 多维度综合优化 | Triangles -30%, Vertices -35%, Shadow Casters -55% | ⭐⭐⭐⭐⭐ |
### 4.2 GPU瓶颈转移分析
```
优化前：
├── CPU瓶颈：实时阴影计算、材质状态切换、UGUI刷新
└── GPU瓶颈：Overdraw严重、Triangles 4.2M、Vertices 3.4M
优化后（SRP Batcher阶段）：
├── CPU瓶颈：已大幅缓解（Frame Time 11.03ms → 5.39ms）
└── GPU瓶颈：成为主要瓶颈（Draw Calls 3.5k, Triangles 4.2M, Shadow Casters 4000+）
最终优化后：
├── CPU瓶颈：进一步优化（波峰减少、UI重建降低）
└── GPU瓶颈：Triangles降至1.6M, Vertices降至1.3M, Shadow Casters降至802
```
### 4.3 GPU优化核心结论
1. **Overdraw优化（收益最大）**：透明粒子优化后，画面从白色高热区（MaxOverdrawCount≈50）降至蓝绿区域，GPU填充率压力显著降低
2. **阴影优化是最大收益来源**：Shadow Casters从4,000+降至802（-55%），实时阴影转烘焙后GPU阴影计算开销锐减
3. **几何负载降低**：Triangles 4.2M→1.6M（-62%），Vertices 3.4M→1.3M（-62%）
4. **显存代价**：Used Buffers从100.5MB增至240.7MB（+139%），需持续关注
5. **移动端收益**：发热、功耗、掉帧概率均有效降低
---
## 5. CPU优化成果整理
### 5.1 CPU优化效果汇总
| 优化项 | CPU收益 | 量化指标 | 效果评级 |
|--------|---------|---------|---------|
| **Static Batching** | 减少DrawCall提交 | DrawCalls -21.7% | ⭐⭐⭐ |
| **GPU Instancing** | 减少实例提交 | 小幅优化 | ⭐⭐ |
| **SRP Batcher** | 材质ConstantBuffer提交优化 | CPU Frame Time -51% | ⭐⭐⭐⭐⭐ |
| **UI优化** | UI重建开销降低 | Canvas Rebuild↓、GC Alloc↓、波峰控制 | ⭐⭐⭐ |
| **透明/粒子优化** | 计算开销降低 | Noise简化、粒子数量削减 | ⭐⭐ |
| **LOD** | 剔除逻辑优化 | DrawCalls -14.3% | ⭐⭐⭐ |
| **阴影光照(Mixed)** | 实时阴影计算移除 | Batches -30.6%, DrawCalls -40% | ⭐⭐⭐⭐⭐ |
| **URP Asset调整** | 渲染状态切换减少 | Batches -33%, SetPass Calls -20% | ⭐⭐⭐⭐ |
| **CPU波峰优化** | 动态文本刷新移除 | CPU波峰明显减少 | ⭐⭐⭐⭐ |
### 5.2 CPU主线程优化路径
```
优化前：
├── CPU渲染线程：~11ms（瓶颈）
├── 实时阴影计算：大量耗时
├── UGUI刷新：频繁Rebuild
└── 动态文本刷新：高频波峰
SRP Batcher后：
├── CPU渲染线程：5.39ms（大幅下降）
├── 瓶颈转移至GPU
└── 后续优化重点：阴影、DrawCall、面数
最终优化后：
├── CPU主线程：波动明显减小，更平稳
├── Canvas Rebuild / BuildBatch：次数减少
├── GC Alloc：明显减少，运行时卡顿下降
└── CPU波峰：大屏投影动态文本→静态贴图UV动画，波峰消除
```
### 5.3 CPU优化核心结论
1. **SRP Batcher是CPU最大收益**：CPU Frame Time从11.03ms降至5.39ms（-51%），Render State切换成本和Material Constant Buffer提交大幅减少
2. **UI优化提升稳定性**：Canvas拆分、降低动态TMP刷新频率，CPU主线程波动明显下降，帧率稳定性提升
3. **阴影计算优化**：实时阴影转烘焙后，CPU端阴影计算开销大幅移除，Batches和DrawCalls显著下降
4. **CPU波峰消除**：大屏投影高频动态文本改为静态贴图Shader UV动画，CPU尖峰帧明显减少
5. **GC优化**：GC Alloc明显减少，运行时卡顿下降
---
## 6. 优化成效总结
### 6.1 核心指标对比（初始 → 最终）
| 指标 | 初始状态 | 最终状态 | 总变化 | 优化率 |
|------|---------|---------|--------|--------|
| **Draw Calls** | ~4,600 | ~1,200 | -3,400 | **-74%** |
| **Batches** | ~4,400 | ~968 | -3,432 | **-78%** |
| **SetPass Calls** | ~400+ | ~164 | -240+ | **-60%+** |
| **Triangles** | 4.2M | 1.6M | -2.6M | **-62%** |
| **Vertices** | 3.4M | 1.3M | -2.1M | **-62%** |
| **Shadow Casters** | 4,000+ | 802 | -3,200+ | **-80%** |
| **CPU Frame Time** | ~11ms | ~5ms | -6ms | **-55%** |
| **Used Buffers** | - | 240.7MB | +140MB | 显存增长⚠️ |
### 6.2 优化手段优先级排序（按收益）
| 优先级 | 优化手段 | 主要收益 | 推荐指数 |
|--------|---------|---------|---------|
| 1 | 阴影优化（Mixed+烘焙+URP参数） | Shadow Casters -80%, DrawCalls -40% | ⭐⭐⭐⭐⭐ |
| 2 | 透明/粒子Overdraw优化 | GPU FillRate显著降低 | ⭐⭐⭐⭐⭐ |
| 3 | SRP Batcher | CPU Frame Time -51% | ⭐⭐⭐⭐⭐ |
| 4 | LOD | Triangles -19%, DrawCalls -14% | ⭐⭐⭐⭐ |
| 5 | Static Batching | Batches -56% | ⭐⭐⭐⭐ |
| 6 | URP Asset参数调整 | 综合多维度优化 | ⭐⭐⭐⭐ |
| 7 | CPU波峰优化 | 稳定性提升 | ⭐⭐⭐⭐ |
| 8 | UI优化 | GC↓、Rebuild↓ | ⭐⭐⭐ |
| 9 | GPU Instancing | Batches -9% | ⭐⭐ |
| 10 | Occlusion Culling | 当前场景无效果 | ⭐ |
