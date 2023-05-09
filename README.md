# SLC-AS-DMSSanityChecks

This Script is used to collect some of the most important DataMiner Sanity checks and it is used to complement the dashboards for Sanity Checks. 

In the end, it will be generated a csv file in C:\Skyline DataMiner\Documents\DMA_COMMON_DOCUMENTS\DMSSanityChecks folder. 
This csv file contains information of each DMA of the DMS and general information of the DMS, namely:
- DMA information: Hostname, DMA_ID, DMA Version, Number of Elements, Number of Services, Number of RTEs in the last X days, Number of Half Open RTEs in the last X days, Number of Crash Dumps in the last X days, Number of Mini Dumps in the last X days, Information of the RTEs and HalOpen RTEs retrieved from SLWatchDog2. Note that X by default is 10 days.
- DMS information: Count of Active Alarms, Errors, Timeouts, Criticals, Majors, Minors, Warnings, Notices, Number of Protocols available in the DMS, Number of Users and if db Offload is enabled or not.

Besides there is also the possibility of sending an email in the end with the above-mentioned csv file as attachement.

Special cases:
- if you get a -1 in the Alarms related fields, for example in the number of Active Alarms or in the number of Errors, it means that the value was not correctly retrieved, possibly 
- if you get a -1000 in the crashdumps or minidumps count, it means that the corresponding directory could not be found
