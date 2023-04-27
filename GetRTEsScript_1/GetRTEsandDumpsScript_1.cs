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

namespace GetRTEsandDumpsScript_1
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Xml;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Messages;

	/// <summary>
	/// Represents a DataMiner Automation script.
	/// </summary>
	public class Script
	{
		/// <summary>
		/// The script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		public void Run(IEngine engine)
		{
			List<string> rtelineList;
			rtelineList = new List<string>();

			rtelineList = HelperClass.GetRTEs("RTE Count");
			string string_numOfRTEs = rtelineList.Last();
			int numOfRTEs = int.Parse(string_numOfRTEs);
			// engine.GenerateInformation("DMSSanityChecks|numOfRTEs: " + numOfRTEs);

			engine.AddScriptOutput("Rtes", numOfRTEs.ToString());

			engine.AddScriptOutput(
				$"LineOfRTEs",
				string.Join("\n",rtelineList.Where(x => !string.IsNullOrEmpty(x))));

			List<string>hf_rtelineList;
			hf_rtelineList = new List<string>();

			hf_rtelineList = HelperClass.GetRTEs("HALFOPEN RTE");
			string string_numOfHFRTEs = hf_rtelineList.Last();
			int numOfHFRTEs = int.Parse(string_numOfHFRTEs);
			// engine.GenerateInformation("DMSSanityChecks|numOfHFRTEs: " + numOfHFRTEs);

			engine.AddScriptOutput("HalfOpenRtes", numOfHFRTEs.ToString());

			StringBuilder hrtes = new StringBuilder();
			foreach (string shf in hf_rtelineList)
			{
				if (shf != null)
				{
					hrtes.AppendLine($"{shf}");
				}
			}

			engine.AddScriptOutput($"LineOfHalfOpenRtes", hrtes.ToString());

			// Crashdumps and Minidumps
			int crashdumpsCount = HelperClass.GetDumpsforLastDays("Crash", 10);
			int minidumpsCount = HelperClass.GetDumpsforLastDays("Mini", 10);

			engine.AddScriptOutput("CrashDumps", crashdumpsCount.ToString());
			engine.AddScriptOutput("MiniDumps", minidumpsCount.ToString());

		}

		public class HelperClass
		{
			public static List<string> GetRTEs(string isRTEorHF)
			{
				string logFile = @"C:\Skyline DataMiner\logging\SLWatchdog2.txt";
				Stream stream = File.Open(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				DateTime endDate = DateTime.Now;
				DateTime startDate = endDate.AddDays(-10);
				int rteCount = 0;
				List<string> saveRTEline;
				saveRTEline = new List<string>();

				using (StreamReader sr = new StreamReader(stream))
				{
					string previousLine = String.Empty;
					while (!sr.EndOfStream)
					{
						string line = sr.ReadLine();
						if (line.Contains(isRTEorHF) && DateTime.TryParse(line.Substring(0, 19), out DateTime dateTime))
						{
							if (dateTime >= startDate && dateTime <= endDate)
							{
								rteCount++;
								saveRTEline.Add(previousLine);
							}
						}

						previousLine = line;
					}
				}

				saveRTEline.Add(rteCount.ToString());

				return saveRTEline;
			}

			public static int GetDumpsforLastDays(string isCrashOrMini, int numOfDays)
			{
				int count = 0;
				DateTime fromDate = DateTime.Now.AddDays(-numOfDays);
				string directoryPath = String.Empty;

				if (isCrashOrMini == "Crash")
				{
					directoryPath = @"C:\Skyline DataMiner\Logging\CrashDump";
				}
				else if (isCrashOrMini == "Mini")
				{
					directoryPath = @"C:\Skyline DataMiner\Logging\MiniDump";
				}
				else
				{
					count = -1000;
					return count;
				}

				DirectoryInfo directory = new DirectoryInfo(directoryPath);
				FileInfo[] files = directory.GetFiles();

				foreach (FileInfo file in files)
				{
					if (file.CreationTime >= fromDate)
					{
						count++;
					}
				}

				return count;
			}
		}
	}
}