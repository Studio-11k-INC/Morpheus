using Doozy.Engine;
using TMPro;
using UnityEngine;

public class PointButton : MonoBehaviour
{
    public int Id;
    TextMeshProUGUI Text;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Init(int id, string name, Color color)
    {
        Text = GetComponentInChildren<TextMeshProUGUI>();

        Id = id;
        Text.text = name;
        Text.color = color;
    }

    public void OnClick()
    {
        GameEventMessage.SendEvent(eMessages.POINTLINE_CLICK.ToString(), this);
    }
}
