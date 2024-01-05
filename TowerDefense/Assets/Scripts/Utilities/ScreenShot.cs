#if UNITY_EDITOR
using System;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using UnityEngine;

namespace Utilities
{
    [Serializable]
    public class ScreenShotData
    {
        public string name;
        public int width;
        public int height;
    }

    public class ScreenShot : MonoBehaviour
    {
        private RecorderController _recorderController;

        [SerializeField] private ScreenShotData[] screenShotData;

        private void Setting(string name, int width, int height)
        {
            var curTime = DateTime.Now.ToString("hh:mm:ss");
            var controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
            _recorderController = new RecorderController(controllerSettings);

            var mediaOutputFolder = Path.Combine(Application.dataPath, "../../../", "screenshot");

            var imageRecorder = ScriptableObject.CreateInstance<ImageRecorderSettings>();
            imageRecorder.name = name;
            imageRecorder.Enabled = true;
            imageRecorder.OutputFormat = ImageRecorderSettings.ImageRecorderOutputFormat.PNG;
            imageRecorder.CaptureAlpha = false;

            imageRecorder.OutputFile = Path.Combine(mediaOutputFolder, name + "_" + width + "x" + height) + curTime;

            imageRecorder.imageInputSettings = new GameViewInputSettings
            {
                OutputWidth = width,
                OutputHeight = height,
            };

            controllerSettings.AddRecorderSettings(imageRecorder);
            controllerSettings.SetRecordModeToSingleFrame(0);
        }

        private void OnGUI()
        {
            if (Input.GetKeyUp(KeyCode.Space))
            {
                Capture().Forget();
            }
        }

        private async UniTaskVoid Capture()
        {
            foreach (var data in screenShotData)
            {
                Setting(data.name, data.width, data.height);
                _recorderController.PrepareRecording();
                _recorderController.StartRecording();
                await UniTask.Delay(100);
            }
        }
    }
}
#endif