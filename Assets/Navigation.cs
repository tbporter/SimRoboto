using UnityEngine;
using System.Collections;

public class Navigation : MonoBehaviour {
	
	private float hori;
	private float vert;
	private IR IRLeftFront;
	private IR IRRightFront;
	private IR IRSideFrontLeft;
	private IR IRSideFrontRight;
	private IR IRSideBackLeft;
	private IR IRSideBackRight;
	
	enum state{
		straight,
		side,
		turn
	};
	
	enum dir{
		left,
		right,
		straight
	};
	
	state curState;
	dir curDir;
	// Use this for initialization
	void Start () {
		IRLeftFront = new IR(transform.FindChild("left_front_IR"));
		IRRightFront = new IR(transform.FindChild("right_front_IR"));
		IRSideFrontLeft = new IR(transform.FindChild("left_side_front_IR"));
		IRSideFrontRight= new IR(transform.FindChild("right_side_front_IR"));
		IRSideBackLeft = new IR(transform.FindChild("left_side_back_IR"));
		IRSideBackRight= new IR(transform.FindChild("right_side_back_IR"));
		
		curState = state.straight;
		curDir = dir.straight;
		hori = 0;
		vert = 0;
	}
	void Update(){
		IRLeftFront.updateDistance();
		IRRightFront.updateDistance();
		IRSideFrontLeft.updateDistance();
		IRSideFrontRight.updateDistance();
		IRSideBackLeft.updateDistance();
		IRSideBackRight.updateDistance();
	}
	void FixedUpdate () {
		
		stateMachine ();
	}
	
	
	void stateMachine(){
		dir tempDir;
		//print(curState);
		switch(curState){
		case state.straight:
			/*
			tempDir = checkSide();
			
			if(tempDir!=dir.straight){
				curDir = tempDir;
				curState = state.side;
				break;
			}
			
			tempDir = checkTurn();
			if(tempDir!=dir.straight){
				curDir = tempDir;
				curState = state.turn;
				break;
			}
			*/
			
			tempDir = checkStraight();
			switch(tempDir){
			case dir.right:
				//servoControl (1.85f,2f);
				break;
			case dir.left:
				//servoControl (2f,1.85f);
				break;
			case dir.straight:
				//servoControl (2f,2f);
				break;
			}
			
			break;
			
		case state.turn:
			tempDir = checkTurn();
			
			switch(tempDir){
			case dir.right:
				IRLeftFront.debug (Color.red);
				IRRightFront.debug (Color.red);
				servoControl (1f,2f);
				break;
			case dir.left:
				IRLeftFront.debug (Color.red);
				IRRightFront.debug (Color.red);
				servoControl (2f,1f);
				break;
			case dir.straight:
				curDir = tempDir;
				curState = state.straight;
				break;
			}
			
			break;
			
		case state.side:
			if(checkParallel(curDir)){
				curDir = dir.straight;
				curState= state.straight;
				break;
			}
			if(curDir==dir.right){
				IRSideBackRight.debug (Color.red);
				IRSideFrontRight.debug (Color.red);
				servoControl (.1f,2f);
			}
			else{
				IRSideBackLeft.debug (Color.red);
				IRSideFrontLeft.debug (Color.red);
				servoControl (2f,.1f);
			}
			break;
		}
	}
	/* It says checkParallel, but really its to make sure the side-front IR 
	 * isn't pointing close to the wall than the side-back
	 * input: direction of IRs wanting to check
	 * output: whether it is parallel*/
	bool checkParallel(dir d){
		const float PARALLEL_DIFF_MIN = .15f;
		float front;
		float back;
		if(d == dir.right){
			front =IRSideFrontRight.getDistance ();
			back =IRSideBackRight.getDistance ();
		}
		else{
			front =IRSideFrontLeft.getDistance ();
			back =IRSideBackLeft.getDistance ();
		}
		if(front+PARALLEL_DIFF_MIN<back){
			return false;
		}
			return true;
	}
	
	/*dir checkStraight(){
		float STRAIGHT_DIFF_MIN = 0f;
		const float TOO_CLOSE = 2f;
		if(IRSideFrontRight.getDistance()>999||IRSideFrontLeft.getDistance()>999){
			return dir.straight;
		}
		float diff= Mathf.Abs(IRSideFrontRight.getDistance()-IRSideFrontLeft.getDistance());
		//IRSideFrontLeft.debug (Color.red);
		//IRSideFrontRight.debug (Color.red);
		//if the difference is too much, try to recenter
		print(diff);
		if(diff>1.5f){
			diff = diff *.3f;
			if(IRSideFrontLeft.getDistance()>IRSideFrontRight.getDistance())
			{
				//if(!checkParallel(dir.right)){
					servoControl (1.9f,2f);
					IRSideFrontRight.debug (Color.red);
					return dir.right;
				//}
			}
			if(IRSideFrontRight.getDistance()>IRSideFrontLeft.getDistance())//&&!checkParallel(dir.left))
			{
				//if(!checkParallel(dir.left)){
					servoControl (2f,1.9f);
					IRSideFrontLeft.debug (Color.red);
					return dir.left;
				//}
			}
		}
		//Also make sure we don't get too close
		/*if(IRSideFrontRight.getDistance()<TOO_CLOSE){
			print ("right is too close");
			servoControl (1.5f,2f);
		}
		else if(IRSideFrontLeft.getDistance()<TOO_CLOSE){
			print ("left is too close");
			servoControl (2f,1.5f);
		}
		return dir.straight;
	}*/
	
