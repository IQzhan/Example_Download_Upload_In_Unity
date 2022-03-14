using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace E.Editor
{
    [CustomEditor(typeof(PreDownloadSettings))]
    public class PreDownloadSettingsEditor : UnityEditor.Editor
    {
        private PreDownloadSettings asset;

        private Type assetType;

        private MethodInfo saveToFileMethod;

        private void OnEnable() { Init(); }

        public override void OnInspectorGUI() { Body(); }

        private void Init()
        {
            asset = target as PreDownloadSettings;
            assetType = asset.GetType();
            saveToFileMethod = assetType.GetMethod("SaveToFile", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        private void Body()
        {
            EditorGUI.BeginChangeCheck();
            VerticalBox("Static", StaticBody);
            VerticalBox("Settings", SettingsBody);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssets();
            }
        }

        private void VerticalBox(string label, Action body)
        {
            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            body();
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }

        private void StaticBody()
        {
            PublicToggleField("enable", "Enable", "开启预载");
            PrivateTextField("downloadFrom", "Download From", "配置文件下载地址");
            PrivateEnumPopup("readFrom", "Read From", "配置文件读取地址");
            PublicObjectField("progressPrefab", "Progress Prefab", "进度条预制体");
            PrivateTextField("saveTo", "Auto Save To Local", "临时保存地址");
        }

        private void SettingsBody()
        {
            EditorGUI.BeginChangeCheck();
            PrivateTextField("sourcePath", "Source Path", "下载源");
            PublicTextField("sourceUsername", "Source Username", "下载源用户名");
            PublicTextField("sourcePassword", "Source Password", "下载源密码");
            PrivateEnumPopup("targetPath", "Target Path", "保存路径");
            PrivateTextField("targetSubPath", "Target Sub Path", "");
            PublicTextField("targetUsername", "Target Username", "下载源用户名");
            PublicTextField("targetPassword", "Target Password", "下载源密码");
            if (EditorGUI.EndChangeCheck()) { saveToFileMethod.Invoke(asset, null); }
        }

        private void PublicToggleField(string name, string label, string desc)
        {
            FieldInfo fieldInfo = assetType.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            fieldInfo.SetValue(asset,
                EditorGUILayout.Toggle(new GUIContent(label, desc),
                (bool)fieldInfo.GetValue(asset)));
        }

        private void PublicObjectField(string name, string label, string desc)
        {
            FieldInfo fieldInfo = assetType.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            fieldInfo.SetValue(asset,
                EditorGUILayout.ObjectField(new GUIContent(label, desc),
                (UnityEngine.Object)fieldInfo.GetValue(asset), fieldInfo.FieldType, false));
        }

        private void PrivateTextField(string name, string label, string desc)
        {
            FieldInfo fieldInfo = assetType.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfo.SetValue(asset,
                EditorGUILayout.TextField(new GUIContent(label, desc),
                (string)fieldInfo.GetValue(asset)));
        }

        private void PublicTextField(string name, string label, string desc)
        {
            FieldInfo fieldInfo = assetType.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            fieldInfo.SetValue(asset,
                EditorGUILayout.TextField(new GUIContent(label, desc),
                (string)fieldInfo.GetValue(asset)));
        }

        private void PrivateEnumPopup(string name, string label, string desc)
        {
            FieldInfo fieldInfo = assetType.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfo.SetValue(asset,
                EditorGUILayout.EnumPopup(new GUIContent(label, desc),
                (Enum)fieldInfo.GetValue(asset)));
        }
    }
}