using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public static class Utils
{
    private static System.Random systemRandom = new System.Random();


    #region Camera
    static Camera _cam;
    public static Camera mainCam
    {
        get
        {
            if (_cam == null)
            {
                _cam = Camera.main;
            }
            return _cam;
        }
    }

    private static Vector2 _size = Vector2.zero;
    public static Vector2 camSize
    {
        get
        {
            if (_size == Vector2.zero)
            {
                float orthographicSize = mainCam.orthographicSize;
                var x = mainCam.aspect * 2f * orthographicSize;
                var y = 2f * orthographicSize;
                _size = new(x, y);
            }

            return _size;
        }
    }

    public static bool IsInsideCamView(Vector2 target, bool checkHor = true, bool checkVert = true)
    {
        var pos = mainCam.transform.position;
        var isInSide = true;

        var padding = 4f;
        if (checkHor)
        {
            var left = pos.x - camSize.x / 2f - padding;
            var right = pos.x + camSize.x / 2f + padding;
            isInSide = target.x.Between(left, right);
        }

        if (checkVert && isInSide)
        {
            var up = pos.y + camSize.y / 2f + padding;
            var down = pos.y - camSize.y / 2f - padding;
            isInSide = target.x.Between(down, up);
        }

        return isInSide;
    }

    public static RaycastHit2D RaycastCamera2D(Camera cam = null)
    {
        if (cam == null) cam = mainCam;

        RaycastHit2D hit = Physics2D.Raycast(cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

        return hit;
    }

    public static RaycastHit2D[] RaycastAllCamera2D(Vector3 position, Camera cam = null)
    {
        if (cam == null) cam = mainCam;

        RaycastHit2D[] hits = Physics2D.RaycastAll(cam.ScreenToWorldPoint(position), Vector2.zero);

        return hits;
    }

    public static Vector3 GetMouseWorldPosition()
    {
        return mainCam.ScreenToWorldPoint(Input.mousePosition);
    }
    #endregion



    public static string GetNameOfEnum(Enum e)
    {
        return e.ToString();
    }

    /// <summary>
    /// Check if a specific layer is in a LayerMask
    /// </summary>
    /// <param name="mask"></param>
    /// <param name="layer"></param>
    /// <returns></returns>
    public static bool CheckIfInMask(LayerMask mask, int layer)
    {
        return mask == (mask | (1 << layer));
    }

    public static string SplitStringByUpperCase(string input)
    {
        StringBuilder result = new StringBuilder();

        for (int i = 0; i < input.Length; i++)
        {
            char currentChar = input[i];

            // Add a space before uppercase letters (except for the first letter)
            if (i > 0 && char.IsUpper(currentChar))
            {
                result.Append(' ');
            }

            result.Append(currentChar);
        }

        return result.ToString();
    }

    public static int GetNumberOfVariables<T>()
    {
        Type type = typeof(T);
        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        return fields.Length;
    }

    /// <summary>
    /// Get random n elements from a list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="n"></param>
    /// <returns></returns>
    public static List<T> GetRandomElements<T>(List<T> list, int n)
    {
        // Check if the list contains enough elements
        if (list.Count < n)
        {
            Debug.LogError("List does not contain enough elements.");
            return null;
        }

        // Create a copy of the list to avoid modifying the original list
        List<T> copyList = new List<T>(list);

        ShuffleList(copyList);

        // Take the first n elements from the shuffled list
        List<T> result = copyList.GetRange(0, n);

        return result;
    }

    public static T GetRandomElement<T>(T[] array)
    {
        if (array == null || array.Length == 0)
        {
            Debug.LogWarning("The array is null or empty.");
            return default(T);
        }

        int randomIndex = Random.Range(0, array.Length);
        return array[randomIndex];
    }

    public static T GetRandomElement<T>(List<T> list)
    {
        if (list == null || list.Count == 0)
        {
            Debug.LogWarning("The list is null or empty.");
            return default(T);
        }

        int randomIndex = Random.Range(0, list.Count);
        return list[randomIndex];
    }

    #region Math
    public static float MapNumber(float value, float inMin, float inMax, float outMin, float outMax)
    {
        // Map the value from one range to another
        return (value - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
    }

    public static bool Between(this float val, float min, float max)
    {
        return val >= min && val <= max;
    }
    public static bool Between(this int val, int min, int max)
    {
        return val >= min && val <= max;
    }
    #endregion

    #region Isometric
    public static Vector3 CartToIso(Vector3Int coord, Vector3 offset)
    {
        float isoX = (coord.x - coord.y) / 2f;
        float isoY = (coord.x + coord.y + 2) / 4f + (coord.z + 1) / 4f;
        return new Vector3(isoX, isoY) + offset;
    }
    #endregion



    /// <summary>
    /// Shuffle a List randomly
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    public static void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = systemRandom.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    /// <summary>
    /// Swap the position two elements in a List
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="element1"></param>
    /// <param name="element2"></param>
    public static void Swap<T>(List<T> list, T element1, T element2)
    {
        int index1 = list.IndexOf(element1);
        int index2 = list.IndexOf(element2);

        Swap(list, index1, index2);
    }

    /// <summary>
    /// Swap the position two elements in a List based on index
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="index1"></param>
    /// <param name="index2"></param>
    public static void Swap<T>(List<T> list, int index1, int index2)
    {
        if (index1 >= 0 && index1 < list.Count && index2 >= 0 && index2 < list.Count)
        {
            T temp = list[index1];
            list[index1] = list[index2];
            list[index2] = temp;
        }
        else
        {
            Debug.LogError("Invalid indices for swapping elements.");
        }
    }

    /// <summary>
    /// Roll a percentage chance, the input probability input must be between 0->1
    /// </summary>
    /// <param name="probability"></param>
    /// <returns></returns>
    public static bool RollChance(float probability)
    {
        float randomValue = Random.Range(0f, 1f);
        return randomValue < probability;
    }

    /// <summary>
    /// Detach all children of a gameObject, including nested children
    /// </summary>
    /// <param name="parent"></param>
    public static List<Transform> DetachAllChildrenRecursively(Transform parent)
    {
        List<Transform> detachedChildren = new List<Transform>();

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            detachedChildren.AddRange(DetachAllChildrenRecursively(child));
            child.parent = null;
            detachedChildren.Add(child);
        }

        return detachedChildren;
    }


    public static Vector2 GetSpritePivot(Sprite sprite)
    {
        Bounds bounds = sprite.bounds;
        float pivotX = -bounds.center.x / bounds.extents.x / 2 + 0.5f;
        float pivotY = -bounds.center.y / bounds.extents.y / 2 + 0.5f;
        return new Vector2(pivotX, pivotY);
    }

    public static Transform FindDeepChild(this Transform aParent, string aName)
    {
        foreach (Transform child in aParent)
        {
            if (child.name == aName)
                return child;
            var result = child.FindDeepChild(aName);
            if (result != null)
                return result;
        }
        return null;
    }

    public static void AddExplosionForce(this Rigidbody2D rb, float explosionForce, Vector2 explosionPosition, float explosionRadius, float upwardsModifier = 0.0F, ForceMode2D mode = ForceMode2D.Impulse)
    {
        var explosionDir = rb.position - explosionPosition;
        var explosionDistance = explosionDir.magnitude;

        if (explosionDistance == 0)
            return;

        if (upwardsModifier == 0)
            explosionDir /= explosionDistance;
        else
        {
            explosionDir.y += upwardsModifier;
            explosionDir.Normalize();
        }

        explosionDistance = Mathf.Clamp(explosionDistance, 0, explosionRadius);
        float attenuation = 1 - (explosionDistance / explosionRadius);
        rb.AddForce(attenuation * explosionForce * explosionDir, mode);
    }

    public static Vector2 GetTexture2DPivot(Texture2D texture)
    {
        if (texture == null)
        {
            Debug.LogError("Texture is null.");
            return Vector2.zero;
        }

        Color32[] pixels = texture.GetPixels32();

        int width = texture.width;
        int height = texture.height;

        int minX = width, maxX = 0, minY = height, maxY = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color32 pixel = pixels[y * width + x];
                if (pixel.a > 0)
                {
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }
        }

        // Calculate the center of the bounding box
        float centerX = (minX + maxX) / 2f;
        float centerY = (minY + maxY) / 2f;

        // Normalize the center coordinates to the range 0-1
        Vector2 normalizedCenter = new Vector2(centerX / width, centerY / height);

        return normalizedCenter;
    }

    public static Texture2D ResizeTexture(Texture2D source, int newWidth, int newHeight, bool preserveAspect = false)
    {
        float sourceAspectRatio = (float)source.width / source.height;
        float targetAspectRatio = (float)newWidth / newHeight;

        if (preserveAspect)
        {
            // Adjust the width or height to preserve aspect ratio
            if (sourceAspectRatio > targetAspectRatio)
            {
                newHeight = Mathf.RoundToInt(newWidth / sourceAspectRatio);
            }
            else
            {
                newWidth = Mathf.RoundToInt(newHeight * sourceAspectRatio);
            }
        }

        // Create a RenderTexture with the adjusted dimensions
        RenderTexture rt = new RenderTexture(newWidth, newHeight, 24);
        RenderTexture.active = rt;

        // Blit the source texture to the RenderTexture
        Graphics.Blit(source, rt);

        // Create a new Texture2D with the adjusted dimensions
        Texture2D result = new Texture2D(newWidth, newHeight, source.format, false);

        // Read the RenderTexture contents into the new Texture2D
        result.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        result.Apply();

        // Cleanup
        RenderTexture.active = null;
        rt.Release();

        return result;
    }

    public static int LevenshteinDistance(string A, string B)
    {
        int[,] matrix = new int[A.Length + 1, B.Length + 1];

        for (int i = 0; i <= A.Length; i++)
            matrix[i, 0] = i;
        for (int j = 0; j <= B.Length; j++)
            matrix[0, j] = j;

        for (int i = 1; i <= A.Length; i++)
        {
            for (int j = 1; j <= B.Length; j++)
            {
                int cost = (A[i - 1] == B[j - 1]) ? 0 : 1;
                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost
                );
            }
        }
        return matrix[A.Length, B.Length];
    }

    public static bool AreStringsSimilar(string A, string B, double threshold = 0.8)
    {
        if (A == B) return true;
        A = A.ToLower();
        B = B.ToLower();
        int distance = LevenshteinDistance(A, B);
        double maxLen = Math.Max(A.Length, B.Length);
        double similarity = (maxLen - distance) / maxLen;
        return similarity >= threshold;
    }

    public static Texture2D RetrieveReadableTexture(Texture2D originalTexture)
    {
        Texture2D inputReferenceImage = originalTexture;

        // Create a new readable texture
        Texture2D readableTexture = new Texture2D(inputReferenceImage.width, inputReferenceImage.height, inputReferenceImage.format, false);
        RenderTexture temporaryRenderTexture = RenderTexture.GetTemporary(
            inputReferenceImage.width,
            inputReferenceImage.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear);

        // Copy the original texture to the temporary render texture
        Graphics.Blit(inputReferenceImage, temporaryRenderTexture);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = temporaryRenderTexture;

        // Copy the render texture to the new readable texture
        readableTexture.ReadPixels(new Rect(0, 0, temporaryRenderTexture.width, temporaryRenderTexture.height), 0, 0);
        readableTexture.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(temporaryRenderTexture);

        return readableTexture;
    }

    public static TextMeshPro CreateWorldText(string text, Transform parent = null, Vector3 localPosition = default, int fontSize = 40, Color color = default, float destroyAfter = 0)
    {
        if (color == null) color = Color.white;
        GameObject gameObject = new GameObject("World_Text", typeof(TextMeshPro));
        Transform transform = gameObject.transform;
        transform.SetParent(parent);
        transform.localPosition = localPosition;
        TextMeshPro textMesh = gameObject.GetComponent<TextMeshPro>();
        textMesh.text = text;
        textMesh.fontSize = fontSize;
        textMesh.color = color;
        textMesh.alignment = TextAlignmentOptions.Center;
        if (destroyAfter != 0)
        {
            UnityEngine.Object.Destroy(gameObject, destroyAfter);
        }
        return textMesh;
    }

    public static Color HexToColor(string hex)
    {
        // Remove the '#' if it exists
        if (hex.StartsWith("#"))
        {
            hex = hex.Substring(1);
        }

        // Ensure that the string is either 6 or 8 characters long
        if (hex.Length != 6 && hex.Length != 8)
        {
            Debug.LogError("Invalid hex string length. Must be 6 or 8 characters.");
            return Color.black; // Return black as a fallback
        }

        // Parse the color components (RGB or RGBA)
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        byte a = 255; // Default alpha is fully opaque

        // If the string includes alpha (RGBA format), parse it
        if (hex.Length == 8)
        {
            a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
        }

        // Normalize the byte values to 0-1 range for Unity's Color
        return new Color32(r, g, b, a);
    }

    public static void CopyValuesFrom(this RectTransform target, RectTransform source)
    {
        if (target == null || source == null)
        {
            Debug.LogError("RectTransform is null!");
            return;
        }

        // Copy position, size, rotation
        target.anchoredPosition = source.anchoredPosition;
        target.sizeDelta = source.sizeDelta;
        target.localScale = source.localScale;
        target.localRotation = source.localRotation;

        // Copy anchor, pivot, and offsets
        target.anchorMin = source.anchorMin;
        target.anchorMax = source.anchorMax;
        target.pivot = source.pivot;
        target.offsetMin = source.offsetMin;
        target.offsetMax = source.offsetMax;
    }

    public static Dictionary<T, int> GetCounts<T>(List<T> items)
    {
        Dictionary<T, int> counts = new Dictionary<T, int>();
        foreach (T item in items)
        {
            if (counts.ContainsKey(item))
            {
                counts[item]++;
            }
            else
            {
                counts[item] = 1;
            }
        }
        return counts;
    }

    public static void CopyToClipboard(string text)
    {
        GUIUtility.systemCopyBuffer = text;
        Debug.Log("Text copied to clipboard: " + text);
    }
}
