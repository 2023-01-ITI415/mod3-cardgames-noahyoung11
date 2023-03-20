using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BezierMover : MonoBehaviour
{
    public enum eState{idle, pre, active, post}
    public enum ePosMode{world, local, ugui}

    [Header("Inscribed")]
    [Tooltip("world sets transform.position," + "local sets transform.localPosition" + "ugui sets anchorMin and anchorMax of RectTransform")]
    public ePosMode posMode = ePosMode.ugui;

    [Header("Dynamic")]
    public eState state = eState.idle;
    public float timeStart = -1f;
    public float timeDuration = 1f;
    public string easingCurve = Easing.InOut;
    public List<Vector3> bezierPts;
    public UnityEvent completionEvent;

    public float u {get; private set;}
    public float uCurved {get; private set;}

    private RectTransform rectTrans;

    public void Init(List<Vector3> pts, float timeD = 1, float timeS = 0){
        if(pts == null || pts.Count == 0){
            Debug.LogError("You must pass at least one point into Init()!");
            return;
        }

        rectTrans = GetComponent<RectTransform>();

        pos = pts[0];

        if(pts.Count == 1){
            completionEvent.Invoke();
            return;
        }

        bezierPts = new List<Vector3>(pts);

        if(timeS == 0) timeS = Time.time;
        timeStart = timeS;
        timeDuration = timeD;

        state = eState.pre;
    }
    
    public void Init(List<Vector2> ptsV2, float timeD = 1, float timeS = 0){
        List<Vector3> ptsV3 = new List<Vector3>();
        foreach(Vector2 v2 in ptsV2){ptsV3.Add((Vector3)v2);}

        Init(ptsV3, timeD, timeS);
    }

    public Vector3 pos{
        get{
            if(posMode == ePosMode.ugui){
                return rectTrans.anchorMin;
            }else if(posMode == ePosMode.local){
                return transform.localPosition;
            }else {
                return transform.position;
            }
        }
        private set{
            if(posMode == ePosMode.ugui){
                rectTrans.anchorMin = rectTrans.anchorMax = value;
            }else if(posMode == ePosMode.local){
                transform.localPosition = value;
            }else{
                transform.position = value;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(state == eState.idle || state == eState.post) return;

        u = (Time.time - timeStart) / timeDuration;

        uCurved = Easing.Ease(u, easingCurve);

        if(u < 0){
            state = eState.pre;
        }else{
            if(u < 1){
                state = eState.active;
            }else{
                uCurved = 1;
                state = eState.post;
                completionEvent.Invoke();
            }
            pos = Utils.Bezier(uCurved, bezierPts);
        }
    }
}
