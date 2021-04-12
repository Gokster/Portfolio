using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace XML {
	class XmlHandler {

		/**********************
		 *			 READER
		 **********************/

		public List<Car> ReadXML(string inputFile) {
			try {
				List<Car> Cars = new List<Car>();

				XmlReaderSettings settings = new XmlReaderSettings();
				settings.IgnoreComments = true;
				settings.IgnoreWhitespace = true;

				using (XmlReader reader = XmlReader.Create(inputFile, settings)) {
					while (reader.ReadToFollowing("car")) {
						Car car = new Car();
						car.Name = reader.GetAttribute("name");

						reader.Read();				// Read to first property
						while (reader.IsStartElement()) {
							AddProperty(reader, car);
						}

						Cars.Add(car);
					}
					return Cars;
				}
			} catch (IOException) {
				Console.WriteLine("Could not open file: " + inputFile);
				return null;
			}
		}

		private void AddProperty(XmlReader reader, Car car) {
			switch (reader.LocalName) {
				case "cylinders":
					car.Cylinders = reader.ReadElementContentAsInt();
					break;
				case "country":
					car.Country = reader.ReadElementContentAsString();
					break;
				default:
					Console.WriteLine("Unknown tag: " + reader.LocalName);
					reader.Skip();				// Skip element
					break;
			}
		}

		/**********************
		 *			 WRITER
		 **********************/

		public void CreateXML(string outputFile, List<Car> cars) {
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.IndentChars = "  ";

			using (XmlWriter writer = XmlWriter.Create(outputFile, settings)) {
				WriteXML(writer, cars);
			}
		}

		private void WriteXML(XmlWriter writer, List<Car> cars) {
			writer.WriteStartDocument();
			writer.WriteStartElement("cars");		// <cars>

			foreach (Car car in cars) {
				writer.WriteStartElement("car");	// <car>

				writer.WriteAttributeString("name", car.Name);

				writer.WriteElementString("cylinders", car.Cylinders.ToString());
				writer.WriteElementString("country", car.Country);

				writer.WriteEndElement();					// </car>
			}

			writer.WriteEndElement();						// </cars>
			writer.WriteEndDocument();
		}
	}
}
