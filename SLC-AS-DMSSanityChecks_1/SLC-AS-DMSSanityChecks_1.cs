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
using System.Linq;
using System.Text;
using Helpers;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.Library.Automation;
using Skyline.DataMiner.Library.Common;
using Skyline.DataMiner.Net.Messages;

/// <summary>
/// DataMiner Script Class.
/// </summary>
public class Script
{
	/// <summary>
	/// The Script entry point.
	/// </summary>
	/// <param name="engine">Link with SLAutomation process.</param>
	public void Run(Engine engine)
	{
		var dms = engine.GetDms();
		dms.GetAgents();

		var dma = dms.GetAgent(1 /*dataminer_id*/);
		string version = dma.VersionInfo;
		int nrOfElements = dma.GetElements().Count();
		int nrOfServices = dma.GetServices().Count();
		int nrOfActiveAlarms = HelperClass.GetActiveAlarms(engine);
	}
}

namespace Helpers
{
	public class DMA
	{
		public int dmaId;
		public string dmaName;
		public string ipAddr;
		public bool isFailover;

		public DMA(string DmaName, string IpAddr, bool IsFailover, int DmaId)
		{
			this.dmaName = DmaName;
			this.ipAddr = IpAddr;
			this.isFailover = IsFailover;
			this.dmaId = DmaId;
		}
	}

	public class HelperClass
	{
		public static void GetBackupSettings(Engine engine)
		{
			//TODO
		}

		public static int GetActiveAlarms(Engine engine)
		{
			ActiveAlarmsResponseMessage[] responses = (ActiveAlarmsResponseMessage[])engine.SendSLNetMessage(new GetActiveAlarmsMessage()); // this will include Suggestion source while on Cube we only see DataMiner System source of alarms.
			return responses.First().ActiveAlarms.Count();
		}

		//Let's use IDms instead of this. Just keeping it to be used as an example on how to use SendSLNetMessage
		public static DMA[] LookupForDmaIpAddress(Engine engine)
		{
			List<DMA> dmas = new List<DMA>();
			DMSMessage[] responses = engine.SendSLNetMessage(new GetInfoMessage(-1, InfoType.DataMinerInfo, ""));

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

		public static Skyline.DataMiner.Net.Messages.DMSMessage SLNet_Advanced_SetDataMinerInfoMessage(Engine engine, int ibInfo1, int ibInfo2, int iDataMinerID, int iElementID, int iHostingDataMinerID, int iIInfo1, int iIInfo2, Skyline.DataMiner.Net.Messages.PSA psa1, Skyline.DataMiner.Net.Messages.PSA psa2, Skyline.DataMiner.Net.Messages.PUIA puia1, Skyline.DataMiner.Net.Messages.PUIA puia2, Skyline.DataMiner.Net.Messages.SA sa1, Skyline.DataMiner.Net.Messages.SA sa2, string sStrInfo1, string sStrInfo2, Skyline.DataMiner.Net.Messages.UIA uia1, Skyline.DataMiner.Net.Messages.UIA uia2, int iWhat)
		{
			// Define the container for the response from the SLNet call
			Skyline.DataMiner.Net.Messages.DMSMessage responseDataMinerInfoMessage = null;

			try
			{
				// Create a message that will contain the request message
				Skyline.DataMiner.Net.Messages.Advanced.SetDataMinerInfoMessage dataminerInfoMessage = new Skyline.DataMiner.Net.Messages.Advanced.SetDataMinerInfoMessage();

				// Setting the request message
				dataminerInfoMessage.bInfo1 = ibInfo1;
				dataminerInfoMessage.bInfo2 = ibInfo2;
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