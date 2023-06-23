# SLC-AS-DMSSanityChecks

This repository contains an automation script solution with scripts to collect some of the most important DataMiner Sanity checks and it is used to complement the dashboards for Sanity Checks.
Depending on the info you provide, it can also be used to collect these metrics using the DataMiner Teams bot.

In normal use, the script will generate a csv file in C:\Skyline DataMiner\Documents\DMA_COMMON_DOCUMENTS\DMSSanityChecks folder. 
This csv file contains information of each DMA of the DMS and general information of the DMS, namely:
- DMA information: Hostname, DMA_ID, DMA Version, Number of Elements, Number of Services, Number of RTEs in the last X days, Number of Half Open RTEs in the last X days, Number of Crash Dumps in the last X days, Number of Mini Dumps in the last X days, Information of the RTEs and HalOpen RTEs retrieved from SLWatchDog2. Note that X by default is 10 days.
- DMS information: Count of Active Alarms, Errors, Timeouts, Criticals, Majors, Minors, Warnings, Notices, Number of Protocols available in the DMS, Number of Users and if db Offload is enabled or not.

If you specify a valid email, then this csv file can be sent via email as an attachment.
If you specify "-1" for the email, then the file will also not be generated but the info will still be generated to retrieve via the chatbot command.

![Sanity Checks Request](/Documentation/SanityChecks_Request.png)

![Sanity Checks Response](/Documentation/SanityChecks_Result.png)

Special cases:
- if you get a -1000 in the crashdumps or minidumps count, it means that the corresponding directory could not be found
