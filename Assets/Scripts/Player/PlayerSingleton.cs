using UnityEngine;

public class PlayerSingleton : MonoBehaviour
{
    //Use this to easily acces any player component without needing explicit references
    //PlayerSingleton.instance.gameobject.GetComponenet<anycomponent>();
    public static PlayerSingleton instance;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("Multiple player singletons detected -> not good");
        }
    }

}
