/*
 * File: Program.cs
 * Author: Savannah Alfaro, sea2985
 */

using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography;

namespace PrimeGen
{
    /// <summary>
    /// Class: PrimeGen
    /// Description: Class that generates primes based on a bit length and count.
    /// </summary>
    public class PrimeGen
    {
        // help message constant
        private const string HelpMessage =
            "Usage: dotnet run <bits> <count=1>\n" +
            "\t- bits - the number of bits of the prime number, this must be a multiple of 8, and at least 32 bits.\n" +
            "\t- count - the number of prime numbers to generate, defaults to 1";

        // class attributes
        private int _bits;
        private int _count;

        /// <summary>
        /// Runs a series of pre-checks before the possible prime enters the isProbablyPrime function.
        /// Checks to see if the given value is negative, odd, or divisible by any of the first 200 primes.
        /// If any of these are true, the pre-check returns false to look for another possible prime.
        /// </summary>
        /// <param name="value">(Big Integer) the given number</param>
        /// <returns>(Boolean) true if the value passes the pre-checks</returns>
        private Boolean PreCheck(BigInteger value)
        {
            // pre-check prime values
            if (value == 2 || value == 3)
                return true;

            // list of first 200 primes
            int[] primes = {
                2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97, 101,
                103, 107, 109, 113, 127, 131, 137, 139, 149, 151, 157, 163, 167, 173, 179, 181, 191, 193, 197, 199
            };
            
            // check to see that the generated value is divisible by one of the first 200 primes
            foreach (BigInteger prime in primes)
            {
                if (value % prime == 0)
                    return false;
            }
            
            // passes all pre-checks
            return true;
        }
        
        /// <summary>
        /// Generates prime numbers given a bit length and count. After each prime number is generated,
        /// the prime is printed to the console.
        /// </summary>
        public void GenPrimes()
        {
            // generates a prime for each count value
            for (int i = 1; i <= _count; i++)
            {
                // generates a prime number and prints out the value
                BigInteger primeNumber = GenPrime();
                Console.WriteLine("{0}: {1}", i, primeNumber.ToString());
                
                // appends a line in-between values
                if (i != _count)
                {
                    Console.WriteLine("");
                }
            }
        }
        
        /// <summary>
        /// Generates a prime number given a bit length and count. Once the number is generated, IsProbablyPrime is
        /// called to see if the number is prime. If the number is prime, the prime is returned.
        /// </summary>
        private BigInteger GenPrime()
        {
            // create random number generator and byte array
            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            byte[] bytes = new byte[_bits / 8];
            BigInteger prime = new BigInteger(-1);

            Parallel.For(1, int.MaxValue, (index, thread) => {
                // generate random number
                rng.GetBytes(bytes);
                var number = BigInteger.Abs(new BigInteger(bytes));
                
                // pre-check generated prime
                if (PreCheck(number))
                {
                    // check to see if the generated number is prime
                    if (number.IsProbablyPrime())
                    {
                        prime = number;
                        thread.Stop();
                    }
                }
            });
            
            // returns the generated prime
            return prime;
        }

        /// <summary>
        /// Main method to instantiate and run the PrimeGen program.
        /// </summary>
        /// <param name="args">(string) Command line arguments</param>
        public static void Main(string[] args)
        {
            // instantiate Prime Gen
            var primeGen = new PrimeGen();

            try
            {
                // check command line arguments
                if (args.Length != 1 && args.Length != 2)
                {
                    Console.WriteLine(HelpMessage);
                    Environment.Exit(0);
                }
                else if (args.Length == 1)
                {
                    primeGen._bits = Int32.Parse(args[0]);
                    primeGen._count = 1;
                }
                else
                {
                    primeGen._bits = Int32.Parse(args[0]);
                    primeGen._count = Int32.Parse(args[1]);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(HelpMessage);
                Environment.Exit(0);
            }

            // print number of bits
            Console.WriteLine("BitLength: {0} bits", primeGen._bits);
                
            // start timer
            var timer = new Stopwatch();
            timer.Start();
            
            // generate random primes
            primeGen.GenPrimes();

            // end timer
            timer.Stop();

            // print time to generate
            Console.WriteLine("Time to Generate: {0}", timer.Elapsed);
        }
    }

    /// <summary>
    /// Class: IsPrime
    /// Description: Class that determines if a number is probably prime.
    /// </summary>
    static class IsPrime
    {
        /// <summary>
        /// Method that determines if a number is probably prime using the Miller Rabin primality test.
        /// </summary>
        /// <param name="value">(Big Integer) the given number</param>
        /// <param name="k">(k) the number of rounds of testing</param>
        /// <returns>(Boolean) true if the value is probably prime</returns>
        public static Boolean IsProbablyPrime(this BigInteger value, int k = 10)
        {
            // find r and d such that n = (2 ^ r) * d + 1
            var r = 0;
            BigInteger d = value - 1;

            // factor out powers of two
            while(d % 2 == 0)
            {
                r += 1;
                d >>= 1;
            }

            // create random number generator and byte array
            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            byte[] bytes = new byte[value.ToByteArray().LongLength];

            // witness loop
            for (var i = 0; i < k; i++)
            {
                // generate a random integer a in range [2, n - 2]
                BigInteger a;
                do
                {
                    rng.GetBytes(bytes);
                    a = new BigInteger(bytes);
                }
                while (a < 2 || a >= value - 2);

                // x = (a ^ d) mod n
                BigInteger x = BigInteger.ModPow(a, d, value);
                if (x == 1 || x == value - 1)
                    continue;

                // repeat r - 1 times
                for (var j = 0; j < r - 1; j++)
                {
                    x = BigInteger.ModPow(x, 2, value);
                    if (x == 1)
                        // composite
                        return false;
                    if (x == value - 1)
                        break;
                }

                if (x != value - 1)
                    // composite
                    return false;
            }
            // probably prime
            return true;
        }
    }
}