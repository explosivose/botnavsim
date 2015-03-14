using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

public class ObjectSerializer {
	
	/// <summary>
	/// Serializes the object.
	/// </summary>
	/// <param name="serializableObject">Serializable object.</param>
	/// <param name="fileName">File name.</param>
	/// <typeparam name="T"></typeparam>
	public static void SerializeObject<T>(T serializableObject, string fileName) {
		if (serializableObject == null) {
			Debug.LogError("Serialize: cannot serialize a null object!");
			return;
		}
		
		try {
			XmlDocument xmlDocument = new XmlDocument();
			XmlSerializer serializer = new XmlSerializer(serializableObject.GetType());
			using (MemoryStream stream = new MemoryStream()) {
				serializer.Serialize(stream, serializableObject);
				stream.Position = 0;
				xmlDocument.Load(stream);
				xmlDocument.Save(fileName);
				stream.Close();
			}
		}
		catch(System.Exception e) {
			Debug.LogException(e);
		}
	}
	
	/// <summary>
	/// Deserializes object.
	/// </summary>
	/// <returns>The deserialized object.</returns>
	/// <param name="fileName">File name.</param>
	/// <typeparam name="T"></typeparam>
	public static T DeSerializeObject<T>(string fileName) {
		if (string.IsNullOrEmpty(fileName)) {
			Debug.LogError("DeSerialize: null or empty filename!");
			return default(T);
		}
		
		T objectOut = default(T);
		
		try {
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(fileName);
			string xmlString = xmlDocument.OuterXml;
			
			using (StringReader read = new StringReader(xmlString)) {
				XmlSerializer serializer = new XmlSerializer(typeof(T));
				using (XmlReader reader = new XmlTextReader(read)) {
					objectOut = (T)serializer.Deserialize(reader);
					reader.Close();
				}
				read.Close();
			}
		}
		catch(System.Exception e) {
			Debug.LogException(e);
		}
		return objectOut;
	}
	
	public static List<string> SearchForObjects(string path) {
		List<string> objectsFound = new List<string>();
		foreach (string file in Directory.GetFiles(path, "*.xml")) {
			objectsFound.Add(Path.GetFileNameWithoutExtension(file));
		}
		return objectsFound;
	}
}
