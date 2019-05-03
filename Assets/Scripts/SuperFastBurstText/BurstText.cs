using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public struct BurstTextStyle
{
    public Color color;
    public FontStyle fontStyle;

    static readonly public BurstTextStyle Damage = new BurstTextStyle() { color = Color.red, fontStyle = FontStyle.Bold };
    static readonly public BurstTextStyle Heal = new BurstTextStyle() { color = Color.green, fontStyle = FontStyle.Italic };
    static readonly public BurstTextStyle Info = new BurstTextStyle() { color = Color.white, fontStyle = FontStyle.Normal };
}

[System.Serializable]
public struct BurstTextEntry
{
    public float time;
    public float lifeTime;
    public float speed;
    public string text;
    public float randomization;
    public bool worldSpace;
    public Vector3 worldOffset;
    public MeshRenderer renderer;
    public Transform transform;
    public BurstTextStyle style;
}

public class BurstText : MonoBehaviour
{
    // max amount of burst text at one time, when the cap is exceeded, oldest deleted to make room
    public int maxBurstText = 200;

    // how long the burst text lives.
    public float burstLifeTimeMin = 1f;
    public float burstLifeTimeMax = 1f;

    // how fast it moves (effected by curves)
    public float burstSpeedMin = 2f;
    public float burstSpeedMax = 2f;

    // curve for moving the text based on lifetime.
    public AnimationCurve burstMoveCurveX;
    public AnimationCurve burstMoveCurveY;

    // curve for scaling the text based on lifetime.
    public AnimationCurve burstScaleCurve;

    static BurstText instance;
    GameObject template;

    LinkedList<BurstTextEntry> entries = new LinkedList<BurstTextEntry>();
    Queue<GameObject> freedEntries = new Queue<GameObject>();

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        template = transform.Find("Template").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        int depthIndex = 0;
        LinkedListNode<BurstTextEntry> nextEntry = entries.First;
        while (nextEntry != null)
        {
            LinkedListNode<BurstTextEntry> newEntry = nextEntry.Next;

            float progressTime = Time.time - nextEntry.Value.time;
            progressTime /= nextEntry.Value.lifeTime;

            nextEntry.Value.transform.localPosition = 
                new Vector3(
                    burstMoveCurveX.Evaluate(progressTime) * nextEntry.Value.randomization, 
                    burstMoveCurveY.Evaluate(progressTime) * nextEntry.Value.speed, 
                    -1 + (depthIndex * 0.01f));

            if (nextEntry.Value.worldSpace)
                nextEntry.Value.transform.localPosition += nextEntry.Value.worldOffset;

            nextEntry.Value.transform.localScale = Vector3.one * burstScaleCurve.Evaluate(progressTime) * 0.1f;
            nextEntry.Value.renderer.gameObject.SetActive(true);

            if (nextEntry.Value.time + nextEntry.Value.lifeTime < Time.time)
            {
                nextEntry.Value.renderer.gameObject.SetActive(false);
                freedEntries.Enqueue(nextEntry.Value.renderer.gameObject);
                entries.Remove(nextEntry);
            }


            depthIndex++;
            nextEntry = newEntry;
        }
    }

    static public void Spawn(BurstTextStyle style, Transform target, string text, bool worldSpace=false)
    {
        instance._Spawn(style, target, text, worldSpace);
    }


    GameObject GetEntry()
    {
        if (freedEntries.Count == 0)
        {
            // to many, steal the oldest one and recycle it.
            if (entries.Count >= maxBurstText && entries.Last != null)
                return entries.Last.Value.renderer.gameObject;

            return GameObject.Instantiate(template);
        }
        else
            return freedEntries.Dequeue();
    }

    void _Spawn(BurstTextStyle style, Transform target, string text, bool worldSpace)
    {
        GameObject newEntry = GetEntry();
        newEntry.SetActive(false);

        if (!worldSpace)
            newEntry.transform.parent = target;

        newEntry.transform.position = target.transform.position;
        newEntry.hideFlags = HideFlags.HideInHierarchy;

        MeshRenderer newRenderer = newEntry.GetComponent<MeshRenderer>();

        TextMesh textMesh = newEntry.GetComponent<TextMesh>();
        textMesh.fontStyle = style.fontStyle;
        textMesh.color = style.color;
        textMesh.text = text;

        TextMesh[] shadows = newEntry.GetComponentsInChildren<TextMesh>();
        foreach (TextMesh shadow in shadows)
        {
            shadow.text = text;
            shadow.fontStyle = style.fontStyle;
        }

        entries.AddFirst(new BurstTextEntry()
        {
            time = Time.time,
            lifeTime = Random.Range(burstLifeTimeMin, burstLifeTimeMax),
            speed = Random.Range(burstSpeedMin, burstSpeedMax),
            text = text,
            style = style,
            renderer = newRenderer,
            worldOffset = newEntry.transform.position,
            worldSpace = worldSpace,
            transform = newEntry.transform,
            randomization = Random.Range(-1f, 1f)
        });
    }
}
