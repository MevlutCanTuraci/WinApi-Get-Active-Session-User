# How to using the class?
```cs
GetActiveSession activeSession = new GetActiveSession();
var activeSessionInfo = activeSession.GetActiveSessionUserName
(
  ServerName: Environment.MachineName
);

``` 

> Return values;
```cs
activeSessionInfo.UserName;       // ->   Getting active session in user name. (User) etc.
activeSessionInfo.SessionId;     // ->   Getting active session id. (4)
activeSessionInfo.WTSSessionId; // ->   Getting active session id. (4)


``` 
