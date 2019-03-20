using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace Protean.Tools
{
    public static partial class Number
    {
        public static decimal RoundUp(object nNumber, int nDecimalPlaces = 2, int nSplitNo = 5)
        {
            try
            {
                // get the dross over with
                if (!Information.IsNumeric(nNumber))
                    return 0;
                // no decimal places to deal with
                if (!nNumber.ToString().Contains("."))
                    return (decimal)nNumber;
                // has correct number of decimal places
                if (Strings.Split(nNumber.ToString(), ".")[1].Length <= nDecimalPlaces)
                    return (decimal)nNumber;

                // now the fun
                int nWholeNo = int.Parse(Strings.Split(nNumber.ToString(), ".")[0]); // the whole number before decimal point
                int nTotalLength = Strings.Split(nNumber.ToString(), ".")[1].Length; // the total number of decimal places

                int nI; // a counter

                int nCarry = 0; // number to carry to next number
                var loopTo = nTotalLength - nDecimalPlaces;

                // loop through until we reach the correct number of decimal places
                for (nI = 0; nI <= loopTo; nI++)
                {
                    int nCurrent; // the number we are working on
                    nCurrent = int.Parse(Strings.Right(Strings.Left(Strings.Split(nNumber.ToString(), ".")[1], nTotalLength - nI), 1));
                    nCurrent += nCarry; // add the carry
                    if (nCurrent >= nSplitNo)
                        nCarry = 1;
                    else
                        nCarry = 0; // make a new carry dependant on whaere we are
                }
                int nDecimal = int.Parse(Strings.Left(Strings.Split(nNumber.ToString(), ".")[1], nDecimalPlaces)); // the decimal value
                nDecimal += nCarry; // add last carry
                if (nDecimal.ToString().Length > nDecimalPlaces)
                {
                    nCarry = 1;
                    nDecimal = int.Parse(Strings.Right(nDecimal.ToString(), nDecimalPlaces));
                }
                else
                    nCarry = 0;
                nWholeNo += nCarry;
                return System.Convert.ToDecimal(nWholeNo + "." + nDecimal);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public static double MinimumValue(double Value, double Min)
        {
            try
            {
                if (Value < Min)
                    return Min;
                else
                    return Value;
            }
            catch (Exception ex)
            {
                return Min;
            }
        }

        public static int ConvertStringToIntegerWithFallback(string input, int defaultInteger = 0)
        {
            try
            {
                if (IsReallyNumeric(input))
                    return Convert.ToInt32(input);
                else
                    return defaultInteger;
            }
            catch (Exception ex)
            {
                return defaultInteger;
            }
        }

        public static bool CheckAndReturnStringAsNumber(string input, ref object numberReturn, Type numericType)
        {
            bool isNumber = false;
            try
            {
                if (IsReallyNumeric(input))
                {
                    // Try to Convert this to the right type
                    numberReturn = Convert.ChangeType(input, numericType);
                    isNumber = true;
                }
            }
            catch (Exception ex)
            {
            }

            return isNumber;
        }
        public static bool IsEven(long inputNumber)
        {
            return (inputNumber % 2 == 0);
        }

        public static bool IsOdd(long inputNumber)
        {
            return !IsEven(inputNumber);
        }

        public static bool IsStringNumeric(string input)
        {
            return !(string.IsNullOrEmpty(input)) && Information.IsNumeric(input);
        }


        /// <summary>
        /// IsNumeric is rubbish (http://classicasp.aspfaq.com/general/what-is-wrong-with-isnumeric.html)
        /// Here's a string check.
        /// The regular expression can be summed up this way
        /// 1. "-" or "+" character, zero or one time
        /// 2. one or more digit
        /// 3. Optionally: "." followed by one or more digit
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool IsReallyNumeric(string input)
        {
            try
            {
                return System.Text.RegularExpressions.Regex.IsMatch(input, @"^(-|\+)?\d+(\.\d+)?$");
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }

}
