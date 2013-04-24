using UnityEngine;
using System.Collections;


public class Navigation : MonoBehaviour {
	private const float  dc = 999999;
	
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
		turn,
		ninety
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
			if(chkDist (dc,dc,6f,6f,dc,dc)){
				if(IRSideFrontRight.getDistance () > IRSideFrontLeft.getDistance ())
					curDir = dir.right;
				else
					curDir = dir.left;
				curState = state.ninety;
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
		case state.ninety:
			curDir = checkNinety ();
			switch(curDir){
			case dir.right:
				servoControl (1f,-.5f);
				print ("turning to right");
				break;
			case dir.left:
				servoControl (-.5f,1f);
				print ("turning to left");
				break;
			case dir.straight:
				curState = state.straight;
				break;
			}
			
			
			break;
		}
	}
	
	dir checkNinety(){
		if(chkDist (dc,dc,8f,8f,dc,dc)){
			return curDir;
		}
		return dir.straight;
	}
	
	dir checkStraight(){

		float front_diff= Mathf.Abs(IRSideFrontRight.getDistance()-IRSideFrontLeft.getDistance());

		if(chkDist(2.5f,dc,dc,dc,dc,dc)){
				servoControl (0f,-2f);
				return dir.left;
		}
		else if(chkDist(dc,2.5f,dc,dc,dc,dc)){
				servoControl (-2f,0f);
				return dir.right;
		}
		else if(chkDist(5f,dc,dc,dc,dc,dc) & front_diff>1.3){
			print ("going to right");
			servoControl (2f,1.5f);
			return dir.left;
		}
		else if(chkDist(dc,5f,dc,dc,dc,dc) & front_diff>1.3) {
			servoControl (1.5f,2f);
			print ("going to left");
			return dir.right;
		}
		
		else if(IRSideFrontRight.getDistance()>IRSideFrontLeft.getDistance ()){
			servoControl (2f,1.98f);
		}
		else if(IRSideFrontRight.getDistance()<IRSideFrontLeft.getDistance ()){
			servoControl (1.98f,2f);
		}else{
			servoControl (2f,2f);
		}
		return dir.straight;
	
	}
	
	bool chkDist( float left, float right, float front_left, float front_right, float side_left, float side_right){
		bool l,r,fl,fr,sl,sr;
		l = IRSideFrontLeft.getDistance()<left;
		r = IRSideFrontRight.getDistance()<right;
		fl = IRLeftFront.getDistance()<front_left;
		fr = IRRightFront.getDistance()<front_right;
		sl = IRSideBackLeft.getDistance()<side_left;
		sr = IRSideBackRight.getDistance()<side_right;
		return l & r & fl & fr & sl & sr;
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
		if(dist == 0)
			dist = 9999;
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