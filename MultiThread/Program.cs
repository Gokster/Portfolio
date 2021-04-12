using System;
using System.Threading;

namespace MultiThread {
	class Program {
		static void Main(string[] args) {
			int port = 6010;
			string[] numberList = { "64", "2", "1", "50", "45", "67", "101", "44" };      // ÆNDRE DETTE!

			ClientManager manager = new ClientManager(port);
			manager.Start();

			Thread.Sleep(100);			// Ensure server online

			new Thread(new Client(port, numberList).Run).Start();

			Thread.Sleep(2000);     // Ensure communication is complete

			manager.Shutdown();
		}
	}
}
