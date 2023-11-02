using System.Runtime.InteropServices;

namespace WinApi.GetActiveSession
{
    public class GetActiveSession
    {
        [DllImport("wtsapi32.dll")]
        static extern IntPtr WTSOpenServer([MarshalAs(UnmanagedType.LPStr)] string pServerName);

        [DllImport("wtsapi32.dll")]
        static extern void WTSCloseServer(IntPtr hServer);

        [DllImport("wtsapi32.dll")]
        static extern int WTSEnumerateSessions(
            IntPtr hServer,
            int Reserved,
            int Version,
            ref IntPtr ppSessionInfo,
            ref int pCount);

        [DllImport("wtsapi32.dll")]
        static extern void WTSFreeMemory(IntPtr pMemory);


        [DllImport("Wtsapi32.dll")]
        static extern bool WTSQuerySessionInformation(
        IntPtr hServer, int sessionId, WTS_INFO_CLASS wtsInfoClass, out IntPtr ppBuffer, out uint pBytesReturned);


        [DllImport("kernel32.dll")]
        public static extern int WTSGetActiveConsoleSessionId();

        //Get active session user name and machine name etc. 

        [StructLayout(LayoutKind.Sequential)]
        private struct WTS_SESSION_INFO
        {
            public int SessionID;

            [MarshalAs(UnmanagedType.LPStr)]
            public string pWinStationName;

            public WTS_CONNECTSTATE_CLASS State;
        }


        public enum WTS_INFO_CLASS
        {
            WTSInitialProgram,
            WTSApplicationName,
            WTSWorkingDirectory,
            WTSOEMId,
            WTSSessionId,
            WTSUserName,
            WTSWinStationName,
            WTSDomainName,
            WTSConnectState,
            WTSClientBuildNumber,
            WTSClientName,
            WTSClientDirectory,
            WTSClientProductId,
            WTSClientHardwareId,
            WTSClientAddress,
            WTSClientDisplay,
            WTSClientProtocolType
        }


        public enum WTS_CONNECTSTATE_CLASS
        {
            WTSActive, //Uzak masaüstünün, aktif oturum bilgisi vb. bilgiler alýnmasý için kullanýlýr.
            WTSConnected, //Uzak masaüstü oturumlarýn baðlantý durumunu verir. Oturuma baðlandýysa connected olarak dönmektedir.
            WTSConnectQuery, //Uzak masaüstüne baðlanma izini alýp almayacaðýný sorgulamak için kullanýlýr.
            WTSShadow, //Belirli bir kullanýcý tarafýndan baþlatýldýðýnda tetiklenir.
            WTSDisconnected, //Uzak masaüstü oturmunu kapandýðýnda disconnect olduðunda dönmektedir.
            WTSIdle, //Ilgili oturum idle'a düþüp düþmediði kontrol edilebilir.
            WTSListen, //Uzak masaüstü oturumlarýný dinler vb.
            WTSReset, //Uzak masaüstünün sýfýrlandýðý zaman döner. Oturumun yeniden baþlatýlmasý gerekmez. Oturum temizlenirken, kullanýcýnýn oturmunun düþmemesini saðlar. vb.
            WTSDown, // ?
            WTSInit // ?
        }


        public (string? UserName, int SessionId, int WTSSessionId) GetActiveSessionUserName(string ServerName)
        {
            IntPtr serverHandle = IntPtr.Zero;
            serverHandle = WTSOpenServer(ServerName); // ServerName = Environment.MachineName
            string? activeUsername = string.Empty;
            string? activeDomain = string.Empty;
            int activeSessionId = 0;

            try
            {
                IntPtr sessionInfoPtr = IntPtr.Zero;
                int sessionCount = 0;
                int retVal = WTSEnumerateSessions(serverHandle, 0, 1, ref sessionInfoPtr, ref sessionCount);
                int dataSize = Marshal.SizeOf(typeof(WTS_SESSION_INFO));
                IntPtr currentSession = sessionInfoPtr;
                uint bytes = 0;

                if (retVal != 0)
                {
                    for (int i = 0; i < sessionCount; i++)
                    {
                        WTS_SESSION_INFO si = (WTS_SESSION_INFO)Marshal.PtrToStructure(currentSession, typeof(WTS_SESSION_INFO));
                        currentSession += dataSize;

                        if (si.State == WTS_CONNECTSTATE_CLASS.WTSActive || si.State == WTS_CONNECTSTATE_CLASS.WTSConnected)
                        {
                            IntPtr userPtr, sessionPtr, domainPtr;
                            WTSQuerySessionInformation(serverHandle, si.SessionID, WTS_INFO_CLASS.WTSDomainName, out domainPtr, out bytes);
                            WTSQuerySessionInformation(serverHandle, si.SessionID, WTS_INFO_CLASS.WTSUserName, out userPtr, out bytes);
                            WTSQuerySessionInformation(serverHandle, si.SessionID, WTS_INFO_CLASS.WTSSessionId, out sessionPtr, out bytes);

                            activeUsername = Marshal.PtrToStringAnsi(userPtr);
                            activeSessionId = Marshal.ReadInt32(sessionPtr);

                            WTSFreeMemory(userPtr);
                            WTSFreeMemory(sessionPtr);
                            WTSFreeMemory(domainPtr);
                        }
                    }

                    WTSFreeMemory(sessionInfoPtr);
                }
            }
            finally
            {
                WTSCloseServer(serverHandle);
            }

            return (activeUsername, activeSessionId, WTSGetActiveConsoleSessionId());
        }

    }
}