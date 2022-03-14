using E.Data;
using UnityEngine;

namespace E
{
    public class PreDownload
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AfterSceneLoaded() { Instantiate(); }

        private static void Instantiate()
        {
            PreDownloadSettings settings = PreDownloadSettings.Instance;
            if (settings.enable)
            {
                ExecuteOrder.Add(-100, StartDownload,
                () => { return finished; });
            }
        }

        private static bool finished = false;

        public static void StartDownload()
        {
            PreDownloadSettings settings = PreDownloadSettings.Instance;
            settings.Init((bool settingsLoaded) =>
            {
                if (!settingsLoaded) { Debug.LogError("预载配置文件未载入!"); return; }
                DataProcessorDebug.enableLog = true;
                DataProcessorDebug.enableLogError = true;
                DataProcessorDebug.enableLogException = true;
                DataProcessorDebug.OverrideLog((object message) => { Debug.Log(message); });
                DataProcessorDebug.OverrideLogError((object message) => { Debug.LogError(message); });
                DataProcessorDebug.OverrideLogException((System.Exception message) => { Debug.LogException(message); });
                string source = settings.GetSourcePath();
                string target = settings.GetTargetPath();
                if (string.IsNullOrWhiteSpace(source) ||
                    string.IsNullOrWhiteSpace(target))
                { finished = true; return; }
                DataProcessor dataProcessor = new StandaloneDataProcessor();
                CheckTick.AddRule(() => { dataProcessor?.Tick(); return finished; }, () => { return true; });
                CloneDirectoryAsyncOperation asyncOperation = dataProcessor.CloneDirectory(source, target);
                asyncOperation.sourceAccount = new ConnectionAsyncOperation.Account
                { username = settings.sourceUsername, password = settings.sourcePassword };
                asyncOperation.targetAccount = new ConnectionAsyncOperation.Account
                { username = settings.targetUsername, password = settings.targetPassword };
                asyncOperation.onClose += () =>
                {
                    EFileTick.AddCommand(() =>
                    {
                        dataProcessor.Dispose();
                        asyncOperation = null;
                        dataProcessor = null;
                        finished = true;
                    });
                };
                if (settings.progressPrefab != null)
                { Object.Instantiate(settings.progressPrefab).asyncOperation = asyncOperation; }
            });
        }
    }
}