
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class GameManager : MonoBehaviour {

    public struct Particle {
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
    List<Matrix4x4>[] meshMatrices;
    Mesh particleMesh;
    public Material[] materials;

    DynamicLayout matrixUI;
    float[,] matrix;
    void Start() {

        particleMesh =  Resources.GetBuiltinResource<Mesh>("Quad.fbx");
        Vector2 size = GetComponent<Renderer>().bounds.size; 
        meshMatrices = new List<Matrix4x4>[typeCount];
        particles = new Particle[particleCount];

        for (int i = 0; i < typeCount; i++) 
            meshMatrices[i] = new List<Matrix4x4>(); 
        

        for(int i = 0; i<particleCount; i++) {
            Particle p = new Particle();
            p.position = new Vector2(Random.Range(-0.5f, 0.5f) * size.x, Random.Range(-0.5f, 0.5f) * size.y);
            p.velocity = Vector2.zero;
            p.type = Random.Range(0, typeCount);
            particles[i] = p;

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
            meshMatrices[i].Clear();
        
        for(int i = 0; i<particleCount; i++){
            particles[i].velocity *= friction;
            for(int j = 0; j<particleCount; j++){
                if(i==j)continue;
                particles[i].velocity+= forceMultiplier(particles[i],particles[j]) * Time.deltaTime;
            }
            particles[i].position += particles[i].velocity * Time.deltaTime;

            //screen wrapping
            if(particles[i].position.x < -8.82f) particles[i].position.x = 8.82f;
            else if (particles[i].position.x > 8.82f) particles[i].position.x = -8.82f;
            if(particles[i].position.y < -4.98f) particles[i].position.y = 4.98f;
            else if(particles[i].position.y > 4.98f) particles[i].position.y = -4.98f;

            meshMatrices[particles[i].type].Add(
                Matrix4x4.TRS(particles[i].position, Quaternion.identity, Vector3.one * particleSize)
            );
        }

        render();
    }

    void render() {
        for(int i = 0; i<typeCount; i++){
            int batchSize = meshMatrices[i].Count;

            for(int j = 0; j< batchSize; j+=1023) {
                Graphics.DrawMeshInstanced(particleMesh, 0, materials[i], meshMatrices[i].ToArray(), Mathf.Min(1023, batchSize - j));
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
        Vector2 delta = (p2.position - p1.position);
        Vector2 direction = (p2.position - p1.position).normalized;
        float distance = Vector2.Distance(p1.position, p2.position);
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
