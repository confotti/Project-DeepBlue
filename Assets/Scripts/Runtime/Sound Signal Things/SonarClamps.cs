using UnityEngine;

public class SonarClamps : MonoBehaviour
{
    public RectTransform left;
    public RectTransform right;

    public void SetOffset(float offset)
    {
        left.position = new Vector3(-offset, 0, 0);
        right.position = new Vector3(offset, 0, 0);

        Debug.Log(left.position);
        Debug.Log(right.position);

        //Fixa skiten imorgon :(
    }
}
