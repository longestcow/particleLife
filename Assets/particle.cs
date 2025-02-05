using UnityEngine;

public class particle : MonoBehaviour
{
    public int type;
    SpriteRenderer sprite;
    GameManager manager;
    Transform particles;
    Vector2 velocity = Vector2.zero;
    void Start(){
        manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        type = Random.Range(1, manager.typeCount+1);
        particles = GameObject.Find("particles").transform;
        sprite = GetComponent<SpriteRenderer>();
        sprite.color = manager.typeToColor(type);
    }

    // Update is called once per frame
    void Update()
    {
        velocity*=manager.friction; 

        for (int i = 0; i< particles.childCount; i++) {
            if(particles.GetChild(i) == transform) continue;
            velocity+= manager.forceMultiplier(this, particles.GetChild(i).GetComponent<particle>()) * Time.deltaTime;
        }


        transform.position += (Vector3) (velocity * Time.deltaTime);

        Vector2 pos = transform.position; //screen wrapping
        if(pos.x < -8.82f) pos.x = 8.82f;
        else if (pos.x > 8.82f) pos.x = -8.82f;
        if(pos.y < -4.98f) pos.y = 4.98f;
        else if(pos.y > 4.98f) pos.y = -4.98f;
        transform.position=pos;

    }


}
