/*
 * Calculates the floor of the lg of the lg of a specified n without any built-in functions
 *  Since calculating the lg of a number < 2 is tough, the program floor between the lg operations
 *  Thus, this program really calculates floor(lg(floor(lg(n)))), but the result will be the same
 *  
 * Toby Sterk, CS 212
 * 20 September 2019
 */

using System;

namespace LgLg
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true) { // keep it going for easier testing
                Console.WriteLine("Enter n (or enter letters to quit): ");

                try {
                    float n = float.Parse(Console.ReadLine());
                    float result = Lg(Lg(n));
                    Console.WriteLine("floor(lg(lg(" + n + ")) = " + result);
                } catch (FormatException fe) {
                    Console.WriteLine(fe.Message + " Exiting...");
                    break;
                } catch (Exception e) {
                    Console.WriteLine(e.Message + " Please enter a number that is > 2");
                }
            }
        }
        /* The Lg function calculates the floor of the logarithm base 2 of an input
         * @param: num, the float that is the number that is being evaluated
         * @return: count, the float that is the floor of the logarithm of num
         * Throws an exception if the input is <= 0 to avoid imaginary or undefined outputs
         */
        static float Lg(float num)
        {
            float count = 0;

            if (num <= 0) { // ERROR
                throw new Exception("Result is imaginary or undefined.");
            } else if (num == 1) {
                return 0;
            } else {
                while (true) {
                    if (num == 2) {
                        count += 1;
                        break;
                    } else if (num > 2) {
                        num /= 2;
                        count += 1;
                    } else { // num < 2
                        break; // flooring intermediately is a lot easier to compute and is still accurate
                    }
                }
            }
            return count;
        }
    }
}
