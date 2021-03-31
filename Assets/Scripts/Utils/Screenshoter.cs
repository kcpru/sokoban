using UnityEngine;
using System.IO;
using UnityEngine.Rendering.PostProcessing;

public class Screenshotter
{
    public const int SIZE_X = 1280;
    public const int SIZE_Y = 720;

    public static void TakeScreenshot(string path, Camera camera)
    {
        try
        {
            PostProcessVolume postProcessing = GameObject.Find("PostProcess").GetComponent<PostProcessVolume>();
            PostProcessProfile profile = postProcessing.sharedProfile;
            profile.GetSetting<MotionBlur>().active = false;

            RenderTexture rt = new RenderTexture(SIZE_X, SIZE_Y, 24);

            camera.GetComponent<Camera>().targetTexture = rt;

            Texture2D screenShot = new Texture2D(SIZE_X, SIZE_Y, TextureFormat.RGB24, false);

            camera.GetComponent<Camera>().Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, SIZE_X, SIZE_Y), 0, 0);
            camera.GetComponent<Camera>().targetTexture = null;
            RenderTexture.active = null;
            Object.DestroyImmediate(rt);

            byte[] bytes = screenShot.EncodeToJPG();

            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }

            File.WriteAllBytes(path, bytes);
            profile.GetSetting<MotionBlur>().active = false;
        }
        catch { }
    }

    public static string GetMapIconPath(string xmlFilePath) =>
        $"{Path.GetDirectoryName(xmlFilePath)}/{Path.GetFileNameWithoutExtension(xmlFilePath)}_icon.jpg";
}