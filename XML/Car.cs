using System;
using System.Collections.Generic;
using System.Text;

namespace XML {
	class Car {
		public string Name { get; set; }
		public int Cylinders { get; set; }
		public string Country { get; set; }

		public override String ToString() {
			return $"{Name} ({Cylinders} cylinders) made in {Country}.";
		}
	}
}
