# Setup notes

## What was installed and versions
- .NET 10.0.102
- MySQL 8.0.44
- MySqlConnector 3.0.0

## How the app connects to the database
- The CampaignPerformanceAnalytics application connects to the database through the connection string that allows it to locate the server (localhost), MySQL’s default port (3306),  the database(campaign_analytics), the database username (campaign_app_user), and the password.
- MySqlConnector allows the app to connect and communicate with the MySQL database server.
- Connection is opened at startup and prints “Connected successfully! Authenticated as: campaign_app_user@localhost” to demonstrate the connection functions.

## Issues
- Initially, there was a permissions error where I could not open MySQL80 service when I wasn’t running PowerShell as an administrator. After reopening and running PowerShell as an administrator, the service still failed and the .err file in ‘C:\ProgramData\MySQL\MySQL Server 8.0\Data’  showed that the data folder was corrupted/damaged (redo log files came from a different data directory). To fix it, I followed these steps:
1. Moved the broken data folder aside renaming ‘Data’ to ‘Data_old_backup’
2. Re-initialized a new data folder 
3. Gave the service write access
4. Started the service
5. Set a root password
