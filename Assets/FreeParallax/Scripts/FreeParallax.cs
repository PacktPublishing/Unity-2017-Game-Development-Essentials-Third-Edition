// License: https://en.wikipedia.org/wiki/MIT_License
// Code created by Jeff Johnson & Digital Ruby, LLC - http://www.digitalruby.com
// Code is from the Free Parallax asset on the Unity asset store: http://u3d.as/bvv
// Modified a little for the book purposes by Tommaso Lintrami, https://www.packtpub.com/game-development/unity-5x-game-development-essentials-third-edition
// Code may be redistributed in source form, provided all the comments at the top here are kept intact

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public delegate void FreeParallaxElementRepositionLogicFunction(FreeParallax p, FreeParallaxElement element, float amount, GameObject obj, Renderer r);

public class FreeParallax : MonoBehaviour
{
    [Tooltip("Camera to use for the parallax. Defaults to main camera.")]
    public Camera parallaxCamera;
    [Tooltip("The speed you want the parallax moves, this multiplies the horizontal input.")]
    public float BasicSpeedMultiplier = 10.0f;
    // The speed at which the parallax moves, which will likely be opposite from the speed at which your character moves. Elements can be set to move as a percentage of this value
    [HideInInspector] public float Speed = 2.0f;

    [Tooltip("The elements in the parallax.")]
    public List<FreeParallaxElement> Elements;

    [Tooltip("Whether the parallax moves horizontally or vertically. Horizontal moves left and right, vertical moves up and down.")]
    public bool IsHorizontal = true;

    [Tooltip("The overlap in world units for wrapping elements. This can help fix rare one pixel gaps.")]
    public float WrapOverlap = 0.0f;

    private void SetupElementAtIndex(int i)
    {
        FreeParallaxElement e = Elements[i];
        if (e.GameObjects == null || e.GameObjects.Count == 0)
        {
            Debug.LogError("No game objects found at element index " + i.ToString() + ", be sure to set at least one game object for each element in the parallax");
            return;
        }
        foreach (GameObject obj in e.GameObjects)
        {
            if (obj == null)
            {
                Debug.LogError("Null game object found at element index " + i.ToString());
                return;
            }
        }

        e.SetupState(this, parallaxCamera, i);
        e.SetupScale(this, parallaxCamera, i);
        e.SetupPosition(this, parallaxCamera, i);
    }

    /// <summary>
    /// Reset the parallax to default state
    /// </summary>
    public void Reset()
    {
        SetupElements();
    }

    /// <summary>
    /// Initialize each element
    /// </summary>
    public void SetupElements()
    {
        if (parallaxCamera == null)
        {
            parallaxCamera = Camera.main;
            if (parallaxCamera == null)
            {
                Debug.LogError("Cannot run parallax without a camera");
                return;
            }
        }
        if (Elements == null || Elements.Count == 0)
        {
            return;
        }

        for (int i = 0; i < Elements.Count; i++)
        {
            SetupElementAtIndex(i);
        }
    }

    /// <summary>
    /// Add a new element to the parallax
    /// </summary>
    /// <param name="e">Element to add</param>
    public void AddElement(FreeParallaxElement e)
    {
        if (Elements == null)
        {
            Elements = new List<FreeParallaxElement>();
        }
        int i = Elements.Count;
        Elements.Add(e);
        SetupElementAtIndex(i);
    }

    /// <summary>
    /// Set the position of an object such that the bottom left of the object ends up being x and y
    /// </summary>
    /// <param name="obj">Object</param>
    /// <param name="r">Renderer</param>
    /// <param name="x">x bottom left</param>
    /// <param name="y">y bottom left</param>
    public static void SetPosition(GameObject obj, Renderer r, float x, float y)
    {
        Vector3 pos = new Vector3(x, y, obj.transform.position.z);
        obj.transform.position = pos;

        float xOffset = r.bounds.min.x - obj.transform.position.x;
        if (xOffset != 0)
        {
            pos.x -= xOffset;
            obj.transform.position = pos;
            if(obj.name== "goldchest(z1)") obj.SendMessage("CloseBox");
        }
        float yOffset = r.bounds.min.y - obj.transform.position.y;
        if (yOffset != 0)
        {
            pos.y -= yOffset;
            obj.transform.position = pos;
            if (obj.name == "goldchest(z1)") obj.SendMessage("CloseBox");
        }
    }

