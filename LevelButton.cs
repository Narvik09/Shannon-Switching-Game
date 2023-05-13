using Godot;
using System;

public class LevelButton : Button
{
    private Vector2 originalSize = new Vector2(1, 1);
    private Vector2 growSize = new Vector2((float)1.05, (float)1.05);
    // Called when the node enters the scene tree for the first time.

    [Export(PropertyHint.File)]
    public string levelScene;


    public SceneTransition sceneTransition;

    public Global global;
    public override void _Ready()
    {
        // Global global = (Global)
        global = GetNode<Global>("/root/Global");
        sceneTransition = GetNode<SceneTransition>("/root/SceneTransition");
    }

    public void OnLevelButtonMouseEntered()
    {
        GrowButton(growSize, (float).1);
    }

    public void OnLevelButtonMouseExited()
    {
        GrowButton(originalSize, (float).1);
    }

    private void GrowButton(Vector2 endSize, float duration)
    {
        var tween = CreateTween().SetTrans(Tween.TransitionType.Linear).SetEase(Tween.EaseType.InOut);
        tween.TweenProperty(this, "rect_scale", endSize, duration);
    }

    public void OnLevelButtonPressed()
    {
        if (levelScene == null)
        {
            return;
        }
        sceneTransition.ChangeScene(levelScene);
    }

    public void OnShortButtonPressed()
    {
        // GD.Print("Short");
        global.isCutPlaying = !global.isPlayingFirst;
        if (global.isSinglePlayer)
        {
            global.isCutComputer = true;
        }
    }

    public void OnCutButtonPressed()
    {
        // GD.Print("Cut");
        global.isCutPlaying = global.isPlayingFirst;
        if (global.isSinglePlayer)
        {
            global.isShortComputer = true;
        }
    }

    public void OnSinglePlayerButtonPressed()
    {
        global.isSinglePlayer = true;
    }

    public void OnMultiPlayerButtonPressed()
    {
        global.isSinglePlayer = false;
    }
}
