using System;
using System.IO;
using UnityEditor;
using UnityEngine;

[Serializable]
public class SerialisedDialogueItem
{
    public string name;
    public string icon;
    public string text;
    public int fontSize;
    public int charactersPerSecond;

    public DialogueItem Deserialised => new DialogueItem() { Name = name, Text = text, FontSize = fontSize, CharactersPerSecond = charactersPerSecond };
}

[Serializable]
public class DialogueItem
{
    public string Name;
    public Sprite Icon;
    [TextArea] public string Text;
    public int FontSize;
    public int CharactersPerSecond;

    public SerialisedDialogueItem Serialised
    {
        get
        {
#if UNITY_EDITOR
            string ico;
            if (Icon != null)
            {
                string path = AssetDatabase.GetAssetPath(Icon);
                string relative = path.Substring("Assets/Resources/".Length);
                string noExtension = Path.ChangeExtension(relative, null);
                ico = noExtension;
            }
            else ico = "none";

			return new SerialisedDialogueItem() { name = Name, icon = ico, text = Text, fontSize = FontSize, charactersPerSecond = CharactersPerSecond };
#else
			return new SerialisedDialogueItem();
#endif
		}
	}
}
