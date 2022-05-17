// ================================================================================================
// Protean.Tools.Counter & Protean.Tools.CounterCollection
// Desc:       A simple counter class as well as a CounterCollection
// Author:     Ali Granger
// Updated:    03-Feb-10
// ================================================================================================
using System;
using System.Collections;
using static Protean.Tools.Text;

namespace Protean.Tools
{
    public class Counter
    {

        #region Declarations

        private string _name;
        private double _count;
        private double _base;
        private double _increment;

        #endregion

        #region Enums and Constants

        public const double DEFAULT_INCREMENT = 1d;
        public const double DEFAULT_BASE = 0d;

        #endregion

        #region Events

        public event OnChangeEventHandler OnChange;

        public delegate void OnChangeEventHandler(Counter sender, EventArgs e);

        public new event OnErrorEventHandler OnError;

        public new delegate void OnErrorEventHandler(object sender, Errors.ErrorEventArgs e);

        private void _OnError(object sender, Errors.ErrorEventArgs e)
        {
            OnError?.Invoke(sender, e);
        }

        #endregion

        #region Constructor

        public Counter() : this(RandomPassword(16, options: TextOptions.LowerCase | TextOptions.UseAlpha | TextOptions.UseNumeric), DEFAULT_INCREMENT, DEFAULT_BASE)
        {
        }

        public Counter(string name) : this(name, DEFAULT_INCREMENT, DEFAULT_BASE)
        {
        }

        public Counter(string name, int incrementInterval) : this(name, Convert.ToDouble(incrementInterval), DEFAULT_BASE)
        {
        }

        public Counter(string name, double incrementInterval) : this(name, incrementInterval, DEFAULT_BASE)
        {
        }

        public Counter(string name, double incrementInterval, double baseCounter)
        {
            _name = name;
            Increment = incrementInterval;
            _base = baseCounter;
            Reset();
        }


        #endregion

        #region Private Properties


        #endregion

        #region Public Properties
        public string Name
        {
            get
            {
                return _name;
            }
        }

        public double Count
        {
            get
            {
                return _count;
            }
        }

        public double Base
        {
            get
            {
                return _base;
            }
        }

        public double Increment
        {
            get
            {
                return _increment;
            }

            set
            {
                _increment = value;
            }
        }

        #endregion

        #region Private Members


        #endregion

        #region Public Members

        public void Reset()
        {
            Reset(DEFAULT_BASE);
        }

        public void Reset(int baseCounter)
        {
            Convert.ToDouble(baseCounter);
        }

        public void Reset(double baseCounter)
        {
            _count = baseCounter;
            OnChange?.Invoke(this, null);
        }

        public void Add()
        {
            Add(Increment);
        }

        public void Add(int incremementInterval)
        {
            Add(Convert.ToDouble(incremementInterval));
        }

        public void Add(double incremementInterval)
        {
            _count += incremementInterval;
            OnChange?.Invoke(this, null);
        }

        public int ToInt()
        {
            return Convert.ToInt32(_count);
        }

        #endregion

        #region Shared Members


        #endregion

    } // Counter

    public class CounterCollection : CollectionBase
    {

        #region Inherited Overrides

        public Counter this[int index]
        {
            get
            {
                return (Counter)List[index];
            }

            set
            {
                List[index] = value;
            }
        }

        public Counter this[string counterName]
        {
            get
            {
                return (Counter)List[IndexOf(counterName)];
            }

            set
            {
                List[IndexOf(counterName)] = value;
            }
        }

        /// <summary>
        /// Creates a counter by counter name and adds it to the collection
        /// </summary>
        /// <param name="counterName">The name of the counter</param>
        /// <returns>Returns the actual Counter object</returns>
        /// <remarks></remarks>
        public Counter Add(string counterName)
        {
            if (!Exists(counterName))
            {
                return this[List.Add(new Counter(counterName))];
            }
            else
            {
                throw new Exception("Counter name already exists");
                return null;
            }
        } // Add

        /// <summary>
        /// Adds a counter by object
        /// </summary>
        /// <param name="value">The counter object to add</param>
        /// <returns>The index of counter in the collection</returns>
        /// <remarks></remarks>
        public int Add(Counter value)
        {
            return List.Add(value);
        } // Add

        public int IndexOf(string counterName)
        {
            int index = -1;
            foreach (Counter counterItem in List)
            {
                if ((counterItem.Name ?? "") == (counterName ?? ""))
                {
                    index = IndexOf(counterItem);
                }
            }

            return index;
        } // IndexOf

        public int IndexOf(Counter value)
        {
            return List.IndexOf(value);
        } // IndexOf

        public void Insert(int index, Counter value)
        {
            List.Insert(index, value);
        } // Insert

        public void Remove(Counter value)
        {
            List.Remove(value);
        } // Remove

        public bool Contains(Counter value)
        {
            // If value is not of type Counter, this will return false.
            return List.Contains(value);
        } // Contains

        protected override void OnInsert(int index, object value)
        {
            // Insert additional code to be run only when inserting values.
        } // OnInsert

        protected override void OnRemove(int index, object value)
        {
            // Insert additional code to be run only when removing values.
        } // OnRemove

        protected override void OnSet(int index, object oldValue, object newValue)
        {
            // Insert additional code to be run only when setting values.
        } // OnSet

        protected override void OnValidate(object value)
        {
            if (!ReferenceEquals(value.GetType(), Type.GetType("Protean.Tools.Counter")))
            {
                throw new ArgumentException("value must be of type Protean.Tools.Counter.", "value");
            }
        } // OnValidate 

        #endregion

        #region Public Non-inherited Members

        public bool Exists(string value)
        {
            bool isFound = false;
            foreach (Counter counterItem in List)
            {
                if ((counterItem.Name ?? "") == (value ?? ""))
                {
                    isFound = true;
                    break;
                }
            }

            return isFound;
        } // Exists

        public void ResetAll()
        {
            foreach (Counter counterItem in List)
                counterItem.Reset();
        }

        #endregion

    } // CounterCollection
}