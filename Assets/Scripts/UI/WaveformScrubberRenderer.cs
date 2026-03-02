using UnityEngine;

public static class WaveformScrubberRenderer
{
    public static Texture2D Render(TrackLevelData track, int width = 512, int height = 32)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        // Desaturate colors for analog feel — 30% toward grey + soft magenta tint
        Color rawPrimary = track?.primaryColor ?? Color.cyan;
        Color rawSecondary = track?.secondaryColor ?? Color.magenta;
        Color primary = Color.Lerp(rawPrimary, MixtapeColors.SoftMagenta, 0.2f);
        primary = Color.Lerp(primary, Color.grey, 0.25f);
        Color secondary = Color.Lerp(rawSecondary, MixtapeColors.SoftMagenta, 0.2f);
        secondary = Color.Lerp(secondary, Color.grey, 0.25f);
        Color accent = track?.accentColor ?? Color.white;
        float duration = (track?.audioClip != null) ? track.audioClip.length : 180f;
        if (duration <= 0f) duration = 180f;

        // Gradient background
        for (int x = 0; x < width; x++)
        {
            float t = (float)x / (width - 1);
            Color bg = Color.Lerp(primary * 0.6f, secondary * 0.6f, t);
            bg.a = 1f;
            for (int y = 0; y < height; y++)
                tex.SetPixel(x, y, bg);
        }

        // Bar lines (every 4 beats, 1px white)
        if (track != null && track.bpm > 0f)
        {
            float barInterval = (60f / track.bpm) * 4f;
            float minPixelGap = 4f;
            float pixelsPerSecond = (width - 1) / duration;
            float effectiveInterval = Mathf.Max(barInterval, minPixelGap / pixelsPerSecond);
            Color beatColor = new Color(1f, 1f, 1f, 0.25f);
            float t = track.beatOffset;
            while (t < duration)
            {
                int x = Mathf.RoundToInt((t / duration) * (width - 1));
                x = Mathf.Clamp(x, 0, width - 1);
                for (int y = 0; y < height; y++)
                    tex.SetPixel(x, y, beatColor);
                t += effectiveInterval;
            }
        }

        // Drop lines (3px accent)
        if (track?.dropTimestamps != null)
        {
            Color dropColor = new Color(accent.r, accent.g, accent.b, 1f);
            foreach (float drop in track.dropTimestamps)
            {
                int cx = Mathf.RoundToInt((drop / duration) * (width - 1));
                for (int dx = -1; dx <= 1; dx++)
                {
                    int px = Mathf.Clamp(cx + dx, 0, width - 1);
                    for (int y = 0; y < height; y++)
                        tex.SetPixel(px, y, dropColor);
                }
            }
        }

        tex.Apply();
        return tex;
    }
}
