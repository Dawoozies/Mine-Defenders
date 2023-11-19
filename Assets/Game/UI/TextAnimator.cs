using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Matrix4x4 = UnityEngine.Matrix4x4;

[Serializable]
public class TextAnimation
{
    public string animName;
    public int frames;
    public List<Vector3> positions;
    public List<Vector3> eulerAngles;
    public List<Vector3> scales;
    public List<Color> colors;
    public List<InterpolationType> positions_InterpolationTypes;
    public List<InterpolationType> eulerAngles_InterpolationTypes;
    public List<InterpolationType> scales_InterpolationTypes;
    public List<InterpolationType> colors_InterpolationTypes;
    [HideInInspector]
    public float time;
    public Vector3 position(int index, int targetIndex, float t)
    {
        if (positions == null || positions.Count == 0)
            return Vector3.zero;
        return Interpolation.Interpolate(
            positions[index % positions.Count],
            positions[targetIndex % positions.Count],
            t,
            positions_InterpolationTypes[index % positions.Count]
            );
    }
    public Quaternion rotation(int index, int targetIndex, float t)
    {
        if(eulerAngles == null || eulerAngles.Count == 0)
            return Quaternion.identity;

        Quaternion current = Quaternion.Euler(eulerAngles[index % eulerAngles.Count]);
        Quaternion target = Quaternion.Euler(eulerAngles[targetIndex % eulerAngles.Count]);

        return Interpolation.Interpolate(
            current,
            target,
            t,
            eulerAngles_InterpolationTypes[index % eulerAngles.Count]
            );
    }
    public Vector3 scale(int index, int targetIndex, float t)
    {
        if(scales == null || scales.Count == 0)
            return Vector3.one;
        return Interpolation.Interpolate(
            scales[index % scales.Count],
            scales[targetIndex % scales.Count],
            t,
            scales_InterpolationTypes[index % scales.Count]
            );
    }
    public Color color(int index, int targetIndex, float t)
    {
        if (colors == null || colors.Count == 0)
            return Color.white;
        return Interpolation.Interpolate(
            colors[index % colors.Count],
            colors[targetIndex % colors.Count],
            t,
            colors_InterpolationTypes[index % colors.Count]
            );
    }
}
public class AnimationArgs
{
    public int currentIndex;
    public int targetIndex;
    public float time;
    public bool started;
    public bool dontPlayAnymore;
    public AnimationArgs()
    {
        currentIndex = 0;
        targetIndex = 1;
        time = 0;
        started = false;
        dontPlayAnymore = false;
    }
    public void Update(int frames, float timeDelta, bool playOnce)
    {
        if (!started || dontPlayAnymore)
            return;
        time += timeDelta;
        if(time > 1)
        {
            currentIndex = (currentIndex + 1) % frames;
            targetIndex = (currentIndex + 1) % frames;
            time = 0;
            if (playOnce)
            {
                if (currentIndex == frames - 1)
                {
                    dontPlayAnymore = true;
                }
            }
        }
    }
    public void ClearArgs()
    {
        currentIndex = 0;
        targetIndex = 1;
        time = 0;
        started = false;
        dontPlayAnymore = false;
    }
}
public class TextAnimator : MonoBehaviour
{
    public string playOnStartAnimName;
    TMP_Text textComponent;
    public float animationSpeed;
    float iterateTime;
    public float argsIterationSpeed;
    TextAnimation currentAnimation;
    TMP_TextInfo textInfo;
    TMP_MeshInfo[] cachedMeshInfo;

