using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace Almanac;

public class CreateSprite : MonoBehaviour
{
    public string path;
    private void Start()
    {
        Sprite sprite = LoadEmbeddedSprite(path);
        if (sprite != null) GetComponent<Image>().sprite = sprite;
    }

    private Sprite LoadEmbeddedSprite(string resourcePath)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        using (var stream = assembly.GetManifestResourceStream(resourcePath))
        {
            if (stream != null)
            {
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                Texture2D texture = new Texture2D(2, 2);
                if (texture.LoadImage(buffer))
                {
                    return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                }
            }
        }

        return null;
    }
}