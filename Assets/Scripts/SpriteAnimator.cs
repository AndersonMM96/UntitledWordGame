using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SpriteAnimator : MonoBehaviour
{
    public int sortingOrder = 0;

    [SerializeField] private Vector2 target = Vector2.zero;
    [SerializeField] private float moveSpeed = 30f;
    private bool moving = false;
    [HideInInspector] public Action<bool> Moving;

    [SerializeField] private bool shake = false;
    [SerializeField] private float shakeIntensity = 0.04f;
    [SerializeField] private float shakeInterval = 0.05f;
    [SerializeField] private float shakeTimer;
    [SerializeField] private Vector3 shakeOffset;

    public bool flash = false;
    private bool flashStatus;
    private float flashTimer;
    [SerializeField] public float flashInterval = 0.5f;

    [SerializeField] private bool animate = false;
    [SerializeField] private List<Sprite> sprites;
    [SerializeField] private List<bool> flipX;
    [SerializeField] private float animateInterval = 0.5f;
    [SerializeField] public int spriteIndex;
    [SerializeField] private float animateTimer;

    public bool pulse = false;
    [SerializeField] public float pulseInterval = 1f;
    [SerializeField] public float pulseIntensity = 0.25f;
    [SerializeField] public float pulseSpeed = 0.5f;
    [SerializeField] public float pulseTimer;
    public Vector3 pulseOffset;

    public bool rotate = false;
    [SerializeField] public bool rotateDirection = true;
    [SerializeField] public float rotateSpeed = 15f;
    private float rotation;

    [SerializeField] private bool bob = false;
    [SerializeField] private float bobIntensity = 0.5f;
    [SerializeField] private float bobSpeed = 1f;
    [SerializeField] private float bobTimer;
    [SerializeField] private Vector3 bobOffset;

    [SerializeField] private bool color = false;
    [SerializeField] private List<Color> colors;
    private int colorIndex = 0;

    private void Update()
    {
        if(GetComponent<SpriteRenderer>() != null)
            GetComponent<SpriteRenderer>().sortingOrder = sortingOrder;

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
                if (flipX != null)
                    GetComponent<SpriteRenderer>().flipX = flipX[spriteIndex];
                animateTimer = animateInterval;
            }
        }
        else
        {
            animateTimer = 0f;
            //spriteIndex = 0;
            if(sprites != null && sprites.Count > 0)
                GetComponent<SpriteRenderer>().sprite = sprites[spriteIndex];
            if (flipX != null && flipX.Count > 0)
                GetComponent<SpriteRenderer>().flipX = flipX[spriteIndex];
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
        if(color && colors != null && colors.Count > 0)
        {
            //TO BE IMPLEMENTED
            GetComponent<SpriteRenderer>().color = colors[colorIndex];
        }
        else
        {
            if (colors != null && colors.Count > 0)
                GetComponent<SpriteRenderer>().color = colors[colorIndex];
        }

        MoveToTarget();
        if(transform.localPosition.Equals((Vector3)target))
        {
            if (moving)
            {
                moving = false;
                Moving?.Invoke(moving);
            }
        }
        else
        {
            if (!moving)
            {
                moving = true;
                Moving?.Invoke(moving);
            }
        }
            
    }

    private void Shake()
    {
        shakeOffset = new Vector3(UnityEngine.Random.Range(-shakeIntensity, shakeIntensity), UnityEngine.Random.Range(-shakeIntensity, shakeIntensity), 0f);
    }
    public bool ToggleShake() { return shake = !shake; }
    public bool ToggleFlash() { return flash = !flash; }

    //Move
    public void SetTarget(Vector2 target)
    {
        this.target = target;
    }
    public void MoveToTarget()
    {
        transform.localPosition = Vector2.MoveTowards(transform.localPosition, (Vector3)target + shakeOffset + bobOffset, moveSpeed * Time.deltaTime);
    }
    public void SetPosition(Vector2 position, bool setTarget)
    {
        transform.localPosition = position;
        if (setTarget)
            target = position;
    }
    public void SetPosition(Vector2 position)
    {
        SetPosition(position, true);
    }
    public void SetMoveSpeed(float moveSpeed)
    {
        this.moveSpeed = moveSpeed;
    }
    //Shake
    public void SetShake(bool shake, float shakeIntensity, float shakeInterval)
    {
        this.shake = shake;
        this.shakeIntensity = shakeIntensity;
        this.shakeInterval = shakeInterval;
    }
    public void SetShake(bool shake)
    {
        SetShake(shake, shakeIntensity, shakeInterval);
    }
    //Pulse
    public void Pulse()
    {
        pulseOffset = new Vector3(pulseIntensity, pulseIntensity, 0f);
    }
    public void SetPulse(bool pulse, float pulseIntensity, float pulseInterval, float pulseSpeed)
    {
        this.pulse = pulse;
        this.pulseIntensity = pulseIntensity;
        this.pulseInterval = pulseInterval;
        this.pulseSpeed = pulseSpeed;
    }
    public void SetPulse(bool pulse)
    {
        SetPulse(pulse, pulseIntensity, pulseInterval, pulseSpeed);
    }
    //Animate
    public void SetAnimate(bool animate, List<Sprite> sprites, List<bool> flipX, float animateInterval)
    {
        this.animate = animate;
        this.sprites = sprites;
        this.flipX = flipX;
        this.animateInterval = animateInterval;
    }
    public void SetAnimate(bool animate, List<Sprite> sprites, List<bool> flipX)
    {
        SetAnimate(animate, sprites, flipX, animateInterval);
    }
    public void SetAnimate(bool animate, List<Sprite> sprites, float animateInterval)
    {
        SetAnimate(animate, sprites, flipX, animateInterval);
    }
    public void SetAnimate(bool animate, List<Sprite> sprites)
    {
        SetAnimate(animate, sprites, animateInterval);
    }
    public void SetAnimate(bool animate)
    {
        SetAnimate(animate, sprites, animateInterval);
    }
    public void SetSprite(int spriteIndex) 
    {
        this.spriteIndex = spriteIndex;
        GetComponent<SpriteRenderer>().sprite = sprites[spriteIndex]; 
    }
    //Rotate
    public void SetRotate(bool rotate, bool rotateDirection, float rotateSpeed)
    {
        this.rotate = rotate;
        this.rotateDirection = rotateDirection;
        this.rotateSpeed = rotateSpeed;
    }
    public void SetRotate(bool rotate, float rotateSpeed)
    {
        SetRotate(rotate, rotateDirection, rotateSpeed);
    }
    public void SetRotate(bool rotate, bool rotateDirection)
    {
        SetRotate(rotate, rotateDirection, rotateSpeed);
    }
    public void SetRotate(bool rotate)
    {
        SetRotate(rotate, rotateDirection, rotateSpeed);
    }
    //Bobbing
    public void SetBob(bool bob, float bobIntensity, float bobSpeed, float bobTimer)
    {
        this.bob = bob;
        this.bobIntensity = bobIntensity;
        this.bobSpeed = bobSpeed;
        this.bobTimer = bobTimer;
    }
    public void SetBob(bool bob, float bobIntensity, float bobSpeed)
    {
        SetBob(bob, bobIntensity, bobSpeed, bobTimer);
    }
    public void SetBob(bool bob)
    {
        SetBob(bob, bobIntensity, bobSpeed);
    }
    //Color
    public void SetColor(bool color, List<Color> colors)
    {
        this.color = color;
        this.colors = colors;
    }
    public void SetColor(bool color)
    {
        SetColor(color, colors);
    }
    public void SetColor(int index)
    {
        colorIndex = index;
        if(colors != null && colors.Count > 0 && colorIndex < colors.Count)
            GetComponent<SpriteRenderer>().color = colors[colorIndex];
    }
    public void AddColor(Color color)
    {
        colors ??= new List<Color>();
        colors.Add(color);
    }
    public List<Color> GetColors() 
    { 
        return colors; 
    }
}
