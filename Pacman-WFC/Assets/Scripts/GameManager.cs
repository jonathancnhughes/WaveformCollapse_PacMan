using JFlex.PacmanWFC;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [HideInInspector]
    public GameManager Instance;

    [SerializeField]
    private GridGenerator gridBuilder;

    private void Awake()
    {
        Instance = this;

    }
}