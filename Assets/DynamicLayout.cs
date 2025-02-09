
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DynamicLayout : MonoBehaviour
{
    public float radius, tooCloseR;
    public float forceScale, friction;
    public int typeCount;
    public int particleCount;
    
    public RectTransform panelColumn;
    public GameObject cell;
    public Transform Board;
    GameObject temp;
    GameManager manager;
    public float[,] matrix;

    bool hidden = false;
    public Animator hideAnim;
    public TextMeshProUGUI text;

    void Start(){
        DontDestroyOnLoad(gameObject);
        manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        matrix = new float[typeCount,typeCount];

        // for(int i = 0; i<typeCount; i++){
        //     for(int j = 0; j < typeCount; j++){
        //         matrix[i,j] = Random.Range(-1f, 1f);
        //         // matrix[i,j]=0;
        //         Board.GetChild(i+1).GetChild(j+1).gameObject.GetComponent<Image>().color = Color.HSVToRGB(matrix[i,j]<0 ? 0 : 120f/355f,1,Mathf.Abs(matrix[i,j]));
        //     }
        // }
    }
    void Update(){
        text.text = (int)(1f / Time.unscaledDeltaTime)+"";
    }
    // void Update(){
    //     if(Input.GetKey(KeyCode.R)){
    //         for(int i = 0; i<typeCount; i++){
    //             for(int j = 0; j < typeCount; j++){
    //                 matrix[i,j] = Random.Range(-1f, 1f);
    //                 Board.GetChild(i+1).GetChild(j+1).gameObject.GetComponent<Image>().color = Color.HSVToRGB(matrix[i,j]<0 ? 0 : 120f/355f,1,Mathf.Abs(matrix[i,j]));
    //             }
    //         }
    //     }
    //     else if(Input.GetKey(KeyCode.E)){
    //         for(int i = 0; i<typeCount; i++){
    //             for(int j = 0; j < typeCount; j++){
    //                 matrix[i,j] = 0;
    //                 Board.GetChild(i+1).GetChild(j+1).gameObject.GetComponent<Image>().color = new Color(0,0,0,0);
    //             }
    //         }
    //     }
    // }

    public void createBoard(int size) {
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
                else
                    temp.GetComponent<Image>().color = UnityEngine.Color.HSVToRGB(0,1,0);

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
        SceneManager.LoadScene(0);
        Destroy(gameObject); // dont delete self, delete the new instance from the new scene load
    }
}
