using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
* While(!game.GameOver)
*  MakeMove()
*    FindValidFields()
*    int[2] xy = CalculateBombChance()
*    game.SelectField(xy[0], xy[1])
*  [end]
* [end]
*/

/*
 * CalculateBombChance() -> return [x, y]
 *  FindBomb                        // 100% chance for bomb
 *  FindGuarantees -> return [x,y]  //   0% chance for bomb
 *  DefaultChance
 *  Normalize
 *  PickField -> return [x, y]
 * [end]
 */

namespace MineSweeper_Bot {
  class MSBot {
    private MineSweeper game;
    private bool[,] validFields;
    private double[,] estTable;

    private int height;
    private int width;
    private bool checkPattern;

    public MSBot(MineSweeper game, bool checkPattern) {
      this.game = game;
      this.height = game.Work.GetLength(0);
      this.width = game.Work.GetLength(1);
      this.checkPattern = checkPattern;
    }

    /*****************************
     *          MOVE
     *****************************/

    internal void MakeMove() {
      estTable = new double[height, width];
      FindValidFields();

      int[] fieldPos = CalculateBombChance();
      game.SelectField(fieldPos[0], fieldPos[1]);
    }

    private void FindValidFields() {
      validFields = new bool[height, width];

      for (int x = 0; x < height; x++) {
        for (int y = 0; y < width; y++) {
          validFields[x, y] = game.Work[x, y] == 0;
        }
      }
    }

    private int[] CalculateBombChance() {
      int[] fieldPos = FindBomb();               // 100% chance for bomb

      if (fieldPos != null) {
        return fieldPos;
      }

      fieldPos = FindGuarantees();              //   0% chance for bomb

      if (fieldPos != null) {
        return fieldPos;
      }

      EstimateBombChance();                     //   0% < x < 100% chance for bomb, on edge
      EstimateDefaultChance();                  //   0% < x < 100% chance for bomb, outside edge 
      Normalize();
      
      return PickField();
    }

    /*****************************
     *           FLAG
     *****************************/

    private int[] FindBomb() {
      for (int x = 0; x < height; x++) {
        for (int y = 0; y < width; y++) {

          int[] nbflag = FindNBandFlag(x, y);     // [0] = hidden nb, [1] = flag

          if (game.Work[x, y] - nbflag[1] == nbflag[0]) {
            int[] nbPos = FindPoFlag(x, y);

            if (nbPos != null) {
              return nbPos;
            }
          } else if (checkPattern && nbflag[0] + nbflag[1] - game.Work[x, y] == 1) {
            int[] nbPos = FindFlagPattern(x, y, nbflag);

            if (nbPos != null) {
              return nbPos;
            }
          }
        }
      }

      return null;
    }

    private int[] FindPoFlag(int x, int y) {
      for (int i = 0; i < deltaX.Length; i++) {

        int nbX = deltaX[i] + x;
        int nbY = deltaY[i] + y;

        if (game.LegalPos(nbX, nbY) && game.Work[nbX, nbY] == 0) {
          game.PlaceFlag(nbX, nbY);
          int[] nbPos = { x, y };
          return nbPos;
        }
      }

      return null;
    }

    private int[] FindFlagPattern(int x, int y, int[] nbflag) {
      int[] field = { x, y };

      for (int i = 0; i < deltaX.Length; i++) {

        int nbX = deltaX[i] + x;
        int nbY = deltaY[i] + y;
        int[] nb = { nbX, nbY };
        
        if (game.LegalPos(nbX, nbY) && game.Work[nbX, nbY] == 1) {
          int sharedNB = HasSharedNeighbours(field, nb);

          if (sharedNB == 2 && sharedNB != nbflag[0]) {
            int[] flagField = NotSharedNeighbour(field, nb);

            if (flagField != null) {
              game.PlaceFlag(flagField[0], flagField[1]);
              return field;
            }
          }
        }
      }

      return null;
    }

    private int[] NotSharedNeighbour(int[] field1, int[] field2) {
      for (int i = 0; i < deltaX.Length; i++) {

        int nbX = deltaX[i] + field1[0];
        int nbY = deltaY[i] + field1[1];
        int[] nbPos = { nbX, nbY };

        if (game.LegalPos(nbX, nbY) && game.Work[nbX, nbY] == 0 && !IsNeighbours(nbPos, field2)) {
          return nbPos;
        }
      }

      return null;
    }

    private int HasSharedNeighbours(int[] field1, int[] field2) {
      int sharedNB = 0;

      for (int i = 0; i < deltaX.Length; i++) {

        int nbX = deltaX[i] + field1[0];
        int nbY = deltaY[i] + field1[1];
        int[] nbPos = { nbX, nbY };

        if (game.LegalPos(nbX, nbY) && game.Work[nbX, nbY] == 0 && IsNeighbours(nbPos, field2)) {
          sharedNB++;
        }
      }

      return sharedNB;
    }

    private bool IsNeighbours(int[] field1, int[] field2) {
      for (int i = 0; i < deltaX.Length; i++) {

        int nbX = deltaX[i] + field1[0];
        int nbY = deltaY[i] + field1[1];

        if (game.LegalPos(nbX, nbY) && nbX == field2[0] && nbY == field2[1]) {
          return true;
        }
      }

      return false;
    }

    /*****************************
     *         GUARANTEE
     *****************************/

