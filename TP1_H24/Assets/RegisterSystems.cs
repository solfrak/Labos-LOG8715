using System.Collections.Generic;

public class RegisterSystems
{
    public static List<ISystem> GetListOfSystems()
    {
        // determine order of systems to add
        var toRegister = new List<ISystem>();
        
        // Add your systems here
        toRegister.Add(new SpawnerSystem());
        toRegister.Add(new MovementSystem());

        return toRegister;
    }
}