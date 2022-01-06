using LobbyServerForLinux.Model;
using MhCore.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LobbyServerForLinux.Presenter
{
    public partial class GamePresenter : FightGame
    {
        private readonly GameScene m = new GameScene();      //場景邏輯&&參數

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roomNo"></param>
        public GamePresenter(int roomNo, int gameArea, int level) : base(roomNo, "")
        {
            m.GameArea = gameArea;
            m.Ante = Program.Ante[level - 1];
            m.LimitEnter = Program.Limit[level - 1];

            for (int i = 0; i < m.seats.Length; i++) m.seats[i] = new GameSeat(i); //座位初始化

            for (int i = 0; i < 52; i++) m.pokers[i] = new Poker(i + 1); // 52張牌
        }

        /// <summary>
        /// 系統處理時間.
        /// </summary>
        public override void SystemClock()
        {

        }

        /// <summary>
        /// 使用者重新連回遊戲房.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public override int UserLinkRoom(GameUser user)
        {
            return 0;
        }

        /// <summary>
        /// 處理 玩家接收命令.(遊戲房部份)
        /// </summary>
        public override void HandleUserCommands(GameUser user, string cmd, string data)
        {

        }

        /// <summary>
        /// 處理 玩家斷線
        /// </summary>
        public int UserLeaveRoom(GameUser user)
        {
            return UserLeaveRoom(user, 0);
        }

        /// <summary>
        /// 使用者離開遊戲房.
        /// </summary>
        /// <param name="user">使用者</param>
        /// <returns>回傳結果. 
        /// 0: 成功, 
        /// 1. 遊戲中, 無法直接離局.
        /// </returns>
        public override int UserLeaveRoom(GameUser user, int state)
        {
            return 0;

        }

        /// <summary>
        /// 是否遊戲中.
        /// </summary>
        /// <returns></returns>
        public override bool IsPlaying()
        {
            return m.isPlaying;
        }

        /// <summary>
        /// 使用者進入遊戲房.
        /// </summary>
        /// <param name="user">使用者</param>
        /// <param name="enterParas">進入時可附帶參數.</param>
        /// <returns>回傳結果. 
        ///  0: 成功
        ///  1: 座位已有人.
        /// </returns>
        public override int UserEnterRoom(GameUser user, object enterParas)
        {

            return 0;
        }

        public override int KickAllUser(int kicktype)
        {

            return 1;
        }

        /// <summary>
        /// 使用者斷線.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public override int UserDisconnect(GameUser user)
        {
            ((Player)user).IsDisconnect = true;
            return 0;
        }

        /// <summary>
        /// 湊桌入房.
        /// </summary>
        /// <param name="players">玩家列表</param>
        public override void DeskmateToRoom(Player[] players)
        {
            if (m.isPlaying)    // 系統錯誤: 遊戲桌正遊戲中.
            {
                //Program.WriteLog(Program.LOG_CodingError, string.Format("[PaiKen] DeskmateToRoom() 遊戲桌正遊戲中!! roomNo:{0}", SerialNo));
                return;
            }
            if (players == null || players.Length < 5)  // 系統錯誤: 湊桌人數不足.
            {
                //Program.WriteLog(Program.LOG_CodingError, string.Format("[PaiKen] DeskmateToRoom() 湊桌人數不足!! players:{0}", (players == null ? "null" : players.Length.ToString())));
                return;
            }

            int code = 0;


            for (int i = 0; i < players.Length; i++)
            {
                // 系統錯誤: 配桌人數超過.
                if (i >= m.seats.Length)
                {
                    //Program.WriteLog(Program.LOG_CodingError, string.Format("[PaiKen] DeskmateToRoom() 入桌人數過量!! seats:{0} players:{1}", m.seats.Length, players.Length));
                    break;
                }

                // 依序進房並配置座位.
                if ((code = UserEnterRoom(players[i], i)) != 0)
                {
                    // 系統錯誤: 入房失敗.
                    //Program.WriteLog(Program.LOG_CodingError, string.Format("[PaiKen] DeskmateToRoom() 入桌人數過量!! seat:{0} code:{1}", i, code));
                }
            }

        }




    }
}
