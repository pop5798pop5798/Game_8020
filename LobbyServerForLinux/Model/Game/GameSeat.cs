using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LobbyServerForLinux.Model
{
    /// <summary>
    /// 遊戲座位
    /// </summary>
    public class GameSeat
    {
        public readonly int No = -1;                                                                        // 座位號碼.

        public Player SitPlayer { get { return m_player; } }
        private Player m_player = null;
        public string SitPlayer_SN { get { return m_player == null ? "" : m_player.SN; } }                  // 辨識碼
        public int SitPlayer_MemberID { get { return m_player == null ? 0 : m_player.MemberID; } }          // 唯一碼
        public string SitPlayer_Account { get { return m_player == null ? "" : m_player.Account; } }        // 帳號
        public string SitPlayer_Nickname { get { return m_player == null ? "" : m_player.Nickname; } }      // 暱稱
        public decimal SitPlayer_StartMoney { get { return m_startMoney; } }            // 初始餘額
        private decimal m_startMoney = 0;
        public decimal SitPlayer_Money { get { return m_player == null ? 0 : m_player.Money; } }            // 餘額
        public string SitPlayer_LoginIP { get { return m_player == null ? "" : m_player.LoginIP; } }        // 登入IP
        public int SitPlayer_ChangeID { get { return m_player == null ? 0 : m_player.ChangeID; } }          // 驗證碼

        private Player bp_player = null;
        public Player BackupPlayer { get { return bp_player; } }
        public void SetBackupPlayer()
        {
            bp_player = m_player;
        }

        public PokersSup SuperPoker { get; set; }

        public decimal startSeatMoney { get; set; } //座位用初始假金額

        public decimal seatMoney { get; set; } //座位用的假金額(給客端)


        public bool IsRobot { get { return m_isRobot; } }
        private bool m_isRobot = false;          // 是否為機器人.


        public int[] HandPokersByID
        {
            get
            {
                // 回傳空牌.
                if (m_handPokers.Count == 0) return new int[0];

                int[] cards = new int[m_handPokers.Count];
                List<Poker> order = m_handPokers.OrderBy(x => x.Number).ToList();
                for (int i = 0; i < order.Count; i++)
                    cards[i] = order[i].ID;

                return cards;
            }
        }



        public List<Poker> HandPokers { get { return m_handPokers; } }
        private List<Poker> m_handPokers = new List<Poker>();       // 所有手上的牌.

        //手牌依權重排序
        public List<Poker> HandPokersByPower { get {
                return m_handPokers.OrderByDescending(x => x.Power).ToList();                      
        } }

        //手牌資訊轉文字
        public string HandPokersToString { get {
                string info = "";

                for(int i = 0;i < HandPokers.Count;i++)
                {
                    info += HandPokers[i].CardTypeToString + HandPokers[i].Number;
                    if (i == (HandPokers.Count - 1)) { }
                    else {
                        info += ",";
                    }
                }
                return info;              
            } 
        }

        //卡片分數總合
        public int allPoints {
            get
            {
                int points = 0;
                for (int i = 0; i < HandPokers.Count; i++)
                {
                    points += HandPokers[i].Score;
                }
                return points;

            }

        }

        public decimal FollowMoney { get { return m_followMoney; } }
        private decimal m_followMoney = 0;                         //跟牌金額

        public decimal BeFollowMoney { get { return m_befollowMoney; } }
        private decimal m_befollowMoney = 0;                         //被跟牌金額

        //跟牌總合
        public decimal allFollow {
            get {

                return m_followMoney - m_befollowMoney;
            }
        
        }


        public List<Poker> beforePoker { get { return m_beforePoker; } }
        private List<Poker> m_beforePoker = new List<Poker>();       // 上回合出過的牌.

        public int Status { get { return m_status; } }
        private int m_status = SeatStatus.None;     // 目前狀態.





        public decimal Bets { get { return m_bets; } }
        private decimal m_bets = 0;               // 目前下注額.

        public decimal Valid
        {
            get { return m_valid; }
            set { m_valid = Math.Abs(value); }
        }
        private decimal m_valid = 0;                // 有效投注

        public decimal Wins { get { return m_wins; } }
        private decimal m_wins = 0;                 // 贏得.

        public decimal OverMoney { get { return m_overMoney; } }
        private decimal m_overMoney = 0;                 // 結束時贏得 (不加公點、跟牌).

        public decimal OldMoney { get { return m_oldMoney; } }
        private decimal m_oldMoney = 0;                 // 結束時(不加公點) 跟牌+贏得overMoney.


        public decimal Com { get { return m_com; } }
        private decimal m_com = 0;                 // 公點.


        // 分潤
        public decimal Divided
        {
            get { return m_divided; }
        }
        private decimal m_divided = 0;
        public void addDivided(decimal point)
        {
            m_divided = point;
        }



        // 本局輸贏.
        public decimal WinLose
        {
            get
            {
                return m_wins;
            }
        }

        public bool isOrder = false;
        public List<int> OrderCards = null;    

        /// <summary>
        /// 
        /// </summary>
        /// <param name="no"></param>
        public GameSeat(int no)
        {
            No = no;
        }

        public void SitStartMoney(decimal money) {
            seatMoney = money;
            startSeatMoney = money;
        }

        /// <summary>
        /// 玩家入座.
        /// </summary>
        /// <param name="player"></param>
        public void Sit(Player player)
        {
            m_player = player;
            m_isRobot = (player is Robot);
            m_startMoney = player.Money;
        }


        //加入公點
        public void addCom(decimal com)
        {
            m_com = com;
        }

        public void upCom(decimal com)
        {
            m_com += com;
        }

        /// <summary>
        /// 玩家離座.
        /// </summary>
        public void Leave()
        {
            m_player = null;
            m_isRobot = false;
            if (IsRobot)
            {
                Robot robot = ((Robot)m_player);
                robot.SitAI(1);
                robot.SitopenCard();
            }
        }
        
        /// <summary>
        /// 新回合開始.
        /// </summary>
        /// <param name="chip">初次注額(底注).</param>
        /// <returns>實際下注額</returns>
        public void NewRound()
        {
            m_status = SeatStatus.Play;
            //return BetChip(chip);   // 先下底注.
        }

        /// <summary>
        /// 發牌補牌
        /// </summary>
        public void Deal(Poker poker)
        {
            m_handPokers.Add(poker);
        }

        /// <summary>
        /// 設置跟牌金額
        /// </summary>
        public void SitFollowMoney(decimal money)
        {
            m_followMoney += money;
           
        }

        /// <summary>
        /// 設置被跟牌金額
        /// </summary>
        public void SitBeFollowMoney(decimal money)
        {
            m_befollowMoney += money;
        }

        /// <summary>
        /// 設置結束的金額 不加跟牌.
        /// </summary>
        /// <param name="money"></param>
        public void SitOverMoney(decimal money)
        {
            // 結束時的原始金額.
            m_overMoney = money;
        }


        /// <summary>
        /// 出牌.(更新手牌)
        /// </summary>
        public void OutCard(int ID)
        {
            m_handPokers = m_handPokers.Where(x => x.ID != ID).ToList();
            m_beforePoker.Add(new Poker(ID));
        }


        /// <summary>
        /// 設置&更新手牌
        /// </summary>
        public void SetHandCard(List<Poker> hand)
        {
            //List<Poker> hand = new List<Poker>();
            //hand.Add(new Poker(1));
            m_handPokers = hand;
        }

        /// <summary>
        /// 測試用.(更新手牌)
        /// </summary>
        public void TestCard()
        {
            List<Poker> test = new List<Poker>();
            test.Add(new Poker(1));
            m_handPokers = test;
            //m_followMoney = 0;
            //m_befollowMoney = 0;
        }

        /// <summary>
        /// 測試用.(跟牌去除)
        /// </summary>
        public void TestPoints()
        {
            m_followMoney = 0;
            m_befollowMoney = 0;
        }


        /// <summary>
        /// 清除出牌.
        /// </summary>
        public void ClearOutCard()
        {
            m_beforePoker.Clear();
        }

        /// <summary>
        /// 下注
        /// </summary>
        /// <param name="chip">下注額</param>
        /// <returns>實際下注額.</returns>
        public decimal BetChip(decimal chip)
        {
            // 判斷剩餘金額是否足夠.
            if (m_player.Money <= chip)
                chip = m_player.Money;

            // 下注.
            m_bets += chip;
            // 扣款.
            m_player.Money -= chip;

            return chip;
        }

        /// <summary>
        /// 設置輸贏狀態.
        /// </summary>
        /// <param name="money"></param>
        public void SitEndState(int state)
        {
            m_status = state;
        }

       

        /// <summary>
        /// 計算總輸贏.
        /// </summary>
        /// <param name="money"></param>
        public void SitWinLose(decimal money,int state)
        {
            m_status = state;
            // 結算.
            m_wins = money;
            m_player.Money += money;
            seatMoney += money;
        }

        public void UpWinLose(decimal money, decimal point)
        {
            m_wins = money;
            m_player.Money += point;
            seatMoney += point;
        }

        /// <summary>
        /// 設置未扣公點金額
        /// </summary>
        /// <param name="money"></param>
        public void SitOldMoney(decimal money)
        {
            m_oldMoney = money;
        }

        /// <summary>
        /// 加入額多獎金.
        /// </summary>
        public void AddBonus(decimal money)
        {
            m_player.Money += Math.Abs(money);
        }


        /// <summary>
        /// 座位重置.
        /// </summary>
        public void Reset()
        {
            m_status = SeatStatus.None;

            isOrder = false;
            OrderCards = null;
            seatMoney = 0;

            m_handPokers.Clear();
            bp_player = null;

            m_divided = 0;
            m_bets = 0;
            m_wins = 0;
            m_followMoney = 0;
            m_befollowMoney = 0;
            m_overMoney = 0;
            m_startMoney = 0;
            m_oldMoney = 0;
            startSeatMoney = 0;
            SuperPoker = null;
            m_beforePoker.Clear();

        }
        //test清除手牌
        public void ClearData() 
        {
            m_handPokers.Clear();
        }
        public void RobotReset()
        {
            m_player = null;
        }
       
    }
}