    // Use this for initialization
    void Start()
    {

#if UNITY_EDITOR

        StartCoroutine(InitializeAfterSlightDelay());

#else
		
		SetupElements();
		
#endif

    }

    IEnumerator InitializeAfterSlightDelay()
    {
        // hack to work around start maximized problem with Unity in editor
        yield return new WaitForSeconds(0.05f);
        SetupElements();
    }

    // Update is called once per frame
    void Update()
    {
        float t = Time.deltaTime * Speed;
        foreach (FreeParallaxElement e in Elements)
        {
            e.Update(this, t, parallaxCamera);
        }
    }
}

public enum FreeParallaxPositionMode
{
    [Tooltip("Wrap and anchor to the top (or right for a vertical parallax) of the screen")]
    WrapAnchorTop,

    [Tooltip("Wrap and anchor to the bottom (or left for a vertical parallax) of the screen")]
    WrapAnchorBottom,

    [Tooltip("No wrap, this is an individual object that starts off screen")]
    IndividualStartOffScreen,

    [Tooltip("No wrap, this is an individual object that starts on screen")]
    IndividualStartOnScreen,

    [Tooltip("Wrap and maintain original position")]
    WrapAnchorNone
}

/// <summary>
/// Contains parameters that repositions an object when it leaves the screen
/// </summary>
[Serializable]
public class FreeParallaxElementRepositionLogic
{
    [Tooltip("Set whether to wrap the object (for a full width (or height for vertical parallax) element, or to use individual elements such as trees, clouds and light rays.")]
    public FreeParallaxPositionMode PositionMode = FreeParallaxPositionMode.WrapAnchorBottom;

    [Tooltip("Set to a percentage of the screen height (or width for vertical parallax) to scale the element. Set to 0 to leave as the original scale, or 1 to scale to the height (or width for a vertical parallax) of the screen.")]
    public float ScaleHeight;

    [Tooltip("Sorting order for rendering. Leave as 0 to use original sort order.")]
    public int SortingOrder;

    [Tooltip("Minimum y percent in viewport space to reposition when object leaves the screen. 0.5 would position it at least half way up the screen for a horizontal parallax, or 5 would position it at least 5 screen heights away for a vertical parallax.")]
    public float MinYPercent;

    [Tooltip("Maximum y percent in viewport space to reposition when object leaves the screen. 0.75 would position it no more than 3/4 up the screen for a horizontal parallax, or 10 would position it no more than 10 screen heights away for a vertical parallax.")]
    public float MaxYPercent;

    [Tooltip("Minimum x percent in viewport space to reposition when object leaves the screen. 5 would position it at least 5 screen widths away for a horizontal parallax, or 0.5 would position it at least half way across the screen for a vertical parallax.")]
    public float MinXPercent;

    [Tooltip("Maximum x percent in viewport space to reposition when object leaves the screen. 10 would position it no more than 10 screen widths away for a horizontal parallax or 0.75 would position it no more than 3/4 across the screen for a vertical parallax.")]
    public float MaxXPercent;
}

[Serializable]
public class FreeParallaxElement
{
    internal readonly List<Renderer> GameObjectRenderers = new List<Renderer>();

    [Tooltip("Game objects to parallax. These will be cycled in sequence, which allows a long rolling background or different individual objects. " +
             "If there is only one, and the reposition logic specifies to wrap, a second object will be added that is a clone of the first. " +
             "It is recommended that these all be the same size.")]
    public List<GameObject> GameObjects;

