using System.Numerics;

namespace _2DFluidSim.Fields;
internal class FieldDifferentiator {
    private static readonly float OneOverRoot2 = 1f / (float) Math.Sqrt(2);

    /// <summary>
    /// The value that points outside the given array are considered to be when calculating. If <c>null</c>, the value
    /// of the current point being differentiated will be used each time
    /// </summary>
    public float? OutsideValue = null;

    public FieldDifferentiator() { }
    public FieldDifferentiator(float? outsideValue) { OutsideValue = outsideValue; }

    public Vector2[,] Differentiate(float[,] field) {
        int width = field.GetLength(0);
        int height = field.GetLength(1);

        float valueAt(int x, int y, float defaultValue) {
            if(x >= 0 && x < width && y >= 0 && y < height) return field[x, y];
            return defaultValue;
        }

        Vector2[,] result = new Vector2[width, height];
        for(int x = 0; x < width; x++) {
            for(int y = 0; y < height; y++) {
                float pointValue = field[x, y];
                float dValue = OutsideValue is null ? pointValue : OutsideValue.Value;
                Vector2 differential = Vector2.Zero;

                differential += new Vector2(1, 0) * (valueAt(x + 1, y, dValue) - pointValue);
                differential += new Vector2(OneOverRoot2, OneOverRoot2) * (valueAt(x + 1, y + 1, dValue) - pointValue);
                differential += new Vector2(0, 1) * (valueAt(x, y + 1, dValue) - pointValue);
                differential += new Vector2(-OneOverRoot2, OneOverRoot2) * (valueAt(x - 1, y + 1, dValue) - pointValue);
                differential += new Vector2(-1, 0) * (valueAt(x - 1, y, dValue) - pointValue);
                differential += new Vector2(-OneOverRoot2, -OneOverRoot2) * (valueAt(x - 1, y - 1, dValue) - pointValue);
                differential += new Vector2(0, -1) * (valueAt(x, y - 1, dValue) - pointValue);
                differential += new Vector2(OneOverRoot2, -OneOverRoot2) * (valueAt(x + 1, y - 1, dValue) - pointValue);

                result[x, y] = differential;
            }
        }

        return result;
    }
}
