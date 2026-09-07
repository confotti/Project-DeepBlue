using UnityEngine;
using UnityEngine.UI;

public class SonarClamps : MonoBehaviour
{
    public Image left;
    public Image right;

    public void SetOffset(float offset)
    {
        left.transform.localPosition = new Vector3(-offset, 0, 0);
        right.transform.localPosition = new Vector3(offset, 0, 0);
    }

    public void SetColor(Color c)
    {
        left.color = c;
        right.color = c;
    }
}
