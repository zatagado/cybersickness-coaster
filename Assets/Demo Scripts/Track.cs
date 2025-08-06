using UnityEngine;

[CreateAssetMenu(menuName = "Cybersick Coaster Track")]
public class Track : ScriptableObject
{
    [SerializeField] private float gravity;
    [SerializeField] private bool continuousLoop;
    [SerializeField] private int loops;
    [SerializeField] private StoredSplinePoint[] points;
    // [SerializeField] private float[] lengths;
    [SerializeField] private CumulativeDistanceLUT[] curveLookUpTables;

    public float Gravity { get => gravity; set => gravity = value; }
    public bool ContinuousLoop { get => continuousLoop; set => continuousLoop = value; }
    public int Loops { get => loops; set => loops = value; }
    public StoredSplinePoint[] Points { get => points; set => points = value; }
    // public float[] Lengths { get => lengths; set => lengths = value; }
    public CumulativeDistanceLUT[] CurveLookUpTables { get => curveLookUpTables; set => curveLookUpTables = value; }
}
