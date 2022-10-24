using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using IceEngine;

namespace Ice
{
    public sealed class Save : IceEngine.Framework.IceSystem<IceEngine.Internal.SettingSave>
    {
        #region Path Configuration
        public static string DataPath => Application.persistentDataPath;
        public static string ToAbsolutePath(string relativePath) => $"{DataPath}/{relativePath}";
        #endregion

        public static class Json
        {
            public static Encoding Format => Encoding.UTF8;

            #region Sync Interface
            // Load
            public static T LoadFromFile<T>(string path, bool absolutePath = false)
            {
                if (!absolutePath) path = ToAbsolutePath(path);
                var json = File.ReadAllText(path, Format);
                return JsonUtility.FromJson<T>(json);
            }
            public static void LoadFromFileOverwrite(object objectToOverwrite, string path, bool absolutePath = false)
            {
                if (!absolutePath) path = ToAbsolutePath(path);
                var json = File.ReadAllText(path, Format);
                JsonUtility.FromJsonOverwrite(json, objectToOverwrite);
            }

            // Save
            public static void SaveToFile(object data, string path, bool absolutePath = false, bool prettyPrint = false)
            {
                if (!absolutePath) path = ToAbsolutePath(path);
                var json = JsonUtility.ToJson(data, prettyPrint);
                File.WriteAllText(path, json, Format);
            }
            #endregion

            #region Async Interface
            // Load
            public static async Task<T> LoadFromFileAsync<T>(string path, bool absolutePath = false, CancellationToken cancellationToken = default)
            {
                if (!absolutePath) path = ToAbsolutePath(path);
                var json = await File.ReadAllTextAsync(path, Format, cancellationToken);
                return JsonUtility.FromJson<T>(json);
            }
            public static async void LoadFromFileOverwriteAsync(object objectToOverwrite, string path, bool absolutePath = false, CancellationToken cancellationToken = default)
            {
                if (!absolutePath) path = ToAbsolutePath(path);
                var json = await File.ReadAllTextAsync(path, Format, cancellationToken);
                JsonUtility.FromJsonOverwrite(json, objectToOverwrite);
            }

            // Save
            public static Task SaveToFileAsync(object data, string path, bool absolutePath = false, bool prettyPrint = false, CancellationToken cancellationToken = default)
            {
                if (!absolutePath) path = ToAbsolutePath(path);
                var json = JsonUtility.ToJson(data, prettyPrint);
                return File.WriteAllTextAsync(path, json, Format, cancellationToken);
            }
            #endregion
        }

        public static class Binary
        {
            #region Sync Interface
            // Load
            public static object LoadFromFile(string path, bool absolutePath = false, int offset = 0, Type baseType = null, bool withExtraInfos = true)
            {
                if (!absolutePath) path = ToAbsolutePath(path);
                var bts = File.ReadAllBytes(path);
                return IceBinaryUtility.FromBytes(bts, offset, baseType, withExtraInfos);
            }
            public static void LoadFromFileOverwrite(object objectToOverwrite, string path, bool absolutePath = false, Type type = null, bool withHeader = true, int offset = 0, bool withExtraInfos = true)
            {
                if (!absolutePath) path = ToAbsolutePath(path);
                var bts = File.ReadAllBytes(path);
                IceBinaryUtility.FromBytesOverwrite(bts, objectToOverwrite, type, withHeader, offset, withExtraInfos);
            }

            // Save
            public static void SaveToFile(object data, string path, bool absolutePath = false, bool withHeader = true, Type baseType = null, bool withExtraInfos = true)
            {
                if (!absolutePath) path = ToAbsolutePath(path);
                var bts = IceBinaryUtility.ToBytes(data, withHeader, baseType, withExtraInfos);
                File.WriteAllBytes(path, bts);
            }
            #endregion

            #region Async Interface
            // Load
            public static async Task<object> LoadFromFileAsync(string path, bool absolutePath = false, int offset = 0, Type baseType = null, bool withExtraInfos = true, CancellationToken cancellationToken = default)
            {
                if (!absolutePath) path = ToAbsolutePath(path);
                var bts = await File.ReadAllBytesAsync(path, cancellationToken);
                return IceBinaryUtility.FromBytes(bts, offset, baseType, withExtraInfos);
            }
            public static async void LoadFromFileOverwriteAsync(object objectToOverwrite, string path, bool absolutePath = false, Type type = null, bool withHeader = true, int offset = 0, bool withExtraInfos = true, CancellationToken cancellationToken = default)
            {
                if (!absolutePath) path = ToAbsolutePath(path);
                var bts = await File.ReadAllBytesAsync(path, cancellationToken);
                IceBinaryUtility.FromBytesOverwrite(bts, objectToOverwrite, type, withHeader, offset, withExtraInfos);
            }

            // Save
            public static Task SaveToFileAsync(object data, string path, bool absolutePath = false, bool withHeader = true, Type baseType = null, bool withExtraInfos = true, CancellationToken cancellationToken = default)
            {
                if (!absolutePath) path = ToAbsolutePath(path);
                var bts = IceBinaryUtility.ToBytes(data, withHeader, baseType, withExtraInfos);
                return File.WriteAllBytesAsync(path, bts, cancellationToken);
            }
            #endregion
        }
    }
}
