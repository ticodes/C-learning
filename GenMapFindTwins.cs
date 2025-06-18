using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class FindTwinsMapGenerator
{
    private Random _random = new Random();

    private string ShuffleString(string str)
    {
        char[] characters = str.ToCharArray();
        for (int i = characters.Length - 1; i > 0; i--)
        {
            int j = _random.Next(0, i + 1);
            (characters[i], characters[j]) = (characters[j], characters[i]);
        }
        return new string(characters);
    }

    public char[,] GenerateMap(
        int rows = 9,
        int cols = 9,
         string symbolsSource = null,
    int repetitions = 2)
    {
        if (rows <= 0 || cols <= 0)
            throw new ArgumentException("Row and column dimensions must be positive.");
        if (repetitions < 2)
            throw new ArgumentException("Number of repetitions must be at least 2.");
        if (symbolsSource == null)
        {
            symbolsSource = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        }

        int totalCells = rows * cols;
        if (totalCells % repetitions != 0)
            throw new ArgumentException($"Total cell count ({totalCells}) must be divisible by the number of repetitions ({repetitions}) to fill the board evenly.");

        int uniqueSymbolsNeeded = totalCells / repetitions;

        List<char> uniqueSymbols = new List<char>();
        if (!string.IsNullOrEmpty(symbolsSource))
        {
            uniqueSymbols.AddRange(symbolsSource.Distinct().Take(uniqueSymbolsNeeded));
        }

        if (uniqueSymbols.Count < uniqueSymbolsNeeded)
        {
            string defaultSymbols = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789?!@#$&";
            string shuffledDefaultSymbols = ShuffleString(defaultSymbols);

            int neededFromDefault = uniqueSymbolsNeeded - uniqueSymbols.Count;
            foreach (char c in shuffledDefaultSymbols)
            {
                if (neededFromDefault == 0) break;
                if (!uniqueSymbols.Contains(c))
                {
                    uniqueSymbols.Add(c);
                    neededFromDefault--;
                }
            }
        }

        if (uniqueSymbols.Count < uniqueSymbolsNeeded)
            throw new ArgumentException($"Not enough unique symbols to generate the map. Required: {uniqueSymbolsNeeded}, available: {uniqueSymbols.Count}. Provide a longer symbolsSource string.");
        StringBuilder symbolPool = new StringBuilder(totalCells);
        foreach (char symbol in uniqueSymbols)
        {
            for (int i = 0; i < repetitions; i++)
            {
                symbolPool.Append(symbol);
            }
        }

        string shuffledSymbols = ShuffleString(symbolPool.ToString());

        char[,] map = new char[rows, cols];
        int k = 0;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                map[r, c] = shuffledSymbols[k++];
            }
        }

        return map;
    }

    public void PrintMap(char[,] map)
    {
        int rows = map.GetLength(0);
        int cols = map.GetLength(1);
        Console.WriteLine($"\nGenerated Map ({rows}x{cols}):");
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                Console.Write($"{map[i, j],3} ");
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
        FindTwinsMapGenerator generator = new FindTwinsMapGenerator();

        char[,] map1 = generator.GenerateMap(rows: 8, cols: 9);
        generator.PrintMap(map1);

        char[,] map2 = generator.GenerateMap(rows: 6, cols: 6, symbolsSource: "ABCDEF", repetitions: 3);
        generator.PrintMap(map2);

        char[,] map3 = generator.GenerateMap(rows: 8, cols: 8, repetitions: 4);
        generator.PrintMap(map3);
    }
}
