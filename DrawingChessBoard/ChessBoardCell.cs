using System.Drawing;
using System.Windows.Forms;

namespace DrawingChessBoard
{
    public class ChessBoardCell : Label
    {
        public Point CoordinatesCell
        {
            get
            {
                return Location;
            }
            set
            {
                Point val = new Point(value.X + 1, value.Y + 1);
                Location = val;
            }
        }             //Местоположение ячейки - Location
        public Size SizeCell
        {
            get
            {
                return Size;
            }

            set
            {
                Size val = new Size(value.Width - 2, value.Height - 2);
                Size = val;
                if (CheckerOnCell != null)
                {
                    CheckerOnCell.Size = new Size(SizeCell.Width, SizeCell.Height);
                }
            }
        }                     //Размер ячейки - Size
        public Color ColorCell { get; set; }        //Цвет ячейки (черный/белый)
        public Сhecker CheckerOnCell
        {
            get { return checkerOnCell; }
            set
            {
                checkerOnCell = value;
            }
        }             //Шашка этой ячейки
        public Сhecker KillСhecker { get; set; }    //Шашка которую нужно убрать
        public bool CanMoveOnThisCell
        {
            get { return canMoveOnThisCell; }
            set
            {
                canMoveOnThisCell = value;

                if (canMoveOnThisCell)
                {
                    Pen p = new Pen(Color.Green);
                    p.Width = 3;
                    parentForm.CreateGraphics().DrawRectangle(p, new Rectangle(Location, Size));
                    BackColor = Color.GreenYellow;
                }
                else
                {
                    Pen p = new Pen(Color.SaddleBrown);
                    p.Width = 3;
                    parentForm.CreateGraphics().DrawRectangle(p, new Rectangle(Location, Size));
                    BackColor = Color.DarkGoldenrod;
                }
            }
        }            //Возможность нахождения шашки на этой ячейке
        public bool KillCheckerAfterMove { get; set; }//Для проверки убийства перепрыгиваемой шашки
        public bool CanKillMore { get; set; }       //Возможность дополнительного убийства шашки после действия

        Form1 parentForm;
        private Сhecker checkerOnCell;
        private bool canMoveOnThisCell = false;

        public ChessBoardCell(int iSize, Color colorCell, Point location, Form1 parent)
        {
            CheckerOnCell = null;
            SizeCell = new Size(iSize, iSize);
            CoordinatesCell = location;
            ColorCell = colorCell;
            this.parentForm = parent;

            MouseHover += ChessBoardCell_MouseHover;
            MouseLeave += ChessBoardCell_MouseLeave;
            MouseClick += ChessBoardCell_MouseClick;
        }

        internal void ChessBoardCell_MouseClick(object sender, MouseEventArgs e)
        {
            if (CheckerOnCell != null)
            {
                if (parentForm.SelectedСhecker == null)
                {
                    if (CheckerOnCell.ColorChecker == parentForm.MoveColor)
                    {
                        parentForm.FoundAvailableCells(this);
                        if (parentForm.availableCellsFromMove.Count > 0)
                        {
                            parentForm.SelectedСhecker = CheckerOnCell;
                            parentForm.SelectedСhecker.BackColor = Color.Gold;
                        }
                    }
                }
                else if (parentForm.SelectedСhecker == CheckerOnCell)
                {
                    parentForm.SelectedСhecker.BackColor = Color.DarkGoldenrod;
                    parentForm.SelectedСhecker = null;

                    foreach (ChessBoardCell cell in parentForm.availableCellsFromMove)
                    {
                        cell.KillCheckerAfterMove = false;
                        cell.CanMoveOnThisCell = false;
                    }
                    parentForm.availableCellsFromMove.Clear();
                }

            }
            else if (parentForm.SelectedСhecker != null && CheckerOnCell == null && ColorCell == Color.Black&&canMoveOnThisCell)//Выбрана шашка , и в текущей клетке нет шашки
            {
                #region"Перемещение выбранной шашки"
                parentForm.SelectedСhecker.ParentCell.CheckerOnCell = null;
                parentForm.SelectedСhecker.ParentCell.Controls.Remove(parentForm.SelectedСhecker);

                parentForm.SelectedСhecker.Parent = this;
                parentForm.SelectedСhecker.ParentCell = this;

                CheckerOnCell = parentForm.SelectedСhecker;
                #endregion

                if (KillCheckerAfterMove)
                {
                    KillСhecker.ParentCell.CheckerOnCell = null;
                    KillСhecker.ParentCell.Controls.Remove(KillСhecker);

                    KillСhecker.Parent = null;
                    KillСhecker.ParentCell = null;

                    if (parentForm.MoveColor == Color.White)
                        parentForm.ScoreWhite++;
                    else
                        parentForm.ScoreBlack++;



                    CanKillMore = false;
                    ChessBoardCell_MouseClick(sender, e);
                    ChessBoardCell_MouseClick(sender, e);
                    ;
                    if(CanKillMore)
                        parentForm.MoveColor = parentForm.MoveColor == Color.Black ? Color.Black : Color.White;//Тот же цвет ходит
                    else if(parentForm.MoveColor != Color.Gray)
                    {
                        parentForm.MoveColor = parentForm.MoveColor == Color.White ? Color.Black : Color.White;//Ход следующего цвета      
                        ChessBoardCell_MouseClick(sender, e);
                    }
                    ChessBoardCell_MouseClick(sender, e);

                }
                else
                {
                    parentForm.MoveColor = parentForm.MoveColor == Color.White ? Color.Black : Color.White;//Ход следующего цвета
                }

                ChessBoardCell_MouseClick(sender, e);


            }
        }
        internal void ChessBoardCell_MouseLeave(object sender, System.EventArgs e)
        {
            ChessBoardCell over = sender as ChessBoardCell;
            Pen p = new Pen(Color.SaddleBrown);
            p.Width = 3;
            parentForm.CreateGraphics().DrawRectangle(p, new Rectangle(over.Location, over.Size));
        }
        internal void ChessBoardCell_MouseHover(object sender, System.EventArgs e)
        {
            ChessBoardCell over = sender as ChessBoardCell;
            Pen p = new Pen(over.CheckerOnCell != null ? (over.CheckerOnCell.ColorChecker == Color.White ? Color.Green : Color.Red) : Color.Gold);
            p.Width = 3;
            parentForm.CreateGraphics().DrawRectangle(p, new Rectangle(over.Location, over.Size));
        }

        public void CreateChecker(Color colorChecker)
        {
            if (CheckerOnCell == null)
            {
                CheckerOnCell = new Сhecker(SizeCell.Width, this, colorChecker);
                Controls.Add(CheckerOnCell);
            }
        }
    }
}