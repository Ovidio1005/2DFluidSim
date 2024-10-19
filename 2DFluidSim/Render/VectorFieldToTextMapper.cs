using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace _2DFluidSim.Render;
internal class VectorFieldToTextMapper
{
    //public const char
    //    DEFAULT_R_CHAR = '▶',
    //    DEFAULT_UR_CHAR = '◥',
    //    DEFAULT_U_CHAR = '▲',
    //    DEFAULT_UL_CHAR = '◤',
    //    DEFAULT_L_CHAR = '◀',
    //    DEFAULT_DL_CHAR = '◣',
    //    DEFAULT_D_CHAR = '▼',
    //    DEFAULT_DR_CHAR = '◢',
    //    DEFAULT_ZERO_CHAR = ' ';

    public const char
        DEFAULT_R_CHAR = '>',
        DEFAULT_UR_CHAR = '┐',
        DEFAULT_U_CHAR = '^',
        DEFAULT_UL_CHAR = '┌',
        DEFAULT_L_CHAR = '<',
        DEFAULT_DL_CHAR = '└',
        DEFAULT_D_CHAR = 'v',
        DEFAULT_DR_CHAR = '┘',
        DEFAULT_ZERO_CHAR = ' ';

    public char
        RChar = DEFAULT_R_CHAR,
        URChar = DEFAULT_UR_CHAR,
        UChar = DEFAULT_U_CHAR,
        ULChar = DEFAULT_UL_CHAR,
        LChar = DEFAULT_L_CHAR,
        DLChar = DEFAULT_DL_CHAR,
        DChar = DEFAULT_D_CHAR,
        DRChar = DEFAULT_DR_CHAR,
        ZeroChar = DEFAULT_ZERO_CHAR;

    public float ZeroAmplitude = 1f;

    public VectorFieldToTextMapper() { }
    public VectorFieldToTextMapper(float zeroAmplitude) { ZeroAmplitude = zeroAmplitude; }

    public char Map(Vector2 vector)
    {
        float amplitude = vector.Length();
        if (amplitude < ZeroAmplitude) return ZeroChar;

        Vector2 normalized = Vector2.Normalize(vector);
        float angle = (float)Math.Acos(normalized.X);
        //if(normalized.Y < 0) angle += (float) Math.PI;
        if (normalized.Y < 0) angle = (float)(2 * Math.PI) - angle;

        if (angle < Math.PI / 8) return RChar;
        if (angle < 3 * Math.PI / 8) return URChar;
        if (angle < 5 * Math.PI / 8) return UChar;
        if (angle < 7 * Math.PI / 8) return ULChar;
        if (angle < 9 * Math.PI / 8) return LChar;
        if (angle < 11 * Math.PI / 8) return DLChar;
        if (angle < 13 * Math.PI / 8) return DChar;
        if (angle < 15 * Math.PI / 8) return DRChar;
        return RChar;
    }



    public char[,] Map(Vector2[,] field)
    {
        int width = field.GetLength(0);
        int height = field.GetLength(1);

        char[,] result = new char[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                result[x, y] = Map(field[x, y]);
            }
        }

        return result;
    }

    public string ToString(char[,] charField)
    {
        int width = charField.GetLength(0);
        int height = charField.GetLength(1);

        StringBuilder sb = new();
        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                sb.Append(charField[x, y]);
            }

            sb.Append('\n');
        }

        sb.Remove(sb.Length - 1, 1); // Get rid of the last \n

        return sb.ToString();
    }

    public string MapToString(Vector2[,] field)
    {
        int width = field.GetLength(0);
        int height = field.GetLength(1);

        StringBuilder sb = new();
        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                sb.Append(Map(field[x, y]));
            }

            sb.Append('\n');
        }

        sb.Remove(sb.Length - 1, 1); // Get rid of the last \n

        return sb.ToString();
    }
}
