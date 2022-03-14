using System;
using UnityEditor;
using UnityEngine;

namespace E.Editor
{
    public class FileUpdaterSettings : ScriptableObject
    {
        private static FileUpdaterSettings instance;

        public static FileUpdaterSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    string[] assetGUIDs = AssetDatabase.FindAssets("FileUpdaterSettings");
                    for (int i = 0; i < assetGUIDs.Length; i++)
                    {
                        string assetPath = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);
                        Type assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
                        if (assetType.Equals(typeof(FileUpdaterSettings)))
                        { instance = AssetDatabase.LoadAssetAtPath<FileUpdaterSettings>(assetPath); break; }
                    }
                }
                return instance;
            }
        }

        [Serializable]
        public class UpdateRule
        {
            public string sourceUri;

            public string sourceUsername;

            public string sourcePassword;

            public string targetUri;

            public string targetUsername;

            public string targetPassword;

            public bool IsCorrect()
            {
                if (string.IsNullOrWhiteSpace(sourceUri) || string.IsNullOrWhiteSpace(targetUri)) { return false; }
                return true;
            }
        }

        public UpdateRule[] updateRules;

        public GameObject ProgressViewPrefab;

        public GameObject ProgressBarPrefab;
    }
}