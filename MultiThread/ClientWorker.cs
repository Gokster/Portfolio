using System;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace MultiThread {
  class ClientWorker {
    private ClientManager manager;
    private Socket connection;
    private bool shutdown;

    private bool firstOdd = true;       // SLET
    private bool firstEven = true;      // SLET

    public ClientWorker(ClientManager manager, Socket connection) {
      this.manager = manager;
      this.connection = connection;

      shutdown = false;
    }

    /****************************
     *         RUNNER
     ****************************/

    public void Run() {
      try {
        PrintMessage("New connection");

        NetworkStream stream = new NetworkStream(connection);
        StreamWriter writer = new StreamWriter(stream);
        StreamReader reader = new StreamReader(stream);

				while (!shutdown) {
          string line = reader.ReadLine();
          PrintMessage("received: " + line);

          if (line != "<EXIT>") {
            SendLine(writer, Handler(line));
          } else {
            Shutdown();
          }
        }

        stream.Close();
        writer.Close();
        reader.Close();
        manager.WorkerTerminated(this);

        PrintMessage("Connection closed");
      } catch {
        PrintMessage("Connection failed");
      }
    }

    /****************************
     *       TERMINATION
     ****************************/

    public void Shutdown() {
      shutdown = true;
		}

    /****************************
     *         HANDLER
     ****************************/

    //********* ÆNDRE DENNE METODE! **********//
    private string Handler(string value) {
      string parity = "";
      
      if (int.Parse(value) % 2 == 0) {
        if (!firstEven) {
          parity = "Igen ";
        }

        parity += "lige";
        firstEven = false;
      } else {
        if (!firstOdd) {
          parity = "Igen ";
        }

        parity += "ulige";
        firstOdd = false;
      }

      return parity;
    }
    //****************************************//

    /****************************
     *         SERVICE
     ****************************/

    private void SendLine(StreamWriter writer, string line) {
      writer.WriteLine(line);
      writer.Flush();

			Thread.Sleep(100);
		}

    private void PrintMessage(string line) {
      Console.ForegroundColor = ConsoleColor.DarkGreen;
      Console.Write("[Worker] ");

      Console.ForegroundColor = ConsoleColor.Black;
      Console.WriteLine(line);
    }
  }
}
