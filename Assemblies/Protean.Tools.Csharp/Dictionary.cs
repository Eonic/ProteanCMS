using System;
using System.Collections;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace Protean.Tools
{
    public static class Dictionary
    {
        public enum Dimension
        {
            Key = 0,
            Value = 1
        }

        /// <summary>
        ///     Creates a hashtable based on a colon and comma separated list of key/value string pairs.
        /// </summary>
        /// <param name="cCSVString">Format is "key1:value1,key2:value2,key3:value3 etc.</param>
        /// <returns>Hashtable</returns>
        /// <remarks></remarks>
        public static Hashtable getSimpleHashTable(string cCSVString)
        {
            try
            {
                var hTable = new Hashtable();
                var cArray = cCSVString.Trim(',').Split(',');
                string[] cItemSplit;
                foreach (string cItem in cArray)
                {
                    cItemSplit = cItem.Split(':');
                    if (cItemSplit.Length == 2)
                    {
                        hTable.Add(cItemSplit[0], cItemSplit[1]);
                    }
                }

                return hTable;
            }
            catch (Exception ex)
            {
                return new Hashtable();
            }
        }

        /// <summary>
        ///    Creates a CSV from a hashtable.
        ///    If the key/value cannot be converted to a string, then this will fail.
        ///    If the key/value contains the separator then it will be removed.
        ///    
        /// </summary>
        /// <param name="hashTableToConvert">The hashtable to convert</param>
        /// <param name="dimension">Determines whether to use the keys or values in the list </param>
        /// <param name="separator">The separator character</param>
        /// <returns>A CSV list of the hastable key or values</returns>
        /// <remarks></remarks>
        public static string hashtableToCSV(ref Hashtable hashTableToConvert, Dimension dimension = Dimension.Value, char separator = ',')
        {
            string csvList = "";
            try
            {
                foreach (DictionaryEntry Item in hashTableToConvert)
                {
                    if (!string.IsNullOrEmpty(csvList))
                        csvList += Conversions.ToString(separator);
                    switch (dimension)
                    {
                        case Dimension.Key:
                            {
                                csvList += Strings.Replace(Conversions.ToString(Item.Key), Conversions.ToString(separator), "");
                                break;
                            }

                        case Dimension.Value:
                            {
                                csvList += Strings.Replace(Conversions.ToString(Item.Value), Conversions.ToString(separator), "");
                                break;
                            }
                    }
                }

                return csvList;
            }
            catch (Exception ex)
            {
                return "";
            }
        }
    }
}