using E.Data;
using UnityEngine;
using UnityEngine.Networking;

namespace E
{
    public class ForTest : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod]
        private static void Create()
        {
            DontDestroyOnLoad(new GameObject("ForTest").AddComponent<ForTest>());
        }

        private readonly System.Uri cacheUri = new System.Uri(@"E:\Downloads\");

        private DataProcessor dataProcessor;

        private void Awake()
        {
            Init();
            TestDownload();
        }

        private void Update()
        {
            dataProcessor.Tick();
            DrawProgress();
        }

        private void Init()
        {
            DataProcessorDebug.OverrideLog((string message) =>
            {
                Debug.Log(message);
            });
            DataProcessorDebug.OverrideLogError((string message) =>
            {
                Debug.LogError(message);
            });
            DataProcessorDebug.OverrideLogException((System.Exception exception) =>
            {
                Debug.LogException(exception);
            });
            dataProcessor = new StandaloneDataProcessor(cacheUri);

            dataProcessor.StartAsyncOperationGroup();
            
            dataProcessor.EndAsyncOperationGroup();
        }

        private void TestDownload()
        {

        }

        private void DrawProgress()
        {

        }

        private void OnDestroy()
        {
            dataProcessor.Dispose();
        }
    }
}