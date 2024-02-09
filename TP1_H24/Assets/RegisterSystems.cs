using System.Collections.Generic;

public class RegisterSystems
{
    public static List<ISystem> GetListOfSystems()
    {
        // determine order of systems to add
        var toRegister = new List<ISystem>();
        
        // Add your systems here
        toRegister.Add(new SpawnerSystem(BaseEntityManager.Instance));
        toRegister.Add(new ColisionSystem(BaseEntityManager.Instance));
        toRegister.Add(new MovementSystem(BaseEntityManager.Instance));
        toRegister.Add(new SizeSystem(BaseEntityManager.Instance));
        toRegister.Add(new ProtectionSystem(BaseEntityManager.Instance));
        toRegister.Add(new ColorSystem(BaseEntityManager.Instance));
        toRegister.Add(new ExplosionSystem(BaseEntityManager.Instance));
        toRegister.Add(new TimeTravellerSystem(BaseEntityManager.Instance));
        toRegister.Add(new TimeMultiplierSystem(BaseEntityManager.Instance));
        toRegister.Add(new DestroySystem(BaseEntityManager.Instance));

        return toRegister;
    }
}