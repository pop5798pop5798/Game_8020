using System;
using MhCore.Game;

namespace LobbyServerForLinux.Model
{
    public enum PlayerState
    {
        Lobby,
        Waiting,
        Gaming,
        Result,
        Disconnect,
    }

    /// <summary>
    /// 遊戲玩家.
    /// </summary>
    public class Player : GameUser
    {
        public readonly int MemberID = 0;           // 會員序號.
        public readonly string Account = "";        // 會員帳號.
        public readonly string LoginIP = "";        // 玩家登入IP.

        public readonly int company_id = 0;         // 對應的公司ID
        public readonly int system_id = 0;          // 對應的系統ID
        public readonly int web_id = 0;             // 對應的站台ID
        public readonly int group_id = 0;       	// 玩家群組，0:代表不可一起玩，其餘數字相同者可一起玩

        public decimal Money { get; set; }          // 遊戲餘額.
        public int ChangeID { get; set; }           // 異動ID.
        public int AvatarID = 0;                    // 頭像編號 (0: 隨機)
        public int UserBlack = 0;                   // 黑名單編號，1:正常、2:黑名單
        public int InnerChannel = 0;                // 伺服器內部分流
        public int WaitTime = 0;                    // 等待時間
        public double SrvPingTime { get; set; }     // 該玩家與Srv的Ping
        public string Nickname { get; set; }        // 暱稱
        public string roundId { get; set; }         // 局將號
        public string SessionId = "";               // 開分號
        public PlayerState State = PlayerState.Lobby;

        public decimal[] WinLose = new decimal[3] { 0, 0, 0 };         // 總輸贏
        public decimal[] BonusPoint = new decimal[3] { 0, 0, 0 };      // 寶箱累積額
        public decimal[] DesignatedBonus = new decimal[3] { 0, 0, 0 }; // 指定中獎

        public bool KickAfterFinish = false;        // 遊戲結算完成踢離玩家
        public bool KickOutServer = false;          // 被踢的玩家設true，同一場的玩家設false
        public bool KickByLine = false;             // 踢線
        public bool IsGetPoint = false;
        public bool IsGetLobbyInfo = false;
        public bool IsForceCatchRobot = false;      // 是否強取機器人
        public System.Threading.Timer ThreadIdx = null;

        public int RoomNo { get { return m_roomNo; } }
        private int m_roomNo = -1;                  // 歸屬之遊戲房.(-1:大廳, 0:大廳等侯湊桌中, >0:入房)

        public int RoomSeat { get { return m_roomSeat; } }
        private int m_roomSeat = -1;                // 座位編號.

        public DateTime StartTime { get { return m_waitTime; } }    
        private DateTime m_waitTime = DateTime.Now; // (大廳等侯湊桌/入房) 起始時間.

        public bool IsDisconnect { 
            get { return m_isDisconnect; }
            set { m_isDisconnect = value; }
        }
        private bool m_isDisconnect = false;     // 是否斷線.

        public DateTime SystemCheckTime { get; set; }       // 系統檢查時間.
        
        public Player(string sn, int memberId, int companyid, int systemid, int webid, int groupid, string account, string loginIP) : base(sn)
        {
            MemberID = memberId;
            company_id = companyid;
            system_id = systemid;
            web_id = webid;
            group_id = groupid;
            Account = account;
            LoginIP = loginIP;
            SystemCheckTime = DateTime.Now;
        }

        /// <summary>
        /// 請求進入遊戲.(等侯湊桌)
        /// </summary>
        public void RequestPlayGame()
        {
            m_roomNo = 0;
            m_waitTime = DateTime.Now;
        }

        /// <summary>
        /// 取消湊桌
        /// </summary>
        public void CancelWait()
        {
            m_roomNo = -1;
            m_waitTime = DateTime.Now;
        }

        /// <summary>
        /// 進入遊戲房.
        /// </summary>
        /// <param name="no">房號</param>
        /// <param name="seat">座位編號</param>
        public void EnterRoom(int no, int seat)
        {
            m_roomNo = no;
            m_roomSeat = seat;
            m_waitTime = DateTime.Now;
        }

        /// <summary>
        /// 設置個人獎金資訊.
        /// </summary>
        public void SitBonus(int level, decimal winlose)
        {
            BonusPoint[level] += Math.Abs(winlose); //寶箱累積額
            WinLose[level] += winlose;  // 存入個人總輸贏

        }

        /// <summary>
        /// 離開遊戲房.
        /// </summary>
        public void LeaveRoom()
        {
            m_roomNo = -1;
            m_roomSeat = -1;
            m_waitTime = DateTime.Now;
        }
    }
}
