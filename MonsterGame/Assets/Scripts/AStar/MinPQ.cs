using System;
using System.Collections;

public class MinPQ<Key> where Key : IComparer, IDenumerable {
    public ArrayList pq;
    private int N = 0;

    public MinPQ(int cap) {
        pq = new ArrayList(cap + 1) {
            null
        };
    }

    public MinPQ()
        : this(1) { }

    public bool IsEmpty() { return N == 0; }

    public int Size() { return N; }

    public Key Min() {
        if (IsEmpty()) throw new Exception("Priority queue underflow");
        return (Key)pq[1];
    }

    public void Add(Key x) {
        N++;
        pq.Add(x);
        Swim(N, x);
    }

    public Key Remove() {
        if (IsEmpty()) throw new Exception("Priority queue underflow");
        Key min = (Key)pq[1];
        Swap(pq, 1, N--);
        Sink(1);
        pq.RemoveAt(N + 1);
        min.SetNumber(0);
        return min;
    }

    private void Swim(int k, Key q) {
        pq[k] = q;
        Key x = (Key)pq[k]; // assigned outside of loop
        while (k > 1 && ((Key)pq[k / 2]).Compare((Key)pq[k / 2], x) > 0) {
            pq[k] = pq[k / 2];
            ((Key)pq[k / 2]).SetNumber(k);
            k = k / 2;
        }
        pq[k] = x;
        x.SetNumber(k);
    }

    private void Sink(int k) {
        Key x = (Key)pq[k];
        Key y;
        while (2 * k <= N) {
            int j = 2 * k;
            y = (Key)pq[j];
            if (j < N && y.Compare(y, (Key)pq[j + 1]) > 0) j++;
            if (x.Compare(x, (Key)pq[j]) <= 0) break;
            pq[k] = pq[j];
            Key key = (Key)pq[j];
            key.SetNumber(k);
            k = j;
        }
        pq[k] = x;
        x.SetNumber(k);
    }

    private static void Swap(ArrayList pq, int i, int j) {
        Key t = (Key)pq[i];
        pq[i] = pq[j];
        pq[j] = t;
        ((Key)pq[i]).SetNumber(i);
        ((Key)pq[j]).SetNumber(j);
    }

    public void Update(Key x) {
        Swim(x.GetNumber(), x);
        Sink(x.GetNumber());
    }
}