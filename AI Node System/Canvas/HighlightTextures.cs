using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightTexturesSingleton
{
    public Texture2D runningHighlight;
    public Texture2D failureHighlight;
    public Texture2D successHighlight;
    public Texture2D notActiveHighlight;

    public HighlightTexturesSingleton()
    {
        runningHighlight = (Texture2D)Resources.Load("Textures/RunningHighlight");
        failureHighlight = (Texture2D)Resources.Load("Textures/FailureHighlight");
        successHighlight = (Texture2D)Resources.Load("Textures/SuccessHighlight");
        notActiveHighlight = (Texture2D)Resources.Load("Textures/NotActiveHighlight");
    }
}

public class HighlightTextures : MonoBehaviour
{
    public static HighlightTexturesSingleton instance;

    public static HighlightTexturesSingleton GetInstance()
    {
        if(instance==null)
        {
            instance = new HighlightTexturesSingleton();
        }
        return instance;
    }
}
