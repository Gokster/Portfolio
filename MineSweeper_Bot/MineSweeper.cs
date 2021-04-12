using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MineSweeper_Bot {
  class MineSweeper {
    internal int[,] Work { get; private set; }
    internal int Flags { get; private set; } = 0;
    internal int TotBombs { get; private set; }

    private int[,] bombMap;
    private int[] selectedPos;
    private int hiddenFields;
    private int height, width;
    
    internal MineSweeper() : this(8, 8, 12) { }

    internal MineSweeper(int height, int width, int bombs) {
      this.height = height;
      this.width = width;
      this.bombMap = new int[height, width];
      this.Work = new int[height, width];
      this.TotBombs = bombs;
      this.hiddenFields = height * width;
    }

    /*****************************
     * GENERATE RANDOMIZED WORLD
     *****************************/

    private static Random rnd = new Random();

    internal void GenerateMap() {
      int bombsPlaced = 0;

      while (bombsPlaced < TotBombs) {
        int x = rnd.Next(bombMap.GetLength(0));
        int y = rnd.Next(bombMap.GetLength(1));

        if (bombMap[x, y] != 1) {
          bombMap[x, y] = 1;
          bombsPlaced++;
        }
      }
    }

    internal void Reset() {
      Work = new int[height, width];
      GameOver = false;
      Win = false;
      Flags = 0;
      hiddenFields = height * width;
    }

    /*****************************
     *        PLAYER MOVES
     *****************************/

    private static int[] deltaX = { +1, +1, 0, -1, -1, -1, 0, +1 };
    private static int[] deltaY = { 0, +1, +1, +1, 0, -1, -1, -1 };

    internal void SelectField(int x, int y) {
      int[] pos = {x, y};
      selectedPos = pos;

      if (bombMap[x, y] != 1 || hiddenFields == height * width) {
        if (bombMap[x, y] == 1) {
          PlaceNewBomb(x, y);
        }

        Reveal(x, y);

      } else {
        //for (int bombX = 0; bombX < bombMap.GetLength(0); bombX++) {
        //  for (int bombY = 0; bombY < bombMap.GetLength(1); bombY++) {
        //    if (bombMap[bombX, bombY] == 1) {
        //      Work[bombX, bombY] = -2;            // Bomb = -2
        //    }
        //  }
        //}
        Work[x, y] = -2;
        GameOver = true;
      }
    }

    private void PlaceNewBomb(int x, int y) {
      bool placed = false;
      bombMap[x, y] = 0;

      while (!placed) {
        int bombX = rnd.Next(bombMap.GetLength(0));
        int bombY = rnd.Next(bombMap.GetLength(1));

        if (bombMap[bombX, bombY] != 1) {
          bombMap[bombX, bombY] = 1;
          placed = true;
        }
      }
    }

    internal void PlaceFlag(int x, int y) {
      Work[x, y] = -3;          // Flag = -3
      Flags++;
    }

    private void Reveal(int x, int y) {
      if (Work[x, y] == 0) {
        int nb = Neighbours(x, y);

        Work[x, y] = (0 < nb) ? nb : -1;          // Reveal field
      }

      hiddenFields--;
      if (TotBombs - Flags == hiddenFields) {
        Win = true;
      }

      for (int i = 0; i < deltaX.Length; i++) {
        int newX = x + deltaX[i];
        int newY = y + deltaY[i];

        if (LegalPos(newX, newY)) {
          bool hidden = Work[newX, newY] == 0;    // Neighbour is hidden
          bool noNb = Work[x, y] == -1;           // Revealed field have no bomb neighbours

          if (hidden && noNb) {
            Reveal(newX, newY);                   // Reveal neighbour
          }
        }
      }
    }

    /*****************************
     *        BOMB CHECK
     *****************************/

    internal bool GameOver { get; private set; } = false;
    internal bool Win { get; private set; } = false;

    internal int Neighbours(int x, int y) {       // Calculates nearby bombs
      int neighbours = 0;

      for (int i = 0; i < deltaX.Length; i++) {
        int newX = x + deltaX[i];
        int newY = y + deltaY[i];

        if (1 <= this[newX, newY]) {
          neighbours++;
        }
      }

      return neighbours;
    }

    private bool LegalX(int x) {
      return (0 <= x && x < bombMap.GetLength(0));
    }

    private bool LegalY(int y) {
      return (0 <= y && y < bombMap.GetLength(1));
    }

    internal bool LegalPos(int x, int y) {
      return (LegalX(x) && LegalY(y));
    }

    private int this[int x, int y] {
      get {
        return (LegalPos(x, y) ? bombMap[x, y] : 0);
      }
    }

    /*****************************
     *          PRINT
     *****************************/

    public override string ToString() {
      StringBuilder sb = new StringBuilder();

      for (int x = 0; x < Work.GetLength(0); x++) {
        for (int y = 0; y < Work.GetLength(1); y++) {
          string s = (0 < bombMap[x, y]) ? "\u25A0 " : "  ";   // Bomb locations

          sb.Append(s);
        }
        sb.AppendLine();
      }

      return sb.ToString();
    }

    private static readonly ConsoleColor[] colors = {
      ConsoleColor.Black,         // Default
      ConsoleColor.Blue,
      ConsoleColor.Yellow,
      ConsoleColor.Green,
      ConsoleColor.DarkBlue,
      ConsoleColor.Cyan,
      ConsoleColor.DarkYellow,
      ConsoleColor.DarkCyan,
      ConsoleColor.Magenta,
      ConsoleColor.Red,           // Bomb
      ConsoleColor.DarkMagenta    // Flag
    }; 

    internal void PrintMap() {
      for (int x = 0; x < Work.GetLength(0); x++) {
        for (int y = 0; y < Work.GetLength(1); y++) {
          string s = "";
          int value = Work[x, y];

          if (selectedPos != null && x == selectedPos[0] && y == selectedPos[1]) {
            Console.BackgroundColor = ConsoleColor.DarkGray;
          }

          if (0 < value) {              // Fields with bomb neighbours
            s = value + "";
          } else if (value == 0) {      // Hidden fields
            s = "\u25A0";
          } else if (value == -1) {     // Fields with no neighbours
            s = " ";
            value = 0;
          } else if (value == -2){      // Bombs
            s = "B";
            value = colors.Length - 2;
          } else {                      // Flags
            s = "F";
            value = colors.Length - 1;
          }

          Console.ForegroundColor = colors[value];
          Console.Write(s);

          Console.BackgroundColor = ConsoleColor.Gray;
          Console.Write(" ");
        }
        Console.WriteLine();
      }
      Console.ForegroundColor = colors[0];          // Default color
    }
  }
}
