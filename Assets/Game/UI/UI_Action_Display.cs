using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Action_Display : MonoBehaviour
{
    [System.NonSerialized] public UI_Action_Display next;
    public float timeTillReturnToPool;
    float time;
    public delegate void ReturnToPoolHandler(UI_Action_Display deactivatedObj);
    public event ReturnToPoolHandler ReturnToPoolEvent;

    public RectTransform rectTransform;
    public Image image;
    IAgent trackingTarget;
    Vector3 trackingOffset;
    public UI_Action_Display TrackingRequest(IAgent agentToTrack, Vector3 offset, Sprite sprite)
    {
        trackingTarget = agentToTrack;
        trackingOffset = offset;
        image.sprite = sprite;
        return this;
    }
    private void Update()
    {
        //Tracking
        if(trackingTarget != null)
        {
            rectTransform.position = trackingTarget.args.ScreenTrackingWithOffset(trackingOffset);
        }

        //time += Time.deltaTime;
        //if(time >= timeTillReturnToPool)
        //{
        //    onTimeDone?.Invoke(this);
        //    gameObject.SetActive(false);
        //    //clear out tracking variables
        //    trackingTarget = null;
        //}
    }
    public void ReturnToPool()
    {
        ReturnToPoolEvent?.Invoke(this);
        gameObject.SetActive(false);
        //clear out all set variables
        trackingTarget = null;
        trackingOffset = Vector3.zero;
        //image.sprite = null;
    }
}
