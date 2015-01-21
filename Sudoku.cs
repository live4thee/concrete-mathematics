using System;
using System.Collections.Generic;

namespace Sudoku
{
    /// <summary>
    /// To represent each cell of a Sudoku board.
    /// </summary>
    class Cell : ICloneable
    {
        private List<int> candidates = new List<int>();
        private void InitCandidates()
        {
            for (int i = 1; i <= 9; ++i)
                candidates.Add(i);
        }

        public Cell(int n)
        {
            if (n != 0) candidates.Add(n);
            else InitCandidates();
        }

        private Cell(List<int> c)
        {
            candidates = c;
        }

        public object Clone()
        {
            List<int> new_c = new List<int>();
            foreach (int i in candidates) new_c.Add(i);
            return new Cell(new_c);
        }

        public int Value
        {
            get { return (candidates.Count == 1) ? candidates[0] : 0; }
        }

        public bool IsSolved()
        {
            return (this.Value != 0);
        }

        public void Exclude(int n)
        {
            if (this.Value == n)
                throw new InvalidOperationException("A Cell has been cleared");
            candidates.Remove(n);
        }

        public List<int> GetCandidates()
        {
            return candidates;
        }
    }

    class Board : ICloneable
    {
        private Cell[,] cells = new Cell[9, 9];

        public Board(int[,] puzzle)
        {
            if (!IsPuzzleOK(puzzle))
                throw new ArgumentException("Puzzle invalid");

            for (int i = 0; i < 9; ++i)
                for (int j = 0; j < 9; ++j)
                    cells[i, j] = new Cell(puzzle[i, j]);
        }

        private Board(Cell[,] cells)
        {
            this.cells = cells;
        }

        public object Clone()
        {
            Cell[,] new_cells = new Cell[9, 9];

            for (int i = 0; i < 9; ++i)
                for (int j = 0; j < 9; ++j)
                    new_cells[i, j] = cells[i, j].Clone() as Cell;

            return new Board(new_cells);
        }

        public Cell GetCell(int row, int col)
        {
            return cells[row, col];
        }

        /// <summary>
        /// Check whether input 'puzzle' is validate.
        /// </summary>
        /// <param name="puzzle"></param>
        /// <returns>boolean</returns>
        static private bool IsPuzzleOK(int[,] puzzle)
        {
            // Check the array height and width.
            int width = puzzle.GetLength(0);
            int heigth = puzzle.GetLength(1);

            if (width != 9 || heigth != 9)
                return false;

            // TODO: check duplication
            return true;
        }

        public void Show()
        {
            int cnt = 0;
            foreach (Cell c in cells)
            {
                Console.Write(c.Value);
                ++cnt;
                if (cnt % 9 != 0)
                    Console.Write(' ');
                else
                    Console.WriteLine();
            }
            Console.WriteLine();
        }

        public Cell[] GetRow(int row)
        {
            Cell[] ary = new Cell[9];

            for (int i = 0; i < 9; ++i)
                ary[i] = cells[row, i];

            return ary;
        }

        public Cell[] GetCol(int col)
        {
            Cell[] ary = new Cell[9];

            for (int i = 0; i < 9; ++i)
                ary[i] = cells[i, col];

            return ary;
        }

        public Cell[] GetGrid(int grid)
        {
            Cell[] c = new Cell[9];
            int row = (grid % 3) * 3;
            int col = (grid / 3) * 3;
            int cnt = 0;

            // Assemble each grid into an array
            for (int i = 0; i < 3; ++i)
                for (int j = 0; j < 3; ++j)
                {
                    c[cnt] = cells[row + i, col + j];
                    ++cnt;
                }

            return c;
        }

        public bool IsSolved()
        {
            foreach (Cell c in cells)
                if (!c.IsSolved())
                    return false;

            return true;
        }

    }

    static class Solver
    {
        private static bool DoScan(Cell[] cell)
        {
            bool hasProgress = false;

            for (int idx = 0; idx < 9; ++idx)
            {
                Cell c = cell[idx];
                if (c.IsSolved()) continue;

                for (int i = 0; i < 9; ++i)
                {
                    if (idx != i)
                    {
                        Cell c2 = cell[i];
                        if (c2.IsSolved())
                        {
                            c.Exclude(c2.Value);
                            if (c.IsSolved())
                                hasProgress = true;
                        }
                    }
                }
            }

            return hasProgress;
        }

        private static bool ScanRow(Board b)
        {
            bool hasProgress = false;

            for (int i = 0; i < 9; ++i)
                if (DoScan(b.GetRow(i)))
                    hasProgress = true;

            return hasProgress;
        }

