using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MineSweeper_Bot {
  class Program {
    static void Main(string[] args) {
      Console.BackgroundColor = ConsoleColor.Gray;
      Console.ForegroundColor = ConsoleColor.Black;

      string input;

      do {
        Console.Clear();
        Console.WriteLine("Welcome to [MineSweeper console edition]!");
        Console.WriteLine("\nPlease choose a preset: ");
        Console.WriteLine("\t 1. For soloplay type 'SOLO'.");
        Console.WriteLine("\t 2. For bot play type 'BOT'.");
        Console.WriteLine("\t 3. To simulate bot win percentage type 'WINP'.");
        Console.WriteLine("\t 4. To get table of win percentage for bot type 'TABLE'.");
        Console.WriteLine("\t 5. To compare the old bot with the new type 'BOTS'.");
        Console.WriteLine("\t 6. To exit the game type 'QUIT'.");

        input = Console.ReadLine().ToUpper();
        PlayerChoice(input);

      } while (input != "QUIT" && input != "6");
    }

    /*****************************
     *      PLAYER CHOICE
     *****************************/

    private static void PlayerChoice(string input) {
      if (input == "SOLO" || input == "1") {                      // SOLO PLAY
        int[] mapSize = MapSize();
        SoloPlay(mapSize);

        AnyKey();

      } else if (input == "BOT" || input == "2") {                // BOT PLAY
        int[] mapSize = MapSize();
        BotPlay(mapSize, false);

        AnyKey();

      } else if (input == "WINP" || input == "3") {               // WIN PERCENTAGE
        int[] mapSize = MapSize();

        Console.Write("\tGame iterations: ");
        int iterations = Int32.Parse(Console.ReadLine());

        Console.Write("\tCheck lost games? ");
        string check = Console.ReadLine();
        bool checkLost = false;

        if (check.ToUpper() == "YES" || check.ToUpper() == "Y" || check.ToUpper() == "TRUE") {
          checkLost = true;
        }

        BotWinPercent(mapSize, iterations, checkLost, false);

        AnyKey();

      } else if (input == "TABLE" || input == "4") {              // WIN TABLE
        Console.WriteLine("\nPlease choose ");

        Console.Write("\tFirst map dimension (ex. 5 = 5x5 map): ");
        int dim = Int32.Parse(Console.ReadLine());

        Console.Write("\tGame iterations: ");
        int iterations = Int32.Parse(Console.ReadLine());

        Console.Write("\tNumber of map sizes: ");
        int nSizes = Int32.Parse(Console.ReadLine());

        BotWinTable(dim, iterations, nSizes);

        AnyKey();

      } else if (input == "BOTS" || input == "5") {
        Console.WriteLine("\nPlease choose ");

        Console.Write("\tFirst map dimension (ex. 5 = 5x5 map): ");
        int dim = Int32.Parse(Console.ReadLine());

        Console.Write("\tGame iterations: ");
        int iterations = Int32.Parse(Console.ReadLine());

        CompareBots(dim, iterations);

        AnyKey();
      }
    }

    private static int[] MapSize() {
      Console.WriteLine("\nPlease choose ");
      Console.Write("\tMap height: ");
      int height = Int32.Parse(Console.ReadLine());

      Console.Write("\tMap width: ");
      int width = Int32.Parse(Console.ReadLine());

      Console.Write("\tNumber of bombs : ");
      int bombs = Int32.Parse(Console.ReadLine());

      int[] mapSize = { height, width, bombs };
      return mapSize;
    }

    private static void AnyKey() {
      while (Console.KeyAvailable) {
        Console.ReadKey(true);
      }

      Console.WriteLine("\nPress any key to continue...");
      Console.ReadKey();
    }

    /*****************************
     *         SOLO PLAY
     *****************************/

    private static void SoloPlay(int[] mapSize) {
      MineSweeper game = new MineSweeper(mapSize[0], mapSize[1], mapSize[2]);
      game.GenerateMap();

      Console.Clear();
      game.PrintMap();

      while (!game.GameOver) {
        Console.Write("Choose row: ");
        int x = Int32.Parse(Console.ReadLine()) - 1;

        Console.Write("Choose column: ");
        int y = Int32.Parse(Console.ReadLine()) - 1;

        if (game.LegalPos(x, y)) {
          game.SelectField(x, y);
          Console.Clear();
          game.PrintMap();
        }
      }
    }

    /*****************************
     *         BOT PLAY
     *****************************/

    private static MineSweeper BotPlay(int[] mapSize, bool winp) {
      MineSweeper game = new MineSweeper(mapSize[0], mapSize[1], mapSize[2]);
      game.GenerateMap();

      MSBot bot = new MSBot(game, true);

      while (!game.GameOver && !game.Win) {
        bot.MakeMove();

        if (!winp) {
          Console.Clear();
          game.PrintMap();
          Thread.Sleep(200);
        }

        //Console.WriteLine("\n" + bot);
        //Console.WriteLine("\n" + game);
        //Console.ReadKey();
      }

      return game;
    }

    private static void BotPlay(MineSweeper game, int botVersion) {
      MSBot bot = (botVersion == 1) ? new MSBot(game, false) : new MSBot(game, true);

      while (!game.GameOver && !game.Win) {
        bot.MakeMove();
      }
    }

    /*****************************
     *      WIN PERCENTAGE
     *****************************/

    private static double BotWinPercent(int[] mapSize, int iterations, bool checkLostGame, bool tableFormat) {
      Stopwatch sw = new Stopwatch();
      int wins = 0;

      for (int i = 0; i < iterations; i++) {
        sw.Start();

        MineSweeper game = BotPlay(mapSize, true);

        if (game.Win) {
          wins++;
        }

        if (checkLostGame && game.GameOver) {
          Console.Clear();
          game.PrintMap();

          Console.ReadLine();
        }

        if (!tableFormat) {
          sw.Stop();
          Console.WriteLine("[" + sw.Elapsed + "] Game: " + (i + 1) + ", " + "win percent: " + String.Format("{0:0.00}", (double)wins / (i + 1) * 100) + "%.");
        }
      }

      double winPercent = (double)wins / iterations * 100;

      if (!tableFormat) {     
        Console.WriteLine("[" + sw.Elapsed + "] Win percent: " + String.Format("{0:0.00}", winPercent) + "%, total wins: " + wins);
      }

      return winPercent;
    }

    /*****************************
     *         WIN TABLE
     *****************************/

    private static void BotWinTable(int start, int iterations, int nSizes) {                // Handels win table
      int maxSize = (start + nSizes - 1) * (start + nSizes - 1);
      double[,] winTable = new double[nSizes, (maxSize / 3) + 1];

      Console.Clear();

      for (int dim = start; dim < nSizes + start && dim < nSizes + start; dim++) {
        for (int bombs = 1; bombs <= (dim * dim) / 3; bombs++) {
          ColorDone();
          Console.WriteLine("Maps: (" + (dim - start + 1) + " / " + nSizes + "), bombs: (" + bombs + " / " + ((dim * dim) / 3) + ").");

          int[] mapSize = {dim, dim, bombs};

          double winP = BotWinPercent(mapSize, iterations, false, true);
          winTable[dim - start, bombs - 1] = winP;
        }
      }

      Console.Clear();
      string header = "Fields:\t";

      for (int i = start; i < winTable.GetLength(0) + start && i < nSizes + start; i++) {
        header += "\t" + (i * i);
      }

      Console.WriteLine(header);

      PrintWinTable(winTable, header);
    }

    private static void ColorDone() {
      Console.ForegroundColor = ConsoleColor.DarkGreen;
      Console.Write("[DONE] ");
      Console.ForegroundColor = ConsoleColor.Black;
    }

    private static void PrintWinTable(double[,] winTable, string header) {    // Prints and formats win table
      
      foreach (char c in header) {
        if (c == '\t') {
          Console.Write("===");
        } else {
          Console.Write("==");
        }
      }

      Console.WriteLine();

      for (int i = 0; i < winTable.GetLength(1); i++) {
        if (i + 1 < 10) {
          Console.Write("Bombs:  " + String.Format("{0:00.#}", (i + 1) + "  |"));
        } else {
          Console.Write("Bombs: " + String.Format("{0:00.#}", (i + 1) + "  |"));
        }

        for (int j = 0; j < winTable.GetLength(0); j++) {
          double winP = winTable[j, i];
          if(winP <= 0) {
            Console.Write("\t  -  %");
          } else if (winP < 10) {
            Console.Write("\t  " + String.Format("{0:0.0}", winP) + "%");
          } else if (winP < 100) {
            Console.Write("\t " + String.Format("{0:0.0}", winP) + "%");
          } else {
            Console.Write("\t" + String.Format("{0:0.0}", winP) + "%");
          }
        }

        Console.WriteLine();
      }
    }

    /*****************************
     *     COMPARISON TABLE
     *****************************/

    private static void CompareBots(int map, int iterations) {
      int maxSize = (map) * (map);
      double[,] winTable = new double[2, (maxSize / 3) + 1];

      Console.Clear();

      for (int bombs = 1; bombs <= maxSize / 3; bombs++) {
        ColorDone();
        Console.WriteLine("Bombs: (" + bombs + " / " + ((map * map) / 3) + ").");

        int[] mapSize = {map, map, bombs};

        double[] winsBots = BotsPercent(mapSize, iterations);
        winTable[0, bombs - 1] = winsBots[0];
        winTable[1, bombs - 1] = winsBots[1];
      }

      Console.Clear();
      string header = "Bot:\t\tBot_old\tBot_new";

      Console.WriteLine(header);

      PrintWinTable(winTable, header);
    }

    private static double[] BotsPercent(int[] mapSize, int iterations) {
      int winsBot1 = 0;
      int winsBot2 = 0;

      for (int i = 0; i < iterations; i++) {
        MineSweeper game = new MineSweeper(mapSize[0], mapSize[1], mapSize[2]);
        game.GenerateMap();

        BotPlay(game, 1);
        if (game.Win) {
          winsBot1++;
        }

        game.Reset();

        BotPlay(game, 2);
        if (game.Win) {
          winsBot2++;
        }
      }

      double winPBot1 = (double) winsBot1 / iterations * 100;
      double winPBot2 = (double) winsBot2 / iterations * 100;
      double[] winTable = { winPBot1, winPBot2 };

      return winTable;
    }
  }
}
