using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DrawingChessBoard
{
    enum CellState { CanMove, CantMove, Enemy };
    public partial class Form1 : Form
    {
        private bool bIsSimpleVersion;      //2 варианта отображения
        private int iWidthHeightCell;       //Размер ячеек
        private int iMarginTop, iMarginLeft;//Отступ для отрисовки
        private ChessBoardCell[][] arrCells;//Массив ячеек(клеток на доске)
        private bool bCheckersCreated;      //Для проверки созданы ли ячейки
        internal List<ChessBoardCell> availableCellsFromMove; //Ячейки доступные для хода

        public Сhecker SelectedСhecker { get; set; }//Выбранная шашка
        public Color MoveColor
        {
            get { return moveColor; }
            set { moveColor = value; ShowWhoNextMove(); }
        }//Цвет шашек ходящего игрока
        public int ScoreWhite 
        {
            get { return scoreWhite; }
            set { scoreWhite = value; ShowScore(); if (scoreWhite >= 12) ShowWinner(); }
        }//Счет игрока с белыми шашками
        public int ScoreBlack 
        {
            get { return scoreBlack; }
            set { scoreBlack = value; ShowScore(); if (scoreBlack >= 12) ShowWinner(); }
        }//Счет игрока с черными шашками

        private int scoreWhite;
        private int scoreBlack;
        private Color moveColor;
        private Timer timerGame;
        private TimeSpan timeGame;

        public Form1()
        {
            InitializeComponent();
            InitializeVariables();
            DrawChessBoard();
            //DoubleBuffered = true;
        }

        private void InitializeVariables()
        {
            availableCellsFromMove = new List<ChessBoardCell>();
            arrCells = new ChessBoardCell[8][];
            for (int i = 0; i < arrCells.Length; i++)
                arrCells[i] = new ChessBoardCell[8];
            bIsSimpleVersion = false;
            bCheckersCreated = false;
            MoveColor = Color.White;
            iMarginTop = menuStrip1.Height;
            Text = "Шашки";
            ScoreWhite = 0;
            ScoreBlack = 0;
            timeGame = new TimeSpan(0, 0, 0);
            timerGame = new Timer();
            timerGame.Tick += TimerGame_Tick;
            timerGame.Interval = 1000;
            timerGame.Start();
        }

        #region"События Form1"

        private void Form1_Resize(object sender, EventArgs e)
        {
            //Text = Width + " " + Height;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            DrawChessBoard();
        }

        #endregion

        #region"Отрисовка шахматной доски и элементов"

        private void DrawChessBoard()
        {
            if (ClientSize.Width < ClientSize.Height - iMarginTop)
            {
                iWidthHeightCell = ClientSize.Width / (bIsSimpleVersion ? 8 : 10);
            }
            else
            {
                iWidthHeightCell = (ClientSize.Height - iMarginTop) / (bIsSimpleVersion ? 8 : 10);
            }
            iMarginLeft = (ClientSize.Width - (iWidthHeightCell * (bIsSimpleVersion ? 8 : 10))) / 2;

            //DarkGoldenrod
            //Wheat
            //SaddleBrown

            DrawVerticalCoords(new Point(iWidthHeightCell, 1 + iMarginTop));

            DrawCells();

            DrawVerticalCoords(new Point(iWidthHeightCell, iWidthHeightCell * 9 + iMarginTop));

            DrawСheckers();

            //PaintArrayFromFile();

            DrawBorders();
        }

        private void DrawVerticalCoords(Point pStart)
        {
            if (!bIsSimpleVersion)
            {
                Graphics gField = CreateGraphics();
                gField.FillRectangle(Brushes.SaddleBrown, 0 + iMarginLeft, pStart.Y, iWidthHeightCell, iWidthHeightCell);
                for (int i = 0; i < 8; i++)
                {
                    gField.FillRectangle(Brushes.DarkGoldenrod, iWidthHeightCell + i * iWidthHeightCell + iMarginLeft, pStart.Y, iWidthHeightCell, iWidthHeightCell);
                    gField.DrawString(((char)(65 + i)) + "", new Font("Arial", iWidthHeightCell / 2 + 5, FontStyle.Regular), Brushes.SaddleBrown, iWidthHeightCell + i * iWidthHeightCell + iMarginLeft, pStart.Y);
                }
                gField.FillRectangle(Brushes.SaddleBrown, 9 * iWidthHeightCell + iMarginLeft, pStart.Y, iWidthHeightCell, iWidthHeightCell);
            }
        }

        private void DrawCells()
        {
            bool bIsEven = false;
            Graphics gField = CreateGraphics();

            int x = 0, y = 0;
            for (int i = 0; i < 8; i++)
            {
                DrawHorizontalCoord(i, 0 + iMarginLeft);

                for (int j = 0; j < 8; j++)
                {
                    x = (bIsSimpleVersion ? 0 : iWidthHeightCell) + i * iWidthHeightCell + +iMarginLeft;
                    y = (bIsSimpleVersion ? 0 : iWidthHeightCell) + j * iWidthHeightCell + iMarginTop;


                    if (arrCells[i][j] == null)
                        arrCells[i][j] = new ChessBoardCell(iWidthHeightCell, (bIsEven ? Color.White : Color.Black), new Point(x, y), this);
                    else
                    {
                        arrCells[i][j].SizeCell = new Size(iWidthHeightCell, iWidthHeightCell);
                        arrCells[i][j].CoordinatesCell = new Point(x, y);
                    }
                    arrCells[i][j].BackColor = bIsEven ? Color.Wheat : Color.DarkGoldenrod;

                    bIsEven = !bIsEven;

                }
                bIsEven = !bIsEven;

                DrawHorizontalCoord(i, iWidthHeightCell * 9 + iMarginLeft);
            }
        }

        private void DrawHorizontalCoord(int i, int iX)
        {
            if (!bIsSimpleVersion)
            {
                Graphics gField = CreateGraphics();
                gField.FillRectangle(Brushes.DarkGoldenrod, iX, iWidthHeightCell + i * iWidthHeightCell + iMarginTop, iWidthHeightCell, iWidthHeightCell);
                gField.DrawString((i + 1) + "", new Font("Arial", iWidthHeightCell / 2 + 5, FontStyle.Regular), Brushes.SaddleBrown, new Point(iX, iWidthHeightCell + i * iWidthHeightCell + iMarginTop));
            }
        }

        private void DrawСheckers()
        {

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (!bCheckersCreated)
                    {
                        if (j <= 2 && arrCells[i][j].CheckerOnCell == null && arrCells[i][j].ColorCell == Color.Black)
                            arrCells[i][j].CreateChecker(Color.White);
                        else if (j >= 5 && arrCells[i][j].CheckerOnCell == null && arrCells[i][j].ColorCell == Color.Black)
                            arrCells[i][j].CreateChecker(Color.Black);

                        Controls.Add(arrCells[i][j]);
                    }
                }
            }
            bCheckersCreated = true;
        }

        private void DrawBorders()
        {
            Graphics gField = CreateGraphics();
            Pen penBoreder = new Pen(Color.SaddleBrown);
            penBoreder.Width = 2;
            for (int i = 0; i < (bIsSimpleVersion ? 8 : 10); i++)
                for (int j = 0; j < (bIsSimpleVersion ? 8 : 10); j++)
                    gField.DrawRectangle(penBoreder, 0 + i * iWidthHeightCell + iMarginLeft, 0 + j * iWidthHeightCell + iMarginTop, iWidthHeightCell, iWidthHeightCell);

        }

        #region"Метод для изображения доски с шашками в виде текста в тестовом файле"
        //private void PaintArrayFromFile()
        //{
        //    using (StreamWriter sw = new StreamWriter("tmp.txt"))
        //    {
        //        for (int i = 0; i < 8; i++)
        //        {
        //            for (int j = 0; j < 8; j++)
        //            {
        //                sw.Write("|");
        //                if (arrCells[i][j].CheckerOnCell != null)
        //                {
        //                    if (arrCells[i][j].CheckerOnCell.ColorChecker == Color.Black)
        //                        sw.Write("B");
        //                    else
        //                        sw.Write("W");
        //                }
        //                else
        //                    sw.Write(" ");
        //                sw.Write("|");
        //            }
        //            sw.Write("\r\n");
        //        }
        //    }
        //}
        #endregion

        #endregion

        #region"Методы для отображения информации"

        private void ShowWhoNextMove()
        {
            colorMoveToolStripMenuItem.BackColor = moveColor == Color.White ? Color.White : Color.Gray;
            colorMoveToolStripMenuItem.Text = "Ход " + (moveColor == Color.White ? "Белых" : "Черных");
        }
        private void ShowScore()
        {
            scoreToolStripMenuItem.Text = $"Счет: Черные: {ScoreBlack} Белые: {ScoreWhite}";
        }
        private void ShowWinner()
        {
            timerGame.Stop();
            MessageBox.Show($"Игрок с " + (scoreBlack >= 12 ? "черными" : "белыми") + " шашками выиграл!");
            moveColor = Color.Gray;
        }

        #endregion

        #region"Меню"
        private void simpleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            simpleToolStripMenuItem.CheckState = CheckState.Checked;
            complexToolStripMenuItem.CheckState = CheckState.Unchecked;
            bIsSimpleVersion = simpleToolStripMenuItem.Checked;
            Invalidate();
        }
        private void complexToolStripMenuItem_Click(object sender, EventArgs e)
        {
            simpleToolStripMenuItem.CheckState = CheckState.Unchecked;
            complexToolStripMenuItem.CheckState = CheckState.Checked;
            bIsSimpleVersion = simpleToolStripMenuItem.Checked;
            Invalidate();
        }
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            timerGame.Stop();
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    Controls.Remove(arrCells[i][j]);
            InitializeVariables();
            complexToolStripMenuItem_Click(complexToolStripMenuItem, new EventArgs());
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion

        internal void FoundAvailableCells(ChessBoardCell selectedCell)
        {
            int iSelectedCell = -1;
            int jSelectedCell = -1;

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (arrCells[i][j] == selectedCell)
                    {
                        iSelectedCell = i;
                        jSelectedCell = j;
                        break;
                    }
                }
            }

            CellState isCanMove;
            if (MoveColor == Color.White)
            {

                #region"Движение белой шашки вперед-влево"
                isCanMove = CheckCell(iSelectedCell - 1, jSelectedCell + 1);
                if (isCanMove == CellState.Enemy)
                {
                    isCanMove = CheckCell(iSelectedCell - 2, jSelectedCell + 2);
                    if (isCanMove == CellState.CanMove)
                    {
                        availableCellsFromMove.Add(arrCells[iSelectedCell - 2][jSelectedCell + 2]);
                        availableCellsFromMove.Last().CanMoveOnThisCell = true;
                        availableCellsFromMove.Last().KillСhecker = arrCells[iSelectedCell - 1][jSelectedCell + 1].CheckerOnCell;
                        availableCellsFromMove.Last().KillCheckerAfterMove = true;
                        selectedCell.CanKillMore = true;
                    }
                }
                else if (isCanMove == CellState.CanMove)
                {
                    availableCellsFromMove.Add(arrCells[iSelectedCell - 1][jSelectedCell + 1]);
                    availableCellsFromMove.Last().CanMoveOnThisCell = true;
                }
                #endregion

                #region"Движение белой шашки вперед-вправо"
                isCanMove = CheckCell(iSelectedCell + 1, jSelectedCell + 1);
                if (isCanMove == CellState.Enemy)
                {
                    isCanMove = CheckCell(iSelectedCell + 2, jSelectedCell + 2);
                    if (isCanMove == CellState.CanMove)
                    {
                        availableCellsFromMove.Add(arrCells[iSelectedCell + 2][jSelectedCell + 2]);
                        availableCellsFromMove.Last().CanMoveOnThisCell = true;
                        availableCellsFromMove.Last().KillСhecker = arrCells[iSelectedCell + 1][jSelectedCell + 1].CheckerOnCell;
                        availableCellsFromMove.Last().KillCheckerAfterMove = true;
                        selectedCell.CanKillMore = true;
                    }
                }
                else if (isCanMove == CellState.CanMove)
                {
                    availableCellsFromMove.Add(arrCells[iSelectedCell + 1][jSelectedCell + 1]);
                    availableCellsFromMove.Last().CanMoveOnThisCell = true;
                }
                #endregion

                #region"Проверка возможности движения белой шашки назад-вправо"
                isCanMove = CheckCell(iSelectedCell + 1, jSelectedCell - 1);
                if (isCanMove == CellState.Enemy)
                {
                    isCanMove = CheckCell(iSelectedCell + 2, jSelectedCell - 2);
                    if (isCanMove == CellState.CanMove)
                    {
                        availableCellsFromMove.Add(arrCells[iSelectedCell + 2][jSelectedCell - 2]);
                        availableCellsFromMove.Last().CanMoveOnThisCell = true;
                        availableCellsFromMove.Last().KillСhecker = arrCells[iSelectedCell + 1][jSelectedCell - 1].CheckerOnCell;
                        availableCellsFromMove.Last().KillCheckerAfterMove = true;
                        selectedCell.CanKillMore = true;
                    }
                }
                #endregion

                #region"Проверка возможности движения белой шашки назад-влево"
                isCanMove = CheckCell(iSelectedCell - 1, jSelectedCell - 1);
                if (isCanMove == CellState.Enemy)
                {
                    isCanMove = CheckCell(iSelectedCell - 2, jSelectedCell - 2);
                    if (isCanMove == CellState.CanMove)
                    {
                        availableCellsFromMove.Add(arrCells[iSelectedCell - 2][jSelectedCell - 2]);
                        availableCellsFromMove.Last().CanMoveOnThisCell = true;
                        availableCellsFromMove.Last().KillСhecker = arrCells[iSelectedCell - 1][jSelectedCell - 1].CheckerOnCell;
                        availableCellsFromMove.Last().KillCheckerAfterMove = true;
                        selectedCell.CanKillMore = true;
                    }
                }
                #endregion
            }
            else
            {
                #region"Движение черной шашки вперед-влево"
                isCanMove = CheckCell(iSelectedCell + 1, jSelectedCell - 1);
                if (isCanMove == CellState.Enemy)
                {
                    isCanMove = CheckCell(iSelectedCell + 2, jSelectedCell - 2);
                    if (isCanMove == CellState.CanMove)
                    {
                        availableCellsFromMove.Add(arrCells[iSelectedCell + 2][jSelectedCell - 2]);
                        availableCellsFromMove.Last().CanMoveOnThisCell = true;
                        availableCellsFromMove.Last().KillСhecker = arrCells[iSelectedCell + 1][jSelectedCell - 1].CheckerOnCell;
                        availableCellsFromMove.Last().KillCheckerAfterMove = true;
                        selectedCell.CanKillMore = true;
                    }
                }
                else if (isCanMove == CellState.CanMove)
                {
                    availableCellsFromMove.Add(arrCells[iSelectedCell + 1][jSelectedCell - 1]);
                    availableCellsFromMove.Last().CanMoveOnThisCell = true;
                }
                #endregion

                #region"Движение черной шашки вперед-вправо"
                isCanMove = CheckCell(iSelectedCell - 1, jSelectedCell - 1);
                if (isCanMove == CellState.Enemy)
                {
                    isCanMove = CheckCell(iSelectedCell - 2, jSelectedCell - 2);
                    if (isCanMove == CellState.CanMove)
                    {
                        availableCellsFromMove.Add(arrCells[iSelectedCell - 2][jSelectedCell - 2]);
                        availableCellsFromMove.Last().CanMoveOnThisCell = true;
                        availableCellsFromMove.Last().KillСhecker = arrCells[iSelectedCell - 1][jSelectedCell - 1].CheckerOnCell;
                        availableCellsFromMove.Last().KillCheckerAfterMove = true;
                        selectedCell.CanKillMore = true;
                    }
                }
                else if (isCanMove == CellState.CanMove)
                {
                    availableCellsFromMove.Add(arrCells[iSelectedCell - 1][jSelectedCell - 1]);
                    availableCellsFromMove.Last().CanMoveOnThisCell = true;
                }
                #endregion

                #region"Проверка возможности движения черной шашки назад-вправо"
                isCanMove = CheckCell(iSelectedCell - 1, jSelectedCell + 1);
                if (isCanMove == CellState.Enemy)
                {
                    isCanMove = CheckCell(iSelectedCell - 2, jSelectedCell + 2);
                    if (isCanMove == CellState.CanMove)
                    {
                        availableCellsFromMove.Add(arrCells[iSelectedCell - 2][jSelectedCell + 2]);
                        availableCellsFromMove.Last().CanMoveOnThisCell = true;
                        availableCellsFromMove.Last().KillСhecker = arrCells[iSelectedCell - 1][jSelectedCell + 1].CheckerOnCell;
                        availableCellsFromMove.Last().KillCheckerAfterMove = true;
                        selectedCell.CanKillMore = true;
                    }
                }
                #endregion

                #region"Проверка возможности движения черной шашки назад-влево"
                isCanMove = CheckCell(iSelectedCell + 1, jSelectedCell + 1);
                if (isCanMove == CellState.Enemy)
                {
                    isCanMove = CheckCell(iSelectedCell + 2, jSelectedCell + 2);
                    if (isCanMove == CellState.CanMove)
                    {
                        availableCellsFromMove.Add(arrCells[iSelectedCell + 2][jSelectedCell + 2]);
                        availableCellsFromMove.Last().CanMoveOnThisCell = true;
                        availableCellsFromMove.Last().KillСhecker = arrCells[iSelectedCell + 1][jSelectedCell + 1].CheckerOnCell;
                        availableCellsFromMove.Last().KillCheckerAfterMove = true;
                        selectedCell.CanKillMore = true;
                    }
                }
                #endregion
            }

        }
        private CellState CheckCell(int i, int j)
        {
            try
            {
                if (arrCells[i][j].CheckerOnCell != null && arrCells[i][j].CheckerOnCell.ColorChecker != MoveColor)
                    return CellState.Enemy;
                else if (arrCells[i][j].CheckerOnCell == null)
                    return CellState.CanMove;
                else
                    return CellState.CantMove;
            }
            catch
            {
                return CellState.CantMove;
            }
        }
        private void TimerGame_Tick(object sender, EventArgs e)
        {
            timeGame = timeGame.Add(new TimeSpan(0, 0, 1));
            ;
            timeToolStripMenuItem.Text = timeGame.Minutes + ":" + timeGame.Seconds;
        }
        
    }

}
