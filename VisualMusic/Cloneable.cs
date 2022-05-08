using System.IO;
using System.Runtime.Serialization;

namespace VisualMusic
{
    static public class Cloning
    {
        static public T clone<T>(this T obj)
        {
            DataContractSerializer dcs = new DataContractSerializer(typeof(T), Form1.projectSerializationTypes);
            MemoryStream stream = new MemoryStream();
            dcs.WriteObject(stream, obj);
            stream.Flush();
            stream.Position = 0;
            return (T)dcs.ReadObject(stream);
        }
    }
}
