
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Collections.Concurrent;


public class GameManager : MonoBehaviour {
    

    // cant change mid sim
    int dimension;
    float radius;
    int typeCount;
    int particleCount;


    Particle[] particles;
    List<Matrix4x4>[] meshPositions;
    Mesh particleMesh;
    public Material[] materials;
    RenderParams[] renderParams;

    List<Particle>[,] grid;

    int gridDim;
    Matrix4x4[] matrixBuffer = new Matrix4x4[1023];

    int gridx,gridy;
    bool pPressed = false;
    Vector2 pMousePosition = Vector2.zero, difference;
    ConcurrentBag<Matrix4x4>[] meshPositionsConcurrent;

    DynamicLayout matrixUI;

    void Start() {
        matrixUI = GameObject.Find("Canvas").GetComponent<DynamicLayout>();

        dimension = matrixUI.dimension;
        radius = matrixUI.radius;
        typeCount = matrixUI.typeCount;
        particleCount = matrixUI.particleCount;
        
        meshPositionsConcurrent = new ConcurrentBag<Matrix4x4>[typeCount];
        transform.localScale = new Vector2(dimension, dimension);
        transform.position = new Vector2(dimension/2, dimension/2);
        renderParams = new RenderParams[typeCount];
        particleMesh =  Resources.GetBuiltinResource<Mesh>("Quad.fbx");
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
            meshPositionsConcurrent[i] = new ConcurrentBag<Matrix4x4>();
            renderParams[i] = new RenderParams(materials[i]);

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


        for(int i = 0; i<typeCount; i++){
            meshPositions[i].Clear();
            meshPositionsConcurrent[i].Clear();
        }
        
        Parallel.For(0, particles.Length, i=>{
            particles[i].velocity *= matrixUI.friction;
        
            for(int offi = -1; offi<=1; offi++){
                for(int offj = -1; offj<=1; offj++){
                    gridx = (int)nfmod((int)(particles[i].position.x/radius)+offi,gridDim);
                    gridy = (int)nfmod((int)(particles[i].position.y/radius)+offj,gridDim);
                    foreach(Particle p2 in grid[gridy,gridx]){
                        if(particles[i].id==p2.id)continue;
                        particles[i].velocity+= forceMultiplier(particles[i],p2) * 0.02f;
                    }
                }
                    
            }
            
            particles[i].position += particles[i].velocity * 0.02f;
            if(particles[i].position.x < 0) particles[i].position.x = dimension-0.1f;
            else if (particles[i].position.x >= dimension) particles[i].position.x = 0;
            if(particles[i].position.y < 0) particles[i].position.y = dimension-0.1f;
            else if(particles[i].position.y >= dimension) particles[i].position.y = 0;

            meshPositionsConcurrent[particles[i].type].Add(
                Matrix4x4.TRS(particles[i].position, Quaternion.identity, Vector3.one * matrixUI.particleSize)
            );

        });

        for(int i = 0 ;i < gridDim; i++){
            for(int j = 0; j < gridDim; j++)
                grid[i,j].Clear();
        }

        for(int i = 0; i<particleCount; i++)
            grid[Mathf.Clamp((int)(particles[i].position.y/radius), 0, gridDim-1), Mathf.Clamp((int)(particles[i].position.x/radius), 0, gridDim-1)].Add(particles[i]);
        

        for (int i = 0; i < typeCount; i++)
            meshPositions[i] = new List<Matrix4x4>(meshPositionsConcurrent[i]);

        render();
    }

    void render() {
        for(int i = 0; i<typeCount; i++){
            int batchSize = meshPositions[i].Count;
            for(int j = 0; j< batchSize; j+=1023) {
                int count = Mathf.Min(1023, batchSize - j);
                for(int k = 0; k<count; k++)
                    matrixBuffer[k] = meshPositions[i][j + k];
                Graphics.RenderMeshInstanced(renderParams[i], particleMesh, 0, matrixBuffer, count);
            } 
        }
    }



    Vector2 forceMultiplier(Particle p1, Particle p2){
        float force = 1;
        Vector2 delta = p2.position - p1.position;
        Vector2 direction = (p2.position - p1.position).normalized;
        float distance = Vector2.Distance(p1.position, p2.position);
        if(distance>radius) {
            if (Mathf.Abs(delta.x) > dimension / 2)
                delta.x -= Mathf.Sign(delta.x) * dimension;

            if (Mathf.Abs(delta.y) > dimension / 2)
                delta.y -= Mathf.Sign(delta.y) * dimension;

            distance = delta.magnitude;
            direction = delta.normalized;
            if(distance>radius)return Vector2.zero;
        }
        if(distance<=matrixUI.tooCloseR && matrixUI.tooCloseR!=0) return mapfloat(distance,  0, matrixUI.tooCloseR, -1, 0) * direction * matrixUI.forceScale; 
        else if(distance<=matrixUI.tooCloseR+((radius-matrixUI.tooCloseR)/2)) force = mapfloat(distance, matrixUI.tooCloseR, matrixUI.tooCloseR+((radius-matrixUI.tooCloseR)/2), 0, 1);
        else force = mapfloat(distance,  matrixUI.tooCloseR+((radius-matrixUI.tooCloseR)/2), radius, 1, 0);
        force*=matrixUI.matrix[p2.type, p1.type]; 
        return force * direction * matrixUI.forceScale;
        
        
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
