using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace E
{
    public class PreDownloadSettings : ScriptableObject
    {
        private static PreDownloadSettings instance;

        public static PreDownloadSettings Instance
        {
            get
            {
                if (instance == null)
                { instance = Resources.Load<PreDownloadSettings>("PreDownloadSettings"); }
                return instance;
            }
        }

        [Serializable]
        public enum PathType
        {
            Application,
            PersistentDataPath
        }

        [Header("开启预载")]
        public bool enable = false;

        [SerializeField]
        [Header("配置文件下载地址")]
        private string downloadFrom;

        private string GetDownloadFrom()
        {
#if UNITY_EDITOR
            return GetSaveTo();
#else
            return downloadFrom;
#endif
        }

        [SerializeField]
        [Header("配置文件读取路径")]
        private PathType readFrom;

        private string GetReadFrom()
        {
            switch (readFrom)
            {
                default:
                case PathType.Application:
                    return Path.Combine(
#if UNITY_EDITOR
                            Environment.CurrentDirectory,
#else
                            Application.dataPath,
#endif
                        "Settings", "PreDownloadSettings.data");
                case PathType.PersistentDataPath:
                    return Path.Combine(Application.persistentDataPath, "Settings", "PreDownloadSettings.data");
            }
        }

        [Header("进度条预制体")]
        public PreDownloadProgress progressPrefab;

#if UNITY_EDITOR
        [SerializeField]
        [Header("保存至")]
        private string saveTo;

        private string GetSaveTo()
        {
            if (!string.IsNullOrWhiteSpace(saveTo))
            { return System.IO.Path.Combine(saveTo, "PreDownloadSettings.data"); }
            return string.Empty;
        }

        private void SaveToFile()
        {
            if (!string.IsNullOrWhiteSpace(saveTo))
            { Save(GetSaveTo()); }
        }

        private void Save(string path)
        {
            string text = ToText();
            byte[] data = Encode(Encoding.UTF8.GetBytes(text));
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) { Directory.CreateDirectory(dir); }
            System.IO.File.WriteAllBytes(path, data);
        }

        private string ToText()
        {
            return
                C(sourcePath) + "\n" +
                C(sourceUsername) + "\n" +
                C(sourcePassword) + "\n" +
                targetPath.ToString() + "\n" +
                C(targetSubPath) + "\n" +
                C(targetUsername) + "\n" +
                C(targetPassword);
        }

        private string C(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "null";
            return input;
        }

        private byte[] Encode(byte[] origin)
        {
            if (origin != null)
            {
                for (int i = 0; i < origin.Length; i++)
                { origin[i] += (byte)(i % 5 + i * 2); }
                return origin;
            }
            else { return null; }
        }
#endif

        public void Init(Action<bool> onComplete)
        {
            string downloadFromPath = GetDownloadFrom();
            if (string.IsNullOrWhiteSpace(downloadFromPath))
            {
                onComplete(Read());
                return;
            }
            Data.DataProcessor dataProcessor = new Data.StandaloneDataProcessor();
            Data.CloneAsyncOperation cloneAsyncOperation = dataProcessor.Clone(downloadFromPath);
            cloneAsyncOperation.Timeout = 500;
            cloneAsyncOperation.LoadData = true;
            bool finished = false;
            cloneAsyncOperation.onClose += () =>
            {
                if (cloneAsyncOperation.IsProcessingComplete)
                {
                    byte[] data = cloneAsyncOperation.Data;
                    string readPath = GetReadFrom();
                    string dir = Path.GetDirectoryName(readPath);
                    if (!Directory.Exists(dir)) { Directory.CreateDirectory(dir); }
                    File.WriteAllBytes(GetReadFrom(), data);
                }
                onComplete(Read());
                finished = true;
            };
            CheckTick.AddRule(() =>
            {
                if (!finished) { dataProcessor?.Tick(); }
                else
                {
                    dataProcessor.Dispose();
                    dataProcessor = null;
                }
                return finished;
            }, () => { return true; });
        }

        private bool Read()
        {
            string path = GetReadFrom();
            if (System.IO.File.Exists(path))
            {
                byte[] data = System.IO.File.ReadAllBytes(path);
                ToProperties(Encoding.UTF8.GetString(Decode(data)));
                return true;
            }
            return false;
        }

        private void ToProperties(string text)
        {
            string[] arr = text.Split('\n');
            sourcePath = T(arr[0]);
            sourceUsername = T(arr[1]);
            sourcePassword = T(arr[2]);
            Enum.TryParse(arr[3], out targetPath);
            targetSubPath = T(arr[4]);
            targetUsername = T(arr[5]);
            targetPassword = T(arr[6]);
        }

        private const string Null = "null";

        private string T(string input)
        {
            if (input.Equals(Null))
            { return string.Empty; }
            return input;
        }

        public byte[] Decode(byte[] encode)
        {
            if (encode != null)
            {
                for (int i = 0; i < encode.Length; i++)
                { encode[i] -= (byte)(i % 5 + i * 2); }
                return encode;
            }
            else { return null; }
        }

        [SerializeField]
        private string sourcePath;

        public string GetSourcePath()
        {
            return sourcePath;
        }

        public string sourceUsername;

        public string sourcePassword;

        [SerializeField]
        private PathType targetPath;

        [SerializeField]
        private string targetSubPath;

        public string GetTargetPath()
        {
            switch (targetPath)
            {
                default:
                case PathType.Application:
                    return Path.Combine(
#if UNITY_EDITOR
                            Environment.CurrentDirectory,
#else
                            Application.dataPath,
#endif
                        targetSubPath);
                case PathType.PersistentDataPath:
                    return Path.Combine(Application.persistentDataPath, targetSubPath);
            }
        }

        public string targetUsername;

        public string targetPassword;
    }
}