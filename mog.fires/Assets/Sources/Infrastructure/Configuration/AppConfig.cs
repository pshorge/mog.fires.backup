using System;
using System.Collections.Generic;
using System.Text;
using Sources.Infrastructure.Input.Actions;

namespace Sources.Infrastructure.Configuration
{
    [Serializable]
    public class AppConfig
    {
        public InputConfig Input { get; set; } = new();
        public GlobeConfig Globe { get; set; } = new();
        public MapConfig Map { get; set; } = new();
        public CameraConfig Camera { get; set; } = new();

    }

    // --- INPUT SECTION ---
    [Serializable]
    public class InputConfig
    {
        public List<KeyBindingConfig> Keyboard { get; set; } = new();
        public List<SerialBindingConfig> Serial { get; set; } = new();

        // Serial Port Settings
        public string PortName { get; set; } = "COM3";
        public int BaudRate { get; set; } = 9600;
        public bool SerialEnabled { get; set; } = true;
        public float DebounceTimeSeconds { get; set; } = 0.2f;

        public float SelectionThresholdPixels { get; set; } = 100f;
        public float MenuScrollThreshold { get; set; } = 20f;
        public int ScrollStep { get; set; } = 1;

        // Cursor settings (MapController)
        public float CursorSensitivity { get; set; } = 1.5f;
        public float CursorSmoothTime { get; set; } = 0.1f;
    }

    [Serializable]
    public class KeyBindingConfig
    {
        public string Key { get; set; }
        public InputActionType Action { get; set; }
    }

    [Serializable]
    public class SerialBindingConfig
    {
        public String Message { get; set; }
        public InputActionType Action { get; set; }
    }

    // --- GLOBE SECTION ---
    [Serializable]
    public class GlobeConfig
    {
        public float LonOffset { get; set; } = -100f;
        public float LatOffset { get; set; } = 0f;
        public bool InvertLon { get; set; } = false;
    }

    // --- MAP SECTION ---
    [Serializable]
    public class MapConfig
    {
        // Original Image Size
        public float OriginalWidth { get; set; } = 1632f;
        public float OriginalHeight { get; set; } = 1621f;

        // Calibration Points
        public GeoPoint Ref1 { get; set; } = new() { Lat = 50.66211, Lon = 17.69515, X = 320f, Y = 263f };
        public GeoPoint Ref2 { get; set; } = new() { Lat = 49.57325, Lon = 19.53078, X = 1524f, Y = 1353f };
    }

    [Serializable]
    public class GeoPoint
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
    }

    // --- CAMERA / ORBIT SECTION (EarthController) ---
    [Serializable]
    public class CameraConfig
    {
        public float YawSensitivity { get; set; } = 0.12f;
        public float PitchSensitivity { get; set; } = 0.1f;
        public float Damping { get; set; } = 0.97f;
        public bool InvertYaw { get; set; } = false;
        public bool InvertPitch { get; set; } = true;
        public float MinPitch { get; set; } = -85f;
        public float MaxPitch { get; set; } = 85f;
    }

    class AppConfigLogger
    {
        private readonly AppConfig _config;

        public AppConfigLogger(AppConfig config)
        {
            _config = config;
        }

        public void Log()
        {

            var sb = new StringBuilder();
            sb.AppendLine("========== APP CONFIG ==========");

            // INPUT
            sb.AppendLine("\n--- INPUT ---");
            sb.AppendLine($"  SelectionThresholdPixels: {_config.Input.SelectionThresholdPixels}");
            sb.AppendLine($"  MenuScrollThreshold: {_config.Input.MenuScrollThreshold}");
            sb.AppendLine($"  ScrollStep: {_config.Input.ScrollStep}");
            sb.AppendLine($"  CursorSensitivity: {_config.Input.CursorSensitivity}");
            sb.AppendLine($"  CursorSmoothTime: {_config.Input.CursorSmoothTime}");

            sb.AppendLine("  Keyboard Bindings:");
            foreach (var kb in _config.Input.Keyboard)
                sb.AppendLine($"    [{kb.Key}] -> {kb.Action}");

            sb.AppendLine("  Serial Bindings:");
            foreach (var sb2 in _config.Input.Serial)
                sb.AppendLine($"    [Button {sb2.Message}] -> {sb2.Action}");

            // GLOBE
            sb.AppendLine("\n--- GLOBE ---");
            sb.AppendLine($"  LonOffset: {_config.Globe.LonOffset}");
            sb.AppendLine($"  LatOffset: {_config.Globe.LatOffset}");
            sb.AppendLine($"  InvertLon: {_config.Globe.InvertLon}");

            // MAP
            sb.AppendLine("\n--- MAP ---");
            sb.AppendLine($"  OriginalSize: {_config.Map.OriginalWidth} x {_config.Map.OriginalHeight}");
            sb.AppendLine($"  Ref1: Lat={_config.Map.Ref1.Lat}, Lon={_config.Map.Ref1.Lon}, X={_config.Map.Ref1.X}, Y={_config.Map.Ref1.Y}");
            sb.AppendLine($"  Ref2: Lat={_config.Map.Ref2.Lat}, Lon={_config.Map.Ref2.Lon}, X={_config.Map.Ref2.X}, Y={_config.Map.Ref2.Y}");

            // CAMERA
            sb.AppendLine("\n--- CAMERA ---");
            sb.AppendLine($"  YawSensitivity: {_config.Camera.YawSensitivity}");
            sb.AppendLine($"  PitchSensitivity: {_config.Camera.PitchSensitivity}");
            sb.AppendLine($"  Damping: {_config.Camera.Damping}");
            sb.AppendLine($"  InvertYaw: {_config.Camera.InvertYaw}");
            sb.AppendLine($"  InvertPitch: {_config.Camera.InvertPitch}");
            sb.AppendLine($"  PitchRange: [{_config.Camera.MinPitch}, {_config.Camera.MaxPitch}]");

            sb.AppendLine("\n================================");

            UnityEngine.Debug.Log(sb.ToString());
        }
    }
}
