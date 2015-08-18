RokuLoader 1.0

The RokuLoader is a command line interface for Windows that allowes developer
builds of Roku channels (packaged az .zip files) to be uploaded to a Roku
device's Developer Application Interface in one command.

Under MIT License http://github.com/patrick0xf/RokuLoader
--------------------------------------------------------------------------------

For details regarding Roku development, visit
http://sdkdocs.roku.com/display/sdkdoc/Developer+Guide

Usage:

```
  -h, --hostname    Required. The hostname or IP address of the Roku Streaming
                    Player.
  -u, --username    (Default: rokudev) Do not set unless you are on very old
                    Roku firware that doesn't require local developer
                    authentication, in which case, use '-u none'.
  -p, --password    The password for the 'rokudev' local developer account to
                    access the Roku Streaming Player's Developer Application
                    Installer
  -z, --zipfile     Required. The local path to the Roku application packaged
                    as a zip file
  --help            Display this help screen.
```

Error Codes:

Returns DOS error code 0 for success, 1 for failure.

Examples:  

``` 
rokuloader -h 192.168.1.10 -p secret -z c:\rokudev\package.zip
rokuloader --hostname 192.168.1.10 --password secret --zipfile c:\package.zip
rokuloader -h myroku -p secret -z "c:\rokudev\project name\package.zip"
rokuloader -h 192.168.1.10 -u "none" -z "c:\project name\n1000version.zip"
```

Example Output:

```
Connecting to 192.168.1.173...
Uploading Snowflakes.zip...

Development Application Installer
Currently Installed Application:

414e18fc1c3568af3e563f125e42f1fc   dev.zip (784178 bytes)

Warning: Application size limits it to Roku 2 players, Roku XDS and Roku XR.
It will not work with other Roku 1 players.  (784178 > 768000)
```

