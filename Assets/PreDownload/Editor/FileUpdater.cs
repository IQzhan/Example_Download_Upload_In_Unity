using E.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace E.Editor
{
    public class FileUpdater
    {
        [MenuItem("Assets/Tools/Update Files")]
        public static void StartUpdate()
        {
            DataProcessorDebug.enableLog = true;
            DataProcessorDebug.enableLogError = true;
            DataProcessorDebug.enableLogException = true;
            DataProcessorDebug.OverrideLog((object message) => { Debug.Log(message); });
            DataProcessorDebug.OverrideLogError((object message) => { Debug.LogError(message); });
            DataProcessorDebug.OverrideLogException((System.Exception message) => { Debug.LogException(message); });
            FileUpdaterSettings settings = FileUpdaterSettings.Instance;
            int count = settings.updateRules.Length;
            int index = count;
            DataProcessor dataProcessor = null;
            GameObject view = null;
            Transform content = null;
            System.Action setProgress = null;
            float lastMarkTime = 0;
            if (count > 0)
            {
                CreateProgressBar();
                dataProcessor = new StandaloneDataProcessor();
                EditorApplication.update += Update;
                for (int i = 0; i < count; i++)
                {
                    FileUpdaterSettings.UpdateRule updateRule = settings.updateRules[i];
                    if (updateRule.IsCorrect())
                    {
                        CloneDirectoryAsyncOperation asyncOperation
                            = dataProcessor.CloneDirectory(updateRule.sourceUri, updateRule.targetUri);
                        AddProgressBar(asyncOperation);
                        asyncOperation.sourceAccount = new ConnectionAsyncOperation.Account
                        { username = updateRule.sourceUsername, password = updateRule.sourcePassword };
                        asyncOperation.targetAccount = new ConnectionAsyncOperation.Account
                        { username = updateRule.targetUsername, password = updateRule.targetPassword };
                        asyncOperation.onClose += () =>
                        { EFileTick.AddCommand(() => { End(); }); };
                    }
                    else { End(); }
                }
            }
            void Update()
            {
                if(Time.realtimeSinceStartup - lastMarkTime > 0.016)
                {
                    lastMarkTime = Time.realtimeSinceStartup;
                    dataProcessor?.Tick();
                    setProgress?.Invoke();
                }
            }
            void CreateProgressBar()
            {
                if (settings.ProgressViewPrefab != null)
                {
                    view = Object.Instantiate(settings.ProgressViewPrefab);
                    content = view.GetComponentInChildren<GridLayoutGroup>().transform;
                }
            }
            void AddProgressBar(CloneDirectoryAsyncOperation asyncOperation)
            {
                if (content != null && settings.ProgressBarPrefab != null)
                {
                    GameObject bar = Object.Instantiate(settings.ProgressBarPrefab);
                    bar.transform.SetParent(content, false);
                    Slider slider = bar.transform.GetComponentInChildren<Slider>();
                    Text text = bar.transform.GetComponentInChildren<Text>();
                    setProgress += () =>
                    {
                        slider.value = (float)asyncOperation.Progress;
                        text.text = (System.Math.Round(asyncOperation.Progress, 2) * 100).ToString() + "%";
                    };
                }
            }
            void DestroyProgressBar()
            {
                if (!Application.isPlaying)
                { Object.DestroyImmediate(view); }
                else { Object.Destroy(view); }
            }
            void End()
            {
                if (--index == 0)
                {
                    EditorApplication.update -= Update;
                    dataProcessor.Dispose();
                    dataProcessor = null;
                    DestroyProgressBar();
                    Debug.Log("同步结束");
                }
            }
        }
    }
}