    private int[] FindGuarantees() {
      for (int x = 0; x < height; x++) {
        for (int y = 0; y < width; y++) {

          int[] nbflag = FindNBandFlag(x, y);     // [0] = nb, [1] = flag

          if (game.Work[x, y] > 0 && (double)(game.Work[x, y] - nbflag[1]) / nbflag[0] <= 0.0) {
            for (int i = 0; i < deltaX.Length; i++) {
              int nbX = deltaX[i] + x;
              int nbY = deltaY[i] + y;

              if (game.LegalPos(nbX, nbY) && game.Work[nbX, nbY] == 0) {
                int[] nbPos = { nbX, nbY };
                return nbPos;
              }
            }
          } else if (checkPattern && game.Work[x, y] == 1) {
            int[] nbPos = FindEmptyPattern(x, y, nbflag);

            if (nbPos != null) {
              return nbPos;
            }
          }
        }
      }

      return null;
    }

    private int[] FindEmptyPattern(int x, int y, int[] nbflag) {
      int[] field = { x, y };

      for (int i = 0; i < deltaX.Length; i++) {

        int nbX = deltaX[i] + x;
        int nbY = deltaY[i] + y;
        int[] nb = { nbX, nbY };

        if (game.LegalPos(nbX, nbY) && game.Work[nbX, nbY] > 1) {
          int[] nbflagOffset = FindNBandFlag(nbX, nbY);

          if (nbflagOffset[0] + nbflagOffset[1] - game.Work[x, y] == 1) {
            int sharedNB = HasSharedNeighbours(field, nb);

            if (sharedNB == 2 && sharedNB != nbflag[0]) {
              int[] emptyField = NotSharedNeighbour(field, nb);

              if (emptyField != null) {
                game.PlaceFlag(emptyField[0], emptyField[1]);
                return field;
              }
            }
          }
        }
      }

      return null;
    }

      /*****************************
       *          CHANCE
       *****************************/

      private void EstimateBombChance() {
      for (int x = 0; x < height; x++) {
        for (int y = 0; y < width; y++) {

          if (game.Work[x, y] > 0) {
            int[] nbflag = FindNBandFlag(x, y);     // [0] = nb, [1] = flag

            for (int i = 0; i < deltaX.Length; i++) {
              int nbX = deltaX[i] + x;
              int nbY = deltaY[i] + y;

              if (game.LegalPos(nbX, nbY) && game.Work[nbX, nbY] == 0) {
                estTable[nbX, nbY] += (double)(game.Work[x, y] - nbflag[1]) / nbflag[0];
                validFields[nbX, nbY] = false;
              }
            }
          }
        }
      }
    }

    private void EstimateDefaultChance() {
      double defaultChance = DefaultChance();

      for (int x = 0; x < height; x++) {
        for (int y = 0; y < width; y++) {

          if (validFields[x, y]) {
            estTable[x, y] = defaultChance;

            int[] nbflag = FindNBandFlag(x, y);
            if (nbflag[0] == 3) {
              estTable[x, y] /= 1.01;
            }
          }
        }
      }
    }

    private double DefaultChance() {
      int defaultFields = 0;

      for (int x = 0; x < height; x++) {
        for (int y = 0; y < width; y++) {
          if (game.Work[x, y] == 0) {
            defaultFields++;
          }
        }
      }

      return (double)(game.TotBombs - game.Flags) / defaultFields;
    }

    private static int[] deltaX = { +1, +1, 0, -1, -1, -1, 0, +1 };
    private static int[] deltaY = { 0, +1, +1, +1, 0, -1, -1, -1 };

    private int[] FindNBandFlag(int x, int y) {
      int[] nbAndFlag = new int[2];

      for (int i = 0; i < deltaX.Length; i++) {

        int nbX = deltaX[i] + x;
        int nbY = deltaY[i] + y;

        if (game.LegalPos(nbX, nbY) && game.Work[nbX, nbY] == 0) {
          nbAndFlag[0]++;
        } else if (game.LegalPos(nbX, nbY) && game.Work[nbX, nbY] == -3) {
          nbAndFlag[1]++;
        }
      }

      return nbAndFlag;
    }

    /*****************************
     *          CHOOSE
     *****************************/

    private void Normalize() {
      for (int x = 0; x < height; x++) {
        for (int y = 0; y < width; y++) {
          int nb = 0;

          if (estTable[x, y] > 0) {
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
        }
      }
    }

    private int[] PickField() {
      double lowestValue = 2.0;
      int[] fieldPos = new int[2];

      for (int x = 0; x < height; x++) {
        for (int y = 0; y < width; y++) {
          if (estTable[x, y] > 0.0 && estTable[x, y] < lowestValue) {
            lowestValue = estTable[x, y];
            fieldPos[0] = x;
            fieldPos[1] = y;
          }
        }
      }

      return fieldPos;
    }



    /*****************************
     *          PRINT
     *****************************/

    public override string ToString() {
      StringBuilder sb = new StringBuilder();

      for (int x = 0; x < height; x++) {
        for (int y = 0; y < width; y++) {
          string s = (0 < estTable[x, y]) ? String.Format("{0:0.0}", estTable[x, y]) + " " : " \u25A0  ";   // Estimate bomb chance

          sb.Append(s);
        }
        sb.AppendLine();
      }

      return sb.ToString();
    }
  }
}
