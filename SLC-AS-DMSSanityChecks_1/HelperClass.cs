namespace Helpers
{
	using System.IO;
	using System.Linq;
	using System.Xml;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Messages;

	public class HelperClass
	{
		public static bool IsDbOffloadEnabled()
		{
			string filedbXml = File.ReadAllText(@"C:\Skyline DataMiner\db.xml");

			// Load the XML document
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(filedbXml); // where xml is a string containing the XML document

			// Create a namespace manager for resolving the default namespace
			XmlNamespaceManager namespaceMgr = new XmlNamespaceManager(doc.NameTable);
			namespaceMgr.AddNamespace("db", "http://www.skyline.be/config/db");

			// Select all User elements using an XPath expression and the namespace manager
			XmlNodeList offloadItems = doc.SelectNodes("//db:Offload", namespaceMgr);

			return offloadItems.Count > 0;
		}

		public static int GetnumOfUsers()
		{
			// Only counts users in local DMA that runs the script
			string usersXml = File.ReadAllText(@"C:\Skyline DataMiner\Security.xml");

			// Load the XML document
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(usersXml); // where xml is a string containing the XML document

			// Create a namespace manager for resolving the default namespace
			XmlNamespaceManager namespaceMgr = new XmlNamespaceManager(doc.NameTable);
			namespaceMgr.AddNamespace("s", "http://www.skyline.be/config/security");

			// Select all User elements using an XPath expression and the namespace manager
			XmlNodeList userNodes = doc.SelectNodes("//s:User", namespaceMgr);

			return userNodes.Count;
		}

		public static int GetActiveAlarms(Engine engine)
		{
			DMSMessage[] responses = engine.SendSLNetMessage(new GetActiveAlarmsMessage());
			var firstMessage = (ActiveAlarmsResponseMessage)responses.FirstOrDefault();
			if (firstMessage == null)
				return -1;

			return firstMessage.ActiveAlarms.Count(x => x.Source == "DataMiner System");
		}

		// method used to calculate all the alarms types counts (Errors, Timeouts, Critical, Major, Minor, Warning)
		public static int GetTypeActiveAlarms(Engine engine, string alarmtype)
		{
			DMSMessage[] responses = engine.SendSLNetMessage(new GetActiveAlarmsMessage());
			var firstMessage = (ActiveAlarmsResponseMessage)responses.FirstOrDefault();
			if (firstMessage == null)
				return -1;

			return firstMessage.ActiveAlarms.Count(x => x.Severity == alarmtype);
		}
	}
}
