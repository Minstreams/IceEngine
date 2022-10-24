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
            Color color = highlightColorOverride ?? IceGUIUtility.CurrentThemeColor;
            var result = new List<(string displayName, string value)>();
            foreach (var candidate in origin)
            {
                if (candidate.IsMatch(filter, color, out var highlight, useRegex, continuousMatching, caseSensitive))
                {
                    result.Add((highlight, candidate));
                }
            }
            return result;
        }
    }
}
