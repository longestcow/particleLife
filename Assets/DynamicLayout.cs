
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DynamicLayout : MonoBehaviour
{ // todo: export import 

    [Header("constants")]
    public int dimension;
    public float radius;
    public int typeCount;
    public int particleCount;


    [Header("variables")]
    public float tooCloseR;
    public float forceScale, friction;
    public float particleSize;

    [Header("inputfield texts")]
    public TMP_InputField dimensionText; // int
    public TMP_InputField radiusText; 
    public TMP_InputField typeCountText; // int
    public TMP_InputField particleCountText; // int

    public TMP_InputField tooCloseRText;
    public TMP_InputField forceScaleText;
    public TMP_InputField frictionText;
    public TMP_InputField particleSizeText;
    
    [Header("canvas stuff")]
    public TextMeshProUGUI errorMessage;
    public Button updateButton;
    public RectTransform panelColumn;
    public GameObject cell;
    public Transform Board;
    GameObject temp;
    GameManager manager;
    public float[,] matrix;

    bool hidden = false, updateEnabled = true;
    public Animator hideAnim;
    public TextMeshProUGUI fps;
    Regex deci = new Regex(@"^0$|^[1-9]\d*$|^\.\d+$|^0\.\d*$|^[1-9]\d*\.\d*$"), inte = new Regex(@"^\d+$");

    public static int selfCount = 0;

    void Start(){
        if(selfCount != 0){
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        typeCount = manager.materials.Length;

        matrix = new float[typeCount,typeCount];
        createBoard(typeCount+1, false);
        randomizeMatrix();
        
        dimensionText.text = dimension+"";
        radiusText.text = radius+"";
        typeCountText.text = typeCount+"";
        particleCountText.text = particleCount+"";

        tooCloseRText.text = tooCloseR+"";
        forceScaleText.text = forceScale+"";
        frictionText.text = friction+"";
        particleSizeText.text = particleSize+"";


    }
    void Update() {
        if(Input.GetKey(KeyCode.R)) randomizeMatrix();
        if(Input.GetKey(KeyCode.E)) clearMatrix();
        fps.text = (int)(1f / Time.unscaledDeltaTime)+"";
        // do constant update of canvas variable elements

        if(tooCloseRText.text=="")tooCloseR = 0;
        else if(deci.IsMatch(tooCloseRText.text) && !tooCloseRText.text.Contains("-"))tooCloseR = float.Parse(tooCloseRText.text);
        else tooCloseRText.text = tooCloseR+"";

        if(forceScaleText.text=="")forceScale = 0;
        else if(deci.IsMatch(forceScaleText.text) && !forceScaleText.text.Contains("-"))forceScale = float.Parse(forceScaleText.text);
        else forceScaleText.text = forceScale+"";

        if(frictionText.text=="")friction = 0;
        else if(deci.IsMatch(frictionText.text) && !frictionText.text.Contains("-"))friction = float.Parse(frictionText.text);
        else frictionText.text = friction+"";

        if(particleSizeText.text=="")particleSize = 0;
        else if(deci.IsMatch(particleSizeText.text) && !particleSizeText.text.Contains("-"))particleSize = float.Parse(particleSizeText.text);
        else particleSizeText.text = particleSize+"";

        if(dimensionText.text=="")dimension = 0;
        else if(inte.IsMatch(dimensionText.text) && !dimensionText.text.Contains("-"))dimension = int.Parse(dimensionText.text);
        else dimensionText.text = dimension+"";

        if(radiusText.text=="")radius = 0;
        else if(deci.IsMatch(radiusText.text) && !radiusText.text.Contains("-"))radius = float.Parse(radiusText.text);
        else radiusText.text = radius+"";

        if(typeCountText.text=="")typeCount = 0;
        else if(inte.IsMatch(typeCountText.text) && !typeCountText.text.Contains("-"))typeCount = int.Parse(typeCountText.text);
        else typeCountText.text = typeCount+"";

        if(particleCountText.text=="")particleCount = 0;
        else if(inte.IsMatch(particleCountText.text) && !particleCountText.text.Contains("-"))particleCount = int.Parse(particleCountText.text);
        else particleCountText.text = particleCount+"";

        if(dimension%radius!=0 || radius==0 || dimension == 0 || typeCount == 0 || typeCount > manager.materials.Length){
            if(updateEnabled){
                updateEnabled=false;
                updateButton.interactable=false;
                errorMessage.enabled=true;
            }

            if(dimension%radius!=0)errorMessage.text="radius must be a factor of board size";
            else if(radius==0)errorMessage.text="radius cant be 0";
            else if(dimension==0)errorMessage.text="board size cant be 0";
            else errorMessage.text="type count must be within 1 to "+manager.materials.Length;

            }
        else if(!updateEnabled) {
            updateEnabled=true;
            updateButton.interactable=true;
            errorMessage.enabled=false;
        }
        

        
    }


    public void createBoard(int size, bool restart) {
        clearBoard();
        
        RectTransform colParent;

        for(int col = 0; col<size; col++){
            colParent = Instantiate(panelColumn, Board);
            for(int row = 0; row<size; row++){
                temp = Instantiate(cell, colParent);
                if(row==0 && col==0)
                    temp.GetComponent<Image>().color = new UnityEngine.Color(0,0,0,0);
                else if(col==0)
                    temp.GetComponent<Image>().color = manager.typeToColor(row);
                else if(row==0)
                    temp.GetComponent<Image>().color = manager.typeToColor(col);
                else if(!restart)
                    temp.GetComponent<Image>().color = UnityEngine.Color.HSVToRGB(0,1,0);
                else 
                    temp.GetComponent<Image>().color = Color.HSVToRGB(matrix[col,row]<0 ? 0 : 120f/355f,1,Mathf.Abs(matrix[col,row]));


            }
        }

    }
    public void clearBoard() {
        for(int i = 0; i<Board.childCount; i++)
            Destroy(Board.GetChild(i).gameObject);
    }

    public void hide(){
        hideAnim.SetTrigger((hidden)?"open":"hide");
        hidden=!hidden;
    }

    public void updateConstants(){
        selfCount+=1;
        SceneManager.LoadScene(0);
        // createBoard(typeCount+1, true);
        manager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public void randomizeMatrix(){
        for(int i = 0; i<typeCount; i++){
            for(int j = 0; j < typeCount; j++){
                matrix[i,j] = Random.Range(-1f, 1f);
                Board.GetChild(i+1).GetChild(j+1).gameObject.GetComponent<Image>().color = Color.HSVToRGB(matrix[i,j]<0 ? 0 : 120f/355f,1,Mathf.Abs(matrix[i,j]));
            }
        }
    }

    public void clearMatrix(){
        for(int i = 0; i<typeCount; i++){
            for(int j = 0; j < typeCount; j++){
                matrix[i,j] = 0;
                Board.GetChild(i+1).GetChild(j+1).gameObject.GetComponent<Image>().color = Color.HSVToRGB(matrix[i,j]<0 ? 0 : 120f/355f,1,Mathf.Abs(matrix[i,j]));
            }
        }
    }

    public void updateMatrix(int x, int y, bool neg, GameObject gameObj){
        matrix[x,y]+= 0.2f * (neg ? -1 : 1);
        matrix[x,y]=Mathf.Clamp(matrix[x,y],-1,1);
        gameObj.GetComponent<Image>().color = Color.HSVToRGB(matrix[x,y]<0 ? 0 : 120f/355f,1,Mathf.Abs(matrix[x,y]));
    }
}
