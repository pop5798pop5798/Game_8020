using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LobbyServerForLinux.Model
{
    /// <summary>
    /// 陪打機器人
    /// </summary>
    public class Robot : Player
    {
        public bool EnableAction { get { return m_enableAction; } } // 機器人動作致能
        private bool m_enableAction = false;

        public int AI_level { get { return m_aiLevel; } }
        private int m_aiLevel = -1;          // AI等級 0:開牌者 1:一般AI 2:指標

        public bool isWin { get { return m_Win; } }
        private bool m_Win = false;          // 是否要贏

        public bool openCard { get { return m_openCard; } }
        private bool m_openCard = false;          // 是否為開牌者

        public int openRound { get { return m_openRound; } }
        private int m_openRound = 4;          // 第幾回合開牌

        private DateTime m_thinkTime = DateTime.Now;                // 思考時間

        private Random rand = new Random(Guid.NewGuid().GetHashCode());

        private List<double> beforeTime = new List<double>();


        public Robot(string sn, int companyid, int systemid, int memberId, int webid, int groupid, string account) : base(sn, companyid, systemid, memberId, webid, groupid, account, "")
        {

        }
        /// <summary>
        /// 設置Ai
        /// </summary>
        /// <param name="level">AI等級</param>
        /// <param name="type">輸贏狀態</param>
        public void SitAI(int level,bool type = false,int openRound = 0)
        {
            m_aiLevel = level;
            m_Win = type;
            //m_openRound = round;
            if(openRound == 0) randOpenRound();
            else m_openRound = openRound;
        }

        /// <summary>
        /// 設置開牌者
        /// </summary>
        public void SitopenCard(bool type = false)
        {
            m_openCard = type;
        }

        /// <summary>
        /// 隨機設置AI開牌回合
        /// </summary>
        private void randOpenRound() 
        {
            int _rand = rand.Next(1, 5);
            if (_rand > 1 && _rand < 4)
            {
                _rand = 2;
            }
            else if (_rand >= 4)
            {
                _rand = 3;
            }

            if (_rand == 1)
            {
                int choice = rand.Next(0, 3);
                if (choice > 0)
                {
                    _rand = rand.Next(2, 5);
                    if (_rand > 1 && _rand < 4)
                    {
                        _rand = 2;
                    }
                    else if (_rand >= 4)
                    {
                        _rand = 3;
                    }

                }
            }
            m_openRound = _rand;

        }



        /// <summary>
        /// 開始思考.(動作致能)
        /// </summary>
        /// <param name="time">思考最大時間</param>
        public void StartThink(double time)
        {
            m_enableAction = true;
            double delay = filterRand();

            // 時間重設.
            m_thinkTime = DateTime.Now.AddSeconds(delay);
        }

        /// <summary>
        /// 過濾思考
        /// </summary>
        private double filterRand() {
            //int i = 0;
            double delay = 0;
            double point = rand.NextDouble();



            /*while (i == 0)
            {
                delay = rand.Next(0, 2);
                //delay += point;
                if (beforeTime.Count > 0)
                {
                    if (delay == beforeTime[0]) { }
                    else i++;
                }
                else i++;
            }*/
            if (beforeTime.Count > 0)
            {
                delay = (0 == beforeTime[0]) ? 1 : 0;
                beforeTime.Clear();
            } 
            else
            {
                delay = rand.Next(0, 2);
                beforeTime.Add(delay);
            }
            

            //delay += point;
            if(delay < 0.4) delay = 0.5;

            return delay + point;

        }

        /// <summary>
        /// 檢查思考時間.
        /// </summary>
        /// <returns>true:時間到, false:時間未到.</returns>
        public bool CheckThinkTime()
        {
            return (DateTime.Compare(DateTime.Now, m_thinkTime) >= 0);
        }

        /// <summary>
        /// 關閉機器人動作致能.
        /// </summary>
        public void DisableAction()
        {
            m_enableAction = false;
        }
    }
}
