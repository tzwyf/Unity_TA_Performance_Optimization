using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TA_Runtime.UI
{
    public class UIRebuildStress : MonoBehaviour
    {
        [Header("UI Rebuild 压力配置")]
        [Tooltip("目标文本组件（TextMeshProUGUI），留空则自动在自身或子节点查找")]
        [SerializeField] private TextMeshProUGUI tmpText;

        [Tooltip("备用 Unity UI Text 组件（如果未使用TMP）")]
        [SerializeField] private Text unityText;

        [Tooltip("每帧更新文本内容（极高频率触发 Canvas Rebuild）")]
        [SerializeField] private bool updateEveryFrame = true;

        [Tooltip("每秒更新次数（当 updateEveryFrame=false 时生效）")]
        [SerializeField] private int updatesPerSecond = 60;

        [Tooltip("是否同时修改颜色（触发额外材质重建）")]
        [SerializeField] private bool changeColor = true;

        [Tooltip("是否修改字体大小（触发完整文本重新排版）")]
        [SerializeField] private bool changeFontSize = false;

        [Header("乱码屏幕风格")]
        [Tooltip("输出行数")]
        [SerializeField] private int lineCount = 12;

        [Tooltip("每行字符数")]
        [SerializeField] private int charsPerLine = 48;

        [Tooltip("是否混入十六进制块")]
        [SerializeField] private bool includeHexBlocks = true;

        [Tooltip("是否混入二进制流")]
        [SerializeField] private bool includeBinaryStream = true;

        [Tooltip("是否混入伪系统日志")]
        [SerializeField] private bool includeSysLog = true;

        [Tooltip("是否混入进度条")]
        [SerializeField] private bool includeProgressBars = true;

        [Tooltip("故障闪烁强度（0=无，1=每行可能故障）")]
        [Range(0f, 1f)]
        [SerializeField] private float glitchIntensity = 0.3f;

        private float timer;
        private float updateInterval;
        private int frameCounter;
        private static System.Text.StringBuilder sb = new System.Text.StringBuilder(2048);

        private static readonly char[] HexChars = "0123456789ABCDEF".ToCharArray();
        private static readonly char[] BinChars = "01".ToCharArray();
        private static readonly char[] AsciiChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
        private static readonly char[] GlitchChars = "!@#$%^&*()_+-=[]{}|;':\",./<>?`~".ToCharArray();
        private static readonly string[] LogLevels = { "[INFO]", "[WARN]", "[ERR]", "[DBG]", "[FATL]", "[TRACE]" };
        private static readonly string[] SysModules = { "MEM", "CPU", "GPU", "NET", "IO", "KERNEL", "RENDER", "AUDIO", "PHYSICS" };

        void Start()
        {
            if (tmpText == null)
                tmpText = GetComponent<TextMeshProUGUI>();
            if (tmpText == null)
                tmpText = GetComponentInChildren<TextMeshProUGUI>();

            if (unityText == null)
                unityText = GetComponent<Text>();
            if (unityText == null)
                unityText = GetComponentInChildren<Text>();

            updateInterval = updatesPerSecond > 0 ? 1f / updatesPerSecond : 0f;
        }

        void Update()
        {
            if (updateEveryFrame)
            {
                UpdateTextContent();
            }
            else
            {
                timer += Time.unscaledDeltaTime;
                if (timer >= updateInterval)
                {
                    timer -= updateInterval;
                    UpdateTextContent();
                }
            }
        }

        void UpdateTextContent()
        {
            frameCounter++;
            sb.Clear();

            int seed = frameCounter * 73856093;

            for (int line = 0; line < lineCount; line++)
            {
                int lineSeed = seed + line * 19349663;
                bool isGlitchLine = Random01(lineSeed) < glitchIntensity;

                if (isGlitchLine)
                {
                    // 故障行：全屏乱码符号
                    AppendGlitchLine(lineSeed);
                }
                else
                {
                    // 按行类型分发
                    int lineType = (lineSeed % 7);
                    switch (lineType)
                    {
                        case 0 when includeHexBlocks:
                            AppendHexDumpLine(lineSeed);
                            break;
                        case 1 when includeBinaryStream:
                            AppendBinaryLine(lineSeed);
                            break;
                        case 2 when includeSysLog:
                            AppendSysLogLine(lineSeed);
                            break;
                        case 3 when includeProgressBars:
                            AppendProgressLine(lineSeed);
                            break;
                        default:
                            AppendRandomAsciiLine(lineSeed);
                            break;
                    }
                }

                sb.Append('\n');
            }

            // 末尾追加不断变化的时间戳和帧号，确保内容绝对不同
            sb.Append(">>> SYS_TICK: ");
            sb.Append(Time.unscaledTime.ToString("F4"));
            sb.Append(" | FRAME: ");
            sb.Append(frameCounter);
            sb.Append(" | SEED: 0x");
            AppendHexValue(seed, 8);

            string finalText = sb.ToString();

            // 应用文本（这会触发 Canvas Rebuild）
            if (tmpText != null)
            {
                tmpText.text = finalText;

                if (changeColor)
                {
                    float hue = (Time.unscaledTime * 0.5f) % 1f;
                    tmpText.color = Color.HSVToRGB(hue, 0.8f, 1f);
                }

                if (changeFontSize)
                {
                    tmpText.fontSize = 14f + Mathf.PingPong(Time.unscaledTime * 10f, 12f);
                }
            }

            if (unityText != null)
            {
                unityText.text = finalText;

                if (changeColor)
                {
                    float hue = (Time.unscaledTime * 0.5f) % 1f;
                    unityText.color = Color.HSVToRGB(hue, 0.8f, 1f);
                }
            }
        }

        // ---------- 行生成器 ----------

        void AppendHexDumpLine(int seed)
        {
            sb.Append("0x");
            AppendHexValue(seed, 8);
            sb.Append("  ");
            int words = charsPerLine / 5;
            for (int i = 0; i < words; i++)
            {
                AppendHexValue(seed + i * 9301, 4);
                sb.Append(' ');
            }
        }

        void AppendBinaryLine(int seed)
        {
            int groups = charsPerLine / 9;
            for (int g = 0; g < groups; g++)
            {
                for (int b = 0; b < 8; b++)
                {
                    sb.Append(BinChars[(int)(Mathf.Abs((seed + g * 17 + b) * 2654435761) % 2)]);
                }
                sb.Append(' ');
            }
        }

        void AppendSysLogLine(int seed)
        {
            sb.Append(LogLevels[Mathf.Abs(seed) % LogLevels.Length]);
            sb.Append(' ');
            sb.Append(SysModules[Mathf.Abs(seed >> 4) % SysModules.Length]);
            sb.Append(": 0x");
            AppendHexValue(seed >> 8, 6);
            sb.Append(" | ADDR=");
            AppendHexValue(seed >> 12, 8);
            sb.Append(" | VAL=");
            AppendHexValue(seed >> 16, 4);
        }

        void AppendProgressLine(int seed)
        {
            int percent = Mathf.Abs(seed) % 101;
            int barLen = charsPerLine / 3;
            int filled = percent * barLen / 100;
            sb.Append('[');
            for (int i = 0; i < barLen; i++)
            {
                sb.Append(i < filled ? '|' : '.');
            }
            sb.Append("] ");
            sb.Append(percent);
            sb.Append('%');
        }

        void AppendRandomAsciiLine(int seed)
        {
            for (int i = 0; i < charsPerLine; i++)
            {
                int idx = Mathf.Abs((seed + i * 1664525) % AsciiChars.Length);
                sb.Append(AsciiChars[idx]);
            }
        }

        void AppendGlitchLine(int seed)
        {
            for (int i = 0; i < charsPerLine; i++)
            {
                int idx = Mathf.Abs((seed + i * 1103515245) % GlitchChars.Length);
                sb.Append(GlitchChars[idx]);
            }
        }

        // ---------- 工具 ----------

        void AppendHexValue(int value, int digits)
        {
            uint u = (uint)value;
            for (int i = digits - 1; i >= 0; i--)
            {
                sb.Append(HexChars[(u >> (i * 4)) & 0xF]);
            }
        }

        float Random01(int seed)
        {
            // 简单的确定性伪随机，基于种子
            uint s = (uint)seed;
            s = s * 1103515245 + 12345;
            return (s % 65536) / 65536f;
        }
    }

}