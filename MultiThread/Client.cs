using System;
using System.Net.Sockets;
using System.IO;
using System.Net;
using System.Threading;

namespace MultiThread {
  class Client {
    private int port;
    private string[] numberList;                      // ÆNDRE HER!

    public Client(int port, string[] numberList) {    // ÆNDRE HER!
      this.port = port;
      this.numberList = numberList;
    }

    /****************************
     *         RUNNER
     ****************************/

    public void Run() {
      try {
        PrintMessage("Requesting connection");

        Socket connection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        connection.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port));

        NetworkStream stream = new NetworkStream(connection);
        StreamWriter writer = new StreamWriter(stream);
        StreamReader reader = new StreamReader(stream);

        //***** TILPAS DETTE! *****//
        foreach (string number in numberList) {
          SendLine(writer, number);

          String line = reader.ReadLine();
          PrintMessage("received: " + line);
        }
        //*************************//

        SendLine(writer, "<EXIT>");     // Stop communication

        stream.Close();
        writer.Close();
        reader.Close();
        PrintMessage("Connection closed");
      } catch {
        PrintMessage("Connection failed");
      }
    }

    /****************************
     *         SERVICE
     ****************************/

    private void SendLine(StreamWriter writer, string line) {
      writer.WriteLine(line);
      writer.Flush();

			Thread.Sleep(100);
		}

    private void PrintMessage(string line) {
      Console.ForegroundColor = ConsoleColor.DarkYellow;
      Console.Write("[Client] ");

      Console.ForegroundColor = ConsoleColor.Black;
      Console.WriteLine(line);
    }
  }
}
