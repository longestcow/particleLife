
using UnityEngine;
using UnityEngine.UI;
using Quaternion = UnityEngine.Quaternion;



public class GameManager : MonoBehaviour
{
    public float radius, tooCloseR;
    public float forceScale, friction;
    public int typeCount;
    public int particleCount;
    public GameObject particleObject, particles;
    DynamicLayout matrixUI;
    public float[,] matrix;
    void Start()
    {
        Vector2 size = GetComponent<Renderer>().bounds.size; 

        for(int i = 0; i<particleCount; i++){
            Instantiate(particleObject, new Vector2(Random.Range(-0.5f, 0.5f) * size.x, Random.Range(-0.5f, 0.5f) * size.y), Quaternion.identity, particles.transform);
        }

        matrix = new float[typeCount,typeCount];
        matrixUI = GameObject.Find("Canvas").GetComponent<DynamicLayout>();

        for(int i = 0; i<typeCount; i++){
            for(int j = 0; j < typeCount; j++)
                matrix[i,j] = 0;
        }
        matrixUI.createBoard(typeCount+1);
    }


    public void updateMatrix(int x, int y, bool neg, GameObject gameObj){
        matrix[x,y]+= 0.2f * (neg ? -1 : 1);
        matrix[x,y]=Mathf.Clamp(matrix[x,y],-1,1);
        gameObj.GetComponent<Image>().color = Color.HSVToRGB(matrix[x,y]<0 ? 0 : 120f/355f,1,Mathf.Abs(matrix[x,y]));
    }

    public Vector2 forceMultiplier(particle p1, particle p2){
        float force = 1;
        Vector2 delta = (p2.transform.position - p1.transform.position);
        Vector2 direction = (p2.transform.position - p1.transform.position).normalized;
        float distance = Vector2.Distance(p1.transform.position, p2.transform.position);
        if(distance>radius) {
            if (Mathf.Abs(delta.x) > transform.localScale.x/ 2)
                delta.x -= Mathf.Sign(delta.x) * transform.localScale.x;

            if (Mathf.Abs(delta.y) > transform.localScale.y / 2)
                delta.y -= Mathf.Sign(delta.y) * transform.localScale.y;

            distance = delta.magnitude;
            direction = delta.normalized;
            if(distance>radius)return Vector2.zero;
        }
        if(distance<tooCloseR) return mapfloat(distance,  0, tooCloseR, -1, 0) * direction * forceScale; 
        else if(distance<=radius/2) force = mapfloat(distance, tooCloseR, radius/2, 0, 1);
        else force = mapfloat(distance,  radius/2, radius, 1, 0);
        force*=matrix[p2.type - 1, p1.type - 1]; 
        return force * direction * forceScale;
        
        
    }
    public Color typeToColor(int type){
        switch(type){
            case 1: return new Color(254/255f, 71/255f, 115/255f);
            case 2: return new Color(70/255f, 179/255f, 165/255f);
            case 3: return new Color(246/255f, 214/255f, 141/255f);
            default: return Color.white;
        }
    }

    float mapfloat(float x, float in_min, float in_max, float out_min, float out_max) {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }


}
