using UnityEngine;

public static class HapticManager
{
    public static void Beat()
    {
        Vibrate(20);
    }

    public static void Death()
    {
        Vibrate(200);
    }

    public static void LevelComplete()
    {
        Vibrate(100);
    }

    static void Vibrate(long milliseconds)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator"))
            {
                if (vibrator != null)
                    vibrator.Call("vibrate", milliseconds);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[HapticManager] Vibration failed: {e.Message}");
        }
#endif
    }
}
