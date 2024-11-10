using System;
using System.Collections.Generic;

public class PriorityQueue<T> where T : IComparable<T>
{
    private List<T> data;

    public PriorityQueue()
    {
        data = new List<T>();
    }

    public void Enqueue(T item)
    {
        data.Add(item);
        int childIndex = data.Count - 1; // The child index
        while (childIndex > 0)
        {
            int parentIndex = (childIndex - 1) / 2; // The parent index

            if (data[childIndex].CompareTo(data[parentIndex]) >= 0)
            {
                break; // Child's value is greater than or equal to parent's value, so we're done
            }

            // Swap the child and parent
            T tmp = data[childIndex];
            data[childIndex] = data[parentIndex];
            data[parentIndex] = tmp;

            childIndex = parentIndex; // Continue with the parent's index
        }
    }

    public T Dequeue()
    {
        // Get the first item (the root of the heap)
        int lastIndex = data.Count - 1; // Get the last index
        T frontItem = data[0]; // The item we want to remove
        data[0] = data[lastIndex]; // Move the last item to the root
        data.RemoveAt(lastIndex); // Remove the last item

        --lastIndex;
        int parentIndex = 0;

        while (true)
        {
            int childIndex = parentIndex * 2 + 1; // Left child index
            if (childIndex > lastIndex)
            {
                break; // No children
            }

            int rightChild = childIndex + 1; // Right child index
            if (rightChild <= lastIndex && data[rightChild].CompareTo(data[childIndex]) < 0)
            {
                childIndex = rightChild; // Use the right child instead if it is smaller
            }

            if (data[parentIndex].CompareTo(data[childIndex]) <= 0)
            {
                break; // Parent is smaller than or equal to the smallest child, we're done
            }

            // Swap the parent with the smaller child
            T tmp = data[parentIndex];
            data[parentIndex] = data[childIndex];
            data[childIndex] = tmp;

            parentIndex = childIndex; // Continue with the child
        }

        return frontItem;
    }

    public int Count
    {
        get { return data.Count; }
    }

    public bool Contains(T item)
    {
        return data.Contains(item);
    }
}
