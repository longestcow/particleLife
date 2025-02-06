
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class GameManager : MonoBehaviour {

    public struct Particle {
        public int id;
        public Vector2 position;
        public Vector2 velocity;
        public int type;
    }

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

    int dimension = 10, gridDim;

    int gridx,gridy;

    DynamicLayout matrixUI;
    float[,] matrix;
    void Start() {

        particleMesh =  Resources.GetBuiltinResource<Mesh>("Quad.fbx");
        Vector2 size = GetComponent<Renderer>().bounds.size; 
        meshPositions = new List<Matrix4x4>[typeCount];
        particles = new Particle[particleCount];
        gridDim = (int)(dimension/radius);
        grid = new List<Particle>[gridDim,gridDim];

        for(int i = 0 ;i < (int) (dimension/radius); i++){
            for(int j = 0; j < (int) (dimension/radius); j++)
                grid[i,j] = new List<Particle>();
            
        }

        for (int i = 0; i < typeCount; i++) {
            meshPositions[i] = new List<Matrix4x4>(); 
        }
        

        for(int i = 0; i<particleCount; i++) {
            Particle p = new Particle();
            p.id = i;
            p.position = new Vector2(Random.Range(0f, 1f) * dimension, Random.Range(0f, 1f) * dimension);
            p.velocity = Vector2.zero;
            p.type = Random.Range(0, typeCount);
            particles[i] = p;
            grid[(int) (p.position.y/radius),(int)(p.position.x/radius)].Add(p);
        }

        matrix = new float[typeCount,typeCount];
        matrixUI = GameObject.Find("Canvas").GetComponent<DynamicLayout>();

        matrixUI.createBoard(typeCount+1);

        for(int i = 0; i<typeCount; i++){
            for(int j = 0; j < typeCount; j++){
                matrix[i,j] = Random.Range(-1f, 1f);
                board.transform.GetChild(i+1).GetChild(j+1).gameObject.GetComponent<Image>().color = Color.HSVToRGB(matrix[i,j]<0 ? 0 : 120f/355f,1,Mathf.Abs(matrix[i,j]));
            }
        }

    }

    void Update(){
        if(Input.GetKey(KeyCode.R)){
            for(int i = 0; i<typeCount; i++){
                for(int j = 0; j < typeCount; j++){
                    matrix[i,j] = Random.Range(-1f, 1f);
                    board.transform.GetChild(i+1).GetChild(j+1).gameObject.GetComponent<Image>().color = Color.HSVToRGB(matrix[i,j]<0 ? 0 : 120f/355f,1,Mathf.Abs(matrix[i,j]));
                }
            }
        }

        for(int i = 0; i<typeCount; i++)
            meshPositions[i].Clear();
        
        // for(int r = 0; r<gridDim; r++) {
        //     for(int c = 0; c<gridDim; c++){
        //         for(int p1 = 0; p1<grid[r,c].Count; p1++){           //each particle in current grid box
        //             grid[rc][p1].velocity *= friction;

        //             for(int i = -1; i<=1; i++){             
        //                 for(int j = -1; j<=1; j++){         //9 grid boxes around current grid box
        //                     gridx = (r+i)%gridDim;
        //                     gridy = (c+j)%gridDim;

        //                     foreach(Particle p2 in grid[gridx,gridy]){
        //                         if(p1.id==p2.id)continue;
                                
        //                     }

        //                 }
        //             }

        //         }

        //     }
        // }

        for(int i = 0; i<particleCount; i++){
            particles[i].velocity *= friction;
            
            for(int j = 0; j<particleCount; j++){
                if(i==j)continue;
                particles[i].velocity+= forceMultiplier(particles[i],particles[j]) * Time.deltaTime;
            }
            grid[(int) (particles[i].position.y/radius),(int)(particles[i].position.x/radius)].Remove(particles[i]);
            particles[i].position += particles[i].velocity * Time.deltaTime;
            grid[(int) (particles[i].position.y/radius),(int)(particles[i].position.x/radius)].Add(particles[i]);

            // screen wrapping
            if(particles[i].position.x < 0) particles[i].position.x = dimension;
            else if (particles[i].position.x > dimension) particles[i].position.x = 0;
            if(particles[i].position.y < 0) particles[i].position.y = dimension;
            else if(particles[i].position.y > dimension) particles[i].position.y = 0;

            meshPositions[particles[i].type].Add(
                Matrix4x4.TRS(particles[i].position, Quaternion.identity, Vector3.one * particleSize)
            );
        }

        render();
    }

    void render() {
        for(int i = 0; i<typeCount; i++){
            int batchSize = meshPositions[i].Count;

            for(int j = 0; j< batchSize; j+=1023) {
                Graphics.DrawMeshInstanced(particleMesh, 0, materials[i], meshPositions[i].ToArray(), Mathf.Min(1023, batchSize - j));
            }
        }
    }

    public void updateMatrix(int x, int y, bool neg, GameObject gameObj){
        matrix[x,y]+= 0.2f * (neg ? -1 : 1);
        matrix[x,y]=Mathf.Clamp(matrix[x,y],-1,1);
        gameObj.GetComponent<Image>().color = Color.HSVToRGB(matrix[x,y]<0 ? 0 : 120f/355f,1,Mathf.Abs(matrix[x,y]));
    }

    public Vector2 forceMultiplier(Particle p1, Particle p2){
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


}
