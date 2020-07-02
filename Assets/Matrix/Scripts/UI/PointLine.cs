using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PointLine : BaseMono
{
    Image Image;
    int Id;
    Color Default;
    public Color Hilight;

    public override void Awake()
    {
        base.Awake();
    }
    // Start is called before the first frame update
    void Start()
    {
        Image = gameObject.GetComponent<Image>();
    }
    
    public void Init(int id, Color d)
    {
        Id = id;
        Default = d;
    }

    public void HilightMe()
    {
        PointButton pointButton = (PointButton)CallbackObject;

        Image.color = Default;

        if (pointButton.Id == Id)
            Image.color = Hilight;        
    }
}
