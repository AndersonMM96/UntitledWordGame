using System.Collections.Generic;
using UnityEngine;

public class Pile : MonoBehaviour
{
    public int max = int.MaxValue;
    protected List<GameObject> values;
    private List<Vector2> kerning;
    private float spacing = 0f;
    public int length
    {
        get { return values.Count; }
    }

    //SpriteAnimator variables
    protected int sortingOrder;
    private bool shake;
    private float shakeIntensity = 0.04f;
    private float shakeInterval = 0.05f;
    private bool bob;
    private float bobIntensity = 0.25f;
    private float bobSpeed = 1f;
    private float bobTimer;
    protected int colorIndex = 0;
    protected List<Color> colors;

    public static Pile Create(Transform parent)
    {
        GameObject pile = new();
        pile.name = "Pile";
        pile.transform.parent = parent;
        pile.transform.localPosition = Vector3.zero;
        pile.AddComponent<Pile>().Initialize();
        return pile.GetComponent<Pile>();
    }
    public void Initialize()
    {
        values ??= new List<GameObject>();
        kerning ??= new List<Vector2>();
        colors ??= new List<Color>();
    }

    public void Add(GameObject value) 
    { 
        Add(value, Vector2.right); 
    }
    public void Add(GameObject value, Vector2 kerning)
    {
        if (values.Count < max)
        {
            values.Add(value);
            this.kerning.Add(kerning);
            UpdatePile();
        }
        else
            Debug.LogWarning("Cannot add " + value + " to Pile " + gameObject.name + ": Pile is at max capacity");
    }
    public void Remove(GameObject value) 
    { 
        RemoveAt(values.IndexOf(value));
    }
    public void RemoveAt(int index)
    {
        values.RemoveAt(index);
        kerning.RemoveAt(index);
        UpdatePile();
    }
    public GameObject Get(int index)
    {
        return values[index];
    }
    public void Clear()
    {
        foreach(GameObject value in values)
            if(value != null)
                Destroy(value);
        values.Clear();
        kerning.Clear();
    }

    private void Update()
    {
        UpdatePosition();
    }

    private void UpdatePile()
    {
        for(int i = 0; i < values.Count; i++)
        {
            UpdatePosition();
            values[i].GetComponent<SpriteAnimator>().SetColor(false, colors);
            SetColor(colorIndex);
            SetSortingOrder(sortingOrder);
            SetBob(bob, bobIntensity, bobSpeed, bobTimer);
            SetShake(shake, shakeIntensity, shakeInterval);
        }
    }

    //Position
    private void UpdatePosition()
    {
        for (int i = 0; i < values.Count; i++)
        {
            if (values[i] != null)
            {
                SpriteAnimator animator = values[i].GetComponent<SpriteAnimator>();
                animator.SetTarget(GetPosition(i));
            }
        }
    }
    protected Vector2 GetPosition(int index)
    {
        Vector2 position = Vector2.zero;
        //kerning.x ==> width of item
        //kerning.y ==> vertical offset

        int length = kerning.Count;
        float total = 0;
        for (int i = 0; i < length; i++)
            total += kerning[i].x + spacing;

        position.x -= total * 0.5f;
        for (int i = 0; i < index; i++)
        {
            position.x += kerning[i].x + spacing;
        }
        position.x += (kerning[index].x + spacing) * 0.5f;
        position.y += kerning[index].y;

        return position;
    }
    public void SetSpacing(float spacing)
    {
        this.spacing = spacing;
        UpdatePosition();
    }

    //Sorting Order
    public void SetSortingOrder(int sortingOrder)
    {
        this.sortingOrder = sortingOrder;
        for (int i = 0; i < values.Count; i++)
            values[i].GetComponent<SpriteAnimator>().sortingOrder = this.sortingOrder;
    }
    //Shake
    public void SetShake()
    {
        for (int i = 0; i < values.Count; i++)
            values[i].GetComponent<SpriteAnimator>().SetShake(shake, shakeIntensity, shakeInterval);
    }
    public void SetShake(bool shake, float shakeIntensity, float shakeInterval)
    {
        this.shake = shake;
        this.shakeIntensity = shakeIntensity;
        this.shakeInterval = shakeInterval;
        SetShake();
    }
    public void SetShake(bool shake)
    {
        SetShake(shake, shakeIntensity, shakeInterval);
    }
    //Bobbing
    public void SetBob()
    {
        for (int i = 0; i < values.Count; i++)
            values[i].GetComponent<SpriteAnimator>().SetBob(bob, bobIntensity, bobSpeed, i * 0.1f);
    }
    public void SetBob(bool bob, float bobIntensity, float bobSpeed, float bobTimer)
    {
        this.bob = bob;
        this.bobIntensity = bobIntensity;
        this.bobSpeed = bobSpeed;
        this.bobTimer = bobTimer;
        SetBob();
    }
    public void SetBob(bool bob, float bobIntensity, float bobSpeed)
    {
        SetBob(bob, bobIntensity, bobSpeed, bobTimer);
    }
    public void SetBob(bool bob)
    {
        SetBob(bob, bobIntensity, bobSpeed, bobTimer);
    }
    //Animate
    public void SetAnimate(bool animate)
    {
        for (int i = 0; i < values.Count; i++)
            values[i].GetComponent<SpriteAnimator>().SetAnimate(animate);
    }
    //Colors
    public void AddColor(Color color)
    {
        colors.Add(color);
        for (int i = 0; i < values.Count; i++)
            values[i].GetComponent<SpriteAnimator>().AddColor(color);
    }
    public void SetColor(int index)
    {
        colorIndex = index;
        for (int i = 0; i < values.Count; i++)
            values[i].GetComponent<SpriteAnimator>().SetColor(index);
    }
}
