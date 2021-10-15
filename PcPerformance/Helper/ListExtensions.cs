using System;
using System.Collections.Generic;
using System.Linq;

namespace PcPerformance.Helper
{
    public static class ListExtensions
    {
        public static void UpdateItems<T1, T2>(
            this IList<T1> targetCollection,
            IList<T2> updateCollection,
            Func<T1, T2, bool> comparer,
            Func<T2, T1> mapper,
            Action<T1, T2> updater)
        {
            var itemsToRemove = new List<T1>();

            UpdateExistingItemsAndIdentifyRemovedItems(targetCollection, updateCollection,
                comparer, updater, itemsToRemove);

            RemoveOldItemsFromTarget(targetCollection, itemsToRemove);

            AddNewItemsToTarget(targetCollection, updateCollection, comparer, mapper);
        }

        private static void UpdateExistingItemsAndIdentifyRemovedItems<T1, T2>(
            IEnumerable<T1> targetCollection,
            IList<T2> updateCollection,
            Func<T1, T2, bool> comparer,
            Action<T1, T2> updater,
            ICollection<T1> itemsToRemove)
        {
            foreach (var targetItem in targetCollection)
            {
                var updateItem = updateCollection.FirstOrDefault(u => comparer(targetItem, u));
                if (updateItem == null)
                {
                    itemsToRemove.Add(targetItem);
                    continue;
                }

                updater?.Invoke(targetItem, updateItem);
            }
        }

        private static void RemoveOldItemsFromTarget<T1>(
            ICollection<T1> targetCollection,
            IEnumerable<T1> itemsToRemove)
        {
            foreach (var item in itemsToRemove)
                targetCollection.Remove(item);
        }

        private static void AddNewItemsToTarget<T1, T2>(
            ICollection<T1> targetCollection,
            IEnumerable<T2> updateCollection,
            Func<T1, T2, bool> comparer,
            Func<T2, T1> mapper)
        {
            var itemsToAdd = updateCollection
                .Where(updateItem => !targetCollection.Any(existingITem => comparer(existingITem, updateItem)))
                .ToList();

            foreach (var item in itemsToAdd)
            {
                targetCollection.Add(mapper(item));
            }
        }
    }
}
