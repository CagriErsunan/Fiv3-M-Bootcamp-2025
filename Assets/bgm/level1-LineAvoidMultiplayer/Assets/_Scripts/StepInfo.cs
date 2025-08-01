using UnityEngine;

[RequireComponent(typeof(Collider))]
public class StepInfo : MonoBehaviour
{
    [Tooltip("1 = Tekli zıplama, 2 = Çiftli, 3 = Üçlü (bonus)")]
    public int requiredJumpLevel = 1;
}
