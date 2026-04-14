using UnityEngine;

public class QuestButtonPanel : MonoBehaviour
{
    [SerializeField]
    private GameObject questButton;

    public void SetButtonPosition(float dx)
    {
        questButton.transform.position += new Vector3(dx, 0, 0);
    }
}
