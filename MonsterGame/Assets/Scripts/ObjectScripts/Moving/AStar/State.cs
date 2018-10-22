using System;
public class State{
    public int[,] board;

    public State(int[,] board) {
        this.board = board;
    }

    public override bool Equals(Object obj) {
        for (int i = 0; i < board.GetLength(0); i++) {
            for (int j = 0; j < board.GetLength(0); j++) {
                if (((State)obj).board[i, j] != this.board[i, j])
                    return false;
            }
        } 
        return true;
    }

    public override int GetHashCode() {
        int hash = 0;

        for(int i = 0; i < board.GetLength(0); i++) {
            for(int j = 0; j < board.GetLength(0); j++) {
                hash = hash * 31 + board[i, j];
            }
        }
        return hash;
    }
}