using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Controller : MonoBehaviour {

    [SerializeField]
    private Rigidbody2D body;

    [SerializeField]
    private float movementSpeed;
    private Vector2 movementForce;
    [SerializeField]
    private float airMovementSpeed;
    [SerializeField]
    private float movementMax;
    [SerializeField]
    private float slowDownCoefficient;
    [SerializeField]
    private float baseBounceSpeed;
    [SerializeField]
    private float cayoteTime;
    [SerializeField]
    private float bouncy;

    private bool moving = false;
    [SerializeField]
    private bool onGround = false;
    [SerializeField]
    private bool onRope = false;

    private bool jumping = false;
    private Vector2 jumpForce;

    private Vector2 prevFrameVel;
    [SerializeField]
    private bool velocityLock = false;
    private Transform ropeTransform;
    private float unlockTime = 0f;
    [SerializeField]
    private float bounceThreshhold;
    private float nextTime;
    private Grapple_Controller gc;

    private void Start() {
        if (body == null) {
            Debug.LogError("No body attached to player");
        }
        gc = this.GetComponent<Grapple_Controller>();
        gc.OnEndGrapple += EndedGrapple;
    }

    private void Update() {
        if (Input.GetButtonDown("Jump") && onRope) {
            Jump();
        }
        if(Input.GetAxisRaw("Horizontal") != 0) {
            Vector2 movement = new Vector2(Input.GetAxisRaw("Horizontal"), 0f);
            movement.Normalize();
            movementForce = movement * movementSpeed;
            moving = true;
        } else {
            moving = false;
        }
        if (!velocityLock) {
            prevFrameVel = body.velocity;
        } else {
            if(Time.time > unlockTime) {
                velocityLock = false;
            }
        }
    }

    private void FixedUpdate() {
        if(jumping == true) {
            body.velocity = jumpForce;
            ropeTransform.gameObject.GetComponent<Rigidbody2D>().AddForce(jumpForce.normalized * 0, ForceMode2D.Impulse);
            jumping = false;
            nextTime = Time.time + 0.15f;
            return;
        }
        if (moving && (onGround || onRope) && (Time.time >= nextTime)) {
            body.AddForce(movementForce);
            if (body.velocity.magnitude > movementMax) {
                body.velocity = body.velocity.normalized * movementMax;
            }
        }
        
        if (!moving && (onGround || onRope) && (Time.time >= nextTime)) {
            if(body.velocity.magnitude < 0.001) {
                body.velocity = Vector2.zero;
            } else {
                body.velocity = body.velocity * slowDownCoefficient;
            }
        }
        if(moving && !(onGround || onRope)) {
            body.AddForce(movementForce.normalized * airMovementSpeed);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        onGround = true;
        if (collision.collider.CompareTag("Rope")) {
            if(onRope == false && !velocityLock) {
                if (prevFrameVel.magnitude > (baseBounceSpeed + bounceThreshhold)) {
                    velocityLock = true;
                    unlockTime = Time.time + cayoteTime;
                    onRope = true;
                    ropeTransform = collision.collider.transform;
                    Jump();
                }
            }
        }
    }
    private void OnCollisionStay2D(Collision2D collision) {
        if (collision.collider.CompareTag("Rope")) {
            onRope = true;
            ropeTransform = collision.collider.transform;
        }

        if (collision.collider.CompareTag("Ground")) {
            onGround = true;
        }
    }
    private void OnCollisionExit2D(Collision2D collision) {
        onGround = false;
        if (collision.collider.CompareTag("Rope")) {
            onRope = false;
        }
    }

    private void Jump() {
        if (ropeTransform.GetComponentInParent<Rope>().getPointToPoint()) {
            float jumpMagnitude;
            if (prevFrameVel.magnitude > baseBounceSpeed + bounceThreshhold) {
                jumpMagnitude = prevFrameVel.magnitude * bouncy;
            } else {
                jumpMagnitude = baseBounceSpeed;
            }
            Debug.Log(jumpMagnitude);
            Vector2 thing;
            Vector2 wohoo = ropeTransform.gameObject.GetComponentInParent<Rope>().getJumpThing(ropeTransform, out thing);
            Vector2 otherThing = (Vector2)this.transform.position - thing;

            Vector2 forceVector;
            if (Vector2.SignedAngle(wohoo, otherThing) > 0) {
                //counter clockwise
                //-y, x
                forceVector = new Vector2(-wohoo.y, wohoo.x);
            } else {
                //clockwise
                //y, -x
                forceVector = new Vector2(wohoo.y, -wohoo.x);
            }
            forceVector.Normalize();
            forceVector *= jumpMagnitude;
            jumping = true;
            jumpForce = forceVector;
        }
    }

    private void EndedGrapple() {
        nextTime = Time.time + 0.15f;
        return;
    }
}