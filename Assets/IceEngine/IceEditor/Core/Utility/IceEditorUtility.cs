using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace IceEditor
{
    public static class IceEditorUtility
    {
        /// <summary>
        /// 将多行字符串写入一个文件的一个region块中<br/>
        /// 文件path的获取可参考 <see cref="System.Runtime.CompilerServices.CallerFilePathAttribute"/>
        /// </summary>
        /// <param name="lines">字符串Array，每个元素是一行的内容，不要开头的空格和制表符</param>
        /// <param name="filePath"><see cref="System.IO.File"/> 能识别的文件路径</param>
        /// <param name="regionName">region块的名字</param>
        public static void WriteToFileRegion(string filePath, string regionName, params string[] lines)
        {
            Regex regRegion = new($"[\r\n]+([ \t]*)#region {regionName}([\r\n]+)[\\w\\W]*?#endregion", RegexOptions.Multiline);
            Regex regClass = new("[\r\n]+([ \t]*)(public |internal |private )?class [\\w]+ ?: ?\\w+[\r\n]*[ \t]*\\{[ \t]*([\r\n]+)", RegexOptions.Multiline);

            var content = File.ReadAllText(filePath);
            {
                if (regRegion.IsMatch(content)) content = regRegion.Replace(content, $"$2$1#region {regionName}$2$1{string.Join("$2$1", lines)}$2$1#endregion", 1);
                else if (regClass.IsMatch(content)) content = regClass.Replace(content, $"$0$1    #region {regionName}$3$1    {string.Join("$3$1    ", lines)}$3$1    #endregion$3$3", 1);
            }
            File.WriteAllText(filePath, content);
        }
    }
}
