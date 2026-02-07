using JFlex.PacmanWFC;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [HideInInspector]
    public GameManager Instance;

    [SerializeField]
    private GridBuilder gridBuilder;

    private void Awake()
    {
        Instance = this;

    }
}