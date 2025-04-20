using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimator : MonoBehaviour
{
    public bool shake = false;
    [SerializeField] public float shakeInterval = 0.1f;
    [SerializeField] public float shakeIntensity = 0.1f;
    private float shakeTimer;
    public Vector3 shakeOffset;

    public bool flash = false;
    private bool flashStatus;
    private float flashTimer;
    [SerializeField] public float flashInterval = 0.5f;

    public bool animate = false;
    public List<Sprite> sprites;
    private int spriteIndex;
    private float animateTimer;
    [SerializeField] public float animateInterval = 0.5f;

    public bool pulse = false;
    [SerializeField] public float pulseInterval = 1f;
    [SerializeField] public float pulseIntensity = 0.25f;
    [SerializeField] public float pulseSpeed = 0.5f;
    [SerializeField] private float pulseTimer;
    public Vector3 pulseOffset;

    public bool rotate = false;
    [SerializeField] public bool rotateDirection = true;
    [SerializeField] public float rotateSpeed = 15f;
    private float rotation;

    public bool bob = false;
    [SerializeField] public float bobIntensity = 0.5f;
    [SerializeField] public float bobSpeed = 1f;
    [SerializeField] public float bobTimer;
    public Vector3 bobOffset;

    public bool color = false;
    [SerializeField] private List<Color> colors;

    private void Update()
    {
        //Shake
        if (shake)
        {
            shakeTimer -= Time.deltaTime;
            if(shakeTimer <= 0)
            {
                Shake();
                shakeTimer = shakeInterval;
            }
        }
        else
        {
            shakeTimer = 0f;
            shakeOffset = Vector3.zero;
        }
        //Flash
        if (flash)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0)
            {
                flashStatus = !flashStatus;
                GetComponent<SpriteRenderer>().enabled = flashStatus;
                flashTimer = flashInterval;
            }
        }
        else
        {
            flashTimer = 0f;
            flashStatus = true;
        }
        //Animate
        if (animate && sprites != null && sprites.Count > 0)
        {
            animateTimer -= Time.deltaTime;
            if (animateTimer <= 0)
            {
                spriteIndex++;
                if (spriteIndex >= sprites.Count)
                    spriteIndex = 0;
                GetComponent<SpriteRenderer>().sprite = sprites[spriteIndex];
                animateTimer = animateInterval;
            }
        }
        else
        {
            animateTimer = 0f;
            spriteIndex = 0;
            if(sprites != null && sprites.Count > 0)
                GetComponent<SpriteRenderer>().sprite = sprites[spriteIndex];
        }
        //Pulse
        if (pulse)
        {
            pulseTimer -= Time.deltaTime;
            if (pulseTimer <= 0)
            {
                Pulse();
                pulseTimer = pulseInterval;
            }
        }
        else
        {
            pulseTimer = 0f;
        }
        if (pulseOffset.x > 0f)
            pulseOffset -= new Vector3(pulseSpeed, pulseSpeed, 0f) * Time.deltaTime;
        else
        {
            pulseOffset = Vector3.zero;
            transform.localScale = Vector3.one;
        }

        transform.localScale = Vector3.one + pulseOffset;
        //Rotate
        if (rotate)
        {
            if (rotateDirection)
                rotation += rotateSpeed * Time.deltaTime;
            else
                rotation -= rotateSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Euler(0f, 0f, rotation);
        }
        else
        {
            rotation = 0f;
            transform.rotation = Quaternion.identity;
        }
        //Bob
        if (bob)
        {
            bobTimer -= Time.deltaTime;
            if(bobTimer <= 0)
            {
                bobTimer = 2f / bobSpeed;
            }
            bobOffset = new Vector3(0f, bobIntensity * Mathf.Sin(Mathf.PI * bobSpeed * bobTimer), 0f);
        }
        else
        {
            bobTimer = 0f;
            bobOffset = Vector3.zero;
        }
    }

    private void Shake()
    {
        shakeOffset = new Vector3(Random.Range(-shakeIntensity, shakeIntensity), Random.Range(-shakeIntensity, shakeIntensity), 0f);
    }
    public void Pulse()
    {
        pulseOffset = new Vector3(pulseIntensity, pulseIntensity, 0f);
    }
    public bool ToggleShake() { return shake = !shake; }
    public bool ToggleFlash() { return flash = !flash; }
}
