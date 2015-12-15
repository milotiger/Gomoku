using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Gomoku
{
    public class AI //Thuật toán AI của Lê Bá Khánh Trình từ trang blog.myclass.vn
    {
        public static void Place()
        {
            //Thread.Sleep(500); //Simulate delay
            AIStart();
            Board.ChessBoard[AIRow, AICol] = Board.PlayingPlayer;
            AIPlaceNotify?.Invoke(AIRow, AICol);
        }

        public delegate void AIPlace(int Row, int Col);

        public static event AIPlace AIPlaceNotify;

        public static int maxDepth = 11; // độ sâu của máy
        public static int maxMove = 3; // số lược đi thử của máy
        public static int depth = 0;
        public static bool fWin = false;

        public static int[] DScore = new int[5] { 0, 1, 9, 81, 729 }; // điểm phòng thủ
        public static int[] AScore = new int[5] { 0, 2, 18, 162, 1458 }; // điểm tấn công
        //public static int[] AScore = new int[5] { 0, 1, 9, 81, 729 };

        public static Point[] PCMove = new Point[maxMove + 2];
        public static Point[] HumanMove = new Point[maxMove + 2];
        public static Point[] WinMove = new Point[maxDepth + 2];
        public static Point[] LoseMove = new Point[maxDepth + 2];

        public static int[,] AIBoard = new int[Board.MaxRow + 2, Board.MaxCol + 2];

        static public int AIRow; // toạ độ mà máy đi
        static public int AICol; // toạ độ mà máy đi
        static bool isLose = false;

        public static void Move(int r, int c)
        {
            AIRow = r;
            AICol = c;
        }

        public int GetRow()
        {
            return AIRow;
        }

        public int GetCol()
        {
            return AICol;
        }

        public static void ResetBoard()
        {
            for (int r = 0; r < Board.MaxRow + 2; r++)
                for (int c = 0; c < Board.MaxCol + 2; c++)
                    AIBoard[c, r] = 0;
        }

        public static Point MaxPos()
        {
            int Max = 0;
            Point p = new Point();
            for (int i = 0; i < Board.MaxRow; i++)
            {
                for (int j = 0; j < Board.MaxCol; j++)
                {
                    if (AIBoard[j, i] > Max)
                    {
                        p.X = i; p.Y = j;
                        Max = AIBoard[j, i];
                    }

                }
            }
            return p;
        }

        public static void EvalBoard(int index)
        {
            int rw, cl, ePC, eHuman;
            ResetBoard();
            //Danh gia theo cot
            for (cl = 0; cl < Board.MaxCol; cl++)
                for (rw = 0; rw < Board.MaxRow - 4; rw++)
                {
                    ePC = 0; eHuman = 0;
                    for (int i = 0; i < 5; i++)
                    {
                        if (Board.ChessBoard[rw + i, cl] == Human) eHuman++;
                        if (Board.ChessBoard[rw + i, cl] == PC) ePC++;
                    }

                    if (eHuman * ePC == 0 && eHuman != ePC)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            if (Board.ChessBoard[rw + i, cl] == State.Free)
                            {
                                if (eHuman == 0)
                                    if (index == 0)
                                        AIBoard[rw + i, cl] += DScore[ePC];
                                    else AIBoard[rw + i, cl] += AScore[ePC];
                                if (ePC == 0)
                                    if (index == 2)
                                        AIBoard[rw + i, cl] += DScore[eHuman];
                                    else AIBoard[rw + i, cl] += AScore[eHuman];
                                if (eHuman == 4 || ePC == 4)
                                    AIBoard[rw + i, cl] *= 2;
                            }
                        }
                    }
                }

            //Danh gia theo hang
            for (rw = 0; rw < Board.MaxRow; rw++)
                for (cl = 0; cl < Board.MaxCol - 4; cl++)
                {
                    ePC = 0; eHuman = 0;
                    for (int i = 0; i < 5; i++)
                    {
                        if (Board.ChessBoard[rw, cl + i] == Human) eHuman++;
                        if (Board.ChessBoard[rw, cl + i] == PC) ePC++;
                    }

                    if (eHuman * ePC == 0 && eHuman != ePC)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            if (Board.ChessBoard[rw, cl + i] == State.Free) // Neu o chua duoc danh
                            {
                                if (eHuman == 0)
                                    if (index == 0)
                                        AIBoard[rw, cl + i] += DScore[ePC];
                                    else AIBoard[rw, cl + i] += AScore[ePC];
                                if (ePC == 0)
                                    if (index == 2)
                                        AIBoard[rw, cl + i] += DScore[eHuman];
                                    else AIBoard[rw, cl + i] += AScore[eHuman];
                                if (eHuman == 4 || ePC == 4)
                                    AIBoard[rw, cl + i] *= 2;
                            }
                        }
                    }
                }

            //Danh gia duong cheo xuong
            for (rw = 0; rw < Board.MaxRow - 4; rw++)
                for (cl = 0; cl < Board.MaxCol - 4; cl++)
                {
                    ePC = 0; eHuman = 0;
                    for (int i = 0; i < 5; i++)
                    {
                        if (Board.ChessBoard[rw + i, cl + i] == Human) eHuman++;
                        if (Board.ChessBoard[rw + i, cl + i] == PC) ePC++;
                    }

                    if (eHuman * ePC == 0 && eHuman != ePC)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            if (Board.ChessBoard[rw + i, cl + i] == State.Free) // Neu o chua duoc danh
                            {
                                if (eHuman == 0)
                                    if (index == 0)
                                        AIBoard[rw + i, cl + i] += DScore[ePC];
                                    else AIBoard[rw + i, cl + i] += AScore[ePC];
                                if (ePC == 0)
                                    if (index == 2)
                                        AIBoard[rw + i, cl + i] += DScore[eHuman];
                                    else AIBoard[rw + i, cl + i] += AScore[eHuman];
                                if (eHuman == 4 || ePC == 4)
                                    AIBoard[rw + i, cl + i] *= 2;
                            }
                        }

                    }
                }

            //Danh gia duong cheo len
            for (cl = 4; cl < Board.MaxCol; cl++)
                for (rw = 0; rw < Board.MaxRow - 4; rw++)
                {
                    ePC = 0; eHuman = 0;
                    for (int i = 0; i < 5; i++)
                    {
                        if (Board.ChessBoard[rw + i, cl - i] == Human) eHuman++;
                        if (Board.ChessBoard[rw + i, cl - i] == PC) ePC++;
                    }

                    if (eHuman * ePC == 0 && eHuman != ePC)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            if (Board.ChessBoard[rw + i, cl - i] == State.Free) // Neu o chua duoc danh
                            {
                                if (eHuman == 0)
                                    if (index == 0)
                                        AIBoard[rw + i, cl - i] += DScore[ePC];
                                    else AIBoard[rw + i, cl - i] += AScore[ePC];
                                if (ePC == 0)
                                    if (index == 2)
                                        AIBoard[rw + i, cl - i] += DScore[eHuman];
                                    else AIBoard[rw + i, cl - i] += AScore[eHuman];
                                if (eHuman == 4 || ePC == 4)
                                    AIBoard[rw + i, cl - i] *= 2;
                            }
                        }
                    }
                }
        }


        //Ham tim nuoc di cho may
        public static void FindMove()
        {
            if (depth > maxDepth) return;
            depth++;

            fWin = false;
            //bool fLose = false;
            Point pcMove = new Point();
            Point humanMove = new Point();
            int countMove = 0;
            EvalBoard(2);

            //Lay ra MaxMove buoc di co diem cao nhat
            Point temp = new Point();
            for (int i = 0; i < maxMove; i++)
            {
                temp = MaxPos();
                PCMove[i] = temp;
                AIBoard[(int)temp.Y, (int)temp.X] = 0;
            }

            //Lay nuoc di trong PCMove[] ra danh thu
            countMove = 0;
            while (countMove < maxMove)
            {

                pcMove = PCMove[countMove++];
                Board.ChessBoard[(int)pcMove.Y, (int)pcMove.X] = PC;
                WinMove.SetValue(pcMove, depth - 1);

                //Tim cac nuoc di toi uu cua nguoi
                ResetBoard();
                EvalBoard(0);
                //Lay ra maxMove nuoc di co diem cao nhat cua nguoi
                for (int i = 0; i < maxMove; i++)
                {
                    temp = MaxPos();
                    HumanMove[i] = temp;
                    AIBoard[(int)temp.Y, (int)temp.X] = 0;
                }
                //Danh thu cac nuoc di
                for (int i = 0; i < maxMove; i++)
                {
                    humanMove = HumanMove[i];

                    Board.ChessBoard[(int)humanMove.Y, (int)humanMove.X] = PC;
                    if (CheckGame.CheckWin((int)humanMove.Y, (int)humanMove.X))
                    {
                        fWin = true;
                    }

                    Board.ChessBoard[(int)humanMove.Y, (int)humanMove.X] = Human;
                    if (CheckGame.CheckWin((int)humanMove.Y, (int)humanMove.X))
                    {
                        isLose = true;
                    }
                    if (isLose)
                    {
                        Board.ChessBoard[(int)pcMove.Y, (int)pcMove.X] = State.Free;
                        Board.ChessBoard[(int)humanMove.Y, (int)humanMove.X] = State.Free;
                        break;
                    }
                    if (fWin)
                    {
                        Board.ChessBoard[(int)pcMove.Y, (int)pcMove.X] = State.Free;
                        Board.ChessBoard[(int)humanMove.Y, (int)humanMove.X] = State.Free;
                        return;
                    }
                    FindMove();
                    Board.ChessBoard[(int)humanMove.Y, (int)humanMove.X] = State.Free;
                }
                Board.ChessBoard[(int)pcMove.Y, (int)pcMove.X] = State.Free;
            }
        }

        private static State Human;
        private static State PC;

        public static void AIStart()
        {
            //Human = 3 - Board.PlayingPlayer;
            //PC = Board.PlayingPlayer;

            Human = Board.PlayingPlayer;
            PC = 3 - Board.PlayingPlayer;


            for (int i = 0; i < maxMove; i++)
            {
                WinMove[i] = new Point();
                PCMove[i] = new Point();
                HumanMove[i] = new Point();
            }

            depth = 0;
            FindMove();

            if (fWin)
            {
                Move((int)WinMove[0].Y, (int)WinMove[0].X);
            }
            else
            {
                EvalBoard(2);
                Point move = new Point();
                move = MaxPos();
                Move((int)move.Y, (int)move.X);
            }
        }
    }
}

