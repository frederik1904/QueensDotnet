using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Queens
{
    class MainClass
    {
        public static async System.Threading.Tasks.Task Main(string[] args)
        {
            for (int i = 2; i <= 14; i++)
            {
                MultiThreadedQueens queens = new MultiThreadedQueens();

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                var NoOfSolutions = await queens.FindNoOfSolutions(i);
                stopwatch.Stop();

                Console.WriteLine($"Found {queens.NoOfSolutions} solutions for {i} queens, it took {stopwatch.ElapsedMilliseconds}ms");
            }
        }
    }

    public class Queen
    {
        public int NoOfQueens { get; set; }
        public int NoOfSolutions { get; set; }
        private int[] QueenPositions;

        public Queen(int noOfQueens)
        {
            NoOfQueens = noOfQueens;
            NoOfSolutions = 0;
            QueenPositions = new int[noOfQueens];
        }

        public void FindSolutions(int pos)
        {
            if (pos == NoOfQueens)
            {
                this.NoOfSolutions++;
                return;
            }

            for (int i = 0; i < NoOfQueens; i++)
            {
                if (LegalMove(pos, i))
                {
                    QueenPositions[pos] = i;
                    FindSolutions(pos + 1);
                }
            }
        }

        private bool LegalMove(int col, int row)
        {
            for (int i = col - 1; i >= 0; i--)
            {
                int pos = QueenPositions[i];
                if (   pos == row
                    || pos == row - (col - i)
                    || pos == row + (col - i)
                    )
                    {
                    return false;
                    }
            }
            return true;
        }

        private void Print()
        {
            for (int i = 0; i < NoOfQueens; i++)
            {
                Console.Write(convert(i, QueenPositions[i]));
            }
            Console.WriteLine();
        }

        private string convert(int row, int col) => (char)(row + 1 + 'A' - 1) + "" + col + " ";

        public void SetPos(int index, int value) => QueenPositions[index] = value;
    }

    class MultiThreadedQueens
    {
        public int NoOfSolutions { get; set; }

        public async Task<int> FindNoOfSolutions(int NoOfQueens)
        {
            List<Queen> queens = new List<Queen>();
            CountdownEvent countdown = new CountdownEvent(NoOfQueens * NoOfQueens);
            for (int i = 0; i < NoOfQueens; i++)
            {
                for (int j = 0; j < NoOfQueens; j++)
                {
                    Queen q = new Queen(NoOfQueens);
                    q.SetPos(0, i);
                    q.SetPos(1, j);
                    queens.Add(q);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(x =>
                    {
                        q.FindSolutions(2);
                        countdown.Signal();
                    }));
                }
            }
            countdown.Wait();
            NoOfSolutions = queens.Aggregate(0, (acc, x) => acc + x.NoOfSolutions);
            return NoOfSolutions;
        }
    }
}
