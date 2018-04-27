//using System.Collections.Generic;
using System.Collections;
public class Node : IDenumerable, IComparer {
    public Node parent;
    public int[,] state;

    public int aix; public int aiy;
    public int g; public int h;

    public int pos;
    public bool inFrontier;

    public Node(int[,] state, int aix, int aiy, Node parent) {
        this.state = state;
        this.aix = aix;
        this.aiy = aiy;
        this.parent = parent;
    }

    public override string ToString() {
        return "( " + aix + ", " + aiy + " ) H: " + h + " G: " + g + " F: " + (h + g);
    }

    public int GetNumber() {
        return pos;
    }

    public void SetNumber(int x) {
        pos = x;
    }

    int IComparer.Compare(object x, object y) {
        Node nx = (Node)x; Node ny = (Node)y;

        int xf = nx.g + nx.h;
        int yf = ny.g + ny.h;

        if (xf == yf) return 0;
        else if (yf > xf) return -1;
        else return 1;
    }
}
