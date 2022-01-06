using MhCore.DB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LobbyServerForLinux.Model
{
    // 基本場景參數.
    class GameScene
    {
        public GameSeat[] seats { get; set; } = new GameSeat[5];       // 座位.(5人座)
        public  decimal Ante { get; set; } = 0;                   // 底注.
        public  int GameArea { get; set; } = 0;                   // 遊戲房級別.

        public decimal[] BonusTemp { get; set; } = new decimal[] { 0, 0, 0, 0, 0 }; // 抽獎金額
        public decimal BonusLimit { get; set; } = 0;                         // 抽獎資格限制

        public int actionType { get; set; } = -1;//目前出牌type
        private List<GameSeat> allWin = new List<GameSeat>(); //全部贏家

        public List<int> throwCard { get; set; } = new List<int>(); //全部目前丟牌Index

        public List<int> recyclePokers { get; set; } = new List<int>(); //出過的牌堆
        public int Index { get; set; } = 0;                     // 當前發出第幾張

        public bool[] isReturnPoint { get; set; } = new bool[5];       // 紀錄是否已回點     

        public int aniTime { get; set; } = 0; //目前動畫時間
        public int drawPoker { get; set; } = 0;//補牌1張

        public int sitIndex { get; set; } = 0;                         // 入座索引            

        public bool isKickUser { get; set; } = false;                  // 取將號後是否剔除玩家
        public bool isForceKick { get; set; } = false;                 // 是否強踢
        public bool isPlaying { get; set; } = false;                   // 是否遊戲中.

        public bool isOneWin { get; set; } = false;                  //是否第一個贏家


        public string[] video { get; set; } = new string[5];           // 影片封包

        public string startInfo { get; set; } = "";                    // 起始時間資訊
        public string path_video { get; set; } = "";                     // 影片寫檔路徑

        public bool IsFeed { get; set; } = false;
        public int FeedRobotID { get; set; } = 0;
        public int FeedModel { get; set; } = 0;  //喂牌模式

        public int pos_banker { get; set; } = 0;                       // 莊家.(最初下注者)
        public int before_thinker { get; set; } = 0;                      // 上家思考玩家.
        public int pos_thinker { get; set; } = 0;                      // 目前思考玩家.

        public int current_step { get; set; } = 0;                //當前個人階段
        public int current_follow { get; set; } = -1;                //當前被跟牌玩家

        public bool isClick { get; set; } = false;

        public bool isSupPk { get; set; } = false;                   //直接特牌開牌


        public bool game_lock { get; set; } = false;                        //鎖住丟牌

        public string round_id { get; set; } = "";                     // 將局號.
        public int round_number { get; set; } = 0;                     // 該局玩家數.
        public int round_step { get; set; } = 0;                       // 已遊玩第幾輪.

        public int robot_step { get; set; } = 0;                       //機器人輪數

        public List<Poker> throwCardTemp { get; set; }                 //自動丟牌暫存

        public int[] throwCardTempID {
            get 
            {
                List<int> poker = new List<int>();
                if(throwCardTemp != null)
                {
                    foreach (var p in throwCardTemp)
                    {
                        poker.Add(p.ID);
                    }
                }
                
                return poker.ToArray();
            
            }
        
        }

        public decimal moneyPool { get; set; } = 0;                    // 彩池金額.

        public int pkSeat { get; set; } = -1;    //比牌座位

        
        public Poker[] pokers { get; set; } = new Poker[52];           // 遊戲用撲克牌.(52張 10以上為10分)
        public List<int> winner = new List<int>();                        //贏家
        public int[] ranks { get; set; } = new int[5];                                  //玩家排名
        public List<decimal> seatWinlose { get; set; } = new List<decimal>();      //玩家輸贏
        public double chance { get; set; } = 0;                        // 公點比

        public string path_start { get; set; } = "";                     // 起始Log路徑

        public string create_date { get; set; } = "";                  // 查帳時間

        public decimal LimitEnter { get; set; } = 0;             // 入場限制金額.


        public int nowScene { get; set; } = PaiKenScene.Wait;      // 目前場景.
        public DataServer_GetGameJiangHao jiang { get; set; } = null;  // 將號封包
        public DateTime nowTime { get; set; } = DateTime.Now;          // 目前時間節點.




        public DateTime centuryBegin { get; set; }                            // 播放起始時間
        public DateTime currentDate { get; set; }                              // 播放場景時間

        public int overScene { get; set; } = 0;                                    //遊戲結束方式

        public int overSup { get; set; } = 0;                                    //特牌結束牌型

        public int WinRoundType { get; set; } = 0;                                    //此局贏家 1:機器人 2:玩家

        public GameSeat winPlayerSeat { get; set; }                                  //必贏的玩家座位

        //---------------------場景時間Start-----------------------
        public double PlayDrawCard { get; set; } = 1; //場景時間>>>>>>丟牌抽牌

        public double Follow { get; set; } = 1.2; //場景時間>>>>>>跟牌

        public double PK { get; set; } = 2; //場景時間>>>>>>比牌
        //---------------------場景時間END------------------------

        public bool isOP { get; set; } = false;

        public bool notAiPK { get; set; } = false; //Ai不比牌

        public bool notSup { get; set; } = false; //特殊牌不比牌

        public Random random
        {
            get
            {
                return new Random(Guid.NewGuid().GetHashCode()); // 遊戲房內 共用亂數
            }
        }


        /// <summary>
        /// 重置場景.
        /// </summary>
        public void InitScene() 
        {
            startInfo = "";
            create_date = "";
            video = new string[5];
            path_video = "";

            path_start = "";
            Index = 0;
            recyclePokers.Clear();

            // 資訊重置.
            round_id = "";
            round_number = 0;
            round_step = 0;
            chance = 0;
            //卡片.
            throwCard = new List<int>();
            isClick = false;
            game_lock = false;
            current_step = 0;

            //喂牌資訊重置
            IsFeed = false;
            FeedRobotID = -1;

            // 桌面資訊重置.
            moneyPool = 0;
            pos_thinker = -1;

            // 遊戲房 重新轉為開放狀態.
            isPlaying = false;
            isKickUser = false;
            isForceKick = false;
            sitIndex = 0;

            winner = new List<int>();           //贏家
            ranks = new int[4];                  //玩家排名
            seatWinlose = new List<decimal>();      //玩家輸贏

            FeedModel = 0;

            isSupPk = false;

            //遊戲房 時間重置
            PlayDrawCard = 1; //丟牌抽牌
            Follow = 1.2; //跟牌
            PK = 2; //比牌

            overScene = 0;
            pkSeat = -1;
            overSup = 0;

            WinRoundType = 0;

            notAiPK = false; //Ai不比牌

            notSup = false; //特殊牌不比牌

            winPlayerSeat = null;
            isOneWin = false;
            allWin.Clear();
            isOP = false;
            BonusTemp = new decimal[] { 0, 0, 0, 0, 0 }; // 抽獎金額
            //seats = new GameSeat[5];


        }

        //場景加速
        public void SpeedUp() {
            PlayDrawCard = 0.3;
            Follow = 0.3;
            PK = 0.3;

        }


        /// <summary>
        /// 檢查是否可出牌.
        /// </summary>
        public bool CheckHandCard(Player player)
        {
            return false;
        }

        /// <summary>
        /// 清除牌相關資訊
        /// </summary>
        /// <returns></returns>
        public void ClearCard()
        {
            throwCard = new List<int>();
            drawPoker = 0;           
        }

        /// <summary>
        /// 丟牌&&跟牌
        /// </summary>
        /// <returns></returns>
        public void ThrowCard(Player _player,int type)
        {
            /*var res = throwCardTemp.GroupBy(x => x.ID)
              .Where(g => g.Count() > 1)
              .Select(y => y.Key)
              .ToList();

            if(res.Count != 0) Program.WriteLog("CardReplace", "ThrowCard round " + round_step + "Replace, Err: " + "重複ID牌");*/


            ClearCard();//清除丟牌資訊
            seats[_player.RoomSeat].ClearOutCard();//清除丟牌記錄
            foreach (var p in throwCardTemp)
            {
                seats[_player.RoomSeat].OutCard(p.ID); //更新手牌 --                                
                recyclePokers.Add(p.ID);//蒐集玩家出的牌 (牌堆)
                throwCard.Add(p.ID);
            }

            if (type == (int)Follow)//設置跟牌金額
            {
                decimal win = Ante * (decimal)0.5 * throwCard.Count();
                seats[current_follow].SitBeFollowMoney(win); //被跟牌

                seats[_player.RoomSeat].SitFollowMoney(win); //跟牌             
            }
            else 
            {
                current_follow = -1;//清除跟牌標記
            }

            actionType = type;
            aniTime = (type == (int)ActionType.Throw)?(int)PlayDrawCard: (int)Follow;
        }


        /// <summary>
        /// 補牌
        /// </summary>
        /// <param name="player">當前玩家</param>
        /// <param name="isChange">是否換牌</param>
        /// <param name="cardID">想換的卡ID號</param>
        /// <returns></returns>
        public void DrawCard(Player player,bool isChange = false,int cardID = 0)
        {
            
            if(isChange)
            {
                Poker temp = pokers[Index];
                for (int i = 0;i < pokers.Length;i++)
                {
                    if(pokers[i].ID == cardID)
                    {
                        pokers[i] = temp;
                        break;
                    }
                }

                pokers[Index] = new Poker(cardID);
            }

            drawPoker = pokers[Index].ID;
            seats[player.RoomSeat].Deal(pokers[Index]);//玩家的牌 ++
            Index++;
            current_follow = -1; //清除跟牌標記
            actionType = (int)ActionType.Draw;
            aniTime = (int)PlayDrawCard;
        }

        /// <summary>
        /// 比牌
        /// </summary>
        /// <returns></returns>
        public void PKCard()
        {
            actionType = (int)ActionType.PK;
            aniTime = (int)PK;
        }

        /// <summary>
        /// 是否可跟牌
        /// </summary>
        /// <returns></returns>
        public bool isTollow(Player player)
        {
            bool _isFollow = false;
            if (recyclePokers.Count != 0) //牌堆有牌
            {
                Poker before_Poker = seats[before_thinker].beforePoker.FirstOrDefault(); //上家出過的牌
                _isFollow = seats[player.RoomSeat].HandPokers.Any(x => x.Number == before_Poker.Number);
                if(_isFollow)
                {
                    if (current_follow == -1)
                    {
                        current_follow = before_thinker;
                    }
                }
                throwCardTemp = seats[player.RoomSeat].HandPokers.Where(x => x.Number == before_Poker.Number).ToList();
            }
            return _isFollow;
        }

        /// <summary>
        /// 設置可出的牌號暫存
        /// </summary>
        /// <param name="player">當前玩家</param>
        /// <param name="isTollow">是否跟牌</param>
        /// <returns></returns>
        public void ThrowPokerNum(Player player)
        {
            var handCard = seats[player.RoomSeat].HandPokers
                     .GroupBy(x => x.Number)
                     .Select(y => new { CardNum = y.Key, Counter = y.Count() }).ToList();
            if (handCard != null)
            {
                if (handCard.Count != 0)
                {
                    int bigPokerNum = handCard.OrderByDescending(x => x.Counter).OrderByDescending(x => x.CardNum).FirstOrDefault().CardNum;//手牌中最多最大的牌
                    int count = 0;
                    throwCardTemp = new List<Poker>();
                    for (int i = seats[player.RoomSeat].HandPokers.Count - 1; i >= 0;i--)
                    {
                        if (seats[player.RoomSeat].HandPokers[i].Number == bigPokerNum)
                        {
                            throwCardTemp.Add(seats[player.RoomSeat].HandPokers[i]);
                            count++;
                        }
                        
                    }

                }

            }

        }


        /// <summary>
        ///  [結算].
        /// </summary>
        public void Result() 
        {

            decimal winMoney = 0;
            decimal overMoney = 0;
            bool isHCardOver = seats.Any(x => x.HandPokers.Count == 0);
            

            //seats[0].SitFollowMoney(50);

            if (Index >= 52) //牌堆歸0>>>>>>>>>>>>>>>>>>
            {
                overScene = OverType.DeckOver;
                //測試多贏用
                /*for(int i = 0;i < 3;i++)
                {
                    seats[i].TestCard();
                }*/


                var winSeat = seats.OrderBy(x => x.allPoints).FirstOrDefault();
                var same = seats.Where(x => x.allPoints == winSeat.allPoints).ToList();
                if (same.Count > 1) allWin = same;
                else allWin.Add(winSeat);
                //輸家
                for (int i = 0; i < seats.Length; i++)
                {
                    if (allWin.Any(x => x.SitPlayer.RoomSeat == i)) continue;

                    decimal odds = 1;

                    decimal _lose = 0;
                    seats[i].SitOverMoney(-Ante * odds);
                    //多贏家Test
                    //seats[i].SitOverMoney(-14);//test
                    //seats[i].TestPoints();
                    _lose = seats[i].OverMoney;

                    SetLose(seats[i], _lose, seats[i].allFollow);
                }

                overMoney = Math.Abs(seats.Sum(x => x.OverMoney));//不加跟牌、公點>>金額

                

                foreach (var a in allWin)
                {                                      
                    decimal _winmoney = Unconditional(overMoney / allWin.Count());
                    seats[a.SitPlayer.RoomSeat].SitOverMoney(_winmoney);
                    //贏家
                    SetWin(seats[a.SitPlayer.RoomSeat], _winmoney, a.allFollow);
                }
                decimal comrem = 0;
                decimal wlrem = 0;
                decimal losepool = Math.Abs(seats.Where(x=>x.WinLose < 0).Sum(x => x.WinLose));//加跟牌>> 輸家金額
                //decimal winpool = Math.Abs(seats.Where(x => x.WinLose > 0).Sum(x => x.WinLose));//加跟牌>> 贏家金額
                decimal totalcom = Math.Abs(seats.Sum(x => x.Com));
                decimal totalwl = Math.Abs(seats.Sum(x => x.WinLose));
                decimal allcom = Unconditional(losepool * (decimal)(1 - chance)); //真公點

                comrem = allcom - totalcom;
                wlrem = totalwl - allcom;

                GameSeat firstWin = seats.Where(x => x.WinLose > 0).FirstOrDefault();
                if(firstWin != null)
                {
                    int seatNum = firstWin.SitPlayer.RoomSeat;
                    seats[seatNum].SitOldMoney(seats[seatNum].OldMoney + wlrem);
                    //設置狀態
                    seats[seatNum].UpWinLose(seats[seatNum].WinLose + wlrem, wlrem);
                    seats[seatNum].Valid += wlrem;//有效投注
                    seats[seatNum].upCom(comrem); //加入公點  
                }
                

                //Program.WriteLog("GameTie", ",Tie : " + round_id);//測試LOG
                //平手用
                //for (int i = 0; i < seats.Length; i++)
                //{
                //    SetTie(seats[i]);
                //}
            }
            else if(isHCardOver) //出完手牌>>>>>>>>>>>>>>>>> 
            {
                overScene = OverType.HCardOver;
                pkSeat = pos_thinker;

                for (int i = 0;i < seats.Length;i++) //先算輸家
                {
                    if (i == pos_thinker) continue;
                    decimal odds = 0;
                    //檢測是否有跟牌者>>>>>跟牌結束那個人 3倍 其他1倍 
                    if (current_follow != -1) odds = (current_follow == i) ? 3 : 1;
                    else odds = 3;


                    decimal _lose = 0;
                    seats[i].SitOverMoney(-Ante * odds);
                    _lose = seats[i].OverMoney;

                    SetLose(seats[i], _lose, seats[i].allFollow);


                }


                overMoney = Math.Abs(seats.Sum(x => x.OverMoney));//不加跟牌、公點>>金額
                winMoney = Math.Abs(seats.Sum(x => x.OldMoney));//加跟牌>>金額


                seats[pos_thinker].SitOverMoney(overMoney);
                //贏家
                SetWin(seats[pos_thinker], winMoney,0);
                



            }
            else if (actionType == (int)ActionType.PK && isSupPk)//特牌結算法>>>>>
            {
                overScene = OverType.SupOver;

                var winSeat = seats.OrderByDescending(x => x.SuperPoker.Type).FirstOrDefault();
                var same = seats.Where(x => x.SuperPoker.Type == winSeat.SuperPoker.Type).ToList();
                
                if (same.Count > 1)
                {
                    if(winSeat.SuperPoker.Type == PokerType.Flush)
                    {                
                       List<int> pk = PokersSup.FlushPK(same);
                       for(int i = 0;i < pk.Count;i++)
                        {
                            same.Where(x => x.SitPlayer.RoomSeat == pk[i]).FirstOrDefault().SuperPoker.Power = 1;
                        }

                    }

                    var firstSeat = same.OrderByDescending(x => x.SuperPoker.Power).FirstOrDefault();
                    var SamePower = same.Where(x => x.SuperPoker.Power == firstSeat.SuperPoker.Power).ToList();
                    if (SamePower.Count > 1)
                    {
                        allWin = same.Where(x=>x.SuperPoker.Power == winSeat.SuperPoker.Power).ToList();
                    }
                    else
                    {
                        allWin.Add(firstSeat);
                    }
                }
                else 
                {
                    allWin.Add(winSeat);
                
                }



                overSup = winSeat.SuperPoker.Type;
                //var allWin = seats.Where(x => x.SuperPoker.Power == winSeat.SuperPoker.Power).ToList();
                pkSeat = winSeat.SitPlayer.RoomSeat;

                for (int i = 0; i < seats.Length; i++)
                {
                    if (allWin.Any(x => x.SitPlayer.RoomSeat == i)) continue;

                    decimal odds = winSeat.SuperPoker.Type + 1;

                    decimal _lose = 0;
                    seats[i].SitOverMoney(-Ante * odds);
                    _lose = seats[i].OverMoney;

                    SetLose(seats[i], _lose, 0);

                    winMoney += Math.Abs(_lose);
                }
                overMoney = Math.Abs(seats.Sum(x => x.OverMoney));//不加跟牌、公點>>金額
                foreach (var a in allWin)
                {
                    decimal rem = 0;
                    if (!isOneWin)
                    {
                        decimal _win = Unconditional(overMoney * (decimal)chance);
                        decimal _com = Unconditional(_win / allWin.Count());
                        rem = _win - _com * allWin.Count();

                        isOneWin = true;
                    }

                    decimal _winmoney = Unconditional(winMoney / allWin.Count());
                    seats[a.SitPlayer.RoomSeat].SitOverMoney(_winmoney);
                    //贏家
                    SetWin(seats[a.SitPlayer.RoomSeat], _winmoney, 0);
                }

                decimal comrem = 0;
                decimal wlrem = 0;
                decimal losepool = Math.Abs(seats.Where(x => x.WinLose < 0).Sum(x => x.WinLose));//加跟牌>> 輸家金額
                //decimal winpool = Math.Abs(seats.Where(x => x.WinLose > 0).Sum(x => x.WinLose));//加跟牌>> 贏家金額
                decimal totalcom = Math.Abs(seats.Sum(x => x.Com));
                decimal totalwl = Math.Abs(seats.Sum(x => x.WinLose));
                decimal allcom = Unconditional(losepool * (decimal)(1 - chance)); //真公點

                comrem = allcom - totalcom;
                wlrem = totalwl - allcom;

                GameSeat firstWin = seats.Where(x => x.WinLose > 0).FirstOrDefault();
                if (firstWin != null)
                {
                    int seatNum = firstWin.SitPlayer.RoomSeat;
                    seats[seatNum].SitOldMoney(seats[seatNum].OldMoney + wlrem);
                    //設置狀態
                    seats[seatNum].UpWinLose(seats[seatNum].WinLose + wlrem, wlrem);
                    seats[seatNum].Valid += wlrem;//有效投注
                    seats[seatNum].upCom(comrem); //加入公點  
                }



            }
            //一般比牌結算法>>>>>>
            else if (actionType == (int)ActionType.PK)
            {
                overScene = OverType.PKOver;
                GameSeat PkSeat = seats[pos_thinker];
                pkSeat = pos_thinker;

                bool isLose = seats.Where(x=>x.SitPlayer.RoomSeat != pos_thinker)
                                    .Any(x => x.allPoints <= PkSeat.allPoints); //比比牌的還小或等於 比牌者輸

                if (isLose)//比輸
                {
                    for (int i = 0; i < seats.Length; i++)
                    {
                        if (i == pos_thinker) continue;

                        decimal odds = 0;
                        if (PkSeat.allPoints < seats[i].allPoints) //贏家 大於比牌者>>>>中間家
                        {
                            odds = (round_step == 0 || round_step == 1) ? 2 : 1;
                        }
                        else //贏家 小於比牌者>>>>>>最贏者
                        {
                            odds = (round_step == 0 || round_step == 1) ? 3 : 2;
                        }

                        decimal _win = Ante * odds;
                        seats[i].SitOverMoney(_win);
                        SetWin(seats[i], _win, seats[i].allFollow);
                    }


                    overMoney = Math.Abs(seats.Sum(x => x.OverMoney));//不加跟牌、公點>>金額

                    winMoney = -overMoney;
                    seats[pos_thinker].SitOverMoney(winMoney);
                    //輸家
                    SetLose(seats[pos_thinker], winMoney, seats[pos_thinker].allFollow);



                }
                else //比贏
                {
                    for (int i = 0; i < seats.Length; i++)
                    {
                        if (i == pos_thinker) continue;

                        decimal odds = (round_step == 0 || round_step == 1) ? 2:1;

                        decimal _lose = 0;

                        seats[i].SitOverMoney(-Ante * odds);
                        _lose = seats[i].OverMoney;
                        SetLose(seats[i], _lose, seats[i].allFollow);
                    }

                    overMoney = Math.Abs(seats.Sum(x => x.OverMoney));//不加跟牌、公點>>金額

                    winMoney = overMoney;
                    seats[pos_thinker].SitOverMoney(winMoney);
                    //贏家
                    SetWin(seats[pos_thinker], winMoney, seats[pos_thinker].allFollow);


                }


            }


            //設置分潤
            //setLoserDivided(winMoney, winner);
            setDivided(); //設置分潤

            for (int i = 0; i < seats.Length; i++)
            {              
                seatWinlose.Add(seats[i].WinLose);
            }

            //測試LOG
            /*bool playerWin = seats.Where(x => !x.IsRobot).Any(x => x.Status == SeatStatus.Winner);

            if(playerWin)
            {
                Program.WriteLog("RobotLose", ",Lose : " + round_id);
            }*/



        }


        /// <summary>
        ///  設置贏家.
        /// </summary>
        private void SetWin(GameSeat gs,decimal winMoney,decimal follow,decimal gap = 0) 
        {
            decimal _chance = 0;
            if (chance != 0) _chance = (decimal)(1 - chance);
            decimal _win = winMoney + follow;
            decimal _com = 0;
            decimal _winLose;

            int status = SeatStatus.Winner;

            if (_win <= 0)
            {
                //運氣差 被跟牌變輸家
                _winLose = _win;
                if(_win < 0) status = SeatStatus.Lose;
            }
            else
            {                
                _com = Unconditional(_win * _chance);
                _winLose = Unconditional(_win * (decimal)chance);
                _com += gap;                
                _winLose += gap;
            }

            gs.SitOldMoney(_win);
            //設置狀態
            gs.SitWinLose(_winLose, status);
            seats[gs.SitPlayer.RoomSeat].Valid = _win;//有效投注
            gs.addCom(_com); //加入公點  
            //winner.Add(gs.SitPlayer.RoomSeat);

        }

        /// <summary>
        ///  設置輸家.
        /// </summary>
        private void SetLose(GameSeat gs, decimal _lose,decimal follow) 
        {
            decimal _chance = 0;
            if (chance != 0) _chance = (decimal)(1 - chance);

            decimal oldwin = _lose + follow;
            decimal winlose = oldwin;
            decimal com = 0;
            int status = SeatStatus.Lose;
            gs.SitOldMoney(oldwin);
            if (oldwin > 0) //跟牌跟成贏家
            {
                com = Unconditional(oldwin * _chance);
                winlose = Unconditional(oldwin * (decimal)chance);
                //winner.Add(gs.SitPlayer.RoomSeat);
                status = SeatStatus.Winner;//跟牌贏家
            }

            gs.SitWinLose(winlose, status);
            
            seats[gs.SitPlayer.RoomSeat].Valid = oldwin;//有效投注
            gs.addCom(com); //加入公點  

        }

        private decimal Unconditional(decimal points) 
        {
            if (points % 1 == 0) return points;
            string[] _split = points.ToString().Split('.');
            string _output = "";
            _output += _split[0];
            string _o2 = (_split[1].Length > 4) ? _split[1].Substring(0,4) : _split[1];
            if (_split.Length > 1) _output += "." + _o2;

            return decimal.Parse(_output);
        }


        /// <summary>
        ///  設置平手.
        /// </summary>
        private void SetTie(GameSeat gs)
        {
            gs.SitWinLose(0, SeatStatus.Tie);
            gs.Valid = 0;//有效投注
            gs.addCom(0); //加入公點  

        }

        /// <summary>
        ///  DB寫帳.
        /// </summary>
        public int savaResultDB(int seat) {
            int state = 0;

            isReturnPoint[seat] = true;

            ReportDB_Game report = new ReportDB_Game();
            report.global_data = new ReportDB_Game.GlobalData();
            report.global_data.game_id = Program.GameID;
            report.global_data.jiang_hao = round_id;
            report.global_data.round_num = 0;

            if (BonusTemp[seat] != 0)
            {
                string _name = seats[seat].SitPlayer.MemberID.ToString();
                decimal _win = Math.Abs(BonusTemp[seat]);
            }

            if (seats[seat].IsRobot) report.bonus = 0;
            else
            {
                seats[seat].SitPlayer.SitBonus(GameArea - 1, seats[seat].WinLose);//更新此房個人獎金
                seats[seat].AddBonus(BonusTemp[seat]);
                report.bonus = (float)BonusTemp[seat];
            }

            report.user_id = seats[seat].SitPlayer.MemberID;
            report.change_id = seats[seat].SitPlayer_ChangeID;
            report.user_ip = seats[seat].SitPlayer_LoginIP;
            report.user_status = Program.GameID;
            report.authority_status = 0;
            report.user_seat = seat;
            report.game_area = GameArea;
            report.user_bet = (double)seats[seat].Valid;
            report.user_bet_valid = (double)seats[seat].Valid;
            report.wl_point = (double)seats[seat].WinLose;
            report.deduction_point = 0;
            report.contribution_point = (double)seats[seat].Com;
            report.remaining_point = (double)seats[seat].SitPlayer.Money;
            report.divided = (double)seats[seat].Divided;
            report.create_date = create_date;
            report.session_id = seats[seat].SitPlayer.SessionId;

           

            //Program.WriteLog3("No." + round_id, path_start, string.Format("[sendResult] Seat: {0}, Data: {1}", seat, JsonConvert.SerializeObject(report)));

            if (seats[seat].IsRobot)
            {
                DBAdapter.Do.WriteRobotRecord(report);
            }
            else
            {
                DataServer_Format reportResult = DBAdapter.Do.WriteGameRecord(report);

                if (reportResult == null)
                {
                    // 通知DB: 更新狀態
                    DataNet_SetUserStatus Statusinfo = new DataNet_SetUserStatus(seats[seat].SitPlayer.MemberID, 5, Program.GameID);
                    DBAdapter.Do.SetUserStatus(Statusinfo);
                    Program.WriteLog("ResultErr", string.Format("UserID: {0}, 寫帳未回應", seats[seat].SitPlayer_MemberID));
                    state = 2;
                }
                else if (reportResult.result_code != 1)
                {
                    state = 1;
                    Program.WriteLog("ResultErr", string.Format("UserID: {0}, 寫帳錯誤", seats[seat].SitPlayer_MemberID));
                }

                seats[seat].SitPlayer.IsGetPoint = false;
            }

            return state;


        }
        //結束狀態轉中文
        public string GetOverState()
        {
            string gameState = "";
            switch (overScene)
            {
                case OverType.DeckOver:
                    gameState = "牌堆歸0";
                    break;
                case OverType.HCardOver:
                    gameState = "手牌歸0";
                    break;

                case OverType.SupOver:
                    gameState = "特殊牌(" + GetEndSup() + ")";
                    break;
                case OverType.PKOver:
                    gameState = "比牌";
                    break;

            }
            return gameState;
        }

        public string GetEndSup() 
        {
            string cardState = "";
            switch (overSup)
            {
                case PokerType.Three:
                    cardState = "三條";
                    break;
                case PokerType.Straight:
                    cardState = "順子";
                    break;

                case PokerType.Flush:
                    cardState = "同花";
                    break;
                case PokerType.Full:
                    cardState = "葫蘆";
                    break;
                case PokerType.Four:
                    cardState = "四條";
                    break;
                case PokerType.StraightF:
                    cardState = "同花順";
                    break;
                case PokerType.Royal:
                    cardState = "皇家同花順";
                    break;

            }

            return cardState;
        }

        //各種狀態轉中文
        public string SwichState(int state)
        {
            string gameState = "";
            switch (state)
            {
                case SeatStatus.Lose:
                    gameState = "輸家";
                    break;
                case SeatStatus.Tie:
                    gameState = "平局";
                    break;
                case SeatStatus.Winner:
                    gameState = "贏家";
                    break;

            }
            return gameState;
        }


        /// <summary>
        ///  結算計算分潤(舊).
        /// </summary>
        ///  <param name="wintotal">總贏</param>
        ///  <param name="winner">贏家列表</param>
        private void setLoserDivided(decimal wintotal, List<int> winner)
        {
            decimal _chance = 0;
            decimal windiv = 0;
            if (chance != 0) _chance = (decimal)(1 - chance);

            //設定贏家 並統計總贏
            for (int i = 0; i < winner.Count; i++)
            {
                decimal win = Math.Floor((wintotal / winner.Count) * 10000) / 10000;  //取最小
                decimal com = win * _chance;

                windiv += com;
            }

            //decimal point = Math.Floor((windiv / seats.Length) * 10000) / 10000;

            //for (int i = 0; i < seats.Length; i++)
           // {

            //    seats[i].addDivided(point);  //設置分潤
            //}

        }

        /// <summary>
        ///  結算計算分潤(新).
        /// </summary>
        private void setDivided()
        {
            decimal lose = 0;
            lose = Math.Abs(seats.Where(x => x.Status == SeatStatus.Lose).Sum(x => x.OldMoney));
            decimal _chance = 0;
            if (chance != 0) _chance = (decimal)(1 - chance);
            decimal com = Unconditional(lose * _chance);
            decimal point = Unconditional(com / seats.Length);
            for (int i = 0; i < seats.Length; i++)
            {
                decimal _point = point;
                if(i == 0) //誤差
                {
                    decimal rem = com - point * seats.Length;
                    _point += rem;
                }
                seats[i].addDivided(_point);  //設置分潤
            }

        }


        public void BackPoint()
        {
            try
            {
                List<int> userID = new List<int>();
                List<int> changeID = new List<int>();
                for (int i = 0; i < 4; i++)
                {   // 回點
                    if (seats[i].SitPlayer != null && !seats[i].IsRobot)
                    {
                        userID.Add(seats[i].SitPlayer.MemberID);
                        changeID.Add(seats[i].SitPlayer.ChangeID);
                        seats[i].SitPlayer.IsGetPoint = false;
                    }
                }

                if (userID.Count > 0)
                {
                    DataNet_BackPoint info = new DataNet_BackPoint(Program.GameID, userID.ToArray(), changeID.ToArray());
                    DBAdapter.Do.BackPoint(info);
                }
            }
            catch (Exception ex)
            {
                Program.WriteLog("TryCatch", "錯誤訊息: " + ex);
            }
        }

        /// <summary>
        /// 機器人必輸
        /// </summary>
        /// <returns></returns>
        public void robotLose()
        {
            WinRoundType = 2;

            pokers = Poker.PlayerWin(pokers, winPlayerSeat.SitPlayer.RoomSeat);
            foreach (var seat in seats)
            {
                if (seat.IsRobot)
                {
                    ((Robot)seat.SitPlayer).SitAI(1, false);
                }
            }


        }

        /// <summary>
        /// 機器人必贏 設置.
        /// </summary>
        /// <returns></returns>
        public void robotWin(int playerCount,GameSeat winRobot)
        {
            WinRoundType = 1;

            int openRound = randOpenRound();
            if (CheckBlack() || isOP) openRound = 1;//黑單設1
            foreach (var seat in seats)
            {
                //if(seat.IsRobot) ((Robot)seat.SitPlayer).SitAI(1, false, openRound);
                
                if (seat.IsRobot && winRobot.SitPlayer.RoomSeat == seat.SitPlayer.RoomSeat)
                {

                    ((Robot)seat.SitPlayer).SitAI(2, false, openRound);
                }
                else if (seat.IsRobot)
                {
                    ((Robot)seat.SitPlayer).SitAI(1, false, openRound);
                }
            }

            if (playerCount > 1) //真人玩家人數大於1 機器人必最小
            {
                pokers = Poker.RobotWin(pokers, winRobot.SitPlayer.RoomSeat);
            }
            else
            {
                pokers = Poker.PlayerLose(pokers, winPlayerSeat.SitPlayer.RoomSeat);
                pokers = Poker.RobotWin(pokers, winRobot.SitPlayer.RoomSeat);

            }

        }




        /// <summary>
        /// 檢查黑單
        /// </summary>
        public bool CheckBlack()
        {
            bool isBlack = seats.Any(x => x.SitPlayer.UserBlack == 2);
            return isBlack;
            //return true;
        }


        /// <summary>
        /// 隨機設置AI開牌回合
        /// </summary>
        private int randOpenRound()
        {
            int _rand = random.Next(1, 5);
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
                int choice = random.Next(0, 3);
                if (choice > 0)
                {
                    _rand = random.Next(2, 5);
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
            return _rand;

        }


    }
}
