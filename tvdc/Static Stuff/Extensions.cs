using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace tvdc
{
    public static class Extensions
    {

        public static void Sort<T>(this ObservableCollection<T> collection) where T : IComparable<T>
        {
            DateTime start = DateTime.Now;
            List<T> sorted = collection.OrderBy(x => x).ToList();
            for (int i = 0; i < sorted.Count(); i++)
                collection.Move(collection.IndexOf(sorted[i]), i);
            Debug.WriteLine((DateTime.Now - start).TotalMilliseconds);
        }

        //Seems to be slightly more efficient but using method above
        //to theoretically support animating the moving process

        //public static void Sort<T>(this Collection<T> source)
        //{
        //    List<T> sortedList = source.OrderBy(x => x).ToList();
        //    source.Clear();
        //    foreach (var sortedItem in sortedList)
        //        source.Add(sortedItem);
        //}

        public static void AddSorted<T>(this ObservableCollection<T> collection, T itemToAdd) where T : IComparable<T>
        {
            for (int i = 0; i < collection.Count - 1; i++)
            {
                if (collection[i].CompareTo(itemToAdd) < 0 && collection[i + 1].CompareTo(itemToAdd) > 0)
                {
                    collection.Insert(i + 1, itemToAdd);
                    return;
                }
            }
            collection.Insert(collection.Count, itemToAdd);
        }

    }
}
