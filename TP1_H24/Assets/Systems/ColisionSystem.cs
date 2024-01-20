using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColisionSystem : ISystem
{
    public string Name { get; set; }

    private float sWidth;
    private float sHeight;
    public ColisionSystem()
    {
        Name = "ColisionSystem";
        var camSize = GetPlayableScreenSize();
        sWidth = camSize.x;
        sHeight = camSize.y;
    }

    private Vector2 GetPlayableScreenSize()
    {
        float cameraHeight = 2f * Camera.main.orthographicSize;
        float cameraWidth = cameraHeight * Camera.main.aspect;

        return new Vector2(cameraWidth, cameraHeight);
    }
    public void UpdateSystem()
    {
        foreach(var currentEntity in EntityManager.Instance.GetEntities())
        {
            PhysicComponent currentPhys = currentEntity.GetComponent<PhysicComponent>();

            if (currentPhys.position.x + (currentPhys.size / 2) >= sWidth / 2 || currentPhys.position.x - (currentPhys.size / 2) <= (-sWidth / 2))
            {
                currentPhys.velocity.x *= -1.0f;
            }
            if (currentPhys.position.y + (currentPhys.size / 2)>= sHeight / 2 || currentPhys.position.y - (currentPhys.size / 2) <= (-sHeight / 2))
            {
                currentPhys.velocity.y *= -1.0f;
            }

            foreach(var otherEntity in EntityManager.Instance.GetEntities())
            {
                if(currentEntity.id != otherEntity.id)
                {
                    PhysicComponent otherPhys = otherEntity.GetComponent<PhysicComponent>();
                    CollisionResult result = CollisionUtility.CalculateCollision(currentPhys.position, currentPhys.velocity, currentPhys.size, otherPhys.position, otherPhys.velocity, otherPhys.size);
                    if(result != null)
                    {
                        currentPhys.position = result.position1;
                        currentPhys.velocity = result.velocity1;
                        otherPhys.position = result.position2;
                        otherPhys.velocity = result.velocity2;
                        otherEntity.UpdateComponent(otherPhys);
                    }
                }
            }

            currentEntity.UpdateComponent(currentPhys);
        }
    }
}

