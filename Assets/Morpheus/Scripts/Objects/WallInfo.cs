using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WallInfo : MonoBehaviour
{
    public TextMeshProUGUI Name;
    public Toggle Toggle_DrawMe;
    public InputField Input_MinVerts;
    public InputField Input_MaxVerts;

    DrawWall DrawWall;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Init(string name, DrawWall drawWall)
    {
        Name.text = name;
        DrawWall = drawWall;
        Toggle_DrawMe.isOn = DrawWall.DrawMe;
        Input_MinVerts.text = DrawWall.StartIndex.ToString();
        Input_MaxVerts.text = DrawWall.EndIndex.ToString();
    }

    public void OnToggleChange()
    {
        DrawWall.DrawMe = Toggle_DrawMe.isOn;
    }

    public void OnInputEndEdit_Min()
    {
        DrawWall.StartIndex = int.Parse(Input_MinVerts.text);
    }

    public void OnInputEndEdit_Max()
    {
        DrawWall.EndIndex = int.Parse(Input_MaxVerts.text);
    }
}
