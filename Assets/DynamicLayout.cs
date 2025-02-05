using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DynamicLayout : MonoBehaviour
{
    public RectTransform panelColumn;
    public GameObject cell;
    public Transform Board;
    GameObject temp;
    GameManager manager;

    void Start(){
        manager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

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
}
