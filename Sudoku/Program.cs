using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Sudoku
{
    class Program
    {
        static void Main(string[] args)
        {





            var b1 = Board.ReadInString("000010000007905400000804000001000200470030096000090000016050940004321600308000102");
            var board1 = new Board(b1);
            board1.PrintOut();
            Console.Out.WriteLine("going to solve now");
            Console.Out.WriteLine("\n\n");
            board1.Solve();
            board1.IsValid();
            //board1.Assign();
            Console.Read();
        }
    }

    class Board
    {
        readonly byte[,] _board;
        readonly List<int>[,] _options;

        readonly byte[] _b = new byte[9];
        public Board(byte[,] initBoard)
        {
            // check bounds
            _board = initBoard;
            _options = new List<int>[9, 9];
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (_board[i, j] == 0)
                    {
                        _options[i, j] = new List<int>(Enumerable.Range(1, 9));
                    }
                }
            }
            IsValid();
        }

        public static byte[,] ReadIn(string filename)
        {
            string data = string.Join("", File.ReadAllLines(filename));
            return ReadInString(data);
        }

        public static byte[,] ReadInString(string text)
        {
            byte[,] b = new byte[9, 9];
            var data = text.Replace("\n", "").Replace("|", "").Replace("-", "");
            for (int index = 0; index < 9; index++)
            {
                for (int i = 0; i < 9; i++)
                {
                    byte value;
                    if (data[index * 9 + i] != '0' && byte.TryParse(data[index * 9 + i].ToString(), out value))
                    {
                        b[index, i] = value;
                    }
                }
            }
            return b;
        }

        public void IsValid()
        {
            for (int i = 0; i < 9; i++)
            {
                Array.Clear(_b, 0, 9);
                for (int j = 0; j < 9; j++)
                {
                    var val = _board[i, j] - 1;
                    if (val != -1)
                    {
                        if (_b[val] != 0)
                        {
                            throw new Exception(string.Format("invalid number at {0}, {1}", i, j));
                        }
                        _b[val] = 1;
                    }
                }
            }
            for (int i = 0; i < 9; i++)
            {
                Array.Clear(_b, 0, 9);
                Console.WriteLine("\n\n");
                for (int j = 0; j < 3; j++)
                {
                    Console.WriteLine("");
                    for (int k = 0; k < 3; k++)
                    {
                        var y = j + (i / 3) * 3;
                        var x = k + (i % 3) * 3;
                        var val = _board[y, x] - 1;
                        if (val != -1)
                        {
                            if (_b[val] != 0)
                            {
                                throw new Exception("invalid number");
                            }
                            _b[val] = 1;
                        }
                    }
                }
            }
        }

        public void PrintOut()
        {
            for (int rows = 0; rows < 9; rows++)
            {
                Console.Out.WriteLine("------------------------------------------------------------------------------------------");
                for (int cellRow = 0; cellRow < 3; cellRow++)
                {
                    Console.Write("|");
                    for (int column = 0; column < 9; column++)
                    {
                        for (int cellColumn = 0; cellColumn < 3; cellColumn++)
                        {
                            var x = _board[rows, column];
                            if (x != 0 && cellColumn == 0 && cellRow == 0)
                            {
                                Console.Write(" {0} ", x);
                            }
                            else if (x == 0)
                            {
                                var items = this._options[rows, column];
                                int offset = cellRow * 3;
                                int index = cellColumn + offset;
                                var val = items[index];
                                if (val != 0)
                                {
                                    Console.Write(" {0} ", val);
                                }
                                else
                                {
                                    Console.Write("   ");
                                }
                            }
                            else
                            {
                                Console.Write("   ");
                            }
                        }
                        Console.Write("|");
                    }
                    Console.WriteLine("");
                }
            }
            Console.WriteLine("------------------------------------------------------------------------------------------");
        }

        public void Solve()
        {
            var go = true;
            while (go)
            {
                go = SolveRow() | SolveColumn() | SolveCube() | Prune() | Assign();
                this.PrintOut();
                //Thread.Sleep(250);
            }
        }

        public bool SolveRow()
        {
            bool solved = false;
            for (int i = 0; i < 9; i++)
            {
                var nums = Enumerable.Range(1, 9).ToList();
                for (int j = 0; j < 9; j++)
                {
                    var val = _board[i, j];
                    if (val > 0)
                    {
                        nums.Remove(val);
                    }
                }
                if (nums.Count == 1)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        var val = _board[i, j];
                        if (val == 0)
                        {
                            this.Set(i, j, nums[0]);
                            solved = true;

                        }
                    }
                }
            }
            return solved;
        }

        public bool SolveColumn()
        {
            bool solved = false;
            for (int i = 0; i < 9; i++)
            {
                var nums = Enumerable.Range(1, 9).ToList();
                for (int j = 0; j < 9; j++)
                {
                    var val = _board[j, i];
                    if (val > 0)
                    {
                        nums.Remove(val);
                    }
                }
                if (nums.Count == 1)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        var val = _board[j, i];
                        if (val == 0)
                        {
                            this.Set(j, i, nums[0]);

                            solved = true;
                        }
                    }
                }
            }
            return solved;
        }


        private bool PruneRow()
        {
            bool pruned = false;
            for (int i = 0; i < 9; i++)
            {
                var seen = new List<int>();
                for (int j = 0; j < 9; j++)
                {
                    var val = _board[i, j];
                    if (val != 0)
                    {
                        seen.Add(val);
                    }
                }
                for (int j = 0; j < 9; j++)
                {
                    var val = _board[i, j];
                    if (val == 0)
                    {
                        List<int> options = _options[i, j];
                        for (int index = 0; index < options.Count; index++)
                        {
                            int option = options[index];
                            if (seen.Contains(option))
                            {
                                options[options.IndexOf(option)] = 0;
                                pruned = true;
                            }
                        }
                    }
                }
            }
            return pruned;
        }

        public bool Assign()
        {
            var modified = false;
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    var val = this._board[i, j];
                    if (val == 0)
                    {
                        var options = this._options[i, j];
                        if (options.Count(x => x == 0) == 8)
                        {
                            int found = options.First(x => x > 0);
                            this.Set(i, j, found);
                            modified = true;
                        }
                    }

                }
            }



            // by row
            for (int i = 0; i < 9; i++)
            {
                //if(i == 6)
                //    Console.Out.WriteLine("here");
                List<int>[] items = new List<int>[9];
                for (int j = 0; j < 9; j++)
                {
                    var val = this._board[i, j];
                    if (val == 0)
                    {
                        var options = this._options[i, j];
                        foreach (int possible in options.Where(x => x > 0))
                        {
                            var index = possible - 1;
                            if (null == items[index])
                            {
                                items[index] = new List<int>();
                            }
                            items[index].Add(j);
                        }
                    }
                }
                for (int index = 0; index < items.Length; index++)
                {
                    List<int> item = items[index];
                    if (null != item && item.Count == 1)
                    {
                        Console.Out.WriteLine("before");
                        this.PrintOut();
                        int col = item[0];
                        this.Set(i, col, index + 1);
                        modified = true;
                        Console.Out.WriteLine("after");
                        this.PrintOut();
                        this.IsValid();

                    }
                }
            }







            // by col
            for (int i = 0; i < 9; i++)
            {
                //if(i == 6)
                //    Console.Out.WriteLine("here");
                List<int>[] items = new List<int>[9];
                for (int j = 0; j < 9; j++)
                {
                    var val = this._board[j, i];
                    if (val == 0)
                    {
                        var options = this._options[j, i];
                        foreach (int possible in options.Where(x => x > 0))
                        {
                            var index = possible - 1;
                            if (null == items[index])
                            {
                                items[index] = new List<int>();
                            }
                            items[index].Add(j);
                        }
                    }
                }
                for (int index = 0; index < items.Length; index++)
                {
                    List<int> item = items[index];
                    if (null != item && item.Count == 1)
                    {
                        Console.Out.WriteLine("before");
                        this.PrintOut();
                        int row = item[0];
                        this.Set(row, i, index + 1);
                        modified = true;
                        Console.Out.WriteLine("after");
                        this.PrintOut();
                        this.IsValid();

                    }
                }
            }



            return modified;

        }

        public bool Prune()
        {
            return this.PruneColumn() | this.PruneRow() | this.PruneCube();
        }

        private bool PruneCube()
        {
            bool pruned = false;
            for (int cubeY = 0; cubeY < 3; cubeY++)
            {
                for (int cubeX = 0; cubeX < 3; cubeX++)
                {
                    var seen = new List<int>();
                    for (int row = 0; row < 3; row++)
                    {
                        for (int column = 0; column < 3; column++)
                        {
                            var y = row + 3 * cubeY;
                            var x = column + 3 * cubeX;
                            var val = _board[x, y];
                            if (val > 0)
                            {
                                seen.Add(val);
                            }
                        }
                    }
                    for (int row = 0; row < 3; row++)
                    {
                        for (int column = 0; column < 3; column++)
                        {
                            var y = row + 3 * cubeY;
                            var x = column + 3 * cubeX;
                            var val = _board[x, y];
                            if (val == 0)
                            {
                                List<int> options = _options[x, y];
                                for (int index = 0; index < options.Count; index++)
                                {
                                    int option = options[index];
                                    if (seen.Contains(option))
                                    {
                                        options[options.IndexOf(option)] = 0;
                                        pruned = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return pruned;
        }

        void Set(int i, int j, int val)
        {
            Console.Out.WriteLine("assigning {0}, {1} = {2}", i, j, val);
            _board[i, j] = (byte) val;
            _options[i, j] = null;
            this.Prune();
        }

        private bool PruneColumn()
        {
            bool pruned = false;
            for (int i = 0; i < 9; i++)
            {
                var seen = new List<int>();
                for (int j = 0; j < 9; j++)
                {
                    var val = _board[j, i];
                    if (val != 0)
                    {
                        seen.Add(val);
                    }
                }
                for (int j = 0; j < 9; j++)
                {
                    var val = _board[j, i];
                    if (val == 0)
                    {
                        List<int> options = _options[j, i];
                        for (int index = 0; index < options.Count; index++)
                        {
                            int option = options[index];
                            if (seen.Contains(option))
                            {
                                options[options.IndexOf(option)] = 0;
                                pruned = true;
                            }
                        }
                    }
                }
            }
            return pruned;
        }


        private bool SolveCube()
        {
            bool solved = false;
            for (int cubeY = 0; cubeY < 3; cubeY++)
            {
                for (int cubeX = 0; cubeX < 3; cubeX++)
                {
                    var nums = Enumerable.Range(1, 9).ToList();
                    for (int row = 0; row < 3; row++)
                    {
                        for (int column = 0; column < 3; column++)
                        {
                            var y = row + 3 * cubeY;
                            var x = column + 3 * cubeX;
                            var val = _board[x, y];
                            if (val > 0)
                            {
                                nums.Remove(val);
                            }
                        }
                    }
                    if (nums.Count == 1)
                    {
                        for (int row = 0; row < 3; row++)
                        {
                            for (int column = 0; column < 3; column++)
                            {
                                var y = row + 3 * cubeY;
                                var x = column + 3 * cubeX;
                                var val = _board[x, y];
                                if (val == 0)
                                {
                                    _board[x, y] = (byte)nums[0];
                                    solved = true;
                                    _options[x, y] = null;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return solved;
        }

    }
}