    [Tooltip("The speed at which this object moves in relation to the speed of the parallax.")]
    [Range(0.0f, 1.0f)] public float SpeedRatio;

    [Tooltip("Contains logic on how this object repositions itself when moving off screen.")]
    public FreeParallaxElementRepositionLogic RepositionLogic;

    [HideInInspector] public FreeParallaxElementRepositionLogicFunction RepositionLogicFunction;

    public void SetupState(FreeParallax p, Camera c, int index)
    {
        // add a second object if we need one to wrap properly
        if (RepositionLogic.PositionMode != FreeParallaxPositionMode.IndividualStartOffScreen &&
            RepositionLogic.PositionMode != FreeParallaxPositionMode.IndividualStartOnScreen &&
            GameObjects.Count == 1)
        {
            GameObject obj = GameObject.Instantiate(GameObjects[0]) as GameObject;
            obj.transform.parent = GameObjects[0].transform.parent;
            obj.transform.position = GameObjects[0].transform.position;
            GameObjects.Add(obj);
        }

        if (GameObjectRenderers.Count == 0)
        {
            foreach (GameObject obj in GameObjects)
            {
                Renderer r = obj.GetComponent<Renderer>();
                if (r == null)
                {
                    Debug.LogError("Null renderer found at element index " + index.ToString() + ", each game object in the parallax must have a renderer");
                    return;
                }
                GameObjectRenderers.Add(r);
            }
        }
    }

    public void SetupScale(FreeParallax p, Camera c, int index)
    {
        Vector3 worldBottom = c.ViewportToWorldPoint(Vector3.zero);

        for (int i = 0; i < GameObjects.Count; i++)
        {
            GameObject obj = GameObjects[i];
            Renderer r = GameObjectRenderers[i];
            Bounds b = r.bounds;

            if (RepositionLogic.ScaleHeight > 0.0f)
            {
                float percent;
                obj.transform.localScale = Vector3.one;
                if (p.IsHorizontal)
                {
                    Vector3 maxPoint = c.WorldToViewportPoint(new Vector3(0.0f, worldBottom.y + b.size.y, 0.0f));
                    percent = RepositionLogic.ScaleHeight / maxPoint.y;
                }
                else
                {
                    Vector3 maxPoint = c.WorldToViewportPoint(new Vector3(worldBottom.x + b.size.x, 0.0f, 0.0f));
                    percent = RepositionLogic.ScaleHeight / maxPoint.x;
                }

                obj.transform.localScale = new Vector3(percent, percent, 1);
                b = r.bounds;
            }

            if (RepositionLogic.PositionMode != FreeParallaxPositionMode.IndividualStartOffScreen &&
                RepositionLogic.PositionMode != FreeParallaxPositionMode.IndividualStartOnScreen &&
                SpeedRatio > 0.0f)
            {
                if (p.IsHorizontal)
                {
                    // if we aren't at least a viewport width * 1.1, resize in x only (a little stretching and we'll log a warning)
                    float objWidth = c.WorldToViewportPoint(new Vector3(worldBottom.x + b.size.x, 0, 0)).x;
                    if (objWidth < 1.1f)
                    {
                        Debug.LogWarning("Game object in element index " + index.ToString() + " did not fit the screen width but was asked to wrap, so it was stretched. This can be fixed " +
                                         "by making sure any parallax graphics that wrap are at least 1.1x times the largest width resolution you support.");
                        Vector3 scale = obj.transform.localScale;
                        scale.x = (scale.x * (1.0f / objWidth)) + 0.1f;
                        obj.transform.localScale = scale;
                    }
                }
                else
                {
                    // if we aren't at least a viewport height * 1.1, resize in y only (a little stretching and we'll log a warning)
                    float objHeight = c.WorldToViewportPoint(new Vector3(0.0f, worldBottom.y + b.size.y, 0.0f)).y;
                    if (objHeight < 1.1f)
                    {
                        Debug.LogWarning("Game object in element index " + index.ToString() + " did not fit the screen height but was asked to wrap, so it was stretched. This can be fixed " +
                                         "by making sure any parallax graphics that wrap are at least 1.1x times the largest height resolution you support.");
                        Vector3 scale = obj.transform.localScale;
                        scale.y = (scale.y * (1.0f / objHeight)) + 0.1f;
                        obj.transform.localScale = scale;
                    }
                }
            }
        }
    }

