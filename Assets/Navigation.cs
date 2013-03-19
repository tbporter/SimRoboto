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
	
	void FixedUpdate () {
		//Vector3 start = new Vector3(0,0,0);
		Vector3 end = new Vector3(1,0,0);
		stateMachine ();
	}
	
	
	void stateMachine(){
		dir tempDir;
		print(curState);
		switch(curState){
		case state.straight:
			
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
			
			
			tempDir = checkStraight();
			switch(tempDir){
			case dir.right:
				//servoControl (1.85f,2f);
				break;
			case dir.left:
				//servoControl (2f,1.85f);
				break;
			case dir.straight:
				servoControl (2f,2f);
				break;
			}
			
			break;
			
		case state.turn:
			tempDir = checkTurn();
			
			switch(tempDir){
			case dir.right:
				servoControl (1f,2f);
				break;
			case dir.left:
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
			if(curDir==dir.right)
				servoControl (.1f,2f);
			else
				servoControl (2f,.1f);

			break;
		}
	}
	/* It says checkParallel, but really its to make sure the side-front IR 
	 * isn't pointing close to the wall than the side-back
	 * input: direction of IRs wanting to check
	 * output: whether it is parallel*/
	bool checkParallel(dir d){
		const float PARALLEL_DIFF_MIN = 1f;
		float temp;
		float temp2;
		if(d == dir.right){
			temp =IRSideFrontRight.getDistance ();
			temp2 =IRSideBackRight.getDistance ();
		}
		else{
			temp =IRSideFrontLeft.getDistance ();
			temp2 =IRSideBackLeft.getDistance ();
		}
		if(temp+PARALLEL_DIFF_MIN<temp2){
			return false;
		}
			return true;
	}
	
	dir checkStraight(){
		const float STRAIGHT_DIFF_MIN = .5f;
		
		if(IRRightFront.getDistance()>999||IRLeftFront.getDistance()>999){
			print("IRRight"+IRRightFront.getDistance());
			print("IRLeft"+IRLeftFront.getDistance());
			return dir.straight;
		}
		float diff= Mathf.Abs(IRRightFront.getDistance()-IRLeftFront.getDistance());
		print (diff);
		diff = diff *.015f;
		
		if(IRSideFrontLeft.getDistance()>IRSideFrontRight.getDistance ()+STRAIGHT_DIFF_MIN){
			servoControl (2f-diff,2f);
			IRSideFrontRight.debug(1);
			return dir.right;
		}
		if(IRSideFrontRight.getDistance()>IRSideFrontLeft.getDistance ()+STRAIGHT_DIFF_MIN){
			IRSideFrontLeft.debug(1);
			servoControl (2f,2f-diff);
			return dir.left;
		}
		
		return dir.straight;
	}
	dir checkTurn(){
		const float TURN_IR_DIFF_MIN = 3f;
		const float TURN_IR_STRAIGHT_MIN = 15f;
		IRRightFront.debug(TURN_IR_STRAIGHT_MIN);
		IRLeftFront.debug(TURN_IR_STRAIGHT_MIN);
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
		const float SIDE_IR_MIN = 5;
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
		left = left*10;
		right = right*10;
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
	public IR(Transform tran){
		transform = tran;
	}
	public float getDistance(){
		RaycastHit[] hits;
		hits = Physics.RaycastAll(transform.position,transform.forward);
		debug (99999);
		if (hits.Length>0)
			return hits[0].distance;
		return 99999;
	}
	public void debug(float distance){
		Vector3 line = transform.position + ( transform.forward * distance );
		Debug.DrawLine(transform.position, line, Color.red);
	}
}