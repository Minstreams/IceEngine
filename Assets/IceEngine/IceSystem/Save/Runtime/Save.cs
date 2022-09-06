using System.IO;
using UnityEngine;
using IceEngine;
using IceEngine.Framework;
using System.Text;

namespace Ice
{
    public sealed class Save : IceSystem<IceEngine.Internal.SettingSave>
    {
        public static void SaveToFileJson(object data, string path)
        {
            var json = JsonUtility.ToJson(data);
            File.WriteAllText(path, json, Encoding.UTF8);
        }
        public static void SaveToFileBinary(object data, string path)
        {
            var bts = IceBinaryUtility.ToBytes(data);
            File.WriteAllBytes(path, bts);
        }
    }
}
