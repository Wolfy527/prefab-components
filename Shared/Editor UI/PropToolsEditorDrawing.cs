namespace Wolfy.PropTools.EditorUI
{
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class PropToolsEditorDrawing
{
    private const int GradientResolution = 256;
    private const int RoundedCornerSegments = 5;
    private static readonly Vector3[] roundedVertices =
        new Vector3[RoundedCornerSegments * 4];
    private static readonly Dictionary<GradientKey, Texture2D> gradientCache =
        new Dictionary<GradientKey, Texture2D>();
    private static readonly Dictionary<RoundedTextureKey, Texture2D>
        roundedTextureCache =
            new Dictionary<RoundedTextureKey, Texture2D>();

    public static Texture2D Texture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }

    public static Texture2D RoundedTexture(
        Color fill,
        Color border,
        int radius = 4)
    {
        radius = Mathf.Max(1, radius);
        RoundedTextureKey key =
            new RoundedTextureKey(fill, border, radius);

        if (roundedTextureCache.TryGetValue(
                key,
                out Texture2D cached
            ) &&
            cached != null)
        {
            return cached;
        }

        int size = radius * 2 + 4;
        Texture2D texture = new Texture2D(
            size,
            size,
            TextureFormat.RGBA32,
            false
        )
        {
            name = "Prop Tools Rounded Surface",
            hideFlags = HideFlags.HideAndDontSave,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        Color32[] pixels = new Color32[size * size];
        Color32 transparent = new Color32(0, 0, 0, 0);
        Color32 fill32 = fill;
        Color32 border32 = border;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool insideOuter = IsInsideRoundedRect(
                    x,
                    y,
                    size,
                    size,
                    radius
                );
                bool insideInner = IsInsideRoundedRect(
                    x - 1f,
                    y - 1f,
                    size - 2f,
                    size - 2f,
                    Mathf.Max(0f, radius - 1f)
                );
                pixels[y * size + x] = !insideOuter
                    ? transparent
                    : insideInner ? fill32 : border32;
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply(false, true);
        roundedTextureCache[key] = texture;
        return texture;
    }

    public static Texture2D HorizontalGradient(Color left, Color middle, Color right, int width = 128, int height = 1)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        for (int x = 0; x < width; x++)
        {
            float t = x / (float)(width - 1);
            Color color = t < 0.5f ? Color.Lerp(left, middle, t * 2f) : Color.Lerp(middle, right, (t - 0.5f) * 2f);
            for (int y = 0; y < height; y++) texture.SetPixel(x, y, color);
        }

        texture.Apply();
        return texture;
    }

    public static void HorizontalGradient(Rect rect, Color left, Color right)
    {
        if (Event.current.type != EventType.Repaint) return;
        GUI.DrawTexture(PixelAlign(rect), Gradient(left, right), ScaleMode.StretchToFill, false);
    }

    public static void HorizontalGradient(Rect rect, Color left, Color middle, Color right)
    {
        if (Event.current.type != EventType.Repaint) return;
        GUI.DrawTexture(PixelAlign(rect), Gradient(left, middle, right), ScaleMode.StretchToFill, false);
    }

    public static void VerticalGradient(Rect rect, Color top, Color bottom)
    {
        if (Event.current.type != EventType.Repaint) return;
        GUI.DrawTexture(PixelAlign(rect), Gradient(top, bottom, true), ScaleMode.StretchToFill, false);
    }

    public static void Border(Rect rect, Color color)
    {
        if (Event.current.type != EventType.Repaint) return;

        rect = PixelAlign(rect);
        float pixel = 1f / EditorGUIUtility.pixelsPerPoint;

        EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, pixel), color);
        EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - pixel, rect.width, pixel), color);
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, pixel, rect.height), color);
        EditorGUI.DrawRect(new Rect(rect.xMax - pixel, rect.y, pixel, rect.height), color);
    }

    public static void BottomLine(Rect rect, Color color)
    {
        if (Event.current.type != EventType.Repaint) return;
        EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), color);
    }

    public static void LabelDivider(
        Rect row,
        float x,
        Color? color = null)
    {
        if (Event.current.type != EventType.Repaint)
            return;

        EditorGUI.DrawRect(
            new Rect(
                x,
                row.y + 6f,
                1f,
                Mathf.Max(0f, row.height - 12f)
            ),
            color ?? PropToolsEditorTheme.BorderSoft
        );
    }

    public static void AccentBar(Rect rect, Color color, float width = 4f)
    {
        if (Event.current.type != EventType.Repaint) return;
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, width, rect.height), color);
    }

    public static void RoundedAccentBar(
        Rect rect,
        Color color,
        float width = 4f,
        float verticalInset = 4f)
    {
        if (Event.current.type != EventType.Repaint)
            return;

        Rect bar = new Rect(
            rect.x + 1f,
            rect.y + verticalInset,
            width,
            Mathf.Max(0f, rect.height - verticalInset * 2f)
        );
        RoundedRect(
            bar,
            color,
            color,
            Mathf.Min(2f, width * 0.5f)
        );
    }

    public static void Divider(float alpha = 0.25f)
    {
        Rect rect = GUILayoutUtility.GetRect(0, 1f, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(rect, new Color(PropToolsEditorTheme.Accent.r, PropToolsEditorTheme.Accent.g, PropToolsEditorTheme.Accent.b, alpha));
    }

    public static void Inset(Rect rect, Color background, Color border)
    {
        if (Event.current.type != EventType.Repaint) return;
        RoundedRect(rect, background, border, 4f);
    }

    public static void RoundedRect(
        Rect rect,
        Color fill,
        Color border,
        float radius = 4f,
        bool shadow = false)
    {
        if (Event.current.type != EventType.Repaint)
            return;

        rect = PixelAlign(rect);
        float pixel = 1f / EditorGUIUtility.pixelsPerPoint;

        Handles.BeginGUI();

        if (shadow)
        {
            Rect shadowRect = new Rect(rect.x, rect.y + pixel, rect.width, rect.height);
            DrawRoundedPolygon(shadowRect, radius, new Color(0f, 0f, 0f, 0.34f));
        }

        DrawRoundedPolygon(rect, radius, border);

        Rect inner = new Rect(
            rect.x + pixel,
            rect.y + pixel,
            Mathf.Max(0f, rect.width - pixel * 2f),
            Mathf.Max(0f, rect.height - pixel * 2f)
        );
        DrawRoundedPolygon(inner, Mathf.Max(0f, radius - pixel), fill);

        Handles.EndGUI();
    }

    private static void DrawRoundedPolygon(Rect rect, float radius, Color color)
    {
        radius = Mathf.Clamp(radius, 0f, Mathf.Min(rect.width, rect.height) * 0.5f);
        int vertex = 0;

        AddRoundedCorner(rect.xMin + radius, rect.yMin + radius, radius, 180f, 270f, ref vertex);
        AddRoundedCorner(rect.xMax - radius, rect.yMin + radius, radius, 270f, 360f, ref vertex);
        AddRoundedCorner(rect.xMax - radius, rect.yMax - radius, radius, 0f, 90f, ref vertex);
        AddRoundedCorner(rect.xMin + radius, rect.yMax - radius, radius, 90f, 180f, ref vertex);

        Color previous = Handles.color;
        Handles.color = color;
        Handles.DrawAAConvexPolygon(roundedVertices);
        Handles.color = previous;
    }

    private static void AddRoundedCorner(
        float centerX,
        float centerY,
        float radius,
        float startAngle,
        float endAngle,
        ref int vertex)
    {
        for (int i = 0; i < RoundedCornerSegments; i++)
        {
            float t = i / (float)(RoundedCornerSegments - 1);
            float radians = Mathf.Lerp(startAngle, endAngle, t) * Mathf.Deg2Rad;
            roundedVertices[vertex++] = new Vector3(
                centerX + Mathf.Cos(radians) * radius,
                centerY + Mathf.Sin(radians) * radius,
                0f
            );
        }
    }

    private static bool IsInsideRoundedRect(
        float x,
        float y,
        float width,
        float height,
        float radius)
    {
        if (x < 0f || y < 0f || x >= width || y >= height)
            return false;

        float centerX = Mathf.Clamp(
            x,
            radius - 0.5f,
            width - radius - 0.5f
        );
        float centerY = Mathf.Clamp(
            y,
            radius - 0.5f,
            height - radius - 0.5f
        );
        float deltaX = x - centerX;
        float deltaY = y - centerY;
        return deltaX * deltaX + deltaY * deltaY <= radius * radius;
    }

    public static void Triangle(Rect rect, bool down, Color color)
    {
        Vector3 a, b, c;
        if (down)
        {
            a = new Vector3(rect.x + rect.width * 0.15f, rect.y + rect.height * 0.30f);
            b = new Vector3(rect.x + rect.width * 0.85f, rect.y + rect.height * 0.30f);
            c = new Vector3(rect.x + rect.width * 0.50f, rect.y + rect.height * 0.75f);
        }
        else
        {
            a = new Vector3(rect.x + rect.width * 0.30f, rect.y + rect.height * 0.15f);
            b = new Vector3(rect.x + rect.width * 0.30f, rect.y + rect.height * 0.85f);
            c = new Vector3(rect.x + rect.width * 0.75f, rect.y + rect.height * 0.50f);
        }

        Handles.BeginGUI();
        Color previous = Handles.color;
        Handles.color = color;
        Handles.DrawAAConvexPolygon(a, b, c);
        Handles.color = previous;
        Handles.EndGUI();
    }

    public static void SetTextColor(GUIStyle style, Color color)
    {
        style.normal.textColor = color;
        style.hover.textColor = color;
        style.active.textColor = color;
        style.focused.textColor = color;
        style.onNormal.textColor = color;
        style.onHover.textColor = color;
        style.onActive.textColor = color;
        style.onFocused.textColor = color;
    }

    public static Rect PixelAlign(Rect rect)
    {
        float pixelsPerPoint = EditorGUIUtility.pixelsPerPoint;
        float xMin = Mathf.Round(rect.xMin * pixelsPerPoint) / pixelsPerPoint;
        float yMin = Mathf.Round(rect.yMin * pixelsPerPoint) / pixelsPerPoint;
        float xMax = Mathf.Round(rect.xMax * pixelsPerPoint) / pixelsPerPoint;
        float yMax = Mathf.Round(rect.yMax * pixelsPerPoint) / pixelsPerPoint;
        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }

    private static Texture2D Gradient(Color first, Color second, bool vertical = false)
    {
        return Gradient(first, Color.Lerp(first, second, 0.5f), second, vertical, false);
    }

    private static Texture2D Gradient(Color first, Color middle, Color last)
    {
        return Gradient(first, middle, last, false, true);
    }

    private static Texture2D Gradient(
        Color first,
        Color middle,
        Color last,
        bool vertical,
        bool useMiddle)
    {
        GradientKey key = new GradientKey(first, middle, last, vertical, useMiddle);
        if (gradientCache.TryGetValue(key, out Texture2D cached) && cached != null)
            return cached;

        int width = vertical ? 1 : GradientResolution;
        int height = vertical ? GradientResolution : 1;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
        {
            name = "Prop Tools Gradient",
            hideFlags = HideFlags.HideAndDontSave,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        Color32[] pixels = new Color32[GradientResolution];
        for (int i = 0; i < GradientResolution; i++)
        {
            float t = i / (float)(GradientResolution - 1);
            pixels[i] = useMiddle
                ? t < 0.5f
                    ? Color.Lerp(first, middle, t * 2f)
                    : Color.Lerp(middle, last, (t - 0.5f) * 2f)
                : Color.Lerp(first, last, t);
        }

        texture.SetPixels32(pixels);
        texture.Apply(false, true);
        gradientCache[key] = texture;
        return texture;
    }

    private readonly struct GradientKey
    {
        private readonly Color32 first;
        private readonly Color32 middle;
        private readonly Color32 last;
        private readonly bool vertical;
        private readonly bool useMiddle;

        public GradientKey(Color first, Color middle, Color last, bool vertical, bool useMiddle)
        {
            this.first = first;
            this.middle = middle;
            this.last = last;
            this.vertical = vertical;
            this.useMiddle = useMiddle;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is GradientKey other)) return false;
            return first.Equals(other.first) &&
                   middle.Equals(other.middle) &&
                   last.Equals(other.last) &&
                   vertical == other.vertical &&
                   useMiddle == other.useMiddle;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = first.GetHashCode();
                hash = (hash * 397) ^ middle.GetHashCode();
                hash = (hash * 397) ^ last.GetHashCode();
                hash = (hash * 397) ^ vertical.GetHashCode();
                return (hash * 397) ^ useMiddle.GetHashCode();
            }
        }
    }

    private readonly struct RoundedTextureKey
    {
        private readonly Color32 fill;
        private readonly Color32 border;
        private readonly int radius;

        public RoundedTextureKey(
            Color fill,
            Color border,
            int radius)
        {
            this.fill = fill;
            this.border = border;
            this.radius = radius;
        }

        public override bool Equals(object obj)
        {
            return obj is RoundedTextureKey other &&
                   fill.Equals(other.fill) &&
                   border.Equals(other.border) &&
                   radius == other.radius;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = fill.GetHashCode();
                hash = (hash * 397) ^ border.GetHashCode();
                return (hash * 397) ^ radius;
            }
        }
    }
}
}
