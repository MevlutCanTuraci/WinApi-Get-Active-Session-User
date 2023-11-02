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
            WTSActive, //Uzak masa�st�n�n, aktif oturum bilgisi vb. bilgiler al�nmas� i�in kullan�l�r.
            WTSConnected, //Uzak masa�st� oturumlar�n ba�lant� durumunu verir. Oturuma ba�land�ysa connected olarak d�nmektedir.
            WTSConnectQuery, //Uzak masa�st�ne ba�lanma izini al�p almayaca��n� sorgulamak i�in kullan�l�r.
            WTSShadow, //Belirli bir kullan�c� taraf�ndan ba�lat�ld���nda tetiklenir.
            WTSDisconnected, //Uzak masa�st� oturmunu kapand���nda disconnect oldu�unda d�nmektedir.
            WTSIdle, //Ilgili oturum idle'a d���p d��medi�i kontrol edilebilir.
            WTSListen, //Uzak masa�st� oturumlar�n� dinler vb.
            WTSReset, //Uzak masa�st�n�n s�f�rland��� zaman d�ner. Oturumun yeniden ba�lat�lmas� gerekmez. Oturum temizlenirken, kullan�c�n�n oturmunun d��memesini sa�lar. vb.
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