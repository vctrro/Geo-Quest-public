using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetManager
{
    public string requestString;
    public Sprite sprite;
    public AudioClip audioClip;
    public byte[] audioClipData;
    public byte[] imageData;
    public IEnumerator GetRequest(string uri)
    {
        requestString = null;
        Debug.Log(uri);
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
                Debug.Log(": Error: " + webRequest.error);
            }
            else
            {
                requestString = webRequest.downloadHandler.text;
            }
        }
    }

    public IEnumerator GetTexture(string uri, bool downloading = false)
    {
        sprite = null;
        imageData = null;
        Debug.Log(uri);
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(uri))
        {
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                // Get downloaded asset bundle
                var texture = DownloadHandlerTexture.GetContent(uwr);

                // if (texture.width > 940)
                // {
                //     float aspectRatio = 940.0f/(float)texture.width;
                //     int height = (int)(texture.height * aspectRatio);
                //     texture = ResizeTex(texture, 940, height);
                // }
                if (downloading)
                {
                    imageData = uwr.downloadHandler.data;
                }
                else
                {
                    texture.Compress(true);
                    sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),  new Vector2(0.5f, 0.5f));
                }
            }
        }
    }

    public IEnumerator GetAudioClip(string uri,  string fileType, bool downloading = false)
    {
        audioClip = null;
        audioClipData = null;

        AudioType audioType = new AudioType();
        switch (fileType)
        {
            case ".mp3":
                audioType = AudioType.MPEG;
                break;
            case ".ogg":
                audioType = AudioType.OGGVORBIS;
                break;
            case ".wav":
                audioType = AudioType.WAV;
                break;
        }
        using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(uri, audioType))
        {
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                if (downloading)
                {
                    audioClipData = uwr.downloadHandler.data;
                }
                else
                {
                    audioClip = DownloadHandlerAudioClip.GetContent(uwr);
                }
                
            }
        }
    }

    public Texture2D ResizeTex(Texture2D source, int newWidth, int newHeight)
    {
        source.filterMode = FilterMode.Trilinear;
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        rt.filterMode = FilterMode.Trilinear;
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);
        Texture2D nTex = new Texture2D(newWidth, newHeight);
        nTex.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0,0);
        nTex.Apply();
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        return nTex;
    }
}
