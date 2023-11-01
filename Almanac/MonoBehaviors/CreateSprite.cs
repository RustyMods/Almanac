using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace Almanac.MonoBehaviors;

public class CreateSprite : MonoBehaviour
{
    public string path = null!;
    private void Start()
    {
        Sprite? sprite = LoadEmbeddedSprite(path);
        if (sprite != null) GetComponent<Image>().sprite = sprite;
    }

    private Sprite? LoadEmbeddedSprite(string resourcePath)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourcePath);
        if (stream == null) return null;
        byte[] buffer = new byte[stream.Length];
        stream.Read(buffer, 0, buffer.Length);
        Texture2D texture = new Texture2D(2, 2);
        return texture.LoadImage(buffer) ? Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero) : null;
    }
}