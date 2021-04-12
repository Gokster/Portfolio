using System;
using System.Collections.Generic;

namespace XML {
	class Program {
		static void Main(string[] args) {
			String inputFile = "http://www.fkj.dk/cars.xml";
			String outputFile = "../../../new_Cars.xml";

			XmlHandler xmlHandler = new XmlHandler();

			List<Car> cars = xmlHandler.ReadXML(inputFile);
			foreach (Car car in cars) {
				Console.WriteLine(car);
			}

			xmlHandler.CreateXML(outputFile, cars);
		}
	}
}
