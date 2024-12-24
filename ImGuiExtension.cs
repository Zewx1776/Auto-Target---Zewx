using ExileCore2.Shared.Helpers;
using ImGuiNET;
using System.Drawing;
using ImGuiVector4 = System.Numerics.Vector4;

namespace WhereAreYouGoing;

public class ImGuiExtension
{
    // Int Drags
    public static int IntDrag(string labelString, int value, int minValue, int maxValue, float dragSpeed)
    {
        var refValue = value;
        ImGui.DragInt(labelString, ref refValue, dragSpeed, minValue, maxValue);
        return refValue;
    }

    // Color Pickers
    public static Color ColorPicker(string labelName, Color inputColor)
    {
        var color = inputColor.ToImguiVec4();
        var colorToVect4 = new ImGuiVector4(color.X, color.Y, color.Z, color.W);

        if (ImGui.ColorEdit4(labelName, ref colorToVect4, ImGuiColorEditFlags.AlphaBar))
        {
            return Color.FromArgb((int)(colorToVect4.W * 255), (int)(colorToVect4.X * 255), (int)(colorToVect4.Y * 255), (int)(colorToVect4.Z * 255));
        }

        return inputColor;
    }

    // Helper method for color conversion
    private static ImGuiVector4 ToImguiVec4(Color color) => new ImGuiVector4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);

    // Checkboxes
    public static bool Checkbox(string labelString, bool boolValue)
    {
        ImGui.Checkbox(labelString, ref boolValue);
        return boolValue;
    }
}