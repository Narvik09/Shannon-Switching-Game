using Godot;
using System;

public class LevelSelect : Control
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    public PackedScene LevelButton = (PackedScene)ResourceLoader.Load("res://LevelButton.tscn");

    [Export(PropertyHint.Dir)]
    public string dirPath;

    public GridContainer grid;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        grid = GetNode<GridContainer>("MarginContainer/VBoxContainer/GridContainer");
        GetLevels(dirPath);
    }

    private void GetLevels(string path)
    {
        Directory dir = new Directory();
        if (dir.Open(path) == Error.Ok)
        {
            dir.ListDirBegin();
            string fileName = dir.GetNext();
            while (fileName != "")
            {
                if (fileName == "TestStage.tscn")
                {
                    fileName = dir.GetNext();

                }
                if (fileName.EndsWith(".tscn"))
                {
                    // GD.Print(fileName);
                    CreateLevelButton((PackedScene)ResourceLoader.Load(dirPath + "/" + fileName), fileName);
                }
                fileName = dir.GetNext();

            }
            dir.ListDirEnd();
        }
        else
        {
            GD.Print("An error occurred while trying to access the path!");
        }
    }

    private void CreateLevelButton(PackedScene levelScene, string levelName)
    {
        var button = (LevelButton)LevelButton.Instance();
        var tempText = levelName.Trim().Substr(0, levelName.LastIndexOf('.'));
        button.Text = tempText.Replace('_', ' ');
        button.levelScene = levelScene;
        button.Icon = (Texture)ResourceLoader.Load("res://buttonIcons/" + tempText + ".png");
        grid.AddChild(button);
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
