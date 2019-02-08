using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Visual_Music
{
	abstract public class Cloneable<T>
	{
		public T clone()
		{
			DataContractSerializer dcs = new DataContractSerializer(typeof(T), Form1.projectSerializationTypes);
			MemoryStream stream = new MemoryStream();
			dcs.WriteObject(stream, this);
			stream.Flush();
			stream.Position = 0;
			return (T)dcs.ReadObject(stream);
		}
	}
}
