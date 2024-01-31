using System.Collections.Generic;

public class RegisterSystems
{
    public static List<ISystem> GetListOfSystems()
    {
        // determine order of systems to add
        var toRegister = new List<ISystem>();
        
        // Add your systems here
        toRegister.Add(new SpawnerSystem());
        toRegister.Add(new ColisionSystem());
        toRegister.Add(new MovementSystem());
        toRegister.Add(new SizeSystem());
        toRegister.Add(new DestroySystem());
        toRegister.Add(new ColorSystem());
        toRegister.Add(new ProtectionSystem());
        toRegister.Add(new ExplosionSystem());

        return toRegister;
    }
}