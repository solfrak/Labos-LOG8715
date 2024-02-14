using UnityEngine;
public struct UnoptimisedStruct1
{
    public UnoptimizedStruct2 mainFriend; // 48
    public Vector3 position;// 12
    public double range;// 8
    public UnoptimizedStruct2[] otherFriends;//8
    public float[] distancesFromObjectives;// 8
    public float velocity;// 4
    public float size;// 4
    public int baseHP;// 4
    public int currentHp;// 4
    public int nbAllies;// 4
    public bool isVisible;// 1
    public bool canJump;// 1
    public bool isStanding;// 1
    public byte colorAlpha;// 1
    // sum: 108
    
    public UnoptimisedStruct1(float velocity, bool canJump, int baseHP, int nbAllies, Vector3 position, int currentHp, float[] distancesFromObjectives, byte colorAlpha, double range, UnoptimizedStruct2 mainFriend, bool isVisible, UnoptimizedStruct2[] otherFriends, bool isStanding, float size)
    {
        this.velocity = velocity;
        this.canJump = canJump;
        this.baseHP = baseHP;
        this.nbAllies = nbAllies;
        this.position = position;
        this.currentHp = currentHp;
        this.distancesFromObjectives = distancesFromObjectives;
        this.colorAlpha = colorAlpha;
        this.range = range;
        this.mainFriend = mainFriend;
        this.isVisible = isVisible;
        this.otherFriends = otherFriends;
        this.isStanding = isStanding;
        this.size = size;
    }
}

public enum FriendState
{
    isFolowing,
    isSearching,
    isPatrolling,
    isGuarding,
    isJumping,
}

public struct UnoptimizedStruct2 
{
    public Vector3 position;// 12
    public double maxSight;// 8
    public FriendState currentState;// 4
    public float height;// 4
    public float velocity;// 4
    public float acceleration;// 4
    public float maxVelocity;// 4
    public int currentObjective;// 4
    public bool isAlive;// 1
    public bool canJump;// 1
    // sum: 46+2 = 48
    
    public UnoptimizedStruct2(bool isAlive, float height, FriendState currentState, float velocity, int currentObjective, double maxSight, bool canJump, float acceleration, Vector3 position, float maxVelocity)
    {
        this.isAlive = isAlive;
        this.height = height;
        this.currentState = currentState;
        this.velocity = velocity;
        this.currentObjective = currentObjective;
        this.maxSight = maxSight;
        this.canJump = canJump;
        this.acceleration = acceleration;
        this.position = position;
        this.maxVelocity = maxVelocity;
    }
}
