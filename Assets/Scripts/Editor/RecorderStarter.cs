#if UNITY_EDITOR
using UnityEditor.Recorder;
using UnityEngine;
using BeachHero;

public static class RecorderStarter
{
    private static RecorderController recorderController;

    public static void StartRecorder()
    {
        if (!Application.isPlaying)
        {
            DebugUtils.LogWarning("[RecorderStarter] Recording can only be started in Play Mode.");
            return;
        }

        if (recorderController != null && recorderController.IsRecording())
        {
            DebugUtils.LogWarning("[RecorderStarter] Recording is already in progress.");
            return;
        }

        RecorderControllerSettings controllerSettings = RecorderControllerSettings.GetGlobalSettings();
        recorderController = new RecorderController(controllerSettings);
        recorderController.PrepareRecording();

        if (!recorderController.StartRecording())
        {
            recorderController.StopRecording();
            recorderController = null;
            DebugUtils.LogWarning(
                "[RecorderStarter] Recording could not be started. Configure and enable a recorder in Window > General > Recorder > Recorder Window.");
        }
    }

    public static void StopRecorder()
    {
        if (recorderController == null)
        {
            return;
        }

        recorderController.StopRecording();
        recorderController = null;
    }
}
#endif
