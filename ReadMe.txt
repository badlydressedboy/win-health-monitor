Use MonitoredServers.xml to specify drives and free space thresholds

Use ServerHealthCheck.exe.config to setup email

Log: ExecutionLog.txt

Arguments: 
Emails will only be send when a command line argument is passed in of SENDEMAIL

Include an argument of SILENT to make sure the app closes and doesnt wait for user to click a button

Create a shortcut to the exe and include that on the end. 
e.g. 
C:\path to exe...\ServerHealthCheck.exe SENDEMAIL SILENT


If program can not connect to a server:

Remote desktop to server and ensure 
 - RPC call service is running (RPC Locator service is not required)
 - Windows Management Instrumentation rules are enabled in firewall
  
 - Open the WMI Control console: Click Start, click Run, type wmimgmt.msc and then click OK.
In the console tree, right-click WMI Control, and then click Properties.
Click the Security tab.
Select CIMV2 then click security
In the Security dialog box, click Add.
In the Select Users, Computers, or Groups dialog box, enter the name of the object (user or group) that miniDBA is running as - this should be a network account or group. Click Check Names to verify your entry and then click OK. You might have to change the location or click the Advanced button to query for objects.
In the Security dialog box, under Permissions, select "Remote Account"
