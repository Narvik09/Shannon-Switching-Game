using Godot;
using System;

public class StartScreen : Control
{
    public PackedScene TestStage;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.GetNode<Button>("VBoxContainer/SinglePlayerButton").GrabFocus();
    }

    public void HideButtons()
    {
        GetNode<Sprite>("HomeScreen").Hide();
        GetNode<Label>("Title").Hide();
        GetNode<VBoxContainer>("VBoxContainer").Hide();
    }

    public void SetScene(string option, string scenePath)
    {
        GD.Print($"{option} pressed!!!");
        var mainScene = (PackedScene)ResourceLoader.Load(scenePath);
        var instance = (TestStage)mainScene.Instance();
        // instance.Option = option;
        AddChild(instance);
    }

    public void OnSinglePlayerButtonPressed()
    {

    }

    public void OnMultiPlayerButtonPressed()
    {
        SetScene("Multiplayer", "res://TestStage.tscn");
        HideButtons();
    }


    public void OnQuitButtonPressed()
    {
        GetTree().Quit();
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
