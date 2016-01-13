using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using Newtonsoft.Json.Linq;

namespace Gomoku
{
    public enum State
    {
        Free,
        Player1,
        Player2
    }
    public class Board
    {
        public static int MaxCol = 12;
        public static int MaxRow = 12; 

        private static State[,] m_ChessBoard;
        public static State PlayingPlayer = State.Player1;
        public static PlayMode CurrentMode;

        public static State[,] ChessBoard
        {
            get
            {
                return m_ChessBoard;
            }

            set
            {
                m_ChessBoard = value;
            }
        }

        public Board()
        {
            ChessBoard = new State[12, 12];
            for (int i = 0; i < 12; i++)
                for (int j = 0; j < 12; j++)
                    ChessBoard[i,j] = State.Free;
        }

        public bool Place(int Row, int Col)
        {
            if (ChessBoard[Row, Col] != State.Free)
            {
                Notify?.Invoke("You Cannot Place Here!");
                return false;
            }

            ChessBoard[Row, Col] = PlayingPlayer;

            if (CheckGame.CheckWin(Row, Col))
            {
                WinNotify?.Invoke(PlayingPlayer);
                return true;
            }

            PlayingPlayer = (State)(3 - (int)PlayingPlayer); //Change player

            return true;
        }

        public void AIPlace()
        {
            AI.Place();

            if (CheckGame.CheckWin(AI.AIRow, AI.AICol))
            {
                WinNotify?.Invoke(PlayingPlayer);
                return;
            }

            PlayingPlayer = (State)(3 - (int)PlayingPlayer); //Change player again
        }

        public static void ResetBoard()
        {
            for (int i = 0; i < 12; i++)
                for (int j = 0; j < 12; j++)
                    ChessBoard[i, j] = State.Free;
        }

        public delegate void InvalidStepAnounce(string ErrorString);
        public static event InvalidStepAnounce Notify;

        public delegate void CheckWinNotify(State Player);
        public static event CheckWinNotify WinNotify;
        

    }

    

    public class CheckGame
    {
        public static bool CheckWin(int Row, int Col)
        {
            int Check = 0;

            int VCheck = DownCheck(Row, Col) + UpCheck(Row, Col);
            int HCheck = LeftCheck(Row, Col) + RightCheck(Row, Col);
            int D1Check = UpLeftCheck(Row, Col) + DownRightCheck(Row, Col);
            int D2Check = UpRightCheck(Row, Col) + DownLeftCheck(Row, Col);

            Check = Math.Max(VCheck, HCheck);
            Check = Math.Max(Check, D1Check);
            Check = Math.Max(Check, D2Check);

            return (Check>=4);
        }
        private static int DownCheck(int Row, int Col)
        {
            int Down = 0;
            for (int i = 1; i < 5; i++)
            {
                if (Row + i >= Board.MaxRow - 1)
                    return Down;
                if (Board.ChessBoard[Row + i, Col] == Board.ChessBoard[Row, Col])
                    Down++;
                else break;
            }
            return Down;
        }

        private static int UpCheck(int Row, int Col)
        {
            int Up = 0;
            for (int i = 1; i < 5; i++)
            {
                if (Row - i <= 0)
                    return Up;
                if (Board.ChessBoard[Row - i, Col] == Board.ChessBoard[Row, Col])
                    Up++;
                else break;
            }
            return Up;
        }

        private static int LeftCheck(int Row, int Col)
        {
            int Left = 0;
            for (int i = 1; i < 5; i++)
            {
                if (Col - i <= 0)
                    return Left;
                if (Board.ChessBoard[Row, Col - i] == Board.ChessBoard[Row, Col])
                    Left++;
                else break;
            }
            return Left;
        }

        private static int RightCheck(int Row, int Col)
        {
            int Right = 0;
            for (int i = 1; i < 5; i++)
            {
                if (Col + i >= Board.MaxCol - 1)
                    return Right;
                if (Board.ChessBoard[Row, Col + i] == Board.ChessBoard[Row, Col])
                    Right++;
                else break;
            }
            return Right;
        }

        private static int DownLeftCheck(int Row, int Col)
        {
            int DownLeft = 0;
            for (int i = 1; i < 5; i++)
            {
                if (Row + i >= Board.MaxRow || Col - i <= 0) return DownLeft;
                if (Board.ChessBoard[Row + i, Col - i] == Board.ChessBoard[Row, Col])
                    DownLeft++;
                else break;
            }
            return DownLeft;
        }

        private static int UpRightCheck(int Row, int Col)
        {
            int UpRight = 0;
            for (int i = 1; i < 5; i++)
            {
                if (Row - i <= 0 || Col + i >= Board.MaxCol - 1) return UpRight;
                if (Board.ChessBoard[Row - i, Col + i] == Board.ChessBoard[Row, Col])
                    UpRight++;
                else break;
            }
            return UpRight;
        }

        private static int UpLeftCheck(int Row, int Col)
        {
            int UpLeft = 0;
            for (int i = 1; i < 5; i++)
            {
                if (Row - i <= 0 || Col - i <= 0) return UpLeft;
                if (Board.ChessBoard[Row - i, Col - i] == Board.ChessBoard[Row, Col])
                    UpLeft++;
                else break;
            }
            return UpLeft;
        }

        private static int DownRightCheck(int Row, int Col)
        {
            int DownRight = 0;
            for (int i = 1; i < 5; i++)
            {
                if (Row + i >= Board.MaxRow || Col + i >= Board.MaxCol - 1) return DownRight;
                if (Board.ChessBoard[Row + i, Col + i] == Board.ChessBoard[Row, Col])
                    DownRight++;
                else break;
            }
            return DownRight;
        }
    }
    

}
