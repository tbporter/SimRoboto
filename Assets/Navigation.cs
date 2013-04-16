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
	private Encoder LeftEncoder;
	private Encoder RightEncoder;
	
	private memory[] mem;
	private memoryObject[] memObj;
	
	private float[] turns;
	
	
	struct memory{
		public float IRLeft;
		public float IRRight;
		public Vector3 dir;
	}
	private int lastMem;
	struct memoryObject{
		public GameObject left;
		public GameObject right;
		public GameObject robot;
	}
	
	private float corner;
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
		
		mem = new memory[1000];
		memObj = new memoryObject[1000];
		LeftEncoder = new Encoder(transform.FindChild ("left_front_wheel"));
		RightEncoder = new Encoder(transform.FindChild ("right_front_wheel"));
		
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
		LeftEncoder.update ();
		RightEncoder.update ();
	}
	void FixedUpdate () {
		int temp = (int)RightEncoder.getDistance ();
		mem[temp].IRLeft = IRSideFrontLeft.getDistance();
		mem[temp].IRRight = IRSideFrontRight.getDistance();
		mem[temp].dir =  transform.forward;
		lastMem = temp;
		stateMachine ();
		UserControl();
	}
	
	
	void stateMachine(){
		dir tempDir;
		//print(curState);
		switch(curState){
		case state.straight:
			
//			tempDir = checkSide();
//			
//			if(tempDir!=dir.straight){
//				curDir = tempDir;
//				curState = state.side;
//				break;
//			}
			
			tempDir = checkTurn();
			if(tempDir!=dir.straight){
				curDir = tempDir;
				curState = state.turn;
				break;
			}
			
			
			tempDir = checkStraight();
			switch(tempDir){
			case dir.right:
				IRSideFrontRight.debug (Color.red);
				//servoControl (1.85f,2f);
				break;
			case dir.left:
				IRSideFrontLeft.debug (Color.red);
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
				//servoControl (1f,2f);
				break;
			case dir.left:
				IRLeftFront.debug (Color.red);
				IRRightFront.debug (Color.red);
				//servoControl (2f,1f);
				break;
			case dir.straight:
				curDir = tempDir;
				curState = state.straight;
				break;
			}
			
			break;
		}
	}
	
	dir checkStraight(){

		float front_diff= Mathf.Abs(IRSideFrontRight.getDistance()-IRSideFrontLeft.getDistance());
		
		if(IRSideFrontLeft.getDistance()<5 & front_diff>1){
			if(IRSideFrontLeft.getDistance()<4){
				servoControl (0f,-2f);
				return dir.left;
			}
			servoControl (2f,1.5f);
			return dir.left;
		}
		else if(IRSideFrontRight.getDistance()<5 & front_diff>1) {
			if(IRSideFrontRight.getDistance()<4){
				servoControl (-2f,0f);
				return dir.right;
			}
			servoControl (1.5f,2f);
			return dir.right;
		}
		
		else if(IRSideFrontRight.getDistance ()>IRSideFrontLeft.getDistance ()){
			print ("lean to right");
			servoControl (2f,1.98f);
		}
		else if(IRSideFrontRight.getDistance ()<IRSideFrontLeft.getDistance ()){
			print ("lean to left");
			servoControl (1.98f,2f);
		}else{
			servoControl (2f,2f);
		}
		return dir.straight;
	
	}
	
	
	
	dir checkTurn(){
		const float TURN_IR_DIFF_MIN = 2f;
		const float TURN_IR_STRAIGHT_MIN = 15f;
		if(IRRightFront.getDistance()<TURN_IR_STRAIGHT_MIN && IRLeftFront.getDistance()<TURN_IR_STRAIGHT_MIN){
			if(IRRightFront.getDistance()<IRLeftFront.getDistance()-TURN_IR_DIFF_MIN){
				servoControl (1f,2f);
				return dir.right;
			}
			else if(IRLeftFront.getDistance()<IRRightFront.getDistance()-TURN_IR_DIFF_MIN){
				servoControl (2f,1f);
				return dir.left;
			}
			else if(IRSideBackLeft.getDistance()>10f && IRSideBackRight.getDistance()<5f){
				servoControl (0f,2f);
				return dir.right;
			}
			else if(IRSideBackRight.getDistance()>10f && IRSideBackLeft.getDistance()<5f){
				servoControl (2f,0f);
				return dir.left;
			}
		}
		return dir.straight;
	}

	
	void servoControl(float left, float right){

		if(left>2f)
			left =2f;

		if(right>2f)
			right = 2f;
		
		left = left*5;
		right = right*5;
		/*if(Input.GetButton("Horizontal")){
			transform.rigidbody.velocity = Vector3.zero;
			transform.rigidbody.angularVelocity = Vector3.zero;
			return;
		}*/
		
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
		if(hori==0 && Input.GetButton("Horizontal")){
			printMemory ();
			hori = 1;
		}
		else
			hori = 0;
		
	}
	
	void printMemory(){
		GameObject robot =  GameObject.CreatePrimitive(PrimitiveType.Cube);
		robot.transform.position = new Vector3(27.90829f, 7.128446f, -25.02266f);
		int i = 1;
		print ("print Memory");
		while(i<lastMem){
			robot.transform.forward = mem[i].dir;
			
			Destroy(memObj[i].robot); 
			memObj[i].robot = GameObject.CreatePrimitive(PrimitiveType.Cube);
			memObj[i].robot.transform.position = robot.transform.position;
			memObj[i].robot.transform.renderer.material.SetColor ("_Color", Color.green);
			
			Destroy(memObj[i].left); 
			memObj[i].left = GameObject.CreatePrimitive(PrimitiveType.Cube);
			memObj[i].left.transform.position = robot.transform.position;
			memObj[i].left.transform.forward = robot.transform.forward;
			memObj[i].left.transform.renderer.material.SetColor ("_Color", Color.red);
			memObj[i].left.transform.Rotate (Vector3.up*308.6562f);
			memObj[i].left.transform.Translate (Vector3.forward * mem[i].IRLeft);
			
			Destroy(memObj[i].right); 
			memObj[i].right = GameObject.CreatePrimitive(PrimitiveType.Cube);
			memObj[i].right.transform.position = robot.transform.position;
			memObj[i].right.transform.forward = robot.transform.forward;
			memObj[i].right.transform.renderer.material.SetColor ("_Color", Color.blue);
			memObj[i].right.transform.Rotate (Vector3.up*51.43085f);
			memObj[i].right.transform.Translate (Vector3.forward * mem[i].IRRight);
			
			robot.transform.Translate (Vector3.forward * 1);
			i++;
		}
		Destroy(robot);
				//GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		//cube.transform.position = new Vector3(x, y, 0);
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
	private float randomizeVal(float dist){
		return dist + ((Random.value*2)-1);
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

class Encoder{
	
	Vector3 prevPos;
	float dist;
	Transform transform;
	public Encoder(Transform tran){
		transform = tran;
		prevPos = tran.position;
	}
	public void update(){
		float temp = Vector3.Distance (transform.position, prevPos);
		if(temp>1){
			dist += temp;
			prevPos = transform.position;
		}
	}
	public float getDistance(){
		return dist;
	}
}