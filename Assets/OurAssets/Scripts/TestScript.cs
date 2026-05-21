using UnityEngine;

public class TestScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LinkedListADT<int> ints = new LinkedListADT<int>()
        {
            1,
            2,
            3,
            4
        };

        foreach (int i in ints)
        {
            Debug.Log(i);
        }
    }
}
