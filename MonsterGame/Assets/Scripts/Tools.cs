//Class provided by http://tutorials.daspete.at/unity3d/maze-runner
//It simply shuffles an array of any type
public class Tools {
    public static T[] Shuffle<T>(T[] arr, System.Random random) {
        for (int i = 0; i < arr.Length - 1; i++) {
            int randomIndex = random.Next(i, arr.Length);

            T tempItem = arr[randomIndex];

            arr[randomIndex] = arr[i];
            arr[i] = tempItem;
        }
        return arr;
    }

}