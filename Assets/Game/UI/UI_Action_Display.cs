using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UI_Action_Display : MonoBehaviour
{
    [System.NonSerialized] public UI_Action_Display next;
    public float timeTillReturnToPool;
    float time;
    public delegate void OnTimeDone(UI_Action_Display deactivatedObj);
    public event OnTimeDone onTimeDone;

    public RectTransform rectTransform;
    public Image image;
    IAgent trackingTarget;
    public void TrackingRequest(IAgent agentToTrack, Sprite sprite, float timeTillReturnToPool)
    {
        trackingTarget = agentToTrack;
        image.sprite = sprite;
        this.timeTillReturnToPool = timeTillReturnToPool;
        time = 0;
    }
    private void Update()
    {
        //Tracking
        if(trackingTarget != null)
        {
            rectTransform.position = trackingTarget.args.screenPos;
        }

        time += Time.deltaTime;
        if(time >= timeTillReturnToPool)
        {
            onTimeDone?.Invoke(this);
            gameObject.SetActive(false);
            //clear out tracking variables
            trackingTarget = null;
        }
    }
}
