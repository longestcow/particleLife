
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;



public class GameManager : MonoBehaviour {
    
    public float radius, tooCloseR;
    public float forceScale, friction;
    public int typeCount;
    public int particleCount;
    public float particleSize = 0.05f;
    public GameObject board;

    Particle[] particles;
    List<Matrix4x4>[] meshPositions;
    Mesh particleMesh;
    public Material[] materials;

    List<Particle>[,] grid;

    int dimension, gridDim;
    Matrix4x4[] matrixBuffer = new Matrix4x4[1023];

    int gridx,gridy;
    bool pPressed = false;
    Vector2 pMousePosition = Vector2.zero, difference;

    DynamicLayout matrixUI;
    float[,] matrix;
    void Start() {
        dimension = (int)transform.localScale.x;

        particleMesh =  Resources.GetBuiltinResource<Mesh>("Quad.fbx");
        Vector2 size = GetComponent<Renderer>().bounds.size; 
        meshPositions = new List<Matrix4x4>[typeCount];
        particles = new Particle[particleCount];
        gridDim = (int)(dimension/radius); //how many boxes
        grid = new List<Particle>[gridDim,gridDim];

        for(int i = 0 ;i < gridDim; i++){
            for(int j = 0; j < gridDim; j++)
                grid[i,j] = new List<Particle>();
            
        }

        for (int i = 0; i < typeCount; i++) {
            meshPositions[i] = new List<Matrix4x4>(); 
        }
        

        for(int i = 0; i<particleCount; i++) {
            Particle p = new Particle(
                i,
                new Vector2(Random.Range(0f, 1f) * dimension, Random.Range(0f, 1f) * dimension),
                Vector2.zero,
                Random.Range(0, typeCount)
            );

            particles[i] = p;
            grid[(int) (p.position.y/radius),(int)(p.position.x/radius)].Add(p);
        }

        matrix = new float[typeCount,typeCount];
        matrixUI = GameObject.Find("Canvas").GetComponent<DynamicLayout>();

        matrixUI.createBoard(typeCount+1);

        for(int i = 0; i<typeCount; i++){
            for(int j = 0; j < typeCount; j++){
                matrix[i,j] = Random.Range(-1f, 1f);
                // matrix[i,j]=0;
                board.transform.GetChild(i+1).GetChild(j+1).gameObject.GetComponent<Image>().color = Color.HSVToRGB(matrix[i,j]<0 ? 0 : 120f/355f,1,Mathf.Abs(matrix[i,j]));
            }
        }

    }

    void Update(){
        Camera.main.orthographicSize = Mathf.Max(Camera.main.orthographicSize - Input.GetAxis("Mouse ScrollWheel") * dimension/4, 2);
        if(Input.GetMouseButton(0)){
            if(pPressed){
                difference = Camera.main.ScreenToWorldPoint(Input.mousePosition) - Camera.main.transform.position;
                Camera.main.transform.position = pMousePosition - difference;
            }
            pPressed=true;
            pMousePosition=Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        else pPressed = false;

        if(Input.GetKey(KeyCode.R)){
            for(int i = 0; i<typeCount; i++){
                for(int j = 0; j < typeCount; j++){
                    matrix[i,j] = Random.Range(-1f, 1f);
                    board.transform.GetChild(i+1).GetChild(j+1).gameObject.GetComponent<Image>().color = Color.HSVToRGB(matrix[i,j]<0 ? 0 : 120f/355f,1,Mathf.Abs(matrix[i,j]));
                }
            }
        }
        else if(Input.GetKey(KeyCode.E)){
            for(int i = 0; i<typeCount; i++){
                for(int j = 0; j < typeCount; j++){
                    matrix[i,j] = 0;
                    board.transform.GetChild(i+1).GetChild(j+1).gameObject.GetComponent<Image>().color = new Color(0,0,0,0);
                }
            }
        }

        for(int i = 0; i<typeCount; i++)
            meshPositions[i].Clear();
        
        for(int r = 0; r<gridDim; r++) {
            for(int c = 0; c<gridDim; c++){
                foreach(Particle p1 in grid[r,c]){           //each particle in current grid box
                    p1.velocity *= friction;
                    for(int i = -1; i<=1; i++){             
                        for(int j = -1; j<=1; j++){         //9 grid boxes around current grid box
                            gridx = (int)nfmod((r+i),gridDim);
                            gridy = (int)nfmod((c+j),gridDim);
                            foreach(Particle p2 in grid[gridx,gridy]){
                                if(p1.id==p2.id)continue;
                                p1.velocity+= forceMultiplier(p1,p2) * 0.02f;
                            }
                        }
                    }
                    p1.position += p1.velocity * 0.02f;

                    if(p1.position.x < 0) p1.position.x = dimension-0.1f;
                    else if (p1.position.x >= dimension) p1.position.x = 0;
                    if(p1.position.y < 0) p1.position.y = dimension-0.1f;
                    else if(p1.position.y >= dimension) p1.position.y = 0;

                    meshPositions[p1.type].Add(
                        Matrix4x4.TRS(p1.position, Quaternion.identity, Vector3.one * particleSize)
                    );
                }

            }
        }
        for(int i = 0 ;i < gridDim; i++){
            for(int j = 0; j < gridDim; j++)
                grid[i,j].Clear();
            
        }

        for(int i = 0; i<particleCount; i++){
            grid[Mathf.Clamp((int)(particles[i].position.y/radius), 0, dimension-1), Mathf.Clamp((int)(particles[i].position.x/radius), 0, dimension-1)].Add(particles[i]);
        }

        render();
    }

    void render() {
        for(int i = 0; i<typeCount; i++){
            int batchSize = meshPositions[i].Count;
            for(int j = 0; j< batchSize; j+=1023) {
                int count = Mathf.Min(1023, batchSize - j);
                for(int k = 0; k<count; k++)
                    matrixBuffer[k] = meshPositions[i][j + k];

                Graphics.DrawMeshInstanced(particleMesh, 0, materials[i], matrixBuffer, count);
            }
        }
    }

    public void updateMatrix(int x, int y, bool neg, GameObject gameObj){
        matrix[x,y]+= 0.2f * (neg ? -1 : 1);
        matrix[x,y]=Mathf.Clamp(matrix[x,y],-1,1);
        gameObj.GetComponent<Image>().color = Color.HSVToRGB(matrix[x,y]<0 ? 0 : 120f/355f,1,Mathf.Abs(matrix[x,y]));
    }

    Vector2 forceMultiplier(Particle p1, Particle p2){
        float force = 1;
        Vector2 delta = p2.position - p1.position;
        Vector2 direction = (p2.position - p1.position).normalized;
        float distance = Vector2.Distance(p1.position, p2.position);
        if(distance>radius) {
            if (Mathf.Abs(delta.x) > transform.localScale.x / 2)
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
        force*=matrix[p2.type, p1.type]; 
        return force * direction * forceScale;
        
        
    }
    public Color typeToColor(int type){
        if (type<=materials.Length)
            return materials[type-1].color;
        else return Color.white;
    }

    float mapfloat(float x, float in_min, float in_max, float out_min, float out_max) {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }

    float nfmod(float a,float b){
        return a - b * Mathf.Floor(a / b);
    }
}

class Particle {
    public int id;
    public Vector2 position;
    public Vector2 velocity;
    public int type;
    public int gridx;
    public int gridy;

    public Particle(int _id, Vector2 _pos, Vector2 _vel, int _type) {
        id = _id;
        position = _pos;
        velocity = _vel;
        type = _type;
    }

}
