using UnityEngine;

public class EnergyView : MonoBehaviour
{
    [SerializeField] private GameObject[] _energyPoints;

    public void UpdateEnergy(int value)
    {
        for (int i = 0; i < _energyPoints.Length; i++)
        {
            _energyPoints[i].SetActive(i < value);
        }
    }
}

