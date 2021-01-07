using System;
using System.Diagnostics;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace quick_code_test
{

    //Please fill in the implementation of the service defined below. This service is to keep track
    //of ids to return if they have been seen before. No 3rd party packages can be used and the method
    //must be thread safe to call.

    //create the implementation as efficiently as possible in both locking, memory usage, and cpu usage

    public interface IDuplicateCheckService
    {

        //checks the given id and returns if it is the first time we have seen it
        //IT IS CRITICAL that duplicates are not allowed through this system but false
        //positives can be tolerated at a maximum error rate of less than 1%
        bool IsThisTheFirstTimeWeHaveSeen(int id);

    }



    // Viktor Sakson - developer notes
    //
    // Hi, this is quick solution for the task
    // This implementation has Read/Write complexity O(1)
    // CPU and Locking is optimized 
    // program is high on memory but depend of circumstances it may be ok
    // in case of high Id's input it will be still efficient
    // if amount of data will not be big then best option is to use HashSet
    // 
    // I think best way to make it is to use Bit as unit of storage so memory will be ok if high input of Ids
    //
    public class HighlyOptimizedThreadSafeDuplicateCheckService : IDuplicateCheckService
    {
        static byte[] idsBit;
        static int numberOfItemsRepeated = 0;
        static readonly object arrLock = new object();

        static int numberOfTests = 1000000; //change this to manipulate results


        private readonly ITestOutputHelper output;

        public HighlyOptimizedThreadSafeDuplicateCheckService(ITestOutputHelper output)
        {
            this.output = output;
        }



        [Fact]
        public void IsThisTheFirstTimeWeHaveSeenTest()
        {
            idsBit = new byte[268435456];

            Thread thread1 = new Thread(RunThreadTest);
            thread1.Start();

            Thread thread2 = new Thread(RunThreadTest);
            thread2.Start();


            while (thread1.IsAlive && thread2.IsAlive) {
                Thread.Sleep(200);
            }

            output.WriteLine($"Run Complete: {numberOfItemsRepeated} seen before");


        }



        private void RunThreadTest()
        {
            bool isSeen = false;
            int testNumber = 0;


            Random rand = new Random();
            for (var i = 0; i < numberOfTests; i++)
            {
                testNumber = rand.Next(1, Int32.MaxValue);
                isSeen = IsThisTheFirstTimeWeHaveSeen(testNumber);
                if (isSeen) numberOfItemsRepeated += 1;
            }

            
        }


        public bool IsThisTheFirstTimeWeHaveSeen(int id)
        {
            if (id < 0) throw new ArgumentOutOfRangeException("id");


            bool isSeen = false;

            int determineByte = (int)(id/8);
            int determineBit = (id%8);


            //move bit to right position
            byte mask = (byte)(1 << determineBit);


            //check if seen before
            lock (arrLock)
            {
                isSeen = (idsBit[determineByte] & mask) != 0;

                if (!isSeen)
                {
                    // set to 1
                    idsBit[determineByte] |= mask;
                }
            }

            return isSeen;
        }


    }
}