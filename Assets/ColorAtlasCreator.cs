using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;
using System;
using TMPro;

public class ColorAtlasCreator : MonoBehaviour
{
  private Texture2D texture;
  private Sprite sprite;
  private Image image;
  [SerializeField]
  private Button genButton;
  [SerializeField]
  private Button saveButton;
  [SerializeField]
  private TMP_Dropdown bitsPerChannelMenu;
  [SerializeField]
  private TMP_InputField pixelsPerColorField;
  private int pixelsPerColor;
  private int bitsPerChannel;
  private int bpcSqrt;
  private int size;

  private void Start()
  {
    image = GetComponentInChildren<Image>();
    OnBitsPerChannelChanged();
    pixelsPerColor = int.Parse(pixelsPerColorField.text);
  }

  public void GenerateTexture()
  {
    size = bitsPerChannel * bpcSqrt * pixelsPerColor;
    texture = new Texture2D(size, size, TextureFormat.RGB24, false);
    for (int r = 0; r < bitsPerChannel; r++)
    {
      for (int g = 0; g < bitsPerChannel; g++)
      {
        for (int bx = 0; bx < bpcSqrt; bx++)
        {
          for (int by = 0; by < bpcSqrt; by++)
          {
            for (int i = 0; i < pixelsPerColor; i++)
            {
              for (int j = 0; j < pixelsPerColor; j++)
              {
                texture.SetPixel(
                  pixelsPerColor * bpcSqrt * r + pixelsPerColor * bx + i,
                  pixelsPerColor * bpcSqrt * g + pixelsPerColor * by + j,
                  new Color(
                    (float)r / (bitsPerChannel - 1),
                    (float)g / (bitsPerChannel - 1),
                    (float)(bx + by * bpcSqrt) / (bitsPerChannel - 1)));
              }
            }
          }
        }
      }
    }
    texture.Apply();
    sprite = Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.zero);
  }

  public void OnGenerateButtonClicked()
  {
    genButton.interactable = false;
    saveButton.interactable = false;
    //Canvas.ForceUpdateCanvases();
    GenerateTexture();
    image.sprite = sprite;
    genButton.interactable = true;
    saveButton.interactable = true;
  }
  public void SaveTexture()
  {
    byte[] bytes = texture.EncodeToPNG();
    string path = EditorUtility.SaveFilePanel("Save atlas", "", "atlas_" + bitsPerChannel + "x" + pixelsPerColor, "png");
    if (path.Length != 0)
      File.WriteAllBytes(path + "color_atlas_" + pixelsPerColor + "x" + bitsPerChannel + ".png", bytes);
    else
      EditorUtility.DisplayDialog("Save fail", "Save path is empty", "ok");
  }

  public void OnPixelsPerColorChanged()
  {
    if (pixelsPerColorField.text.Length != 0)
    {
      string error = "Pixels per color invalid value";
      int value = int.Parse(pixelsPerColorField.text);
      if (value < 0)
        EditorUtility.DisplayDialog(error, "Value is negative", "ok");
      else if (value * bpcSqrt * bitsPerChannel > 16384)
      {
        EditorUtility.DisplayDialog(error, "Resulting texture is too big. An appropriate value was paste in an input field", "ok");
        pixelsPerColorField.text = ((int)(16384.0 / bpcSqrt / bitsPerChannel)).ToString();
      }
      else
        genButton.interactable = true;
    }
    else
      genButton.interactable = false;
  }

  public void OnBitsPerChannelChanged()
  {
    bitsPerChannel = int.Parse(bitsPerChannelMenu.captionText.text);
    bpcSqrt = (int)Mathf.Sqrt(bitsPerChannel);
  }

}