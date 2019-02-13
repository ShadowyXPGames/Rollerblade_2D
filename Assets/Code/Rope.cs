using UnityEngine;

public class Rope : MonoBehaviour
{
    #region Editor options
    [SerializeField]
    private bool initOnStart;
    [SerializeField]
    private bool drawLineInEditor;
    [SerializeField]
    private Color editorLineColor;
    public Vector2 startPosEditor { get { return startPos; } set { startPos = value; } }
    public Vector2 endPosEditor { get { return endPos; } set { endPos = value; } }
    [SerializeField]
    private Vector2 startPos;
    [SerializeField]
    private Vector2 endPos;
    [SerializeField]
    private bool pointToPoint;
    [SerializeField]
    private Rigidbody2D rigidBody1;
    [SerializeField]
    private Rigidbody2D rigidBody2;
    #endregion
    //butt
    [SerializeField]
    private float thickness;
    [SerializeField]
    private float ropeResolution;

    [SerializeField]
    private Rigidbody2D hook;
    [SerializeField]
    private GameObject link;

    [SerializeField]
    private LineRenderer lineRenderer;
    private Transform[] linePath;
    Vector3[] updatedPos;

    private void Start() {
        if (initOnStart) {
            Init(rigidBody1, startPos, -endPos, pointToPoint, rigidBody2);
        }
    }

    private void Update() {
        if (linePath != null) {
            updatedPos = new Vector3[linePath.Length];
            UpdateLineRenderer(linePath.Length, linePath, updatedPos);
            lineRenderer.SetPositions(updatedPos);
        }
    }

    public Transform Init(Rigidbody2D beginningRB, Vector2 startPoint, Vector2 endPoint, bool P2P, Rigidbody2D endRB) {

        Vector2 lineFromSTE = endPoint - startPoint;
        float ropeLen = lineFromSTE.magnitude;
        int manySegments = (int)(ropeLen * ropeResolution);
        float segmentDist = (ropeLen / (manySegments));

        HingeJoint2D hookJoint = hook.GetComponent<HingeJoint2D>();
        hookJoint.connectedBody = beginningRB;
        hookJoint.connectedAnchor = beginningRB.transform.InverseTransformPoint(hook.position);

        linePath = new Transform[manySegments + 1];
        linePath[manySegments] = hook.transform;
        if (!P2P) {
            CreateRope(manySegments, hook, segmentDist * .98f, -lineFromSTE, false, endRB);
        } else {
            CreateRopePointToPoint(manySegments, hook, segmentDist, -lineFromSTE, endRB);
        }

        lineRenderer.positionCount = manySegments + 1;
        lineRenderer.startWidth = thickness;
        lineRenderer.endWidth = thickness;

        pointToPoint = P2P;

        return linePath[0];

    }
    
    public bool getPointToPoint() {
        return pointToPoint;
    }

    private void CreateRope(int numLinks, Rigidbody2D prevBody, float len, Vector2 offset, bool leaveLast, Rigidbody2D endRB) {
        if(numLinks == 0) {
            return;
        }
        HingeJoint2D newLink = Instantiate(link, prevBody.transform.position + ((Vector3)offset.normalized * len), Quaternion.identity, transform).GetComponent<HingeJoint2D>();
        newLink.connectedBody = prevBody;
        newLink.connectedAnchor = offset.normalized * len;

        linePath[numLinks - 1] = newLink.transform;
        if(leaveLast && numLinks == 1) {
            HingeJoint2D newJoint = newLink.gameObject.AddComponent<HingeJoint2D>();
            newJoint.connectedBody = endRB;
            newJoint.connectedAnchor = endRB.transform.InverseTransformPoint(hook.position);
        }
        CreateRope(numLinks - 1, newLink.GetComponent<Rigidbody2D>(), len, offset, leaveLast, endRB);
    }

    private void CreateRopePointToPoint(int numLinks, Rigidbody2D prevBody, float len, Vector2 offset, Rigidbody2D endRB) {
        CreateRope(numLinks, prevBody, len, offset, true, endRB);
    }

    private void UpdateLineRenderer(int numLinks, Transform[] path, Vector3[] list) {
        if(numLinks == 0) {
            return;
        }
        list[numLinks - 1] = path[numLinks - 1].position;
        UpdateLineRenderer(numLinks - 1, path, list);
    }

    public Vector2 getJumpThing(Transform ropeSegment, out Vector2 thing) {
        Vector2 finalLine = new Vector2();
        int index = indexOfRopeTransform(ropeSegment, linePath.Length - 1);
        if(index == -1) {
            Debug.LogError("Tried to jump on nonexistent rope?");
            thing = Vector2.negativeInfinity;
            return Vector2.negativeInfinity;
        }
        Vector2 spot1 = new Vector2();
        Vector2 spot2 = new Vector2();
        int offset = 0;

        try {
            spot1 = linePath[index - 2].position;
        } catch (System.IndexOutOfRangeException e) {
            spot1 = linePath[0].position;
        }

        try {
            spot2 = linePath[index + 2].position;
        } catch (System.IndexOutOfRangeException e) {
            spot2 = linePath[linePath.Length - 1].position;
        }
        finalLine = spot1 - spot2;
        thing = spot1;
        return finalLine;
    }

    private int indexOfRopeTransform(Transform segment, int length) {
        if(length == -1) {
            return -1;
        }
        if(segment == linePath[length]) {
            return length;
        }
        return indexOfRopeTransform(segment, length - 1);
    }

    private void OnDrawGizmos() {
        if (drawLineInEditor) {
            Gizmos.color = editorLineColor;
            Gizmos.DrawLine((Vector2)this.transform.position + startPos, (Vector2)this.transform.position + endPos);
        }
    }
}
