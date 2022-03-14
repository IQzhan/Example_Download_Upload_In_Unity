using E.Data;
using UnityEngine;
using UnityEngine.Events;

namespace E
{
    public class PreDownloadProgress : MonoBehaviour
    {
        public CloneDirectoryAsyncOperation asyncOperation;

        private float lastTime;

        private readonly float maxDeltaTime = 0.01f;

        private void Update()
        {
            if(asyncOperation != null)
            {
                if(Time.time - lastTime > maxDeltaTime)
                {
                    lastTime = Time.time;
                    if (asyncOperation.IsClosed)
                    { Destroy(gameObject); return; }
                    SetProgress(asyncOperation.Progress);
                    SetProgress((asyncOperation.Progress * 100).ToString() + "%");
                    long size = asyncOperation.Size;
                    SetSize(asyncOperation.Size);
                    if (size > 0) { SetSize(Utility.FormatDataSize(size, "<n> <u>")); }
                    long processedBytes = asyncOperation.ProcessedBytes;
                    SetProcessedBytes(processedBytes);
                    if (processedBytes > 0) SetProcessedBytes(Utility.FormatDataSize(processedBytes, "<n> <u>"));
                    double speed = asyncOperation.Speed;
                    SetSpeed(speed);
                    if (speed > 0) SetSpeed(Utility.FormatDataSize(asyncOperation.Speed, "<n> <u>/s"));
                    double remainingTime = asyncOperation.RemainingTime;
                    SetRemainingTime(remainingTime);
                    if (remainingTime > 0) SetRemainingTime(asyncOperation.RemainingTime + " s");
                }
            }
        }

        public UnityEvent<float> progressValue;

        public void SetProgress(double value)
        {
            progressValue.Invoke((float)value);
        }

        public UnityEvent<string> progressText;

        public void SetProgress(string value)
        {
            progressText.Invoke(value);
        }

        public UnityEvent<long> sizeValue;

        public void SetSize(long value)
        {
            sizeValue.Invoke(value);
        }

        public UnityEvent<string> sizeText;

        public void SetSize(string value)
        {
            sizeText.Invoke(value);
        }

        public UnityEvent<long> processedBytesValue;

        public void SetProcessedBytes(long value)
        {
            processedBytesValue.Invoke(value);
        }

        public UnityEvent<string> processedBytesText;

        public void SetProcessedBytes(string value)
        {
            processedBytesText.Invoke(value);
        }

        public UnityEvent<double> remainingTimeValue;

        public void SetRemainingTime(double value)
        {
            remainingTimeValue.Invoke(value);
        }

        public UnityEvent<string> remainingTimeText;

        public void SetRemainingTime(string value)
        {
            remainingTimeText.Invoke(value);
        }

        public UnityEvent<double> speedTimeValue;

        public void SetSpeed(double value)
        {
            speedTimeValue.Invoke(value);
        }

        public UnityEvent<string> speedTimeText;

        public void SetSpeed(string value)
        {
            speedTimeText.Invoke(value);
        }
    }
}