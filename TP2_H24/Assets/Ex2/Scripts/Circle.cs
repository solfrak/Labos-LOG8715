using UnityEngine;
using UnityEngine.Serialization;

public class Circle : MonoBehaviour
{
    [FormerlySerializedAs("I")] [HideInInspector]
    public int i;

    [FormerlySerializedAs("J")] [HideInInspector]
    public int j;

    public float Health { get; private set; }

    private const float BaseHealth = 1000;

    private const float HealingPerSecond = 1;
    private const float HealingRange = 3;

    private float healingSelfPerSecond = 0;
    private GridShape grid;
    private SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    private void Start()
    {
        Health = BaseHealth;
        grid = FindObjectOfType<GridShape>();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        // As circles do not move, we can gather all collisions at the start
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, HealingRange);
        for (int i = 0; i < nearbyColliders.Length; i++)
        {
            if (nearbyColliders[i].TryGetComponent<Circle>(out var circle))
            {
                circle.AddHealingEffect(HealingPerSecond);
            }
        }
    }

    // Update is called once per frame
    private void Update()
    {
        UpdateColor();
        HealSelf();
    }

    private void UpdateColor()
    {
        spriteRenderer.color = grid.Colors[i, j] * Health / BaseHealth;
    }

    private void HealSelf()
    {
        // The heal is based on the number of nearby circles, 
        // but as they do not move, the collision count is always the same.
        // Instead of healing others, the circle can heal itself to its number of nearby circles.
        ReceiveHp(healingSelfPerSecond * Time.deltaTime);
    }

    public void AddHealingEffect(float healingPerSecond)
    {
        healingSelfPerSecond += healingPerSecond;
    }

    public void ReceiveHp(float hpReceived)
    {
        Health += hpReceived;
        Health = Mathf.Clamp(Health, 0, BaseHealth);
    }
}
