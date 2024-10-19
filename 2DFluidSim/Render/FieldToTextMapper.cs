using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2DFluidSim.Render;
internal class FieldToTextMapper
{
    public const char
        DEFAULT_LOWEST_CHAR = ' ',
        DEFAULT_LOWER_CHAR = '.',
        DEFAULT_LOW_CHAR = '░',
        DEFAULT_MEDIUM_CHAR = '▒',
        DEFAULT_HIGH_CHAR = '▓',
        DEFAULT_HIGHER_CHAR = '█',
        DEFAULT_HIGHEST_CHAR = 'X';

    public const float
        DEFAULT_LOWER_LIMIT = 0.0f,
        DEFAULT_LOW_LIMIT = 0.3f,
        DEFAULT_MEDIUM_LIMIT = 0.6f,
        DEFAULT_HIGH_LIMIT = 0.9f,
        DEFAULT_HIGHER_LIMIT = 1.5f,
        DEFAULT_HIGHEST_LIMIT = 3.0f;

    public char
        LowestChar = DEFAULT_LOWEST_CHAR,
        LowerChar = DEFAULT_LOWER_CHAR,
        LowChar = DEFAULT_LOW_CHAR,
        MediumChar = DEFAULT_MEDIUM_CHAR,
        HighChar = DEFAULT_HIGH_CHAR,
        HigherChar = DEFAULT_HIGHER_CHAR,
        HighestChar = DEFAULT_HIGHEST_CHAR;

    public float
        LowerLimit = DEFAULT_LOWER_LIMIT,
        LowLimit = DEFAULT_LOW_LIMIT,
        MediumLimit = DEFAULT_MEDIUM_LIMIT,
        HighLimit = DEFAULT_HIGH_LIMIT,
        HigherLimit = DEFAULT_HIGHER_LIMIT,
        HighestLimit = DEFAULT_HIGHEST_LIMIT;

    public char Map(float value)
    {
        if (value > HighestLimit) return HighestChar;
        if (value > HigherLimit) return HigherChar;
        if (value > HighLimit) return HighChar;
        if (value > MediumLimit) return MediumChar;
        if (value > LowLimit) return LowChar;
        if (value > LowerLimit) return LowerChar;
        return LowestChar;
    }

    public char[,] Map(float[,] field)
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

    public string MapToString(float[,] field)
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
