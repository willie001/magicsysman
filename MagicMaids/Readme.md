*********************************************************************************
							BUTTON COLOURS
*********************************************************************************

Cancel Button:						btn btn-default
Delete button: 						btn btn-danger
Form main action (save):			btn btn-success
In-Form sub action: 				btn btn-primary

Expanded context of current instance (more info):				btn btn-info
Navigate away from current instance without state retaining:	btn btn-warning


NodaTime
https://hanson.io/taming-the-datetime-beast-with-noda-time/

** Parent / Child management
https://stackoverflow.com/questions/27176014/how-to-add-update-child-entities-when-updating-a-parent-entity-in-ef


MySQL Connection issues: Remember to install MySql .NET Connector
https://stackoverflow.com/questions/9527303/could-not-load-file-or-assembly-mysql-data-version-6-3-6-0
https://bugs.mysql.com/bug.php?id=76597

Connection issues persisting:
https://docs.plesk.com/en-US/12.5/administrator-guide/website-management/websites-and-domains/advanced-extended-website-management/using-virtual-directories-windows/configuring-aspnet-for-virtual-directories.65219/

Deployment Pipeline
Config - The web.config is generic and no specific environment settings are contained in this file.

A directory called Config contains the respective config files for app settings and connection strings. 
This directory is added to the git.ignore file to ensure that it is not pushed to the TEST and PROD systems.

When creating a new instance make sure you add the Config directory and config files manually.