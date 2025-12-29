using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helper script to let an UI-Element blink
/// </summary>
public class BlinkBehavior : MonoBehaviour
{
    public float blinkTime = 4.0f;
    
    private float cTime = 0.0f;
    private Image image;
    private float startAlpha = 0.0f;

    /// <summary>
    /// Initialize the class
    /// </summary>
    void Start()
    {
        this.cTime = blinkTime;
        this.image = this.GetComponent<Image>();
        this.startAlpha = this.image.color.a;
    }

    /// <summary>
    /// Updates the blinking of the object this will be applied to
    /// </summary>
    void Update()
    {
        this.cTime -= Time.deltaTime;

        if(this.cTime <= 0.0f)
        {
            this.cTime = this.blinkTime;
        }

        float percentage = this.cTime / (this.blinkTime);
        percentage = 1.0f - percentage;
        percentage = (0.5f - percentage) * 2.0f;
        percentage = percentage * percentage;


        this.image.color = new Color(this.image.color.r, this.image.color.g, this.image.color.b, percentage * this.startAlpha);

    }
}