    Matrix4x4 matrix;
    int currentIndex;
    int targetIndex;
    int currentArgs;
    public bool oneAtATime;
    public bool playOnce;
    List<AnimationArgs> animationArgs;
    public List<TextAnimation> animations;
    bool dontPlayAnymore;
    private void Start()
    {
        textComponent = GetComponent<TMP_Text>();
        textComponent.ForceMeshUpdate();
        textInfo = textComponent.textInfo;
        cachedMeshInfo = textInfo.CopyMeshInfoVertexData();
        currentIndex = 0;
        targetIndex = 1;

        if(oneAtATime)
        {
            animationArgs = new List<AnimationArgs>();
            foreach (char c in textComponent.text)
            {
                animationArgs.Add(new AnimationArgs());
            }
            animationArgs[0].started = true;
        }
        if(playOnStartAnimName != null && playOnStartAnimName.Length > 0)
        {
            PlayAnimation(playOnStartAnimName, textComponent.text, oneAtATime, playOnce);
        }
    }
    public void PlayAnimation(string animName, string textInput, bool oneAtATimeOption, bool playOnce)
    {
        this.playOnce = playOnce;
        foreach (var animation in animations)
        {
            if(animation.animName == animName)
            {
                currentAnimation = animation;
                currentAnimation.time = 0;
                currentIndex = 0;
                targetIndex = 1;
                iterateTime = 0;
                currentArgs = 0;
                oneAtATime = oneAtATimeOption;
                textComponent.SetText(textInput);
                textComponent.ForceMeshUpdate();
                textInfo = textComponent.textInfo;
                cachedMeshInfo = textInfo.CopyMeshInfoVertexData();
                if (oneAtATime)
                {
                    animationArgs = new List<AnimationArgs>();
                    foreach (char c in textComponent.text)
                    {
                        animationArgs.Add(new AnimationArgs());
                    }
                    animationArgs[0].started = true;
                }
                dontPlayAnymore = false;
            }
        }
    }
    private void Update()
    {
        if (currentAnimation == null || dontPlayAnymore)
            return;
        currentAnimation.time += Time.deltaTime * animationSpeed;
        if(currentAnimation.time > 1)
        {
            currentIndex = (currentIndex + 1) % currentAnimation.frames;
            targetIndex = (currentIndex + 1) % currentAnimation.frames;
            currentAnimation.time = 0;
            if(playOnce && !oneAtATime)
            {
                if(currentIndex == currentAnimation.frames - 1)
                {
                    dontPlayAnymore = true;
                }
            }
        }
        if(animationArgs != null && animationArgs.Count > 0)
        {
            iterateTime += Time.deltaTime * argsIterationSpeed;
            if (iterateTime > 1)
            {
                currentArgs = (currentArgs + 1) % animationArgs.Count;
                iterateTime = 0;

                animationArgs[currentArgs].started = true;
            }
            foreach (AnimationArgs args in animationArgs)
            {
                args.Update(currentAnimation.frames, Time.deltaTime*animationSpeed, playOnce);
            }
        }

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible)
                continue;
            int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
            int vertexIndex = textInfo.characterInfo[i].vertexIndex;
            Vector3[] sourceVertices = cachedMeshInfo[materialIndex].vertices;

            Vector3[] destinationVertices = textInfo.meshInfo[materialIndex].vertices;
            Vector3 offset = new Vector2(
                (sourceVertices[vertexIndex].x + sourceVertices[vertexIndex+2].x)/2,
                textInfo.characterInfo[i].baseLine
                );


            destinationVertices[vertexIndex] = sourceVertices[vertexIndex] - offset;
            destinationVertices[vertexIndex + 1] = sourceVertices[vertexIndex + 1] - offset;
            destinationVertices[vertexIndex + 2] = sourceVertices[vertexIndex + 2] - offset;
            destinationVertices[vertexIndex + 3] = sourceVertices[vertexIndex + 3] - offset;

            Vector3 translation = Vector3.zero;
            Quaternion rotation = Quaternion.identity;
            Vector3 scale = Vector3.one;
            Color color = Color.white;
            if (oneAtATime)
            {
                AnimationArgs args = animationArgs[i];
                translation = currentAnimation.position(args.currentIndex, args.targetIndex, args.time);
                rotation = currentAnimation.rotation(args.currentIndex, args.targetIndex, args.time);
                scale = currentAnimation.scale(args.currentIndex, args.targetIndex, args.time);
                color = currentAnimation.color(args.currentIndex, args.targetIndex, args.time);

            }
            else
            {
                translation = currentAnimation.position(currentIndex, targetIndex, currentAnimation.time);
                rotation = currentAnimation.rotation(currentIndex, targetIndex, currentAnimation.time);
                scale = currentAnimation.scale(currentIndex, targetIndex, currentAnimation.time);
                color = currentAnimation.color(currentIndex, targetIndex, currentAnimation.time);

            }

            #region Translation, rotation, scaling
            matrix = Matrix4x4.TRS(translation, rotation, scale);


            destinationVertices[vertexIndex] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex]);
            destinationVertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 1]);
            destinationVertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 2]);
            destinationVertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 3]);

            destinationVertices[vertexIndex] += offset;
            destinationVertices[vertexIndex + 1] += offset;
            destinationVertices[vertexIndex + 2] += offset;
            destinationVertices[vertexIndex + 3] += offset;
            #endregion
            Color32[] newVertexColors = textInfo.meshInfo[materialIndex].colors32;
            newVertexColors[vertexIndex] = color;
            newVertexColors[vertexIndex + 1] = color;
            newVertexColors[vertexIndex + 2] = color;
            newVertexColors[vertexIndex + 3] = color;
        }
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            textComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }

    }
}
