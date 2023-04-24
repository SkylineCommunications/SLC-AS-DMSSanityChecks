/*
****************************************************************************
*  Copyright (c) 2023,  Skyline Communications NV  All Rights Reserved.    *
****************************************************************************

By using this script, you expressly agree with the usage terms and
conditions set out below.
This script and all related materials are protected by copyrights and
other intellectual property rights that exclusively belong
to Skyline Communications.

A user license granted for this script is strictly for personal use only.
This script may not be used in any way by anyone without the prior
written consent of Skyline Communications. Any sublicensing of this
script is forbidden.

Any modifications to this script by the user are only allowed for
personal use and within the intended purpose of the script,
and will remain the sole responsibility of the user.
Skyline Communications will not be responsible for any damages or
malfunctions whatsoever of the script resulting from a modification
or adaptation by the user.

The content of this script is confidential information.
The user hereby agrees to keep this confidential information strictly
secret and confidential and not to disclose or reveal it, in whole
or in part, directly or indirectly to any person, entity, organization
or administration without the prior written consent of
Skyline Communications.

Any inquiries can be addressed to:

	Skyline Communications NV
	Ambachtenstraat 33
	B-8870 Izegem
	Belgium
	Tel.	: +32 51 31 35 69
	Fax.	: +32 51 31 01 29
	E-mail	: info@skyline.be
	Web		: www.skyline.be
	Contact	: Ben Vandenberghe

****************************************************************************
Revision History:

DATE		VERSION		AUTHOR			COMMENTS

dd/mm/2023	1.0.0.1		XXX, Skyline	Initial version
****************************************************************************
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

using Helpers;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.Library.Automation;
using Skyline.DataMiner.Library.Common;
using Skyline.DataMiner.Net.Messages;

/// <summary>
/// DataMiner Script Class.
/// </summary>
///

namespace Helpers
{
	public class DMA
	{
		private int dmaId;
		private string dmaName;
		private string ipAddr;
		private bool isFailover;

		public DMA(string dmaName, string ipAddr, bool isFailover, int dmaId)
		{
			this.dmaName = dmaName;
			this.ipAddr = ipAddr;
			this.isFailover = isFailover;
			this.dmaId = dmaId;
		}
	}

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

			// Count the number of User elements
			int userCount = userNodes.Count;

			return userCount;
		}

		public static void GetBackupSettings(Engine engine)
		{
			// TODO
		}

		public static int GetActiveAlarms(Engine engine)
		{
			DMSMessage[] responses = engine.SendSLNetMessage(new GetActiveAlarmsMessage());
			return ((ActiveAlarmsResponseMessage)responses.First()).ActiveAlarms.Count(x => x.Source == "DataMiner System");
		}

		// method used to calculate all the alarms types counts (Errors, Timeouts, Critical, Major, Minor, Warning)
		public static int GetTypeActiveAlarms(Engine engine, string alarmtype)
		{
			DMSMessage[] responses = engine.SendSLNetMessage(new GetActiveAlarmsMessage());
			return ((ActiveAlarmsResponseMessage)responses.First()).ActiveAlarms.Count(x => x.Severity == alarmtype);
		}

		// Let's use IDms instead of this. Just keeping it to be used as an example on how to use SendSLNetMessage
		public static DMA[] LookupForDmaIpAddress(Engine engine)
		{
			List<DMA> dmas = new List<DMA>();
			DMSMessage[] responses = engine.SendSLNetMessage(new GetInfoMessage(-1, InfoType.DataMinerInfo, string.Empty));

			foreach (var response in responses)
			{
				GetDataMinerInfoResponseMessage getDataminerInfoResponse = (GetDataMinerInfoResponseMessage)response;
				string dmaName = getDataminerInfoResponse.AgentName;
				string primaryIP = getDataminerInfoResponse.PrimaryIP;
				int dmaId = getDataminerInfoResponse.ID;

				DMA dma = new DMA(dmaName, primaryIP, getDataminerInfoResponse.IsFailover, dmaId);
				dmas.Add(dma);
			}

			return dmas.ToArray();
		}

		public static Skyline.DataMiner.Net.Messages.DMSMessage SLNet_Advanced_SetDataMinerInfoMessage(Engine engine, int imbInfo1, int imbInfo2, int iDataMinerID, int iElementID, int iHostingDataMinerID, int iIInfo1, int iIInfo2, Skyline.DataMiner.Net.Messages.PSA psa1, Skyline.DataMiner.Net.Messages.PSA psa2, Skyline.DataMiner.Net.Messages.PUIA puia1, Skyline.DataMiner.Net.Messages.PUIA puia2, Skyline.DataMiner.Net.Messages.SA sa1, Skyline.DataMiner.Net.Messages.SA sa2, string sStrInfo1, string sStrInfo2, Skyline.DataMiner.Net.Messages.UIA uia1, Skyline.DataMiner.Net.Messages.UIA uia2, int iWhat)
		{
			// Define the container for the response from the SLNet call
			Skyline.DataMiner.Net.Messages.DMSMessage responseDataMinerInfoMessage = null;

			try
			{
				// Create a message that will contain the request message
				Skyline.DataMiner.Net.Messages.Advanced.SetDataMinerInfoMessage dataminerInfoMessage = new Skyline.DataMiner.Net.Messages.Advanced.SetDataMinerInfoMessage();

				// Setting the request message
				dataminerInfoMessage.bInfo1 = imbInfo1;
				dataminerInfoMessage.bInfo2 = imbInfo2;
				dataminerInfoMessage.DataMinerID = iDataMinerID;
				dataminerInfoMessage.ElementID = iElementID;
				dataminerInfoMessage.HostingDataMinerID = iHostingDataMinerID;
				dataminerInfoMessage.IInfo1 = iIInfo1;
				dataminerInfoMessage.IInfo2 = iIInfo2;
				dataminerInfoMessage.Psa1 = psa1;
				dataminerInfoMessage.Psa2 = psa2;
				dataminerInfoMessage.Puia1 = puia1;
				dataminerInfoMessage.Puia2 = puia2;
				dataminerInfoMessage.Sa1 = sa1;
				dataminerInfoMessage.Sa2 = sa2;
				dataminerInfoMessage.StrInfo1 = sStrInfo1;
				dataminerInfoMessage.StrInfo2 = sStrInfo2;
				dataminerInfoMessage.Uia1 = uia1;
				dataminerInfoMessage.Uia2 = uia2;
				dataminerInfoMessage.What = iWhat;

				// Via SLNet, we send the SLNet message
				responseDataMinerInfoMessage = Engine.SLNet.SendMessage(dataminerInfoMessage).First();
			}// End try
			catch (Exception exception)
			{
				engine.GenerateInformation("[ERROR]|SLNet_Advanced_SetDataMinerInfoMessage|Exception:" + exception.ToString());
			}// End catch

			return responseDataMinerInfoMessage;
		}// End method SLNet_Advanced_SetDataMinerInfoMessage
	}
}

public class Script
{
	private static string delimiter = ",";

	private string csvFileName = DateTime.Now.Day + "_" + DateTime.Now.Month + "_" + DateTime.Now.Year + " " + DateTime.Now.Hour + "h" + DateTime.Now.Minute + "m" + ".csv";

	private string fullFileName = @"C:\Skyline DataMiner\Documents\DMA_COMMON_DOCUMENTS\DMSSanityChecks\DMSSanityChecks_" + DateTime.Now.Day + "_" + DateTime.Now.Month + "_" + DateTime.Now.Year + " " + DateTime.Now.Hour + "h" + DateTime.Now.Minute + "m" + ".csv";

	/// <summary>
	/// The Script entry point.
	/// </summary>
	/// <param name="engine">Link with SLAutomation process.</param>
	public void Run(Engine engine)
	{
		Dictionary<string, string> results = new Dictionary<string, string>();
		var dms = engine.GetDms();
		var dmas = dms.GetAgents();

		results.Add(" ", String.Empty);

		foreach (var dma in dmas)
		{
			string hostname = dma.HostName;
			// engine.GenerateInformation("DMSSanityChecks|hostname: " + hostname);
			results.Add("hostname", hostname);

			int dma_ID = dma.Id;
			// engine.GenerateInformation("DMSSanityChecks|hostname: " + hostname);
			results.Add("DMA_ID", dma_ID.ToString());

			string version = dma.VersionInfo;
			// engine.GenerateInformation("DMSSanityChecks|version: " + version);
			results.Add("version", version);

			int numOfElements = dma.GetElements().Count();
			// engine.GenerateInformation("DMSSanityChecks|nrOfElements: " + numOfElements);
			results.Add("nrOfElements", numOfElements.ToString());

			int numOfServices = dma.GetServices().Count();
			// engine.GenerateInformation("DMSSanityChecks|numOfServices: " + numOfServices);
			results.Add("numOfServices", numOfServices.ToString());

			// Get RTEs and HF_RTEs
			Skyline.DataMiner.Net.Messages.ExecuteScriptMessage scriptRTEMessage = new ExecuteScriptMessage()
			{
				DataMinerID = dma.Id,// DMA ID
				ScriptName = "GetRTEsScript",
				Options = new SA(new[] { $"DEFER:{bool.FalseString}" }),
			};

			var response_RTE = Engine.SLNet.SendSingleResponseMessage(scriptRTEMessage) as ExecuteScriptResponseMessage;
			var scriptRTEResult = response_RTE?.ScriptOutput;

			foreach (var item in scriptRTEResult)
			{
				if (item.Key == "Rtes")
				{
					results.Add("numOfRtes", item.Value);
				}
				if (item.Key == "HalfOpenRtes")
				{
					results.Add("numOfHalfOpenRtes", item.Value);
				}

				if (item.Key.Contains("LineOfRTEs"))
				{
					results.Add(item.Key, item.Value);
				}

				if (item.Key.Contains("LineOfHalfOpenRtes"))
				{
					results.Add(item.Key, item.Value);
				}

				// CrashDumps and MiniDumps
				if (item.Key.Contains("CrashDumps"))
				{
					results.Add(item.Key, item.Value);
				}

				if (item.Key.Contains("MiniDumps"))
				{
					results.Add(item.Key, item.Value);
				}
			}

			// Crashdumps and memory dumps
			//Skyline.DataMiner.Net.Messages.ExecuteScriptMessage scriptCrashDumpMessage = new ExecuteScriptMessage()
			//{
			//	DataMinerID = dma.Id,// DMA ID
			//	ScriptName = "GetCrashdumpScript",
			//	Options = new SA(new[] { $"DEFER:{bool.FalseString}" }),
			//};

			//var response_Dumps = Engine.SLNet.SendSingleResponseMessage(scriptCrashDumpMessage) as ExecuteScriptResponseMessage;
			//var scriptCrashResult = response_Dumps?.ScriptOutput;

			//engine.GenerateInformation(scriptCrashResult.ToString());

			// results.Add("numOfCrashDumps", scriptCrashResult.Keys("CrashDumps"));

			// Log levels

		}

		results.Add("  ", String.Empty);
		results.Add("DMS", "Detailed Information about the DMS");

		int numOfActiveAlarms = HelperClass.GetActiveAlarms(engine);
		// engine.GenerateInformation("DMSSanityChecks|numOfActiveAlarms: " + numOfActiveAlarms);
		results.Add("numOfActiveAlarms", numOfActiveAlarms.ToString());

		int numOfError = HelperClass.GetTypeActiveAlarms(engine, "Error");
		// engine.GenerateInformation("DMSSanityChecks|Errors: " + numOfError);
		results.Add("numOfError", numOfError.ToString());

		int numOfTimeout = HelperClass.GetTypeActiveAlarms(engine, "Timeout");
		// engine.GenerateInformation("DMSSanityChecks|Timeouts: " + numOfTimeout);
		results.Add("numOfTimeout", numOfTimeout.ToString());

		int numOfCriticals = HelperClass.GetTypeActiveAlarms(engine, "Critical");
		// engine.GenerateInformation("DMSSanityChecks|Criticals: " + numOfCriticals);
		results.Add("numOfCritical", numOfCriticals.ToString());

		int numOfMajor = HelperClass.GetTypeActiveAlarms(engine, "Major");
		// engine.GenerateInformation("DMSSanityChecks|Majors: " + numOfMajor);
		results.Add("numOfMajor", numOfMajor.ToString());

		int numOfMinor = HelperClass.GetTypeActiveAlarms(engine, "Minor");
		// engine.GenerateInformation("DMSSanityChecks|Minors: " + numOfMinor);
		results.Add("numOfMinor", numOfMinor.ToString());

		int numOfWarning = HelperClass.GetTypeActiveAlarms(engine, "Warning");
		// engine.GenerateInformation("DMSSanityChecks|Warnings: " + numOfWarning);
		results.Add("numOfWarning", numOfWarning.ToString());

		int numOfNotices = HelperClass.GetTypeActiveAlarms(engine, "Notice");
		// engine.GenerateInformation("DMSSanityChecks|Notices: " + numOfNotices);
		results.Add("numOfNotices", numOfNotices.ToString());

		int numOfProtocols = dms.GetProtocols().Count();
		// engine.GenerateInformation("DMSSanityChecks|Protocols: " + numOfProtocols);
		results.Add("numOfProtocols", numOfProtocols.ToString());

		// count of users - can be retrieved from Security.xml
		int numOfUsers = HelperClass.GetnumOfUsers();
		// engine.GenerateInformation("DMSSanityChecks|numOfUsers: " + numOfUsers);
		results.Add("numOfUsers", numOfUsers.ToString());

		// backup - can be retrieved from slnetmessages but not clear

		// offload - can be retrieved from db.xml
		bool isOffloadEnabled = HelperClass.IsDbOffloadEnabled();
		// engine.GenerateInformation("DMSSanityChecks|isOffloadEnabled: " + isOffloadEnabled);
		results.Add("isOffloadEnabled", isOffloadEnabled.ToString());

		File.WriteAllText(fullFileName,$"Metric{delimiter}" + $"Value{delimiter}" + "\r\n");

		File.AppendAllLines(fullFileName, results.Select(x => x.Key + delimiter + x.Value + delimiter));

		engine.SendSLNetMessage(
		 new SendEmailMessage
		 {
			 To = "carolina.costa@skyline.be",
			 Body = "This an example e-mail! :)",
			 IsHtml = false,
			 Attachments = new[]
		 {
		 new EmailAttachment
		 {
			 Data = File.ReadAllBytes(fullFileName),
			 FileName = csvFileName,
		 },
		 },
		 });
	}
}