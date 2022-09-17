using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Obj = UnityEngine.Object;

using IceEngine;


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
            Obj[] selection = Selection.GetFiltered(typeof(Obj), SelectionMode.Assets);

            //遍历选中的资源以返回路径
            foreach (Obj obj in selection)
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


        /// <summary>
        /// 筛选一个string集合
        /// </summary>
        /// <param name="origin">待筛选的string集合</param>
        /// <param name="filter">关键字</param>
        /// <param name="useRegex">使用正则表达式</param>
        /// <param name="continuousMatching">连续匹配</param>
        /// <param name="caseSensitive">区分大小写</param>
        /// <param name="highlightColorOverride">设定高亮颜色（默认为当前主题颜色）</param>
        /// <returns>筛选过的集合（高亮后的名字|原始值）</returns>
        public static List<(string displayName, string value)> Filter(this IEnumerable<string> origin, string filter, bool useRegex = false, bool continuousMatching = false, bool caseSensitive = false, Color? highlightColorOverride = null)
        {
            if (string.IsNullOrWhiteSpace(filter)) return origin.Select(s => (s, s)).ToList();

            var result = new List<(string displayName, string value)>();

            string colorExpr = ColorUtility.ToHtmlStringRGB(highlightColorOverride ?? IceGUIUtility.CurrentThemeColor);
            if (useRegex)
            {
                // 正则表达式匹配
                foreach (var candidate in origin)
                {
                    try
                    {
                        var option = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
                        if (Regex.IsMatch(candidate, filter, option))
                        {
                            string displayName = Regex.Replace(candidate, filter, "$0".Color(colorExpr), option);
                            result.Add((displayName, candidate));
                        }
                    }
                    catch (Exception)
                    {
                        return result;
                    }
                }
            }
            else
            {
                // 判断是否包含filter关键字
                if (continuousMatching)
                {
                    foreach (var candidate in origin)
                    {
                        // 连续匹配
                        int index = candidate.IndexOf(filter, caseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase);
                        if (index < 0) continue;

                        // 替换关键字为指定颜色
                        string displayName = candidate
                            .Insert(index + filter.Length, "</color>")
                            .Insert(index, $"<color={colorExpr}>");

                        result.Add((displayName, candidate));
                    }
                }
                else
                {
                    foreach (var candidate in origin)
                    {
                        int l = filter.Length;
                        {
                            // 离散匹配
                            int i = 0;
                            foreach (char c in candidate) if (c.CompareChar(filter[i], caseSensitive) && ++i == l) break;
                            // 不包含则跳过
                            if (i < l) continue;
                        }


                        string displayName = string.Empty;
                        {
                            // 替换关键字为指定颜色
                            int i = 0;
                            foreach (char c in candidate)
                            {
                                if (i < l && c.CompareChar(filter[i], caseSensitive))
                                {
                                    displayName += c.ToString().Color(colorExpr);
                                    ++i;
                                }
                                else displayName += c;
                            }
                        }
                        result.Add((displayName, candidate));
                    }
                }
            }
            return result;
        }
    }
}
