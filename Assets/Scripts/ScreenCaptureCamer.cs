using UnityEngine;
using System.IO;
using UnityEditor;
using System.Collections.Generic;

public class ScreenCaptureCamer : MonoBehaviour
{
    private Camera _camera;
    public int fileCounter = 0;
    private List<Texture2D> images;

    private void Update()
    {
        if (Input.GetKey(KeyCode.R))
        {
            CamCapture();
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            ConvertAndWriteVideo();
        }
    }

    private void ConvertAndWriteVideo()
    {
        foreach(Texture2D image in images)
        {

            byte[] bytes = image.EncodeToPNG();
            Destroy(image);

            File.WriteAllBytes(Application.dataPath + "/Captures/" + fileCounter + ".png", bytes);
            fileCounter++;
        }
    }

#if UNITY_EDITOR
    [MenuItem("GameObject/Create Folder")]
    static void CreateFolder()
    {
        string guid = AssetDatabase.CreateFolder("Assets", "Captures");
        string newFolderPath = AssetDatabase.GUIDToAssetPath(guid);
    }
#endif
    void CamCapture()
    {
        RenderTexture activeRenderTexture = RenderTexture.active;
        RenderTexture.active = _camera.targetTexture;

        _camera.Render();

        Texture2D image = new Texture2D(_camera.targetTexture.width, _camera.targetTexture.height);
        image.ReadPixels(new Rect(0, 0, _camera.targetTexture.width, _camera.targetTexture.height), 0, 0);
        image.Apply();
        this.images.Add(image);
        RenderTexture.active = activeRenderTexture;

    }
    // Start is called before the first frame update
    void Start()
    {
#if UNITY_EDITOR
        CreateFolder();
#endif
        images = new List<Texture2D>();
        Directory.CreateDirectory(Application.dataPath + "/Captures/");
        this._camera = GetComponent<Camera>();
    }

    
}
