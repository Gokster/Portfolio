using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace MineSweeper_Bot {
  class Bot {
    private MineSweeper game;
    private double[,] estTable;
    private bool[,] pickTable;

    internal Bot(MineSweeper game) {
      this.game = game;
    }

    /*****************************
     *           Move
     *****************************/

    internal void MakeMove() {
      int height = game.Work.GetLength(0);
      int width = game.Work.GetLength(1);
      pickTable = new bool[height, width];

      CalcMove(height, width);      // Set flags
      CalcMove(height, width);      // Estimate chance

      double lowVal = 2.0;
      int posX = 0; 
      int posY = 0;

      for (int x = 0; x < estTable.GetLength(0); x++) {
        for (int y = 0; y < estTable.GetLength(1); y++) {

          if (pickTable[x, y]) {
            if (estTable[x, y] < lowVal) {
              lowVal = estTable[x, y];
              posX = x;
              posY = y;
            }
          } 
          
        }
      }

      game.SelectField(posX, posY);
    }
    
    private void CalcMove(int height, int width) {
      estTable = new double[height, width];

      for (int x = 0; x < estTable.GetLength(0); x++) {
        for (int y = 0; y < estTable.GetLength(1); y++) {

          if (0 < game.Work[x, y]) {
            Estimate(x, y);
          }

        }
      }

      for (int x = 0; x < estTable.GetLength(0); x++) {
        for (int y = 0; y < estTable.GetLength(1); y++) {

          if (0.0 < estTable[x, y]) {
            Normalize(x, y);
          }

        }
      }
    }

    private static int[] deltaX = { +1, +1,  0, -1, -1, -1,  0, +1 };
    private static int[] deltaY = {  0, +1, +1, +1,  0, -1, -1, -1 };

    private void Estimate(int x, int y) {
      int unb = 0; 
      int flag = 0;

      for (int i = 0; i < deltaX.Length; i++) {
        int newX = x + deltaX[i];
        int newY = y + deltaY[i];

        if (game.LegalPos(newX, newY) && game.Work[newX, newY] == 0) {
          //estTable[newX, newY] = estTable[newX, newY] + game.Work[x, y];
          unb++;
        } else if (game.LegalPos(newX, newY) && game.Work[newX, newY] == -3) {
          flag++;
        }
      }

      double percent = 0.0;
      if(unb != 0) {
        percent = (double)(game.Work[x, y] - flag) / unb;
      }

      for (int i = 0; i < deltaX.Length; i++) {
        int newX = x + deltaX[i];
        int newY = y + deltaY[i];

        if (game.LegalPos(newX, newY) && game.Work[newX, newY] == 0) {
          if (percent >= 1.0) {
            game.PlaceFlag(newX, newY);
            pickTable[newX, newY] = false;
          } else {
            estTable[newX, newY] += percent;
            pickTable[newX, newY] = true;
          }
        }
      }
    }

    private void Normalize(int x, int y) {
      int nb = 0;

      for (int i = 0; i < deltaX.Length; i++) {
        int newX = x + deltaX[i];
        int newY = y + deltaY[i];

        if (game.LegalPos(newX, newY) && game.Work[newX, newY] > 0) {
          nb++;
        }
      }

      if (nb != 0) {
        estTable[x, y] /= nb;
      }
    }

    /*****************************
     *          PRINT
     *****************************/

    public override string ToString() {
      StringBuilder sb = new StringBuilder();

      for (int x = 0; x < estTable.GetLength(0); x++) {
        for (int y = 0; y < estTable.GetLength(1); y++) {
          string s = (0 < estTable[x, y]) ? String.Format("{0:0.0}", estTable[x, y]) + " " : " \u25A0  ";   // Estimate bomb chance
          
          sb.Append(s);
        }
        sb.AppendLine();
      }

      return sb.ToString();
    }

  }
}
