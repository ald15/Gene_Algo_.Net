using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lib
{
    public class QSort
    {
        public static void QuickSort(IEntity[] arr, int low, int high)
        {
            if (low < high)
            {
                int pivotIndex = Partition(arr, low, high);
                QuickSort(arr, low, pivotIndex - 1); // Рекурсивно сортируем левую часть
                QuickSort(arr, pivotIndex + 1, high); // Рекурсивно сортируем правую часть
            }
        }

        static int Partition(IEntity[] arr, int low, int high)
        {
            IEntity pivot = arr[high];
            int i = low - 1;

            for (int j = low; j < high; j++)
            {
                if (CompareFScore(arr[j], pivot) <= 0)
                {
                    i++;
                    Swap(arr, i, j);
                }
            }
            Swap(arr, i + 1, high);
            return i + 1;
        }

        static int CompareFScore(IEntity x, IEntity y)
        {
            if (x.FScore == null && y.FScore == null) return 0;
            if (x.FScore == null) return -1;
            if (y.FScore == null) return 1;
            return x.FScore.CompareTo(y.FScore);
        }

        static void Swap(IEntity[] arr, int i, int j)
        {
            IEntity temp = arr[i];
            arr[i] = arr[j];
            arr[j] = temp;
        }
    }
}
