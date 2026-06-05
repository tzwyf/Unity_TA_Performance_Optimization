using UnityEngine;
using UnityEditor;
using System.IO;

namespace TA_Editor
{
    public class BatchPrefixRenamer : EditorWindow
    {
        private DefaultAsset targetFolder;
        private string prefix = "";
        private string extension = ".png";
        private Vector2 scrollPos;
        private string log = "";

        [MenuItem("Tools/TA/Batch Prefix Renamer")]
        public static void ShowWindow()
        {
            GetWindow<BatchPrefixRenamer>("Batch Prefix Renamer");
        }

        private void OnGUI()
        {
            GUILayout.Label("批量文件名加前缀", EditorStyles.boldLabel);

            targetFolder = EditorGUILayout.ObjectField("目标文件夹", targetFolder, typeof(DefaultAsset), false) as DefaultAsset;
            prefix = EditorGUILayout.TextField("前缀", prefix);
            extension = EditorGUILayout.TextField("文件类型 (如 .png)", extension);

            EditorGUILayout.Space(10);

            if (GUILayout.Button("执行重命名", GUILayout.Height(30)))
            {
                RenameFiles();
            }

            EditorGUILayout.Space(10);

            if (!string.IsNullOrEmpty(log))
            {
                EditorGUILayout.LabelField("执行日志:");
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(200));
                EditorGUILayout.HelpBox(log, MessageType.Info);
                EditorGUILayout.EndScrollView();
            }
        }

        private void RenameFiles()
        {
            log = "";
            if (targetFolder == null)
            {
                log = "错误: 请先选择目标文件夹";
                return;
            }

            string folderPath = AssetDatabase.GetAssetPath(targetFolder);
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                log = "错误: 选择的不是文件夹";
                return;
            }

            if (string.IsNullOrWhiteSpace(prefix))
            {
                log = "错误: 前缀不能为空";
                return;
            }

            if (string.IsNullOrWhiteSpace(extension))
            {
                log = "错误: 文件类型不能为空";
                return;
            }

            // 确保扩展名以 . 开头
            if (!extension.StartsWith("."))
                extension = "." + extension;

            string absPath = Path.Combine(Directory.GetCurrentDirectory(), folderPath);
            string[] files = Directory.GetFiles(absPath, "*" + extension, SearchOption.AllDirectories);

            int renamedCount = 0;
            int skippedCount = 0;

            foreach (string file in files)
            {
                // 计算相对于目标文件夹的相对路径（保留子文件夹结构）
                string relativePath = file.Substring(absPath.Length + 1).Replace('\\', '/');
                string fileName = Path.GetFileName(relativePath);

                // 忽略 meta 文件
                if (fileName.EndsWith(".meta"))
                    continue;

                if (fileName.StartsWith(prefix))
                {
                    skippedCount++;
                    continue;
                }

                string newName = prefix + fileName;
                string oldRelativePath = folderPath + "/" + relativePath;
                string dirPath = Path.GetDirectoryName(relativePath)?.Replace('\\', '/');
                string newRelativePath = folderPath + "/" + (string.IsNullOrEmpty(dirPath) ? newName : dirPath + "/" + newName);

                string result = AssetDatabase.MoveAsset(oldRelativePath, newRelativePath);
                if (string.IsNullOrEmpty(result))
                {
                    renamedCount++;
                }
                else
                {
                    log += $"失败: {relativePath} -> {newName} ({result})\n";
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            log += $"完成! 重命名: {renamedCount}, 跳过(已有前缀): {skippedCount}";
        }
    }

}