    public void SetupPosition(FreeParallax p, Camera c, int index)
    {
        Vector3 screenLeft = c.ViewportToWorldPoint(Vector3.zero);
        Vector3 screenTop = c.ViewportToWorldPoint(Vector3.one);
        float start, offset;

        if (p.IsHorizontal)
        {
            start = screenTop.y + 1.0f;
            offset = screenLeft.x + GameObjectRenderers[0].bounds.size.x;
        }
        else
        {
            start = screenTop.x + 1.0f;
            offset = screenLeft.y + GameObjectRenderers[0].bounds.size.y;
        }

        for (int i = 0; i < GameObjects.Count; i++)
        {
            GameObject obj = GameObjects[i];
            Renderer r = GameObjectRenderers[i];
            if (RepositionLogic.SortingOrder != 0)
            {
                r.sortingOrder = RepositionLogic.SortingOrder;
            }

            if (RepositionLogic.PositionMode == FreeParallaxPositionMode.IndividualStartOffScreen ||
                RepositionLogic.PositionMode == FreeParallaxPositionMode.IndividualStartOnScreen)
            {
                float x, y;
                if (p.IsHorizontal)
                {
                    x = (RepositionLogic.PositionMode == FreeParallaxPositionMode.IndividualStartOnScreen ? r.bounds.min.x : 0);
                    y = (RepositionLogic.PositionMode == FreeParallaxPositionMode.IndividualStartOnScreen ? r.bounds.min.y : start + r.bounds.size.y);
                }
                else
                {
                    x = (RepositionLogic.PositionMode == FreeParallaxPositionMode.IndividualStartOnScreen ? r.bounds.min.x : start + r.bounds.size.x);
                    y = (RepositionLogic.PositionMode == FreeParallaxPositionMode.IndividualStartOnScreen ? r.bounds.min.y : 0);
                }
                FreeParallax.SetPosition(obj, r, x, y);
            }
            else
            {
                // position in the next spot in line
                if (p.IsHorizontal)
                {
                    offset -= (r.bounds.size.x - p.WrapOverlap);
                }
                else
                {
                    offset -= (r.bounds.size.y - p.WrapOverlap);
                }
                obj.transform.rotation = Quaternion.identity;

                // anchor to the top of the screen
                if (RepositionLogic.PositionMode == FreeParallaxPositionMode.WrapAnchorTop)
                {
                    if (p.IsHorizontal)
                    {
                        Vector3 topWorld = c.ViewportToWorldPoint(new Vector3(0.0f, 1.0f, 0.0f));
                        FreeParallax.SetPosition(obj, r, offset, topWorld.y - r.bounds.size.y);
                    }
                    else
                    {
                        Vector3 topWorld = c.ViewportToWorldPoint(new Vector3(1.0f, 0.0f, 0.0f));
                        FreeParallax.SetPosition(obj, r, topWorld.x - r.bounds.size.x, offset + r.bounds.size.y);
                    }
                }
                else if (RepositionLogic.PositionMode == FreeParallaxPositionMode.WrapAnchorBottom)
                {
                    if (p.IsHorizontal)
                    {
                        FreeParallax.SetPosition(obj, r, offset, screenLeft.y);
                    }
                    else
                    {
                        FreeParallax.SetPosition(obj, r, screenLeft.x, offset);
                    }
                }
                else
                {
                    // no anchor, maintain position
                    if (p.IsHorizontal)
                    {
                        FreeParallax.SetPosition(obj, r, offset, r.bounds.min.y);
                    }
                    else
                    {
                        FreeParallax.SetPosition(obj, r, r.bounds.min.x, offset);
                    }
                }

                GameObjects.RemoveAt(i);
                GameObjects.Insert(0, obj);
                GameObjectRenderers.RemoveAt(i);
                GameObjectRenderers.Insert(0, r);
            }
        }
    }

