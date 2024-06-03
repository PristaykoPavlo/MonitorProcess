## MonitorProcess ## 
MonitorProcess is a command-line utility for monitoring and killing Windows processes that exceed a specified runtime. 
The utility continuously checks processes based on a given frequency and kills any process that exceeds the allowed duration. 
The utility logs these actions and allows the user to terminate the monitoring by pressing 'q'.

### Features ###
 - Monitors specified processes at regular intervals.
 - Kills processes that run longer than a specified maximum lifetime.
 - Logs each process termination with details.
 - Allows the user to stop monitoring by pressing 'q'.

| Usage:  MonitorProcess.exe <process_name> <max_lifetime_minutes> <frequency_minutes>

| Result: Killed process 'notepad' after running for 2.05 minutes