	dir checkStraight(){

		float front_diff= Mathf.Abs(IRSideFrontRight.getDistance()-IRSideFrontLeft.getDistance());
		float left_side_diff= Mathf.Abs(IRSideFrontLeft.getDistance()-IRSideBackLeft.getDistance());
		float right_side_diff= Mathf.Abs(IRSideFrontRight.getDistance()-IRSideBackRight.getDistance());
		if(front_diff>1f){
			if(IRSideFrontRight.getDistance ()>IRSideFrontLeft.getDistance ()){
				if(right_side_diff>.04f){
					print ("right side");
					servoControl (2f,1.85f);
					return dir.left;
				}
				
			}
			else{
				if(left_side_diff>.04f){
					
					print ("left side");
					servoControl (1.85f,2f);
					return dir.right;
				}

			}
		}
		
		if(IRSideFrontRight.getDistance ()>IRSideFrontLeft.getDistance ()){
			print ("lean to right");
			servoControl (2f,1.98f);
		}
		else{
			print ("lean to left");
			servoControl (1.98f,2f);
		}
		return dir.straight;
	
	}
	
	
	
	dir checkTurn(){
		const float TURN_IR_DIFF_MIN = 3f;
		const float TURN_IR_STRAIGHT_MIN = 15f;
		if(IRRightFront.getDistance()<TURN_IR_STRAIGHT_MIN && IRLeftFront.getDistance()<TURN_IR_STRAIGHT_MIN){
			if(IRRightFront.getDistance()<IRLeftFront.getDistance()+TURN_IR_DIFF_MIN){
				return dir.right;
			}
			else if(IRLeftFront.getDistance()<IRRightFront.getDistance()+TURN_IR_DIFF_MIN){
				return dir.left;
			}
		}
		return dir.straight;
	}

	dir checkSide(){
		
		//we need to prioritize the direction we should turn by looking at which is closest
		if(IRSideFrontRight.getDistance()<IRSideFrontLeft.getDistance()){
			if(checkSideDir(dir.right))
				return dir.right;
			if(checkSideDir(dir.left))
				return dir.left;
		}
		else{
			if(checkSideDir(dir.left))
				return dir.left;
			if(checkSideDir(dir.right))
				return dir.right;
		}
		return dir.straight;
	}
	/* Here we check to seeing if we are too close to the sides,
	 * but if we are Parallel to the side, it's okay to be close
	 * output: if the side should be turning*/
	bool checkSideDir(dir d){
		const float SIDE_IR_MIN = 3;
		switch(d){
		case dir.right:
			if(!checkParallel(dir.right)&&IRSideBackRight.getDistance()<SIDE_IR_MIN && IRSideFrontRight.getDistance()<SIDE_IR_MIN)
				return true;
			break;
		case dir.left:
			if(!checkParallel(dir.left)&&IRSideBackLeft.getDistance()<SIDE_IR_MIN && IRSideFrontLeft.getDistance()<SIDE_IR_MIN)
				return true;
			break;
		}
		return false;
	}
	
	void servoControl(float left, float right){
		if(left<0)
			left = 0;
		else if(left>2f)
			left =2f;
		if(right<0)
			right = 0;
		else if(right>2f)
			right = 2f;
		
		left = left*5;
		right = right*5;
		if(Input.GetButton("Horizontal")){
			transform.rigidbody.velocity = Vector3.zero;
			transform.rigidbody.angularVelocity = Vector3.zero;
			return;
		}
		
		Transform childT = transform.FindChild ("left_front_wheel");
		transform.gameObject.rigidbody.AddForceAtPosition(transform.forward * left,childT.position);
		childT = transform.FindChild ("left_back_wheel");
		transform.gameObject.rigidbody.AddForceAtPosition(transform.forward * left,childT.position);
		childT = transform.FindChild ("right_front_wheel");
		transform.gameObject.rigidbody.AddForceAtPosition(transform.forward * right,childT.position);
		childT = transform.FindChild ("right_back_wheel");
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
	RaycastHit[] hits;
	Transform transform;
	float dist;
	public IR(Transform tran){
		transform = tran;
	}
	public float getDistance(){
		
		return dist;
	}
	public void updateDistance(){
		RaycastHit hit;
		if (Physics.Raycast(transform.position,transform.forward,out hit,100)){
			dist = hit.distance;			
		}
		else{
			dist = 9999;
		}
	}
	public void debug(float distance,Color col){
		Vector3 line = transform.position + ( transform.forward * distance );
		Debug.DrawLine(transform.position, line, col);
	}
		public void debug(Color col){
		Vector3 line = transform.position + ( transform.forward * dist );
		Debug.DrawLine(transform.position, line, col);
	}
}