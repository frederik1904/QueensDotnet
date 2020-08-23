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
            MultiThreadedQueens queens = new MultiThreadedQueens();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var NoOfSolutions = await queens.FindNoOfSolutions(16);
            stopwatch.Stop();

            Console.WriteLine($"Found {queens.NoOfSolutions} solutions for 16 queens, it took {stopwatch.ElapsedMilliseconds}ms");
        }
    }

    public class Queen
    {
        public int NoOfQueens { get; set; }
        public int NoOfSolutions { get; set; }
        private int[] QueenPositions;

        public Queen(int noOfQueens, ReaderWriterLock rw)
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
                int diff = (col - i);
                if (   pos == row
                    || pos == row - diff
                    || pos == row + diff
                    )
                    {
                    return false;
                    }
            }
            return true;
        }
        
        public bool SetPos(int index, int value)
        {
            if (LegalMove(index, value))
            {
                QueenPositions[index] = value;
                return true;
            }

            return false;
        }
    }

    class MultiThreadedQueens
    {
        public int NoOfSolutions { get; set; }
        private ReaderWriterLock rw = new ReaderWriterLock();

        public async Task<int> FindNoOfSolutions(int NoOfQueens)
        {
            List<Queen> queens = new List<Queen>();
            List<Queen> oddQueens = new List<Queen>();
            int halfQueens = NoOfQueens / 2;
            int noOfThreads = halfQueens * NoOfQueens;

            if (NoOfQueens % 2 == 1)
            {
                noOfThreads += NoOfQueens;
            }

            CountdownEvent countdown = new CountdownEvent(noOfThreads);

            if (NoOfQueens % 2 ==  1)
            {
                for (int i = 0; i < NoOfQueens; i++)
                {
                  oddQueens.Add(CreateThread(NoOfQueens, halfQueens + 1, i, countdown));  
                }    
            }
            
            for (int i = 0; i < halfQueens; i++)
            {
                for (int j = 0; j < NoOfQueens; j++)
                {
                    queens.Add(CreateThread(NoOfQueens, i, j, countdown));
                }
            }
            countdown.Wait();
            NoOfSolutions = 
                2 * queens.Aggregate(0, (acc, x) => acc + x.NoOfSolutions)
                + oddQueens.Aggregate(0, (acc, x) => acc + x.NoOfSolutions);
            return NoOfSolutions;
        }

        private Queen CreateThread(int NoOfQueens, int i, int j, CountdownEvent countdown)
        {
            Queen q = new Queen(NoOfQueens, rw);
            q.SetPos(0, i);
            if (!q.SetPos(1, j))
            {
                countdown.Signal();
                return q;
            }
            ThreadPool.QueueUserWorkItem(x =>
            {
                q.FindSolutions(2);
                countdown.Signal();
            });

            return q;
        }
    }
}
