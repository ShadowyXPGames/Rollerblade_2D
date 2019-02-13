using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grapple_Controller : MonoBehaviour
{

    #region events
    public delegate void GrappleEventHandler();

    public event GrappleEventHandler OnBeginGrapple;
    public event GrappleEventHandler OnEndGrapple;
    public event GrappleEventHandler OnWinchPull;

    public void CallOnBeginGrapple() {
        OnBeginGrapple?.Invoke();
    }
    public void CallOnEndGrapple() {
        OnEndGrapple?.Invoke();
    }
    public void CallOnWinchPull() {
        OnWinchPull?.Invoke();
    }
    #endregion

    #region ropeInitVars
    [SerializeField]
    private GameObject ropePrefab;
    [SerializeField]
    private bool canGrapple = true;
    private bool rgrappling = false;
    [SerializeField]
    private LayerMask grapplableMask;
    #endregion

    #region misc things
    [SerializeField]
    private Camera mainCamera;
    [SerializeField]
    private Rigidbody2D body;
    private Vector2 currentRope;
    [SerializeField]
    private HingeJoint2D hinge;
    [SerializeField]
    private Transform GrappleHolder;
    [SerializeField]
    private LineRenderer p2pLine;
    private Vector2 beginningLoc;
    private Rigidbody2D beginningRB;
    private bool goodToGo = false;
    private Vector2 endLoc;
    private Rigidbody2D endRB;
    [SerializeField]
    private float tolerance;
    #endregion

    void Start() {
        if(ropePrefab == null) {
            Debug.LogError("Did not set a rope prefab on player");
        }
        if(mainCamera == null) {
            Debug.LogError("No main camera attached to player");
        }
        if(hinge == null) {
            Debug.LogError("No hinge attached to player");
        }
        if (p2pLine == null) {
            Debug.LogError("No line renderer attached to player");
        } else {
            p2pLine.enabled = false;
        }
        hinge.enabled = false;
    }

    void Update() {
        //Base grapple begin
        if (Input.GetMouseButtonDown(0) &&canGrapple == true) {
            if (canGrapple) {
                Vector2 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(this.transform.position, mouseWorld - (Vector2)this.transform.position, (mouseWorld - (Vector2)this.transform.position).magnitude, grapplableMask);
                if (hit) {
                    //TODO fix
                    Transform ropeSegment = Instantiate(ropePrefab, hit.point, Quaternion.identity).GetComponent<Rope>().Init(hit.rigidbody, GrappleHolder.position, hit.point, false, null);
                    hinge.enabled = true;
                    hinge.connectedBody = ropeSegment.GetComponent<Rigidbody2D>();
                    hinge.connectedAnchor = (this.transform.position - ropeSegment.position) + (ropeSegment.position - GrappleHolder.position);
                    canGrapple = false;
                    rgrappling = true;
                    currentRope = hit.point;
                    CallOnBeginGrapple();
                }
            }
        }
        //Base grapple end
        if (Input.GetMouseButtonUp(0) && rgrappling == true) {
            canGrapple = true;
            rgrappling = false;
            hinge.enabled = false;
            CallOnEndGrapple();
        }

        if (Input.GetMouseButtonDown(1)) {
            Vector2 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(this.transform.position, mouseWorld - (Vector2)this.transform.position, (mouseWorld - (Vector2)this.transform.position).magnitude, grapplableMask);
            if (hit) {
                beginningLoc = hit.point + (((Vector2)this.transform.position - hit.point).normalized * tolerance);
                p2pLine.enabled = true;
                p2pLine.SetPosition(0, beginningLoc);
                p2pLine.SetPosition(1, beginningLoc);
                beginningRB = hit.rigidbody;
            }
        } else if (Input.GetMouseButton(1)) {
            Vector2 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(beginningLoc, mouseWorld - beginningLoc, (mouseWorld - beginningLoc).magnitude, grapplableMask);
            if (hit && (hit.point - beginningLoc).magnitude > 0.1f) {
                p2pLine.startColor = Color.green;
                p2pLine.endColor = Color.green;
                p2pLine.SetPosition(1, hit.point);
                goodToGo = true;
                endLoc = hit.point;
                endRB = hit.rigidbody;
            } else {
                p2pLine.SetPosition(1, mouseWorld);
                p2pLine.startColor = Color.red;
                p2pLine.endColor = Color.red;
                goodToGo = false;
            }
        }
        if (Input.GetMouseButtonUp(1)){
            p2pLine.enabled = false;
            if (goodToGo) {
                //TODO FIX
                Transform ropeSegment = Instantiate(ropePrefab, endLoc, Quaternion.identity).GetComponent<Rope>().Init(null, beginningLoc, endLoc, true, null);
            }
        }

    }
}