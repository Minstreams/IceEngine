using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

// 代码模板写法
// 代码模板写法
// 代码模板写法
// 代码模板写法
public class EditorUtil : Editor
{
    [MenuItem("Assets/Create/BaseProgram/GameManager_xx", false, 201)]
    public static void CreateFacade()
    {
        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
            ScriptableObject.CreateInstance<CreateAssetAction>(),
            GetSelectPath() + "/GameManager_xx.cs", null,
            "Assets/Tools/Editor/Template/GameManager_xx.txt"
        );
    }

    private static string GetSelectPath()
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
}
class CreateAssetAction : UnityEditor.ProjectWindowCallback.EndNameEditAction
{
    public override void Action(int instanceId, string pathName, string resourceFile)
    {
        Object obj = CreateAssetFormTemplate(pathName, resourceFile);
        ProjectWindowUtil.ShowCreatedAsset(obj);
    }
    internal static Object CreateAssetFormTemplate(string pathName, string resourceFile)
    {
        //获取要创建资源的绝对路径
        string fullName = Path.GetFullPath(pathName);
        //读取本地模版文件
        string content = string.Empty;
        using (StreamReader reader = new StreamReader(resourceFile))
        {
            content = reader.ReadToEnd();
            reader.Close();
        }

        //获取资源的文件名称
        string fileName = Path.GetFileNameWithoutExtension(pathName);
        //替换默认的文件名称
        content = content.Replace("#SCRIPTNAME#", fileName);

        //写入新文件
        using (StreamWriter writer = new StreamWriter(fullName, false, System.Text.Encoding.UTF8))
        {
            writer.Write(content);
            writer.Close();
        }

        //刷新本地资源
        AssetDatabase.ImportAsset(pathName);
        AssetDatabase.Refresh();

        return AssetDatabase.LoadAssetAtPath(pathName, typeof(Object));
    }
}
