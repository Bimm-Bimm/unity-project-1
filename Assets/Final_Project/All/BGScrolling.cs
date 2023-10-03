using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BGScrolling : MonoBehaviour
{
    [SerializeField] RawImage BG_Image;
    [SerializeField] float x, y;

    // Update is called once per frame
    void Update()
    {
        BG_Image.uvRect = new Rect(BG_Image.uvRect.position + new Vector2(x , y) * Time.deltaTime , BG_Image.uvRect.size);
    }
}
