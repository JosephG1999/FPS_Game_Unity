using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;

public class Weapon : MonoBehaviour
{
    //Bullet
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public float bulletVelocity = 30f;
    public float bulletPrefabLifeTime = 3f; 

    // Update is called once per frame
    
    public Camera playerCamera;
    //all of the variables below will be fined tuned to created different weapons
    //Shooting
    public bool isShooting, readyToShoot;
    bool allowReset = true;
    public float shootingDelay;
    //Burst Fire
    public int bulletsPerBurst = 3;
    public int bulletsLeftInBurst;
    //Bullet Spread
    public float spreadIntensity;

    public enum ShootingMode //different firing modes
    {
        Single,
        Burst,
        Auto
    }

    public ShootingMode shootingMode; //used to determine a weapon's firing mode

    void Start()
    {
        readyToShoot = true;
        bulletsLeftInBurst = bulletsPerBurst;
    }

    void Update()
    {
        if (shootingMode == ShootingMode.Auto)
        {
            isShooting = Input.GetKey(KeyCode.Mouse0); //hold down M1 to shoot
        }
        else if (shootingMode == ShootingMode.Burst || shootingMode == ShootingMode.Single) //click M1 to shoot
        {
            isShooting = Input.GetKeyDown(KeyCode.Mouse0);
        }

        if (readyToShoot && isShooting)
        {
            bulletsLeftInBurst = bulletsPerBurst;
            FireWeapon();
        }

    }

    private void FireWeapon()
    {
        readyToShoot = false; //keeps the players from shooting again before the first shot is even done
        
        Vector3 shootingDirection = CalculateDirectionAndSpread();
        
        GameObject bullet = Instantiate (bulletPrefab, bulletSpawn.position, Quaternion.identity); //instantiate bullet
        bullet.transform.forward = shootingDirection; //turns the bullet towards where player is shooting
        bullet.GetComponent<Rigidbody>().AddForce(bulletSpawn.forward * bulletVelocity, ForceMode.Impulse); //shoot bullet

        StartCoroutine(DestroyBulletAfterTime(bullet, bulletPrefabLifeTime));

        if (allowReset) //checks if the previous shot is done before allowing the next one to occur
        {
            Invoke("ResetShot", shootingDelay);
            allowReset = false;
        }

        if (shootingMode == ShootingMode.Burst && bulletsLeftInBurst > 1) //allows burst fire weapons to finish their burst
        {
            bulletsLeftInBurst--;
            Invoke("FireWeapon", shootingDelay);
        }
        Debug.Log("Bullet position: " + bullet.transform.position);
        Debug.Log("Bullet rotation: " + bullet.transform.rotation);
        Debug.Log("Bullet scale: " + bullet.transform.localScale);
    }

    private void ResetShot()
    {
        readyToShoot = true;
        allowReset = true;
    }

    public Vector3 CalculateDirectionAndSpread()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); //ray shooting out of center screen
        RaycastHit hit;

        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(100);
        }
        //the code above basically does this:
        //if (the ray hits something "targetPoint", send the bullet flying there when the player shoots)
        //else (if the ray is just going into the abyss, set a point far away "targetPoint" for the bullet to fly away to)

        Vector3 direction = targetPoint - bulletSpawn.position;

        float x = Random.Range(-spreadIntensity, spreadIntensity);
        float y = Random.Range(-spreadIntensity, spreadIntensity);

        return direction + new Vector3(x, y, 0); //returns direction and spread
    }

    private IEnumerator DestroyBulletAfterTime(GameObject bullet, float delay) //destroys the bullet after delay
    {
        yield return new WaitForSeconds(delay);
        Destroy(bullet);
    }
}
