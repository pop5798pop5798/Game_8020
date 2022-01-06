using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LobbyServerForLinux.Model
{
    /// <summary>
    /// 牌肯牌.
    /// </summary>
    public class Poker
    {
        public readonly int ID;
        // 牌卡分數.()
        public readonly int Score;

        // 牌卡數值.()
        public readonly int Number;

        // 特卡牌強度.()
        public readonly int Power;

        // 花色.(1:方塊, 2:梅花, 3:紅心, 4:黑桃 
        public readonly int CardType;

        //顏色
        public readonly int Color;

        public string CardTypeToString {
            get {
                string name = "";
                switch(CardType)
                {
                    case 1:
                        name = "梅花";
                        break;
                    case 2:
                        name = "方塊";
                        break;
                    case 3:
                        name = "紅心";
                        break;
                    case 4:
                        name = "黑桃";
                        break;
                }
                return name;
            
            }
        
        }


        //牌組辨認
        public Poker(int id)
        {
            ID = id;
            decimal c = (decimal)id / 13;
            int type = Convert.ToInt32(Math.Ceiling(c)); //取大於指定數字的最小數值
            int color = (type % 4 == 0) ? 4 : type % 4; //取商數 (顏色 1:方塊, 2:梅花, 3:紅心, 4:黑桃 )

            int _num = (id % 13 == 0) ? 13 : id % 13;

            int score = (_num > 10) ? 10 : _num; //取餘數 (數字10以上都10分 )

            Color = color;
            Number = _num;
            Power = (_num - 1 == 0) ? 13 : _num - 1;
            Score = score;

            CardType = type;
        }

        ////////////////////////////////////////////////////////////
        /////   [Static] 撲克牌相關操作.
        ////////////////////////////////////////////////////////////
        /// <summary>
        /// 輸出字串.
        /// </summary>
        /// <param name="pokers"></param>
        /// <param name="joinChar"></param>
        /// <returns></returns>
        public static string ToString(Poker[] pokers, char joinChar)
        {
            if (pokers == null) return "";

            string strResult = "";
            for (int i = 0; i < pokers.Length; i++)
            {
                // 前面有資料時,加連接字以示區隔.
                if (strResult != "") strResult += joinChar;

                // 資料輸出.
                strResult += pokers[i].ID.ToString();
            }

            return strResult;
        }

        /// <summary>
        /// 洗牌.
        /// </summary>
        /// <param name="pokers">想洗的牌卡</param>
        /// <param name="number"></param>
        /// <returns>洗完後之牌卡.(同原牌卡,但位置不同)</returns>
        public static Poker[] Shuffle(Poker[] pokers, int number)
        {
            // List<Poker> cards = new List<Poker>(pokers.Length);
            //重置牌
            for (int i = 0; i < 52; i++) pokers[i] = new Poker(i + 1);


            List<Poker> cards = new List<Poker>();
            List<Poker> _cards = new List<Poker>();
            try
            {
                if (pokers == null) return null;

                Random rand = new Random();
                Poker zTemp = null;
                int cut = 0, r = 0;

                // step: 先切牌.
                cut = rand.Next(pokers.Length);                 // 隨機取一值, 切割兩邊牌.

                for (int i = 0; i < pokers.Length; i++)
                {
                    if ((cut + i) >= pokers.Length) cut = -i;  // 發到底了, 重0計數.
                    cards.Add(pokers[cut + i]);
                }

                // step: 洗亂. 
                for (int n = 0; n < number; n++)
                    for (int i = 0; i < pokers.Length; i++)
                    {
                        r = rand.Next(pokers.Length);
                        zTemp = cards[r];
                        cards[r] = cards[i];
                        cards[i] = zTemp;
                    }


            }
            catch (Exception ex)
            {
                Program.WriteLog("TryCatch", ",Shuffle Err: " + ex);
            }
            //找出重復值 測試log
            /*var data = (from a in cards
                        group a by a.ID into g
                        where g.Count() > 1
                        select g.Key).ToList();
            if (data.Count > 0) Program.WriteLog("Card_Repeat", ",Repeat01: " + JsonConvert.SerializeObject(data));*/

            return cards.ToArray();

        }


        /// <summary>
        /// 指定換牌.
        /// </summary>
        /// <param name="pokers">原始牌卡</param>
        /// <param name="seat">更換的座位號</param>
        /// <param name="Pokers_change">想更換的牌</param>
        /// <returns>牌卡移至之指定位置</returns>
        public static Poker[] ShuffleOrder(Poker[] pokers,int seat, Poker[] Pokers_change)
        {
            Poker[] cards = pokers;
            try
            {
                if (pokers == null) return null;

                int start = seat * 5;
                int total = 5 * (seat + 1);
                int j = 0;
                for(int i = start;i < total; i++)
                {                 
                    Poker temp = pokers[i];
                    int idx = -1;

                    for (int p = 0; p < pokers.Length; p++)
                    {
                        if (pokers[p].ID == Pokers_change[j].ID)
                        {
                            idx = p;
                            break;
                        }
                    }

                    pokers[i] = Pokers_change[j];

                    pokers[idx] = temp;


                    j++;
                }
                cards = pokers;

            }
            catch (Exception ex)
            {
                Program.WriteLog("TryCatch", ",ShuffleOrder Err: " + ex);
            }

            return cards;

        }


        /// <summary>
        /// 玩家贏 取得最低牌組.
        /// </summary>
        /// <param name="pokers">原始牌卡</param>
        /// <param name="seat">必贏玩家</param>
        /// <returns>最低牌卡移至之必贏玩家位置</returns>
        public static Poker[] PlayerWin(Poker[] pokers, int seat)
        {
            Poker[] cards = pokers;

            List<PokerSeat> ps = new List<PokerSeat>();

            for(int i = 0;i < 5;i++)
            {
                int start = i * 5;
                int total = 5 * (i + 1);
                PokerSeat p = new PokerSeat();
                p.Pokers = new List<Poker>();
                for(int j = start;j < total;j++)
                {
                    p.Pokers.Add(pokers[j]);
                }

                p.seat = i;
                ps.Add(p);
            }

            var rankList = ps.OrderBy(x => x.Pokers.Sum(s => s.Score))
                        .OrderByDescending(x => x.supType.Type)
                        .ToList();
            var rank = rankList.FirstOrDefault();

            bool isSupSame = ps.Where(x=> x.seat != rank.seat).Any(x => x.supType.Type == rank.supType.Type);

            if(isSupSame)
            {
                rank = rankList.Where(x=>x.supType.Type == rank.supType.Type)
                    .OrderByDescending(x => x.supType.Power)
                    .FirstOrDefault();
            }

            int count = 0;
            for(int i = rank.seat * 5; i < 5 * (rank.seat + 1);i++)
            {
                Poker temp = pokers[seat * 5 + count];
                pokers[seat * 5 + count] = pokers[i];
                pokers[i] = temp;
                count++;

            }

            cards = pokers;



            return cards;
        }

        /// <summary>
        /// 玩家輸 並給機器人的牌組.
        /// </summary>
        /// <param name="pokers">原始牌卡</param>
        /// <param name="seat">必贏玩家</param>
        /// <param name="num">第幾組小</param>
        /// <param name="isGetSup">是否給於特殊卡</param>
        /// <returns>最低牌卡移至之必贏玩家位置</returns>
        public static Poker[] RobotWin(Poker[] pokers, int seat)
        {
            Poker[] cards = pokers;

            List<PokerSeat> ps = new List<PokerSeat>();

            for (int i = 0; i < 5; i++)
            {
                int start = i * 5;
                int total = 5 * (i + 1);
                PokerSeat p = new PokerSeat();
                p.Pokers = new List<Poker>();
                for (int j = start; j < total; j++)
                {
                    p.Pokers.Add(pokers[j]);
                }

                p.seat = i;
                ps.Add(p);
            }

            var rankList = ps.OrderBy(x => x.Pokers.Sum(s => s.Score))
                        .OrderByDescending(x => x.supType.Type)
                        .ToList();
            var rank = rankList.FirstOrDefault();

            bool isSupSame = ps.Where(x => x.seat != rank.seat).Any(x => x.supType.Type == rank.supType.Type);

            if (isSupSame)
            {
                rank = rankList.Where(x => x.supType.Type == rank.supType.Type)
                    .OrderByDescending(x => x.supType.Power)
                    .FirstOrDefault();
            }

            int count = 0;
            for (int i = rank.seat * 5; i < 5 * (rank.seat + 1); i++)
            {
                Poker temp = pokers[seat * 5 + count];
                pokers[seat * 5 + count] = pokers[i];
                pokers[i] = temp;
                count++;

            }

            cards = pokers;



            return cards;
        }

        /// <summary>
        /// 玩家輸 並給機器人的牌組.
        /// </summary>
        /// <param name="pokers">原始牌卡</param>
        /// <param name="seat">必贏玩家</param>
        /// <param name="num">第幾組小</param>
        /// <param name="isGetSup">是否給於特殊卡</param>
        /// <returns>第二低牌卡移至之玩家位置</returns>
        public static Poker[] PlayerLose(Poker[] pokers, int seat)
        {
            Poker[] cards = pokers;

            List<PokerSeat> ps = new List<PokerSeat>();

            for (int i = 0; i < 5; i++)
            {
                int start = i * 5;
                int total = 5 * (i + 1);
                PokerSeat p = new PokerSeat();
                p.Pokers = new List<Poker>();
                for (int j = start; j < total; j++)
                {
                    p.Pokers.Add(pokers[j]);
                }

                p.seat = i;
                ps.Add(p);
            }

            var rankList = ps.OrderBy(x => x.Pokers.Sum(s => s.Score))
                        .OrderByDescending(x => x.supType.Type)
                        .ToList();
            var rank = rankList[1];

            bool isSupSame = ps.Where(x => x.seat != rank.seat).Any(x => x.supType.Type == rank.supType.Type);

            if (isSupSame)
            {
                var same = rankList.Where(x => x.supType.Type == rank.supType.Type)
                    .OrderByDescending(x => x.supType.Power).ToList();
                rank = same[1];
            }

            int count = 0;
            for (int i = rank.seat * 5; i < 5 * (rank.seat + 1); i++)
            {
                Poker temp = pokers[seat * 5 + count];
                pokers[seat * 5 + count] = pokers[i];
                pokers[i] = temp;
                count++;

            }

            cards = pokers;



            return cards;
        }




    }

    //卡牌點數差距
    public class PokerGap
    {
        public int PokerNum { get; set; }
        public int PokerID { get; set; }

        public int gap { get; set; }
    
    }

    //座位點數
    public class PokerSeat
    {
        public int seat { get; set; }
        public List<Poker> Pokers { get; set; }

        public int allPoint { 
            get {
                return Pokers.Sum(x => x.Score);
            } 
        }

        public PokersSup supType
        {
            get
            {
                return new PokersSup(Pokers);
            }
        }

    }


    //手牌相同點數
    public class HandPokerSame
    {
        public int CardNum { get; set; }
        public int AllScore { get; set; }

    }


    public class PokersSup
    {
        // 卡牌代號
        public readonly int Type;
        // 卡牌強度.
        public int Power;

        //牌型判斷
        public PokersSup(List<Poker> pokers) 
        {
            List<int> TList = new List<int>();
            List<int> PList = new List<int>();

            //三條
            var cardRepair = pokers.GroupBy(x => x.Power).
                Select(x => new { CardID = x.Key, Counter = x.Count() }).ToList();
            int pokerNumber = 0;
            if (cardRepair.Any(x=>x.Counter > 2))
            {
                pokerNumber = cardRepair.Where(x => x.Counter > 2).FirstOrDefault().CardID;
                TList.Add(PokerType.Three);
                Power = pokerNumber;
            }


            //順子
            List<Poker> cardSort = pokers.OrderBy(x=>x.Number).ToList();
         
            int first = cardSort[0].Number;
            int sort_j = 0;
            //先偵測是否有1-5
            /*for (int i = 0; i < 5; i++)
            {
                bool exitCard = pokers.Any(x => x.Number == (i+1));
                if (exitCard) sort_j++;
            }*/
            /*if (sort_j == 5)
            {
                //Type = PokerType.Straight;
                TList.Add(PokerType.Straight);
                PList.Add(9);
            }*/

            //var test = TList.Any(x => x == 2);
            //先偵測是否有A-K最大順
            bool isAK = false;
            int RoyalCount = 0;
            for (int i = 10; i < 15; i++)
            {
                int num = (i == 14) ? 1 : i;
                bool exit = cardSort.Any(x => x.Number == num);
                if (exit) RoyalCount += 1;
            }
            if (RoyalCount == 5)
            {
                TList.Add(PokerType.Straight);
                Power = 14;
                isAK = true;
            } 
            //其他順
            if (first < 10 && !isAK)
            {
                sort_j = 0;
                for (int i = 0; i < 5; i++)
                {
                    int cardnum = first + sort_j;
                    bool exitCard = pokers.Any(x => x.Number == first + sort_j);
                    if (exitCard) sort_j++;
                }
                if (sort_j == 5)
                {
                    TList.Add(PokerType.Straight);
                    Power = first;
                }
            }


            //同花
            List<Poker> Cardsame = pokers.Where(x => x.Color == pokers[0].Color).ToList();
            List<Poker> CardPSort = pokers.OrderBy(x => x.Power).ToList();
            if (Cardsame.Count == 5)
            {
                TList.Add(PokerType.Flush);
                Power =  0;
            }

            //葫蘆
            if(cardRepair.Any(x => x.Counter > 2) && cardRepair.Any(x => x.Counter == 2))
            {
                TList.Add(PokerType.Full);
                Power = pokerNumber;
            }

            //四條
            if (cardRepair.Any(x => x.Counter > 3))
            {
                TList.Add(PokerType.Four);
                Power = pokerNumber;
            }

            //同花順
            if(Cardsame.Count == 5 && TList.Any(x=>x == 2))
            {
                TList.Add(PokerType.StraightF);
                Power = first;

            }

            //皇家同花順
            bool isRoyal = false;
            if (isAK && Cardsame.Count == 5) isRoyal = true;
           
            if (isRoyal)
            {
                TList.Add(PokerType.Royal);
                Power = 15;
            }

            Type = TList.LastOrDefault();

        }

        //同花PK判斷
        public static List<int> FlushPK(List<GameSeat> seats)
        {
            List<GameSeat> allseat = seats;
            List<int> bigPoints = new List<int>();
            GameSeat bigPower = null;
            List<int> bigSeat = new List<int>();

            for (int i = 0;i < 5;i++)
            {
                bigPower = allseat.OrderByDescending(x => x.HandPokersByPower[i].Power).FirstOrDefault();
                var samePower = allseat.Where(x => x.HandPokersByPower[i].Power == bigPower.HandPokersByPower[i].Power).ToList();

                if (samePower.Count > 1)
                {
                    allseat = new List<GameSeat>();
                    allseat = samePower;
                    if(i == 4)
                    {
                        foreach(var seat in allseat)
                        {
                            bigSeat.Add(seat.SitPlayer.RoomSeat);
                        }
                    }
                }
                else {
                    bigSeat.Add(bigPower.SitPlayer.RoomSeat);
                    break;               
                }              

            }

            return bigSeat;

        }
        //同花比牌
        /*public static List<int> FlushPK(GameSeat first, GameSeat second) {
            List<int> pkresult = new List<int>();

            List<Poker> fristOrderby = first.HandPokers.OrderByDescending(x=>x.Power).ToList();
            List<Poker> secondOrderby = second.HandPokers.OrderByDescending(x => x.Power).ToList();

            for(int i = 0;i < first.HandPokers.Count;i++)
            {
                if (fristOrderby[i].Power == secondOrderby[i].Power) continue;
                if (fristOrderby[i].Power < secondOrderby[i].Power)
                {
                    pkresult.Add(second.SitPlayer.RoomSeat);
                    break;
                }
                else {
                    pkresult.Add(first.SitPlayer.RoomSeat);
                    break;
                }

            }

            if(pkresult.Count == 0) //都相同
            {
                pkresult.Add(second.SitPlayer.RoomSeat);
                pkresult.Add(first.SitPlayer.RoomSeat);
            }


            return pkresult;

        }*/



    }


}