    /// <summary>
    /// Update the element given a distance of parallax move, t
    /// </summary>
    /// <param name="p">Parallax container</param>
    /// <param name="t">Total amount the parallax moved. Elements will move a percetage of this distance.</param>
    public void Update(FreeParallax p, float t, Camera c)
    {
        if (GameObjects == null || GameObjects.Count == 0 || GameObjects.Count != GameObjectRenderers.Count)
        {
            // cannot update, something went wrong in setup
            return;
        }

        // move everything first
        if (p.IsHorizontal)
        {
            foreach (GameObject obj in GameObjects)
            {
                obj.transform.Translate(t * SpeedRatio, 0.0f, 0.0f);
            }
        }
        else
        {
            foreach (GameObject obj in GameObjects)
            {
                obj.transform.Translate(0.0f, t * SpeedRatio, 0.0f);
            }
        }

        bool wrap = RepositionLogic.PositionMode != FreeParallaxPositionMode.IndividualStartOffScreen &&
            RepositionLogic.PositionMode != FreeParallaxPositionMode.IndividualStartOnScreen;

        // if it's an individual object we let it go an extra screen width in case the player changes direction
        float padding = (wrap ? 0.0f : 1.0f);
        float minEdge, maxEdge;
        if (p.IsHorizontal)
        {
            minEdge = c.rect.x - padding;
            maxEdge = c.rect.width + padding;
        }
        else
        {
            minEdge = c.rect.y - padding;
            maxEdge = c.rect.height + padding;
        }

        int end = GameObjects.Count;

        // now check for wrapping and stuff going off screen
        for (int i = 0; i < end; i++)
        {
            GameObject obj = GameObjects[i];
            Renderer r = GameObjectRenderers[i];
            Bounds b = r.bounds;
            Vector3 screenEdge = (t > 0 ? c.WorldToViewportPoint(b.min) : c.WorldToViewportPoint(b.max));
            float screenEdgeValue = (p.IsHorizontal ? screenEdge.x : screenEdge.y);

            if (wrap)
            {
                if (t > 0 && screenEdgeValue >= maxEdge)
                {
                    if (p.IsHorizontal)
                    {
                        // move to the back of the line at far left
                        float newX = (GameObjectRenderers[0].bounds.min.x - r.bounds.size.x) + p.WrapOverlap;
                        FreeParallax.SetPosition(obj, r, newX, r.bounds.min.y);
                    }
                    else
                    {
                        // move to the back of the line at far bottom
                        float newY = (GameObjectRenderers[0].bounds.min.y - r.bounds.size.y) + p.WrapOverlap;
                        FreeParallax.SetPosition(obj, r, r.bounds.min.x, newY);
                    }

                    GameObjects.RemoveAt(i);
                    GameObjects.Insert(0, obj);
                    GameObjectRenderers.RemoveAt(i);
                    GameObjectRenderers.Insert(0, r);
                }
                else if (t < 0 && screenEdgeValue <= minEdge)
                {
                    if (p.IsHorizontal)
                    {
                        // move to the front of the line at far right
                        float newX = (GameObjectRenderers[GameObjects.Count - 1].bounds.max.x) - p.WrapOverlap;
                        FreeParallax.SetPosition(obj, r, newX, r.bounds.min.y);
                    }
                    else
                    {
                        // move to the front of the line at far top
                        float newY = (GameObjectRenderers[GameObjects.Count - 1].bounds.max.y) - p.WrapOverlap;
                        FreeParallax.SetPosition(obj, r, r.bounds.min.x, newY);
                    }

                    GameObjects.RemoveAt(i);
                    GameObjects.Add(obj);
                    GameObjectRenderers.RemoveAt(i--);
                    GameObjectRenderers.Add(r);
                    end--;
                }
            }
            else if (p.IsHorizontal)
            {
                if (t > 0 && (screenEdge.y >= c.rect.height || screenEdgeValue >= maxEdge))
                {
                    if (RepositionLogicFunction != null)
                    {
                        RepositionLogicFunction(p, this, t, obj, r);
                    }
                    else
                    {
                        Vector3 leftEdge = c.ViewportToWorldPoint(Vector3.zero);
                        float randX = UnityEngine.Random.Range(RepositionLogic.MinXPercent, RepositionLogic.MaxXPercent);
                        float randY = UnityEngine.Random.Range(RepositionLogic.MinYPercent, RepositionLogic.MaxYPercent);
                        Vector3 newWorldPoint = c.ViewportToWorldPoint(new Vector3(randX, randY));
                        FreeParallax.SetPosition(obj, r, leftEdge.x - newWorldPoint.x, newWorldPoint.y);
                    }
                }
                else if (t < 0 && (screenEdge.y >= c.rect.height || screenEdge.x < minEdge))
                {
                    if (RepositionLogicFunction != null)
                    {
                        RepositionLogicFunction(p, this, t, obj, r);
                    }
                    else
                    {
                        Vector3 rightEdge = c.ViewportToWorldPoint(Vector3.one);
                        float randX = UnityEngine.Random.Range(RepositionLogic.MinXPercent, RepositionLogic.MaxXPercent);
                        float randY = UnityEngine.Random.Range(RepositionLogic.MinYPercent, RepositionLogic.MaxYPercent);
                        Vector3 newWorldPoint = c.ViewportToWorldPoint(new Vector3(randX, randY));
                        FreeParallax.SetPosition(obj, r, rightEdge.x + newWorldPoint.x, newWorldPoint.y);
                    }
                }
            }
            else
            {
                if (t > 0 && (screenEdge.x >= c.rect.width || screenEdgeValue >= maxEdge))
                {
                    if (RepositionLogicFunction != null)
                    {
                        RepositionLogicFunction(p, this, t, obj, r);
                    }
                    else
                    {
                        Vector3 bottomEdge = c.ViewportToWorldPoint(Vector3.zero);
                        float randX = UnityEngine.Random.Range(RepositionLogic.MinXPercent, RepositionLogic.MaxXPercent);
                        float randY = UnityEngine.Random.Range(RepositionLogic.MinYPercent, RepositionLogic.MaxYPercent);
                        Vector3 newWorldPoint = c.ViewportToWorldPoint(new Vector3(randX, randY));
                        FreeParallax.SetPosition(obj, r, newWorldPoint.x, bottomEdge.y - newWorldPoint.y);
                    }
                }
                else if (t < 0 && (screenEdge.x >= c.rect.width || screenEdge.y < minEdge))
                {
                    if (RepositionLogicFunction != null)
                    {
                        RepositionLogicFunction(p, this, t, obj, r);
                    }
                    else
                    {
                        Vector3 topEdge = c.ViewportToWorldPoint(Vector3.one);
                        float randX = UnityEngine.Random.Range(RepositionLogic.MinXPercent, RepositionLogic.MaxXPercent);
                        float randY = UnityEngine.Random.Range(RepositionLogic.MinYPercent, RepositionLogic.MaxYPercent);
                        Vector3 newWorldPoint = c.ViewportToWorldPoint(new Vector3(randX, randY));
                        FreeParallax.SetPosition(obj, r, newWorldPoint.x, topEdge.y + newWorldPoint.y);
                    }
                }
            }
        }
    }
}