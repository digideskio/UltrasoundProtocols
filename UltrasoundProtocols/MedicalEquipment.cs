﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UltrasoundProtocols
{
	public class MedicalEquipment
	{
		public int Id { get; set; }
		public string Name { get; set; }

		public MedicalEquipment(int id, string name)
		{
			this.Id = id;
			this.Name = name;
		}
	}
}
