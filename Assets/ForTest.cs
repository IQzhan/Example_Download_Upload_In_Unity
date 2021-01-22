using E.Data;
using UnityEngine;

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

        private Cloner cloner;

        private void Awake()
        {
            Init();
            TestDownload();
        }

        private void Update()
        {
            cloner.Tick();
            DrawProgress();
        }

        private void Init()
        {
            ClonerDebug.OverrideLog((string message) =>
            {
                Debug.Log(message);
            });
            ClonerDebug.OverrideLogError((string message) =>
            {
                Debug.LogError(message);
            });
            ClonerDebug.OverrideLogWarning((string message) =>
            {
                Debug.LogWarning(message);
            });
            ClonerDebug.OverrideLogException((System.Exception exception) =>
            {
                Debug.LogException(exception);
            }); 
            cloner = new StandaloneCloner(cacheUri);

            cloner.MaxCommandDeltaTick = 40;
            
        }

        private void TestDownload()
        {
            
        }

        private void DrawProgress()
        {

        }

        private void OnDestroy()
        {
            cloner.Close();
        }
    }
}