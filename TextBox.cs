using Godot;
using System;

public class TextBox : Control
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    [Export(PropertyHint.File)]
    public string dialogFilePath;
    [Export(PropertyHint.File)]
    public string levelScene;
    public string[] dialog = {
        "Hi! This is [color=red]random[/color] text...",
        "This is some random text...",
        "Bye!!!"
    };

    public int dialogID = 0;
    public bool finished = false;
    public SceneTransition sceneTransition;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        sceneTransition = GetNode<SceneTransition>("/root/SceneTransition");
        dialogFilePath = dialogFilePath.Substring("res://".Length());
        dialog = System.IO.File.ReadAllLines(dialogFilePath);
        LoadDialog();
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("ui_accept"))
        {
            LoadDialog();
        }
    }

    public void LoadDialog()
    {
        if (dialogID < dialog.Length)
        {
            var richTextLabel = GetNode<RichTextLabel>("DialogBox/RichTextLabel");
            richTextLabel.BbcodeText = dialog[dialogID];
            richTextLabel.PercentVisible = 0;
            GetNode<Tween>("DialogBox/Tween").InterpolateProperty(richTextLabel, "percent_visible", 0, 1, 1, Tween.TransitionType.Linear, Tween.EaseType.InOut);
            GetNode<Tween>("DialogBox/Tween").Start();
        }
        else
        {
            if (levelScene == null)
            {
                QueueFree();
            }
            else
            {
                sceneTransition.ChangeScene(levelScene);
            }
        }
        dialogID++;
    }
}
