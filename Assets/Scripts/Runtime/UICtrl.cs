using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TA_Runtime.UI
{
    public class UICtrl : MonoBehaviour
    {
        private GameObject goLight;
        private Button btnLightSwitch;
        private TextMeshProUGUI fpsText;

        private float deltaTime = 0.0f;
        // UI 刷新间隔（秒），避免每帧重建文本开销
        private float updateInterval = 0.2f;
        private float nextUpdate = 0.0f;
        // 掉帧阈值（毫秒）
        private const float WARNING_MS = 33.3f;   // < 30 FPS
        private const float CRITICAL_MS = 50.0f;  // < 20 FPS

        private void Awake()
        {
            goLight = GameObject.Find("LightRoot");
            btnLightSwitch = GameObject.Find("BtnLightSwitch").GetComponent<Button>();
            fpsText = GameObject.Find("TextFPS").GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            btnLightSwitch.onClick.AddListener(ShowLight);
        }

        private void Update()
        {
            // 1. 平滑帧耗时（指数移动平均，用于观察整体趋势）
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

            // 2. 按间隔刷新 UI，降低 TextMeshPro 每帧重建的开销
            if (Time.unscaledTime >= nextUpdate)
            {
                nextUpdate = Time.unscaledTime + updateInterval;
                UpdateFPS();
            }
        }

        private void ShowLight()
        {
            goLight.SetActive(!goLight.activeInHierarchy);
        }

        void UpdateFPS()
        {
            // ---- 瞬时数据（当前帧，精准反映卡顿）----
            float instantMs = Time.unscaledDeltaTime * 1000f;
            float instantFps = 1.0f / Time.unscaledDeltaTime;

            // ---- 平滑数据（历史趋势，过滤偶发噪声）----
            float avgFps = 1.0f / deltaTime;

            // ---- 颜色警告：根据瞬时帧耗时分级 ----
            Color color = Color.green;
            if (instantMs >= CRITICAL_MS)
                color = new Color(1f, 0.3f, 0.3f); // 红
            else if (instantMs >= WARNING_MS)
                color = Color.yellow;

            fpsText.color = color;

            // 格式：平均FPS | 瞬时FPS | 当前帧耗时
            fpsText.text = $"FPS: {Mathf.Ceil(avgFps)} [{Mathf.Ceil(instantFps)}] {instantMs:F1}ms";
        }
    }
}
