//Code given by Dr. Simon
// adapted from Sedgewick.

//Code given by Dr. Simon

// Translated to C#, using generics
public interface IDenumerable
{
	int GetNumber();
	void SetNumber(int x);
}

public class MinPQ<Key> where Key : IComparable<Key>, IDenumerable {

	private ArrayList pq;
	private int N = 0;

	public MinPQ(int cap)
	{
		pq = new ArrayList(cap + 1);
		pq.Add(null);
	}
	
	public MinPQ()
		: this(1) { }

	public Boolean IsEmpty() { return N == 0; }

	public int Size() { return N; }

	public Key Min()
	{
		if (IsEmpty()) throw new Exception("Priority queue underflow");
		return (Key)pq[1];
	}

	public void AddKey(Key x)
	{
		N++;
		pq.Add(x);
		Swim(N);
	}

	public Key Remove()
	{
		if (IsEmpty()) throw new Exception("Priority queue underflow");
		Key min = (Key)pq[1];
		Swap(pq, 1, N--);
		Sink(1);
		pq.RemoveAt(N + 1);
		min.SetNumber(0);
		return min;
	}

	private void Swim(int k)
	{
		Key x = (Key)pq[k]; // assigned outside of loop
		Key y = (Key)pq[k / 2]; // value is changed inside loop
		while (k > 1 && y.CompareTo(x) > 0)
		{
			pq[k] = pq[k / 2];
			y.SetNumber(k);
			k = k / 2;
			y = (Key)pq[k / 2];
		}
		pq[k] = x;
		x.SetNumber(k);
	}
	private void Sink(int k)
	{
		Key x = (Key)pq[k];
		Key y;
		while (2 * k <= N)
		{
			int j = 2 * k;
			y = (Key)pq[j];
			//if (j < N && pq[j].CompareTo((Key)pq[j + 1]) > 0) j++;
			if (j < N && y.CompareTo((Key)pq[j + 1]) > 0) j++;
			if (x.CompareTo((Key)pq[j]) <= 0) break;
			pq[k] = pq[j];
			Key key = (Key)pq[j];
			key.SetNumber(k);
			k = j;
		}
		pq[k] = x;
		x.SetNumber(k);
	}

	private static void Swap(ArrayList pq, int i, int j)
	{
		Key t = (Key)pq[i];
		pq[i] = pq[j];
		pq[j] = t;
		Key key1 = (Key)pq[i];
		Key key2 = (Key)pq[j];
		key1.SetNumber(i);
		key2.SetNumber(j);
	}

	public void Update(Key x)
	{
		Swim(x.GetNumber());
	}