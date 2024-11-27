using System;

namespace Protean.Tools
{
    public static partial class Number
    {
        public static decimal RoundUp(object nNumber, int nDecimalPlaces = 2, int nSplitNo = 5)
        {
            try
            {
                string[] parts = nNumber.ToString().Split('.');

                string decimalPart = nNumber.ToString().Split('.')[1]; // Get the decimal part
                string leftPart = decimalPart.Substring(0, Math.Min(nDecimalPlaces, decimalPart.Length));

                // get the dross over with
                if (!double.TryParse(nNumber.ToString(), out _))
                    return 0;
                // no decimal places to deal with
                if (!nNumber.ToString().Contains("."))
                    return (decimal)nNumber;
                // has correct number of decimal places
                if (parts.Length > 1 && parts.Length <= nDecimalPlaces)
                    return (decimal)nNumber;

                // now the fun
                int nWholeNo = int.Parse(nNumber.ToString().Split('.')[0]);
                // the whole number before decimal point
                int nTotalLength = parts.Length > 1 ? parts[1].Length : 0; // the total number of decimal places

                int nI; // a counter

                int nCarry = 0; // number to carry to next number
                var loopTo = nTotalLength - nDecimalPlaces;

                // loop through until we reach the correct number of decimal places
                for (nI = 0; nI <= loopTo; nI++)
                {
                    char lastChar = nNumber.ToString().Split('.')[1].Substring(0, nTotalLength - nI)[nNumber.ToString().Split('.')[1].Substring(0, nTotalLength - nI).Length - 1]; // Get the last character

                    int nCurrent; // the number we are working on
                    nCurrent = nCurrent = int.Parse(lastChar.ToString()); // Convert to integer
                    nCurrent += nCarry; // add the carry
                    if (nCurrent >= nSplitNo)
                        nCarry = 1;
                    else
                        nCarry = 0; // make a new carry dependant on whaere we are
                }
                int nDecimal = nDecimal = int.Parse(leftPart); // the decimal value
                nDecimal += nCarry; // add last carry
                if (nDecimal.ToString().Length > nDecimalPlaces)
                {
                    nCarry = 1;
                    nDecimal = int.Parse(nDecimal.ToString().Substring(Math.Max(0, nDecimal.ToString().Length - nDecimalPlaces)));
                }
                else
                    nCarry = 0;
                nWholeNo += nCarry;
                return System.Convert.ToDecimal(nWholeNo + "." + nDecimal);
            }
            catch (Exception)
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
            catch (Exception)
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
            catch (Exception)
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
            catch (Exception)
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
            return !string.IsNullOrEmpty(input) && double.TryParse(input, out _);
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
            catch (Exception)
            {
                return false;
            }
        }

        public static double DistanceCalculate(double fromLatitude, double fromLongitude,
                  double toLatitude, double toLongitude)
        {
            double x = 69.1 * (toLatitude - fromLatitude);
            double y = 69.1 * (toLongitude - fromLongitude) * Math.Cos(fromLatitude / 57.3);

            // Convert to KM by multiplying 1.609344
            return (Math.Sqrt(x * x + y * y) * 1.609344);
        }
    }

}
