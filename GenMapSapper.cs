using System;
using System.Collections.Generic;
using System.Linq;

public class SapperMapGenerator
{
    private Random _random = new Random();

    public char[,] GenerateMap(
        int rows = 9,
        int cols = 9,
        int minesCount = 10,
        IEnumerable<(int r, int c)> predefinedMines = default,
        (int r, int c)? firstMove = null)
    {
        // Проверка входных параметров
        if (rows <= 0 || cols <= 0)
            throw new ArgumentException("Размеры поля должны быть положительными.");

        int totalCells = rows * cols;
        if (predefinedMines == null && minesCount >= totalCells)
            throw new ArgumentException("Количество мин не может быть больше или равно общему количеству клеток.");
        if (predefinedMines == null && firstMove.HasValue && minesCount >= totalCells - 1)
            throw new ArgumentException("Количество мин слишком велико для обеспечения безопасного первого хода.");

        int[,] internalMap = new int[rows, cols];
        List<(int r, int c)> mineLocations = new List<(int, int)>();

        // Обработка предопределенных мин
        if (predefinedMines != null)
        {
            foreach (var minePos in predefinedMines)
            {
                if (IsValidCoordinate(minePos.r, minePos.c, rows, cols) &&
                    !mineLocations.Contains(minePos) &&
                    (!firstMove.HasValue || firstMove.Value != minePos))
                {
                    internalMap[minePos.r, minePos.c] = -1;
                    mineLocations.Add(minePos);
                }
                else if (firstMove.HasValue && firstMove.Value == minePos)
                {
                    Console.WriteLine($"Предупреждение: Предопределенная мина в {minePos} совпадает с первым ходом и не будет установлена.");
                }
                else if (!IsValidCoordinate(minePos.r, minePos.c, rows, cols))
                {
                    Console.WriteLine($"Предупреждение: Предопределенная мина в {minePos} вне поля.");
                }
            }
            minesCount = Math.Max(0, minesCount - mineLocations.Count);
        }

        // Генерация случайных мин
        int currentRandomMines = 0;
        while (currentRandomMines < minesCount && mineLocations.Count < totalCells - (firstMove.HasValue ? 1 : 0))
        {
            int r = _random.Next(0, rows);
            int c = _random.Next(0, cols);
            var potentialMine = (r, c);

            if ((!firstMove.HasValue || firstMove.Value != potentialMine) &&
                internalMap[r, c] != -1)
            {
                internalMap[r, c] = -1;
                mineLocations.Add(potentialMine);
                currentRandomMines++;
            }
        }

        if (currentRandomMines < minesCount)
        {
            Console.WriteLine($"Предупреждение: удалось разместить только {currentRandomMines} из {minesCount} случайных мин из-за ограничений (первый ход/размер поля).");
        }

        // Расчет количества мин вокруг клеток
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (internalMap[r, c] == -1)
                    continue;

                int adjacentMines = 0;
                for (int dr = -1; dr <= 1; dr++)
                {
                    for (int dc = -1; dc <= 1; dc++)
                    {
                        if (dr == 0 && dc == 0)
                            continue;

                        int nr = r + dr;
                        int nc = c + dc;

                        if (IsValidCoordinate(nr, nc, rows, cols) && internalMap[nr, nc] == -1)
                        {
                            adjacentMines++;
                        }
                    }
                }
                internalMap[r, c] = adjacentMines;
            }
        }

        // Преобразование в символьную карту
        char[,] resultMap = new char[rows, cols];
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                resultMap[r, c] = internalMap[r, c] == -1
                    ? '*'
                    : internalMap[r, c].ToString()[0];
            }
        }
        return resultMap;
    }

    private bool IsValidCoordinate(int r, int c, int maxRows, int maxCols)
    {
        return r >= 0 && r < maxRows && c >= 0 && c < maxCols;
    }

    public void PrintMap(char[,] map)
    {
        int rows = map.GetLength(0);
        int cols = map.GetLength(1);
        Console.WriteLine("\nКарта сапера:");
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                Console.Write($"{map[i, j]} ");
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    }
}

class Program
{
    static void Main()
    {
        SapperMapGenerator generator = new SapperMapGenerator();

        Console.WriteLine("Пример 1: Карта по умолчанию (9x9, 10 мин)");
        char[,] map1 = generator.GenerateMap();
        generator.PrintMap(map1);

        Console.WriteLine("Пример 2: Карта 16x16, 40 мин");
        char[,] map2 = generator.GenerateMap(16, 16, 40);
        generator.PrintMap(map2);

        Console.WriteLine("Пример 3: Карта с предопределенными минами и первым ходом");
        var predefinedMines = new List<(int, int)> { (0, 0), (1, 1), (2, 2) };
        char[,] map3 = generator.GenerateMap(9, 9, 10, predefinedMines, (3, 3));
        generator.PrintMap(map3);
    }
}
