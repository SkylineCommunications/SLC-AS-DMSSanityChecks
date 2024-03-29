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
using System.Security.Cryptography;
using AdaptiveCards;
using Helpers;
using Newtonsoft.Json;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.Core.DataMinerSystem.Automation;
using Skyline.DataMiner.Core.DataMinerSystem.Common;
using Skyline.DataMiner.Net;
using Skyline.DataMiner.Net.Exceptions;
using Skyline.DataMiner.Net.Messages;
using Skyline.DataMiner.Net.Messages.SLDataGateway;

/// <summary>
/// DataMiner Script Class.
/// </summary>
///
public class Script
{
	private const string Delimiter = ",";
	private static readonly DateTime Datenow = DateTime.Now;
	private readonly string csvFileName = $"DMSSanityChecks_{Datenow.Day}_{Datenow.Month}_{Datenow.Year} {Datenow.Hour}h{Datenow.Minute}m.csv";
	private readonly string filePath = @"C:\Skyline DataMiner\Documents\DMA_COMMON_DOCUMENTS\DMSSanityChecks\";
	private string subId;
	private Dictionary<int, TimeSpan> uptimes;
	
	/// <summary>
	/// The Script entry point.
	/// </summary>
	/// <param name="engine">Link with SLAutomation process.</param>
	public void Run(Engine engine)
	{
		Dictionary<string, string> results = new Dictionary<string, string>();
		var dms = engine.GetDms();
		var dmas = dms.GetAgents();

		var emailToSend = engine.GetScriptParam("E-mail Destination").Value;
		var sendEmail = emailToSend != "-1";

		int i = 0;
		List<TimeSpan> dmaUptimes = GetDmaUptime(engine);

		results.Add(" ", String.Empty);

		var header = $"Hostname,DMA_ID,Version, DMA Uptime, Nr Elements,Nr Services,Nr RTEs,Nr Half Open, Nr Crash Dumps, Nr of Mini Dumps, Line of RTE, Line of Half Open";
		var lines = new List<string>();

		var adaptiveCardBody = new List<AdaptiveElement>();

		foreach (var dma in dmas)
		{
			try
			{
				var rte = GetRteInfo(dma);
				var stringdmauptimes = dmaUptimes[i].ToString("d'd 'h'h 'm'm 's's'");

				// engine.GenerateInformation(string.Join(";", rte.Keys));
				lines.Add($"{dma.HostName},{dma.Id},{dma.VersionInfo},{stringdmauptimes},{dma.GetElements().Count},{dma.GetServices().Count},{rte["Rtes"]},{rte["HalfOpenRtes"]},{rte["CrashDumps"]},{rte["MiniDumps"]},{rte[$"LineOfRTEs"]},{rte[$"LineOfHalfOpenRtes"]}");
				i++;

				adaptiveCardBody.Add(new AdaptiveTextBlock($"Info for {dma.HostName} [{dma.Id}]") { Type = "TextBlock", Weight = AdaptiveTextWeight.Bolder });
				adaptiveCardBody.Add(new AdaptiveFactSet
				{
					Type = "FactSet",
					Facts = new List<AdaptiveFact>
					{
						new AdaptiveFact("Version", dma.VersionInfo),
						new AdaptiveFact("Up time", stringdmauptimes),
						new AdaptiveFact("Number of elements", dma.GetElements().Count.ToString()),
						new AdaptiveFact("Number of services", dma.GetServices().Count.ToString()),
						new AdaptiveFact("Number of RTEs", rte["Rtes"]),
						new AdaptiveFact("Number of half open RTEs", rte["HalfOpenRtes"]),
						new AdaptiveFact("Number of crashdumps", rte["CrashDumps"]),
						new AdaptiveFact("Number of minidumps", rte["MiniDumps"]),
					},
				});
			}
			catch (DataMinerCommunicationException)
			{
				engine.GenerateInformation($"DataMiner ID {dma.Id} is not available");
			}
		}

		results.Add("  ", String.Empty);
		results.Add("DMS", "Detailed Information about the DMS");

		var activealarms = HelperClass.GetActiveAlarms(engine);

		results.Add("numOfActiveAlarms", activealarms.Count(x => x.Source == "DataMiner System").ToString());
		results.Add("numOfError", activealarms.Count(x => x.Severity == "Error").ToString());
		results.Add("numOfTimeout", activealarms.Count(x => x.Severity == "Timeout").ToString());
		results.Add("numOfCritical", activealarms.Count(x => x.Severity == "Critical").ToString());
		results.Add("numOfMajor", activealarms.Count(x => x.Severity == "Major").ToString());
		results.Add("numOfMinor", activealarms.Count(x => x.Severity == "Minor").ToString());
		results.Add("numOfWarning", activealarms.Count(x => x.Severity == "Warning").ToString());
		results.Add("numOfNotices", activealarms.Count(x => x.Severity == "Notice").ToString());
		results.Add("numOfProtocols", dms.GetProtocols().Count.ToString());
		results.Add("numOfUsers", HelperClass.GetnumOfUsers().ToString());
		results.Add("isOffloadEnabled", HelperClass.IsDbOffloadEnabled().ToString());

		adaptiveCardBody.Add(new AdaptiveTextBlock($"Info for DMS") { Type = "TextBlock", Weight = AdaptiveTextWeight.Bolder });
		adaptiveCardBody.Add(new AdaptiveFactSet
		{
			Type = "FactSet",
			Facts = new List<AdaptiveFact>
			{
				new AdaptiveFact("Number of active alarms", activealarms.Count(x => x.Source == "DataMiner System").ToString()),
				new AdaptiveFact("Number of errors", activealarms.Count(x => x.Severity == "Error").ToString()),
				new AdaptiveFact("Number of timeouts", activealarms.Count(x => x.Severity == "Timeout").ToString()),
				new AdaptiveFact("Number of criticals", activealarms.Count(x => x.Severity == "Critical").ToString()),
				new AdaptiveFact("Number of majors", activealarms.Count(x => x.Severity == "Major").ToString()),
				new AdaptiveFact("Number of minors", activealarms.Count(x => x.Severity == "Minor").ToString()),
				new AdaptiveFact("Number of warnings", activealarms.Count(x => x.Severity == "Warning").ToString()),
				new AdaptiveFact("Number of notices", activealarms.Count(x => x.Severity == "Notice").ToString()),
				new AdaptiveFact("Number of protocols", dms.GetProtocols().Count.ToString()),
				new AdaptiveFact("Number of users", HelperClass.GetnumOfUsers().ToString()),
				new AdaptiveFact("Is DB offload enabled", HelperClass.IsDbOffloadEnabled().ToString()),
			},
		});

		engine.AddScriptOutput("AdaptiveCard", JsonConvert.SerializeObject(adaptiveCardBody));

		try
		{
			Directory.CreateDirectory(filePath);
			var fullFileName = $"{filePath}{csvFileName}";
			File.WriteAllText(fullFileName, header + "\r\n");
			File.AppendAllLines(fullFileName, lines);
			File.AppendAllLines(fullFileName, results.Select(x => x.Key + Delimiter + x.Value + Delimiter));

			if (sendEmail)
			{
				// Send an email with the file output of this script
				engine.SendSLNetMessage(
				 new SendEmailMessage
				 {
					 To = emailToSend,
					 Body = "Automation Script - Sanity Checks",
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
		catch (DataMinerCOMException)
		{
			engine.GenerateInformation($"There was an issue with sending an email. Email not sent. Please check your connection.");
		}
		catch (DataMinerSecurityException)
		{
			engine.GenerateInformation($"There was an issue with sending an email. Email not sent. Missing permission. Not allowed to Send email.");
		}
		catch (DataMinerException)
		{
			engine.GenerateInformation($"There was an issue with sending an email. Email not sent. Please check if the email is valid.");
		}
	}

	private static Dictionary<string, string> GetRteInfo(IDma dma)
	{
		// Get RTEs and HF_RTEs
		Skyline.DataMiner.Net.Messages.ExecuteScriptMessage scriptRTEMessage = new ExecuteScriptMessage
		{
			DataMinerID = dma.Id,// DMA ID
			ScriptName = "GetRTEsandDumpsScript",
			Options = new SA(new[] { $"DEFER:{bool.FalseString}" }),
		};

		var response_RTE = Engine.SLNet.SendSingleResponseMessage(scriptRTEMessage) as ExecuteScriptResponseMessage;
		var scriptRTEResult = response_RTE?.ScriptOutput;
		return scriptRTEResult;
	}

	private void HandleMessage(object sender, NewMessageEventArgs newevent)
	{
		if (!newevent.FromSet(subId))
		{
			return;
		}

		if (newevent.Message is DataMinerPerformanceInfoEventMessage performanceMessage)
		{
			uptimes[performanceMessage.DataMinerID] = DateTime.Now - performanceMessage.StartupTime;
		}
	}

	private List<TimeSpan> GetDmaUptime(Engine engine)
	{
		subId = Guid.NewGuid().ToString();
		uptimes = new Dictionary<int, TimeSpan>();
		IConnection connection = Engine.SLNetRaw;
		connection.OnNewMessage += HandleMessage;
		connection.AddSubscription(subId, new SubscriptionFilter(typeof(DataMinerPerformanceInfoEventMessage)));

		engine.Sleep(5000);

		connection.OnNewMessage -= HandleMessage;
		connection.RemoveSubscription(subId);

		var dmauptimesToList = uptimes.Values.ToList();

		return dmauptimesToList;
	}
}