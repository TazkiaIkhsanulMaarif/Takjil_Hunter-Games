using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CarScript : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 180f;

    // Menyimpan arah pergerakan saat ini
    private Vector3 currentDirection;

    private void Start()
    {
        // Set arah awal pergerakan
        currentDirection = Vector3.forward;
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        // Gerakan maju (diarahkan ke depan objek)
        transform.Translate(currentDirection * moveSpeed * Time.deltaTime);

        // Periksa apakah ada belokan yang diperlukan
        CheckForTurn();
    }

    private void CheckForTurn()
    {
        // Raycast ke depan objek untuk mendeteksi belokan
        Ray ray = new Ray(transform.position, currentDirection);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1f))
        {
            // Jika ada belokan, atur arah baru
            currentDirection = GetNewDirection();
        }
    }

    private Vector3 GetNewDirection()
    {
        int x = Mathf.RoundToInt(transform.position.x / 6);
        int z = Mathf.RoundToInt(transform.position.z / 6);

        Vector3[] directions = { Vector3.forward, Vector3.right, Vector3.back, Vector3.left };

        foreach (Vector3 direction in directions)
        {
            int newX = Mathf.RoundToInt((transform.position.x + direction.x * 6) / 6);
            int newZ = Mathf.RoundToInt((transform.position.z + direction.z * 6) / 6);

            if (MazeLogic.instance.IsInBounds(newX, newZ))
            {
                // Periksa jenis sel di depan mobil
                byte cellType = MazeLogic.instance.GetCellType(newX, newZ);

                // if (cellType == EmptyCell)
                // {
                //     return direction;
                // }
                // else if (cellType == HoleCell)
                // {
                //     // Lakukan tindakan khusus untuk lubang atau rintangan
                //     HandleHole();
                // }
            }
        }

        return Vector3.forward;
    }

    private void HandleHole()
    {
        // Tambahkan tindakan khusus yang diambil mobil ketika menemui lubang atau rintangan
        Debug.Log("Mobil menemui lubang atau rintangan!");
        // Misalnya, bisa membuat mobil berhenti atau melakukan sesuatu yang sesuai dengan permainan Anda.
    }

}
