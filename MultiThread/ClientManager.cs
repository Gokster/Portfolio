using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace MultiThread {
  class ClientManager {
    private List<ClientWorker> workers;
    private bool shutdown;
    private int port;

    public ClientManager(int port) {
      this.port = port;

      shutdown = false;
      workers = new List<ClientWorker>();
    }

    internal void Start() {
      new Thread(this.Run).Start();
    }

    /****************************
     *         RUNNER
     ****************************/

    public void Run() {
      try {
        TcpListener server = new TcpListener(IPAddress.Any, port);
        server.Start();

        PrintMessage("Online");

        while (!shutdown) {
          if (server.Pending()) {
            Socket connection = server.AcceptSocket();

            ClientWorker worker = new ClientWorker(this, connection);
            workers.Add(worker);
            new Thread(worker.Run).Start();

          } else {
            Thread.Sleep(100);
          }
        }

				foreach (ClientWorker worker in workers) {
          worker.Shutdown();
				}

        PrintMessage("Offline");
      } catch {
        PrintMessage("Failed to start up");
      }
    }

    /****************************
     *       TERMINATION
     ****************************/

    public void WorkerTerminated(ClientWorker worker) {
      workers.Remove(worker);
    }

    public void Shutdown() {
      shutdown = true;
    }

    /****************************
     *         SERVICE
     ****************************/

    private void PrintMessage(string line) {
      Console.ForegroundColor = ConsoleColor.DarkBlue;
      Console.Write("[ClientManager] ");

      Console.ForegroundColor = ConsoleColor.Black;
      Console.WriteLine(line);
    }
  }
}
