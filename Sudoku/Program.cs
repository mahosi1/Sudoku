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
            //var b1 = Board.ReadIn(@"sample.txt");
            //var b1 = Board.ReadInString("008450371020097560503800492300705849280030056745609003837006904012970030956043100");
            //var b1 = Board.ReadInString("..845.371.2..9756.5.38..4923..7.584928..3..567456.9..3837..69.4.1297..3.956.431..");
            var b1 = Board.ReadInString("  845 371 2  9756 5 38  4923  7 584928  3  567456 9  3837  69 4 1297  3 956 431  ");
            var board1 = new Board(b1);
            board1.PrintOut2();
            Console.Out.WriteLine("going to solve now");
            Console.Out.WriteLine("\n\n");
            board1.Solve();
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
            var data = text.Replace("\n", "").Replace("|", "").Replace("-","");
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
                            throw new Exception("invalid number");
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
        
        public void PrintOut2()
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
                go = SolveRow() | SolveColumn() | SolveCube() | PruneRow() | PruneColumn() | PruneCube();
                this.PrintOut2();
                Thread.Sleep(5000);
            }
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
                            this._board[i, j] = (byte)found;
                            this._options[i, j] = null;
                        }
                    }

                }
            }
                return pruned;
        }

        private bool PruneCube()
        {
            bool solved = false;
            for (int cubeY = 0; cubeY < 3; cubeY++)
            {
                for (int cubeX = 0; cubeX < 3; cubeX++)
                {
                    var seen = new List<int>();
                    for (int row = 0; row < 3; row++)
                    {
                        for (int column = 0; column < 3; column++)
                        {
                            var y = row + 3*cubeY;
                            var x = column + 3*cubeX;
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
                            var y = row + 3*cubeY;
                            var x = column + 3*cubeX;
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
                                        solved = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
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
                            this._board[i, j] = (byte)found;
                            this._options[i, j] = null;
                        }
                    }
                }
            }
            return solved;
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
                            this._board[i, j] = (byte)found;
                            this._options[i, j] = null;
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
                            _board[i, j] = (byte)nums[0];
                            solved = true;
                            _options[i, j] = null;

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
                            _board[j, i] = (byte)nums[0];
                            solved = true;
                            _options[j, i] = null;
                        }
                    }
                }
            }
            return solved;
        }
    }
}