using UnityEngine;
using System.Collections;

public class Navigation : MonoBehaviour {
	
	private float hori;
	private float vert;
	private IR IRLeftFront;
	private IR IRRightFront;
	private IR IRLeft;
	private IR IRRight;
	// Use this for initialization
	void Start () {
		IRLeftFront = new IR(transform.FindChild("left_front_IR"));
		IRRightFront = new IR(transform.FindChild("right_front_IR"));
		IRLeft = new IR(transform.FindChild("left_IR"));
		IRRight= new IR(transform.FindChild("right_IR"));
		hori = 0;
		vert = 0;
	}
	
	void Update () {
		//servoControl (5f,0f);
		if(IRLeft.getDistance()<4){
				servoControl (2f,.1f);
			print ("irLeft");
		}
		else if(IRRight.getDistance()<4){
				servoControl (.1f,2f);
				print ("irReft");
		}
		else{
			print (IRRightFront.getDistance());
			print (IRLeftFront.getDistance());
			if(IRRightFront.getDistance()<14 && IRLeftFront.getDistance()<14){
			//servoControl (2f,0f);
				if(IRRightFront.getDistance()<IRLeftFront.getDistance()){
					servoControl (1f,2f);
					print ("\\");
				}
				else{
					servoControl (2f,1f);
					print ("/");
				}
			}
			else{
				servoControl (2f,2f);
			}
		}
	}
	
	void servoControl(float left, float right){
		
		Transform childT = transform.FindChild ("left_front_wheel");
		transform.gameObject.rigidbody.AddForceAtPosition(transform.forward * left,childT.position);
		
		childT = transform.FindChild ("right_front_wheel");
		transform.gameObject.rigidbody.AddForceAtPosition(transform.forward * right,childT.position);
		
	}
	
	
	void UserControl(){
		if(hori==0 && Input.GetButton("Horizontal"))
			hori = Input.GetAxis ("Horizontal");
		else
			hori = 0;
		
		if(hori==0 && Input.GetButton("Vertical"))
			vert = Input.GetAxis ("Vertical");
		else
			vert = 0;
		
		if(hori>0)
			transform.Translate(1*50f*Time.deltaTime, 0f, 0f);
		else if(hori<0)
			transform.Translate(-1*50f*Time.deltaTime, 0f, 0f);
		
		
		if(vert>0)
			transform.Translate(0f, 0f, 1*50f*Time.deltaTime);
		else if(vert<0)
			transform.Translate(0f, 0f, -1*50f*Time.deltaTime);
	}
}


class IR
{
	Transform transform;
	RaycastHit hit;
	public IR(Transform tran){
		transform = tran;
	}
	public float getDistance(){
		if(Physics.Raycast(transform.position,transform.forward,out hit))
			return hit.distance;
		return 99999;
	}
}