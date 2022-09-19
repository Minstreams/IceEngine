using System;
using System.Collections.Generic;
using IceEditor.Framework;

namespace IceEditor.Internal
{
    public class EditorSettingHotScriptBox : IceEditorSetting<EditorSettingHotScriptBox>
    {
        public List<HotScriptItem> scripts = new();
    }

    public enum HotScriptType
    {
        Run,
        GUI,
    }
    [Serializable]
    public class HotScriptItem
    {
        public string name;
        public HotScriptType type;
        public List<string> assemblies = new();
        public string code = @"";
    }
}
