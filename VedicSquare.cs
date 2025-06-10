using System;

class VedicSquare
{
    // Генерация таблицы Пифагора (9x9)
    public static int[,] GetMultiplicationTable()
    {
        int[,] table = new int[9, 9];
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                table[i, j] = (i + 1) * (j + 1);
            }
        }
        return table;
    }

    // Генерация ведического квадрата
    public static int[,] GetVedicSquare()
    {
        int[,] table = GetMultiplicationTable();
        int[,] vedic = new int[9, 9];

        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                int num = table[i, j];
                vedic[i, j] = DigitalRoot(num);
            }
        }
        return vedic;
    }

    // Вычисление цифрового корня числа
    private static int DigitalRoot(int number)
    {
        while (number > 9)
        {
            int sum = 0;
            while (number > 0)
            {
                sum += number % 10;
                number /= 10;
            }
            number = sum;
        }
        return number;
    }

    // Генерация узора для указанного числа
    public static int[,] GetPattern(int patternNumber)
    {
        int[,] vedic = GetVedicSquare();
        int[,] pattern = new int[9, 9];

        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                pattern[i, j] = vedic[i, j] == patternNumber ? 1 : 0;
            }
        }
        return pattern;
    }

    // Красивый вывод матрицы в консоль
    public static void PrintMatrix(int[,] matrix, bool useSymbols = false)
    {
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                if (useSymbols)
                {
                    Console.Write(matrix[i, j] == 1 ? "■" : "□");
                }
                else
                {
                    Console.Write($"{matrix[i, j],3}");
                }
            }
            Console.WriteLine();
        }
    }

    public static void Main()
    {
        Console.WriteLine("Таблица Пифагора:");
        PrintMatrix(GetMultiplicationTable());

        Console.WriteLine("\nВедический квадрат:");
        PrintMatrix(GetVedicSquare());

        Console.WriteLine("\nУзор для числа 1:");
        PrintMatrix(GetPattern(1), true);
    }
}
