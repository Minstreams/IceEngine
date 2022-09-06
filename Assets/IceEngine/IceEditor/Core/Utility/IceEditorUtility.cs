using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;

namespace IceEditor
{
    public static class IceEditorUtility
    {
        /// <summary>
        /// 将多行字符串写入一个文件的一个region块中<br/>
        /// 文件path的获取可参考 <see cref="System.Runtime.CompilerServices.CallerFilePathAttribute"/>
        /// </summary>
        /// <param name="regionName">region块的名字</param>
        /// <param name="content">字符串，每个元素是一行的内容，不要开头的空格和制表符</param>
        /// <param name="filePath"><see cref="System.IO.File"/> 能识别的文件路径</param>
        public static void WriteToFileRegion(string regionName, string content, [System.Runtime.CompilerServices.CallerFilePath] string filePath = null)
        {
            string[] lines = content.Replace(" ", "").Replace("\t", "").Split('\n');

            Regex regRegion = new($"[\r\n]+([ \t]*)#region {regionName}([\r\n]+)[\\w\\W]*?#endregion", RegexOptions.Multiline);
            Regex regClass = new("[\r\n]+([ \t]*)(public |internal |private )?class [\\w]+ ?: ?\\w+[\r\n]*[ \t]*\\{[ \t]*([\r\n]+)", RegexOptions.Multiline);

            var fileContent = File.ReadAllText(filePath);
            {
                if (regRegion.IsMatch(fileContent)) fileContent = regRegion.Replace(fileContent, $"$2$1#region {regionName}$2$1{string.Join("$2$1", lines)}$2$1#endregion", 1);
                else if (regClass.IsMatch(fileContent)) fileContent = regClass.Replace(fileContent, $"$0$1    #region {regionName}$3$1    {string.Join("$3$1    ", lines)}$3$1    #endregion$3$3", 1);
            }
            File.WriteAllText(filePath, fileContent);
        }

        /// <summary>
        /// 获取当前选择的路径
        /// </summary>
        public static string GetSelectPath()
        {
            //默认路径为Assets
            string selectedPath = "Assets";

            //获取选中的资源
            Object[] selection = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);

            //遍历选中的资源以返回路径
            foreach (Object obj in selection)
            {
                selectedPath = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(selectedPath) && File.Exists(selectedPath))
                {
                    selectedPath = Path.GetDirectoryName(selectedPath);
                    break;
                }
            }

            return selectedPath;
        }

        /// <summary>
        /// 尝试创建目录
        /// </summary>
        public static bool TryCreateDirectory(string path)
        {
            if (Directory.Exists(path)) return false;
            Directory.CreateDirectory(path);
            return true;
        }
    }
}