        private static bool ScanColumn(Board b)
        {
            bool hasProgress = false;

            for (int i = 0; i < 9; ++i)
                if (DoScan(b.GetCol(i)))
                    hasProgress = true;

            return hasProgress;
        }

        private static bool ScanGrid(Board b)
        {
            bool hasProgress = false;

            for (int i = 0; i < 9; ++i)
                if (DoScan(b.GetGrid(i)))
                    hasProgress = true;

            return hasProgress;
        }

        private static Board DoSolver(Board b)
        {
            while (ScanRow(b) || ScanColumn(b) || ScanGrid(b))
                ;

            return b;
        }

        /// <summary>
        /// This solver will NOT modify the original board.
        /// </summary>
        /// <param name="old"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="n"></param>
        /// <returns>The last board status</returns>
        private static Board TrySolver(Board old, int row, int col, int n)
        {
            Board b = old.Clone() as Board;
            Cell c = b.GetCell(row, col);
            c.Exclude(n);

#if (DEBUG)
            Console.Write("Assuming Cell({0}, {1}) != {2} ... ", row, col, n);
#endif

            try {
                DoSolver(b);
            } catch (InvalidOperationException) {
            }

#if (DEBUG)
            if (b.IsSolved())
                Console.WriteLine("Bingo!");
            else
                Console.WriteLine("No luck.");
#endif

            return b;
        }

        public static Board RunSolver(Board b)
        {
            if (DoSolver(b).IsSolved())
                return b;

            for (int i = 0; i < 9; ++i)
                for (int j = 0; j < 9; ++j)
                {
                    Cell c = b.GetCell(i, j);
                    List<int> cdds = c.GetCandidates();

                    // make a guess for cells with two candidates
                    if (cdds.Count == 2)
                    {
                        for (int k = 0; k < cdds.Count; ++k)
                        {
                            Board res = TrySolver(b, i, j, cdds[k]);
                            if (res.IsSolved())
                                return res;
                        }
                    }
                }

            return b;
        }
    }

    static class Puzzles
    {
        private static int[,] p0 = new int[9, 9]
            {
                { 0, 6, 0, 5, 0, 0, 0, 0, 0},
                { 4, 5, 7, 0, 0, 0, 0, 8, 0},
                { 0, 0, 0, 0, 0, 0, 0, 0, 0},
                { 0, 0, 0, 0, 0, 3, 0, 1, 0},
                { 5, 0, 6, 9, 0, 0, 0, 4, 0},
                { 0, 9, 0, 2, 0, 0, 0, 0, 0},
                { 1, 0, 8, 4, 0, 7, 6, 5, 0},
                { 3, 0, 9, 1, 0, 5, 2, 0, 8},
                { 6, 0, 5, 0, 0, 8, 1, 0, 4},
            };

        private static int[,] p1 = new int[9, 9]
            {
                { 0, 1, 4, 2, 7, 0, 0, 8, 0},
                { 0, 0, 8, 6, 0, 0, 0, 3, 0},
                { 0, 0, 2, 0, 4, 0, 5, 0, 0},
                { 8, 0, 0, 0, 0, 0, 4, 0, 1},
                { 0, 0, 0, 0, 0, 0, 0, 0, 0},
                { 0, 6, 3, 1, 0, 0, 0, 9, 0},
                { 5, 0, 7, 3, 0, 0, 0, 4, 0},
                { 2, 0, 0, 7, 0, 0, 0, 0, 5},
                { 0, 0, 0, 0, 0, 0, 9, 0, 0},
            };

        private static int[,] p2 = new int[9, 9]
            {
                { 0, 0, 0, 0, 0, 0, 4, 5, 6},
                { 0, 0, 0, 0, 0, 0, 0, 0, 0},
                { 0, 5, 0, 0, 4, 6, 0, 9, 2},
                { 2, 8, 0, 7, 0, 0, 0, 0, 3},
                { 6, 0, 0, 0, 1, 0, 5, 0, 0},
                { 0, 0, 0, 0, 0, 8, 0, 0, 0},
                { 0, 0, 0, 0, 0, 0, 3, 0, 0},
                { 8, 0, 0, 9, 0, 4, 6, 0, 0},
                { 0, 2, 0, 8, 0, 0, 0, 0, 1},
            };

        public static int[][,] puzzles = { p0, p1, p2 };
    }

    class Program
    {
        static void Main(string[] args)
        {
            foreach (var p in Puzzles.puzzles)
            {
                Board b = new Board(p);
                Console.WriteLine("The puzzle:");
                b.Show();

                Board res = Solver.RunSolver(b);
                if (res.IsSolved())
                    Console.WriteLine("The answer:");
                else
                    Console.WriteLine("Not solved yet:");

                res.Show();
            }
        }
    }
}
