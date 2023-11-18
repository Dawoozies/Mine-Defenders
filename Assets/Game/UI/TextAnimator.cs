using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Unity.VisualScripting;

[Serializable]
public class TextAnimation
{
    public string animName;
    public int frames;
    public List<float> horizontalSpaces;
    public List<float> rotations;
    public List<Color> colors;
    public List<float> verticalOffsets;
    public List<InterpolationType> horizontalSpaces_interpolationTypes;
    public List<InterpolationType> rotations_interpolationTypes;
    public List<InterpolationType> colors_interpolationTypes;
    public List<InterpolationType> verticalOffsets_interpolationTypes;
    [HideInInspector]
    public float time;
    public string horizontalTag(int index, int targetIndex, float argTime)
    {
        if (horizontalSpaces == null || horizontalSpaces.Count == 0)
            return string.Empty;
        float tagValue = Interpolation.Interpolate(
            horizontalSpaces[index % horizontalSpaces.Count],
            horizontalSpaces[targetIndex % horizontalSpaces.Count],
            argTime,
            horizontalSpaces_interpolationTypes[index % horizontalSpaces_interpolationTypes.Count]
            );
        return $"<space={tagValue}>";
    }
    public (string, string) rotationTag(int index, int targetIndex, float argTime)
    {
        if (rotations == null || rotations.Count == 0)
            return (string.Empty, string.Empty);
        float tagValue = Interpolation.Interpolate(
            rotations[index % rotations.Count],
            rotations[targetIndex % rotations.Count],
            argTime,
            rotations_interpolationTypes[index % rotations_interpolationTypes.Count]
            );
        return ($"<rotate={tagValue}>", "</rotate>");
    }
    public (string, string) colorTag(int index, int targetIndex, float argTime)
    {
        if (colors == null || colors.Count == 0)
            return (string.Empty, string.Empty);
        Color tagValue = Interpolation.Interpolate(
            colors[index % colors.Count],
            colors[targetIndex % colors.Count],
            argTime,
            colors_interpolationTypes[index % colors_interpolationTypes.Count]
            );
        string tagHexString = tagValue.ToHexString();
        return ($"<color=#{tagHexString}>","</color>");
    }
    public (string, string) verticalOffsetTag(int index, int targetIndex, float argTime)
    {
        if (verticalOffsets == null || verticalOffsets.Count == 0)
            return (string.Empty, string.Empty);
        float tagValue = Interpolation.Interpolate(
            verticalOffsets[index % verticalOffsets.Count],
            verticalOffsets[targetIndex % verticalOffsets.Count],
            argTime,
            verticalOffsets_interpolationTypes[index % verticalOffsets_interpolationTypes.Count]
            );
        return ($"<voffset={tagValue}>", "</voffset>");
    }
}
public class CharAnimationArgs
{
    public int currentIndex;
    public int targetIndex;
    public float time;
    public CharAnimationArgs()
    {
        currentIndex = 0;
        targetIndex = 1;
        time = 0;
    }
    public void Update(int frames, float timeDelta)
    {
        time += timeDelta;
        Debug.Log($"currentIndex = {currentIndex} targetIndex = {targetIndex} time = {time}");
        if (time > 1)
        {
            currentIndex = (currentIndex + 1) % frames;
            targetIndex = (currentIndex + 1) % frames;
            time = 0;
        }
    }
}
//apply to all characters
//apply to each character individually
public class TextAnimator : MonoBehaviour
{
    TextMeshProUGUI textMesh;
    [TextArea(10,20)]
    public string textInput;
    public float charIterationSpeed;
    public float animationSpeed;
    public TextAnimation currentAnimation;
    int currentChar;
    List<CharAnimationArgs> charAnimationArgs = new List<CharAnimationArgs>();
    public bool onlyResetAll;
    private void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        foreach (char c in textInput)
        {
            charAnimationArgs.Add(new CharAnimationArgs());
        }
        currentChar = 0;
    }

    private void Update()
    {
        if (charAnimationArgs == null || charAnimationArgs.Count == 0)
            return;

        currentAnimation.time += Time.deltaTime * charIterationSpeed;
        if(currentAnimation.time > 1)
        {
            currentChar = (currentChar + 1) % charAnimationArgs.Count;

            int iterationGuard = 0;
            while (char.IsWhiteSpace(textInput[currentChar]))
            {
                currentChar = (currentChar + 1) % charAnimationArgs.Count;

                iterationGuard++;
                if (iterationGuard > 30)
                    break;
            }

            currentAnimation.time = 0;
        }
        string displayedText = string.Empty;
        for (int i = 0; i < charAnimationArgs.Count; i++)
        {
            if (char.IsWhiteSpace(textInput[i]))
            {
                displayedText = displayedText + " ";
                continue;
            }
            string s = textInput[i].ToString();

            CharAnimationArgs args = charAnimationArgs[i];

            string horizontalSpaceTag = currentAnimation.horizontalTag(args.currentIndex, args.targetIndex, args.time);
            (string, string) rotateTag = currentAnimation.rotationTag(args.currentIndex, args.targetIndex, args.time);
            (string, string) colorTag = currentAnimation.colorTag(args.currentIndex, args.targetIndex, args.time);
            (string, string) verticalOffsetTag = currentAnimation.verticalOffsetTag(args.currentIndex, args.targetIndex, args.time);
            displayedText =
                displayedText
                + horizontalSpaceTag
                + rotateTag.Item1
                + colorTag.Item1
                + verticalOffsetTag.Item1
                + s
                + verticalOffsetTag.Item2
                + colorTag.Item2
                + rotateTag.Item2
                ;

            args.Update(currentAnimation.frames, Time.deltaTime * animationSpeed);
        }
        textMesh.text = displayedText;
        //Debug.Log($"textMesh.text.Length = {textMesh.text.Length}");
    }

}
