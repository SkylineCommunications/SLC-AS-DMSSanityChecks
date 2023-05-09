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

09/05/2023	1.0.0.1		CCO, Skyline	Initial version
****************************************************************************
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Helpers;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.Library.Automation;
using Skyline.DataMiner.Library.Common;
using Skyline.DataMiner.Net.Exceptions;
using Skyline.DataMiner.Net.Messages;

/// <summary>
/// DataMiner Script Class.
/// </summary>
///

namespace Helpers
{
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
			return ((ActiveAlarmsResponseMessage)responses.First()).ActiveAlarms.Count(x => x.Source == "DataMiner System");
		}

		// method used to calculate all the alarms types counts (Errors, Timeouts, Critical, Major, Minor, Warning)
		public static int GetTypeActiveAlarms(Engine engine, string alarmtype)
		{
			DMSMessage[] responses = engine.SendSLNetMessage(new GetActiveAlarmsMessage());
			return ((ActiveAlarmsResponseMessage)responses.First()).ActiveAlarms.Count(x => x.Severity == alarmtype);
		}
	}
}

public class Script
{
	private static readonly DateTime Datenow = DateTime.Now;
	private static readonly string Delimiter = ",";
	private readonly string csvFileName = $"DMSSanityChecks_{Datenow.Day}_{Datenow.Month}_{Datenow.Year} {Datenow.Hour}h{Datenow.Minute}m.csv";
	private readonly string filePath = @"C:\Skyline DataMiner\Documents\DMA_COMMON_DOCUMENTS\DMSSanityChecks\";

	/// <summary>
	/// The Script entry point.
	/// </summary>
	/// </summary>
	/// <param name="engine">Link with SLAutomation process.</param>
	public void Run(Engine engine)
	{
		Dictionary<string, string> results = new Dictionary<string, string>();
		var dms = engine.GetDms();
		var dmas = dms.GetAgents();

		int numOfDMAs = dmas.Count();

		results.Add(" ", String.Empty);

		var header = $"Hostname,DMA_ID,Version,Nr Elements,Nr Services,Nr RTEs,Nr Half Open, Nr Crash Dumps, Nr of Mini Dumps, Line of RTE, Line of Half Open";
		var lines = new List<string>();

		foreach (var dma in dmas)
		{
			try
			{
				var rte = GetRteInfo(dma);

				// engine.GenerateInformation(string.Join(";", rte.Keys));
				lines.Add($"{dma.HostName},{dma.Id},{dma.VersionInfo},{dma.GetElements().Count},{dma.GetServices().Count},{rte["Rtes"]},{rte["HalfOpenRtes"]},{rte["CrashDumps"]},{rte["MiniDumps"]}, {rte[$"LineOfRTEs"]},{rte[$"LineOfHalfOpenRtes"]}");
			}
			catch (DataMinerCommunicationException)
			{
				engine.GenerateInformation($"DataMiner ID {dma.Id} is not available");
			}
		}

		var fullFileName = $"{filePath}{csvFileName}";
		File.WriteAllText(fullFileName, header + "\r\n");
		File.AppendAllLines(fullFileName, lines);

		results.Add("  ", String.Empty);
		results.Add("DMS", "Detailed Information about the DMS");

		results.Add("numOfActiveAlarms", HelperClass.GetActiveAlarms(engine).ToString());
		results.Add("numOfError", HelperClass.GetTypeActiveAlarms(engine, "Error").ToString());
		results.Add("numOfTimeout", HelperClass.GetTypeActiveAlarms(engine, "Timeout").ToString());
		results.Add("numOfCritical", HelperClass.GetTypeActiveAlarms(engine, "Critical").ToString());
		results.Add("numOfMajor", HelperClass.GetTypeActiveAlarms(engine, "Major").ToString());
		results.Add("numOfMinor", HelperClass.GetTypeActiveAlarms(engine, "Minor").ToString());
		results.Add("numOfWarning", HelperClass.GetTypeActiveAlarms(engine, "Warning").ToString());
		results.Add("numOfNotices", HelperClass.GetTypeActiveAlarms(engine, "Notice").ToString());
		results.Add("numOfProtocols", dms.GetProtocols().Count.ToString());
		results.Add("numOfUsers", HelperClass.GetnumOfUsers().ToString());
		results.Add("isOffloadEnabled", HelperClass.IsDbOffloadEnabled().ToString());

		File.AppendAllLines(fullFileName, results.Select(x => x.Key + Delimiter + x.Value + Delimiter));

		// Send an email with the file output of this script
		engine.SendSLNetMessage(
		 new SendEmailMessage
		 {
			 To = "carolina.costa@skyline.be",
			 Body = $"Automation Script - Sanity Checks",
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

	private static Dictionary<string, string> GetRteInfo(IDma dma)
	{
		// Get RTEs and HF_RTEs
		Skyline.DataMiner.Net.Messages.ExecuteScriptMessage scriptRTEMessage = new ExecuteScriptMessage()
		{
			DataMinerID = dma.Id,// DMA ID
			ScriptName = "GetRTEsandDumpsScript",
			Options = new SA(new[] { $"DEFER:{bool.FalseString}" }),
		};

		var response_RTE = Engine.SLNet.SendSingleResponseMessage(scriptRTEMessage) as ExecuteScriptResponseMessage;
		var scriptRTEResult = response_RTE?.ScriptOutput;
		return scriptRTEResult;
	}
}